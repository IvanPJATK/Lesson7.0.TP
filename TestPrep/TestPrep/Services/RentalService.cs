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
            const string sql =
                """
                    select rental_id, rental_date, return_date, status_id
                    from Rental
                    where customer_id = @id
                """;
            return Ok();
        }
    }
}
