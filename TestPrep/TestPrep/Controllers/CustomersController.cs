using Microsoft.AspNetCore.Mvc;
using TestPrep.DTO;
using TestPrep.Services;

namespace TestPrep.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IRentalService _rentalService;
        public CustomersController(IRentalService rentalservice) 
        {
            _rentalService = rentalservice;
        }
        [HttpGet("{id:int}/rentals")]
        public async Task<IActionResult> GetCustomerRentals(int id)
        {
            var customer = await _rentalService.GetCustomerRentalsAsync(id);

            if (customer == null)
            {
                return NotFound();
            }
            
            return Ok(customer);
        }
        [HttpPost("{customer_id:int}/rentals")]
        public async Task<IActionResult> AddNewRental(int customer_id, [FromBody]RentalsDTO rental)
        {
            if(rental == null) return BadRequest();

            var created_rental = await _rentalService.AddNewRentalAsync(customer_id, rental);
            if (created_rental == null) return BadRequest(rental);
            
            return Ok(created_rental);
        }
    }
}
