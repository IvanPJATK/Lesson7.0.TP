using System.Data.SqlTypes;
using Microsoft.AspNetCore.Http.HttpResults;
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

        //public async Task<CustomerRentalDTO?> GetCustomerRentalsBrokenAsync(int id)
        //{
        //    await using var connection = new SqlConnection(_connectionString);
        //    const string customer_sql =
        //        """
        //            select first_name, last_name
        //            from Customer
        //            where customer_id = @id
        //        """;
        //    await using var command = new SqlCommand(customer_sql, connection);
        //    command.Parameters.AddWithValue("@id", id);
        //    await connection.OpenAsync();
        //    await using var reader = await command.ExecuteReaderAsync();
        //    string FirstName = null;
        //    string LastName = null;
        //    if (await reader.ReadAsync())
        //    {
        //        FirstName = reader.GetString(reader.GetOrdinal("first_name"));
        //        LastName = reader.GetString(reader.GetOrdinal("last_name"));
        //    }
        //    //Rentals and movies

        //    const string rentalsSql =
        //        """
        //            select r.rental_id, r.rental_date, r.return_date, 
        //            s.name as status_name,
        //            m.movie_id, m.title, m.price_per_day
        //            from Rental r
        //            join Status s on r.status_id = s.status_id
        //            join Rental_Item ri on r.rental_id = ri.rental_id
        //            join Movie m on m.movie_id = ri.movie_id
        //            where customer_id = @id
        //        """;
        //    await using var commandR = new SqlCommand(rentalsSql, connection);
        //    commandR.Parameters.AddWithValue("@id", id);
        //    await connection.OpenAsync();
        //    await using var readerR = await command.ExecuteReaderAsync();
        //    var rentalsWithMovies = new List<RentalsDTO>();
        //    while(await readerR.ReadAsync())
        //    {
        //        var rental = new RentalsDTO
        //        {
        //            Id = readerR.GetInt32(readerR.GetOrdinal("rental_id")),
        //            RentalDate = reader.GetDateTime(reader.GetOrdinal("rental_date")),
        //            status = reader.GetString(reader.GetOrdinal("status_name")),
        //            movies = new List<MoviesDTO>()
        //        };
        //        if (!rentalsWithMovies.Contains(rental))
        //        {
        //            rentalsWithMovies.Add(rental);
        //        }
        //        else
        //        {
        //            rental.movies.Add(new MoviesDTO
        //            {
        //                Title = readerR.GetString(readerR.GetOrdinal("title")),
        //                PriceAtRental = readerR.GetDecimal(readerR.GetOrdinal("price_per_day"))
        //            });
        //        }
        //    }
        //    return new CustomerRentalDTO {FirstName = FirstName, LastName = LastName, Rentals = rentalsWithMovies};
        //}
    }
}
