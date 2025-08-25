using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManagement.Models;

namespace HotelManagement.Services
{
    internal class ReservationService
    {
        private readonly List<Room> _rooms;
        private readonly List<Customer> _customers;
        private readonly List<Reservation> _reservations = new();
        private int _nextReservationCount = 1;

        public ReservationService(List<Room> rooms, List<Customer> customers)
        {
            _rooms = rooms;
            _customers = customers;
        }

        public Reservation? CreaReservation(int roomID, int customerId, DateOnly checkIn, DateOnly checkOut)
        {
            if (checkOut <= checkIn) return null;
            var room = _rooms.FirstOrDefault(r => r.Id == roomID);
            var customer = _customers.FirstOrDefault(c => c.Id == customerId);
            if (room == null || customer == null) return null;

            var nights = checkOut.DayNumber - checkIn.DayNumber;
            var total = nights * room.Price;

            var res = new Reservation
            {
                Id = _nextReservationCount,
                RoomId = roomID,
                CustomerId = customerId,
                CheckIn = checkIn,
                CheckOut = checkOut,
                Total = total,
            };

            _reservations.Add(res);
            return res;
        }

        public IEnumerable<Reservation> GetAllReservations() => _reservations;


    }
}
