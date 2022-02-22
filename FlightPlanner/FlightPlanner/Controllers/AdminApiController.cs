using FlightPlanner.Models;
using FlightPlanner.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace FlightPlanner.Controllers
{
    [Route("admin-api")]
    [ApiController]
    [EnableCors]
    public class AdminApiController : ControllerBase
    {
        private static readonly object _lock = new object();

        [HttpGet]
        [Route("flights/{id}")]
        [Authorize]
        public IActionResult GetFlights(int id)
        {
            var flight = FlightStorage.GetFlight(id);
            if (flight == null)
                return NotFound();

            return Ok(flight);
        }

        [HttpDelete]
        [Route("flights/{id}")]
        [Authorize]
        public IActionResult DeleteFlights(int id)
        {
            lock (_lock)
            {
                FlightStorage.DeleteFlight(id);
                return Ok();
            }
        }

        [HttpPut, Authorize]
        [Route("flights")]
        public IActionResult PutFlights(AddFlightRequest request)
        {
            lock (_lock)
            {
                if (FlightStorage.Exists(request))
                {
                    return Conflict();
                }

                if (FlightStorage.IsValid(request) == false)
                {
                    return BadRequest();
                }
                var flight = FlightStorage.AddFlight(request);
                return Created("", flight);
            }
        }
    }
}