using TestPrep.DTO;

namespace TestPrep.Services
{
    public interface IRentalService
    {
        Task<CustomerRentalDTO?> GetCustomerRentalsAsync(int id);
    }
}
