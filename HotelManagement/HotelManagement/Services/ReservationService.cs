// ./Services/ReservationService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using HotelManagement.Models;

namespace HotelManagement.Services
{
    public class ReservationService
    {
        private readonly List<Room> _rooms;
        private readonly List<Customer> _customers;
        private readonly List<Reservation> _reservations = new();
        private int _nextReservationId = 1;

        public ReservationService(List<Room> rooms, List<Customer> customers)
        {
            _rooms = rooms;
            _customers = customers;
        }

        private static bool Overlap(DateOnly aStart, DateOnly aEnd, DateOnly bStart, DateOnly bEnd)
            => aStart < bEnd && bStart < aEnd;

        public bool IsRoomAvailable(int roomId, DateOnly from, DateOnly to, int excludeReservationId = 0)
        {
            return !_reservations.Any(r =>
                r.RoomId == roomId &&
                r.Id != excludeReservationId &&
                Overlap(r.CheckIn, r.CheckOut, from, to));
        }

        public Reservation? CreateReservation(int roomId, int customerId, DateOnly checkIn, DateOnly checkOut, IPricingStrategy pricingStrategy)
        {
            if (checkOut <= checkIn) return null;
            if (checkIn < DateOnly.FromDateTime(DateTime.Today)) return null;

            var room = _rooms.FirstOrDefault(r => r.Id == roomId);
            var customer = _customers.FirstOrDefault(c => c.Id == customerId);
            if (room is null || customer is null) return null;

            if (!IsRoomAvailable(roomId, checkIn, checkOut)) return null;

            var total = pricingStrategy.CalculateTotal(room, checkIn, checkOut);

            var res = new Reservation
            {
                Id = _nextReservationId++,
                RoomId = roomId,
                CustomerId = customerId,
                CheckIn = checkIn,
                CheckOut = checkOut,
                Total = total
            };
            _reservations.Add(res);
            return res;
        }

        public bool CancelReservation(int reservationId)
        {
            var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
            if (reservation is null) return false;

            return _reservations.Remove(reservation);
        }

        public Reservation? UpdateReservation(
            int reservationId,
            DateOnly newCheckIn,
            DateOnly newCheckOut,
            IPricingStrategy pricingStrategy)
        {
            var existingRes = _reservations.FirstOrDefault(r => r.Id == reservationId);
            if (existingRes is null) return null;

            if (newCheckOut <= newCheckIn) return null;

            if (!IsRoomAvailable(existingRes.RoomId, newCheckIn, newCheckOut, existingRes.Id))
            {
                return null; 
            }

            var room = _rooms.First(r => r.Id == existingRes.RoomId);
            var newTotal = pricingStrategy.CalculateTotal(room, newCheckIn, newCheckOut);
            var updatedRes = new Reservation
            {
                Id = existingRes.Id,
                RoomId = existingRes.RoomId,
                CustomerId = existingRes.CustomerId,
                CheckIn = newCheckIn,
                CheckOut = newCheckOut,
                Total = newTotal 
            };

            _reservations.Remove(existingRes);
            _reservations.Add(updatedRes);

            return updatedRes;
        }

        public IEnumerable<Reservation> GetAllReservations() => _reservations;
    }
}
