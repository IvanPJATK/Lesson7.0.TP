using Microsoft.AspNetCore.Mvc;
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
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCustomerRentals(int id)
        {
            var customer = await _rentalService.GetCustomerRentalsAsync(id);

            if (customer == null)
            {
                return NotFound();
            }
            
            return Ok(customer);
        }
    }
}
