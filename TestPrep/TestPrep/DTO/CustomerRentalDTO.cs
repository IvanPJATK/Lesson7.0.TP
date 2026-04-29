namespace TestPrep.DTO
{
    public class CustomerRentalDTO
    {
        public string FirstName { get; set; }  = null!;
        public string LastName { get; set; } = null!;

        public List<RentalsDTO> Rentals { get; set; } = new List<RentalsDTO>();
    }
}
