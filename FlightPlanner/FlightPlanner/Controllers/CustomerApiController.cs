using System.Linq;
using FlightPlanner.Models;
using FlightPlanner.Storage;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightPlanner.Controllers
{
    [Route("api")]
    [ApiController]
    [EnableCors]
    public class CustomerApiController : ControllerBase
    {
        private readonly FlightPlannerDbContext _context;
        private static readonly object _lock = new object();

        public CustomerApiController(FlightPlannerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("Airports")]
        public IActionResult SearchAirports(string search)
        {
            lock (_lock)
            {
                var airports = FlightStorage.FindAirports(search, _context);
                return Ok(airports);
            }
        }

        [HttpPost]
        [Route("Flights/search")]
        public IActionResult SearchFlights(SearchFlightRequest request)
        {
            lock (_lock)
            {
                if (FlightStorage.IsValidRequest(request))
                {
                    return BadRequest();
                }

                return Ok(FlightStorage.SearchFlightRequest(request, _context));
            }

        }

        [HttpGet]
        [Route("flights/{id}")]
        public IActionResult SearchFlightsById(int id)
        {
            lock (_lock)
            {
                var flight = _context.Flights
                    .Include(f => f.From)
                    .Include(f => f.To)
                    .SingleOrDefault(f => f.Id == id);

                if (flight == null)
                {
                    return NotFound();
                }

                return Ok(flight);
            }
        }
    }
}
