using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.Models
{
    public class Reservation
    {
        public int Id { get; init; }
        public int RoomId { get; init; }
        public int CustomerId { get; init; }
        public DateOnly CheckIn { get; init; }
        public DateOnly CheckOut { get; init; }
        public decimal Total { get; init; }

        public override string ToString()
            => $"Res#{Id} - Room:{RoomId} Customer:{CustomerId} {CheckIn:yyyy-MM-dd}→{CheckOut:yyyy-MM-dd} Total ${Total:0.00}";
    }
}
