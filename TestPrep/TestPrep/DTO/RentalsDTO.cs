namespace TestPrep.DTO
{
    public class RentalsDTO
    {
        public int Id { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime ReturnlDate { get; set; }
        public string status { get; set; } = null!;
        public List<MoviesDTO> movies =  new List<MoviesDTO> { };
    }
}
