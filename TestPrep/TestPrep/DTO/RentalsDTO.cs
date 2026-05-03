using Microsoft.AspNetCore.Http.HttpResults;

namespace TestPrep.DTO
{
    public class RentalsDTO
    {
        public int Id { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime? ReturnlDate { get; set; }
        public string status { get; set; } = "Rented"; // was null! changed it to Rented
        public List<MoviesDTO> movies { get; set; } = new List<MoviesDTO> { };
    }
}
