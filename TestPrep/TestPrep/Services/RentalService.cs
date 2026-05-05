using System.Data.SqlTypes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TestPrep.DTO;

namespace TestPrep.Services
{
    public class RentalService : IRentalService
    {
        private readonly string _connectionString;
        public RentalService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException();
        }

        public async Task<RentalsDTO?> AddNewRentalAsync(int id, RentalsDTO rental)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

            try
            {
                const string customer_check_sql = """
                                            select first_name 
                                            from Customer
                                            where customer_id = @id
                """;

                await using var check_command = new SqlCommand(customer_check_sql, connection, transaction);
                check_command.Parameters.AddWithValue("@id", id);
                await using var check_reader = await check_command.ExecuteReaderAsync();
                string firstName = null;
                while (await check_reader.ReadAsync())
                {
                    firstName = check_reader.GetString(check_reader.GetOrdinal("first_name"));
                }
                if (firstName == null) return null;
                check_reader.Close();

                var movies = rental.movies.Select(m => m.Title).ToList();
                if (movies.Count == 0) return null;

                var moviesToSqlParam = string.Join(",", movies.Select((_, i) => $"@m{i}"));
                string movies_check_sql = $"select count(*) from Movie where title in ({moviesToSqlParam})";

                await using var command = new SqlCommand(movies_check_sql, connection, transaction);
                for (int i = 0; i < movies.Count; i++)
                {
                    command.Parameters.AddWithValue($"@m{i}", movies[i]);
                }

                int existingcount = (int)await command.ExecuteScalarAsync();
                if (existingcount != movies.Count) return null;

                const string insert_rental_sql = """
                SET IDENTITY_INSERT Rental ON;
                                    insert into Rental (rental_id, rental_date, return_date, customer_id, status_id) 
                                    values (@rental_id, @s_date, @e_date, @customer_id, (select status_id from status where name = @status))
                
                SET IDENTITY_INSERT Rental OFF;
                """;

                await using var insert_rental_command = new SqlCommand(insert_rental_sql, connection, transaction);
                insert_rental_command.Parameters.AddWithValue("@rental_id", rental.Id);
                insert_rental_command.Parameters.AddWithValue("@s_date", rental.RentalDate);
                insert_rental_command.Parameters.AddWithValue("@e_date", (object?)rental.ReturnlDate ?? DBNull.Value);
                insert_rental_command.Parameters.AddWithValue("@customer_id", id);
                insert_rental_command.Parameters.AddWithValue("@status", rental.status.Length == 0 ? "Rented" : rental.status);
                await insert_rental_command.ExecuteNonQueryAsync();

                foreach (var movie in rental.movies)
                {
                    const string insert_rental_item_sql = """
                                                    insert into Rental_Item (rental_id, movie_id, price_at_rental)
                                                    select @rental_id, movie_id, price_per_day
                                                    from Movie where title = @title
                    """;
                    await using var insert_rental_item_command = new SqlCommand(insert_rental_item_sql, connection, transaction);
                    insert_rental_item_command.Parameters.AddWithValue("@rental_id", rental.Id);
                    insert_rental_item_command.Parameters.AddWithValue("@title", movie.Title);
                    await insert_rental_item_command.ExecuteNonQueryAsync();
                }
                await transaction.CommitAsync();
                return rental;
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            }

        }

        public async Task<CustomerDTO?> GetCustomerAsync(int id)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            const string selectSql = """
                                    select first_name, last_name from Customer where customer_id = @id
                """;
            await using var command = new SqlCommand(selectSql, connection);
            command.Parameters.AddWithValue("@id", id);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string first_name = null;
                first_name = reader.GetString(reader.GetOrdinal("first_name"));
                if (first_name == null)
                {
                    return null;
                }
                string last_name = reader.GetString(reader.GetOrdinal("last_name"));
                CustomerDTO customer = new CustomerDTO
                {
                    First_Name = first_name,
                    Last_Name = last_name,
                    Id = id
                };
                return customer;
            }

            return null;
        }

        public async Task<CustomerRentalDTO?> GetCustomerRentalsAsync(int id)
        {
            var rentalDict = new Dictionary<int, RentalsDTO>();
            string firstName = "";
            string lastName = "";

            await using var connection = new SqlConnection(_connectionString);
            const string sql = """
                    select c.first_name, c.last_name,
                    r.rental_id, r.rental_date, s.name as status_name,
                    m.title, m.price_per_day
                    from Customer c
                    join Rental r on c.customer_id = r.customer_id
                    join Status s ON r.status_id = s.status_id
                    left join Rental_Item ri on r.rental_id = ri.rental_id
                    left join Movie m on ri.movie_id = m.movie_id
                    where c.customer_id = @id 
                """;
            await connection.OpenAsync();
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            { 
                firstName = reader.GetString(reader.GetOrdinal("first_name"));
                lastName = reader.GetString(reader.GetOrdinal("last_name"));
                if(reader.IsDBNull(reader.GetOrdinal("rental_id"))) continue; //checking if there is any reservations associated with this customer 
                int rentalId = reader.GetInt32(reader.GetOrdinal("rental_id"));
                if(!rentalDict.TryGetValue(rentalId, out var rental))
                {
                    rental = new RentalsDTO
                    {
                        Id = rentalId,
                        RentalDate = reader.GetDateTime(reader.GetOrdinal("rental_date")),
                        status = reader.GetString(reader.GetOrdinal("status_name")),
                        movies = new List<MoviesDTO>()
                    };
                    rentalDict.Add(rentalId, rental);
                }
                rental.movies.Add(new MoviesDTO
                {
                    Title = reader.GetString(reader.GetOrdinal("title")),
                    PriceAtRental = reader.GetDecimal(reader.GetOrdinal("price_per_day"))
                });
            }
            if (string.IsNullOrEmpty(firstName)) return null;
            return new CustomerRentalDTO
            { FirstName = firstName, LastName = lastName, Rentals = rentalDict.Values.ToList()};
        }
    }
}
