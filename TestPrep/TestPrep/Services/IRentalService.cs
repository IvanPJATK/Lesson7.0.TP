using TestPrep.DTO;

namespace TestPrep.Services
{
    public interface IRentalService
    {
        Task<CustomerRentalDTO?> GetCustomerRentalsAsync(int id);
        Task<RentalsDTO?> AddNewRentalAsync(int id, RentalsDTO rental);

        Task<CustomerDTO?> GetCustomerAsync(int id);
    }
}
