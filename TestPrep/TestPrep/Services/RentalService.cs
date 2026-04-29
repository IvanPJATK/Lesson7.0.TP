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
            await using var connection = new SqlConnection(_connectionString);
            const string customer_sql =
                """
                    select first_name, last_name
                    from Customer
                    where customer_id = @id
                """;
            await using var command = new SqlCommand(customer_sql, connection);
            command.Parameters.AddWithValue("@id", id);
            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();
            var FirstName = reader["first_name"];
            var LastName = reader["last_name"];
            //Rentals and movies
            
            const string rentalsSql =
                """
                    select r.rental_id, r.rental_date, r.return_date, 
                    s.name as status_name,
                    m.movie_id, m.title, m.price_per_day
                    from Rental r
                    join Status s on r.status_id = s.status_id
                    join Rental_Item ri on r.rental_id = ri.rental_id
                    join Movie m on m.movie_id = ri.movie_id
                    where customer_id = @id
                """;
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            await connection.OpenAsync();
            await using var readerR = await command.ExecuteReaderAsync();
            var rentalsAndMovies = new Dictionary<int, RentalsDTO>();
            while(readerR.ReadAsync())
            {
                int rental_id = readerR.GetInt32(readerR.GetOrdinal("rental_id"));
                if(!rentasAndMovies.TryGetValue(rental_id, out var rental))
                {
                    rental = new RentalsDTO
                    {
                        Id = rental_id,
                        RentalDate = reader.GetDateTime(reader.GetOrdinal("rental_date")),
                        status = reader.GetString(reader.GetOrdinal("status_name")),
                        movies = new List<MoviesDTO>()
                    };
                    rentalsAndMovies.Add(rental_id, rental);
                }
                rental.Movies.Add(new MoviesDTO
                {
                    MovieId = readerR.GetInt32(readerR.GetOrdinal("movie_id")),
                    Title = readerR.GetString(readerR.GetOrdinal("title")),
                    PriceAtRental = readerR.GetDecimal(readerR.GetOrdinal("price_per_day"))
                });
            }
            return rentalsAndMovies.Values;
        }
    }
}
