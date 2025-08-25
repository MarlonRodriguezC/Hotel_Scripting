using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.Models
{
    public enum RoomStatus { Available, Occupied, Cleaning }

    public class Room
    {
        public int Id { get; init; }
        public string Number { get; init; } = "";
        public int Capacity { get; init; }
        public decimal Price { get; init; }
        public RoomStatus Status { get; set; } = RoomStatus.Available;

        public override string ToString()
            => $"Room {Number} (cap {Capacity}) - {Status} - ${Price:0.00}/night";
    }
}
