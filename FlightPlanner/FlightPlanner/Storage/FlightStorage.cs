using FlightPlanner.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightPlanner.Storage
{
    public static class FlightStorage
    {
        private static List<Flight> _flights = new List<Flight>();
        private static int _id;
        private static readonly object _lock = new object();

        public static Flight AddFlight(AddFlightRequest request)
        {
            lock (_lock)
            {
                var flight = new Flight
                {
                    From = request.From,
                    To = request.To,
                    ArrivalTime = request.ArrivalTime,
                    DepartureTime = request.DepartureTime,
                    Carrier = request.Carrier,
                    Id = ++_id
                };
                _flights.Add(flight);

                return flight;
            }
        }

        public static Flight ConvertToFlight(AddFlightRequest request)
        {
            var flight = new Flight
            {
                From = request.From,
                To = request.To,
                ArrivalTime = request.ArrivalTime,
                DepartureTime = request.DepartureTime,
                Carrier = request.Carrier,
            };
            return flight;
        }

        public static Flight GetFlight(int id)
        {
            lock (_lock)
            {
                return _flights.SingleOrDefault(f => f.Id == id);
            }
        }

        public static void DeleteFlight(int id)
        {
            lock (_lock)
            {
                var flight = GetFlight(id);
                if (flight != null)
                {
                    _flights.Remove(flight);
                }
            }
        }

        public static List<Airport> FindAirports(string input, FlightPlannerDbContext context)
        {
            lock (_lock)
            {
                input = input.ToLower().Trim();
                var fromAirports = context.Flights.Where(f => f.From.AirportName.ToLower().Trim().Contains(input)
                                                                  || f.From.City.ToLower().Trim().Contains(input)
                                                                  || f.From.Country.ToLower().Trim().Contains(input)).
                                                        Select(a => a.From).ToList();
                var toAirports = context.Flights.Where(f => f.To.AirportName.ToLower().Trim().Contains(input)
                                                            || f.To.City.ToLower().Trim().Contains(input)
                                                            || f.To.Country.ToLower().Trim().Contains(input)).
                                                        Select(f => f.To).ToList();

                return fromAirports.Concat(toAirports).ToList();
            }
        }

        public static void ClearFlights()
        {
            _flights.Clear();
            _id = 0;
        }

        public static bool Exists(AddFlightRequest request)
        {
            lock (_lock)
            {
                return _flights.Any(f => f.Carrier.ToLower().Trim() == request.Carrier.ToLower().Trim() &&
                                         f.DepartureTime == request.DepartureTime &&
                                         f.ArrivalTime == request.ArrivalTime &&
                                         f.From.AirportName.ToLower().Trim() == request.From.AirportName.ToLower().Trim() &&
                                         f.To.AirportName.ToLower().Trim() == request.To.AirportName.ToLower().Trim());
            }
        }

        public static bool IsValid(AddFlightRequest request)
        {
            lock (_lock)
            {

                if (request == null)
                    return false;

                if (string.IsNullOrEmpty(request.ArrivalTime) || string.IsNullOrEmpty(request.Carrier) ||
                    string.IsNullOrEmpty(request.DepartureTime))
                    return false;

                if (request.From == null || request.To == null)
                    return false;

                if (string.IsNullOrEmpty(request.From.AirportName) || string.IsNullOrEmpty(request.From.City) ||
                    string.IsNullOrEmpty(request.From.Country))
                    return false;

                if (string.IsNullOrEmpty(request.To.AirportName) || string.IsNullOrEmpty(request.To.City) ||
                    string.IsNullOrEmpty(request.To.Country))
                    return false;

                if (request.From.Country.ToLower().Trim() == request.To.Country.ToLower().Trim() &&
                    request.From.City.ToLower().Trim() == request.To.City.ToLower().Trim()
                    && request.From.AirportName.ToLower().Trim() == request.To.AirportName.ToLower().Trim())
                    return false;

                var arrivalTime = DateTime.Parse(request.ArrivalTime);
                var departureTime = DateTime.Parse(request.DepartureTime);

                if (arrivalTime <= departureTime)
                {
                    return false;
                }

                return true;
            }

        }

        public static bool IsValidRequest(SearchFlightRequest request)
        {
            lock (_lock)
            {
                if (request.From == request.To)
                {
                    return true;
                }

                if (request.From == null || request.To == null || request.DepartureDate == null)
                {
                    return true;
                }

                return false;
            }
        }

        public static PageResult SearchFlightRequest(SearchFlightRequest request, FlightPlannerDbContext context)
        {

            lock (_lock)
            {
                var flights = context.Flights.Where(x =>
                    x.From.AirportName == request.From &&
                    x.To.AirportName == request.To &&
                    x.DepartureTime.Substring(0, 10) == request.DepartureDate).ToList();

                return new PageResult(flights);
            }
        }
    }
}
