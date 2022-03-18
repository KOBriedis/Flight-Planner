using System.Linq;
using FlightPlanner.Models;
using FlightPlanner.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightPlanner.Controllers
{
    [Route("admin-api")]
    [ApiController]
    [EnableCors]
    public class AdminApiController : ControllerBase
    {
        private static readonly object _lock = new object();

        private readonly FlightPlannerDbContext _context;

        public AdminApiController(FlightPlannerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("flights/{id}")]
        [Authorize]
        public IActionResult GetFlights(int id)
        {

            var flight = _context.Flights.Include(f => f.From)
                .Include(f => f.To)
                .SingleOrDefault(f => f.Id == id);
            if (flight == null)
                return NotFound();

            return Ok(flight);
        }

        [HttpPut, Authorize]
        [Route("flights")]
        public IActionResult AddFlights(AddFlightRequest request)
        {
            lock (_lock)
            {
                if (!FlightStorage.IsValid(request))
                    return BadRequest();
                if (Exists(request))
                    return Conflict();
                var flight = FlightStorage.ConvertToFlight(request);
                _context.Flights.Add(flight);
                _context.SaveChanges();
                return Created("", flight);
            }
        }

        [HttpDelete]
        [Route("flights/{id}")]
        [Authorize]
        public IActionResult DeleteFlights(int id)
        {
            lock (_lock)
            {
                var flight = _context.Flights
                    .Include(f => f.From)
                    .Include(f => f.To)
                    .SingleOrDefault(f => f.Id == id);

                if (flight != null)
                {
                    _context.Flights.Remove(flight);
                    _context.SaveChanges();
                }
                return Ok();
            }
        }

        private bool Exists(AddFlightRequest request)
        {
            lock (_lock)
            {
                return _context.Flights.Any(f => f.ArrivalTime == request.ArrivalTime && f.DepartureTime == request.DepartureTime &&
                          f.From.AirportName.Trim().ToLower() == request.From.AirportName.Trim().ToLower() &&
                          f.To.AirportName.Trim().ToLower() == request.To.AirportName.Trim().ToLower());
            }
        }
    }
}