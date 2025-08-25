using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.Models
{
    public class Customer
    {
        public int Id { get; init; }
        public string FullName { get; init; } = "";
        public string PersonalID { get; init; }
        public string Email { get; init; } = "";
        public string EmergencyContact {  get; init; }
        public override string ToString() => $"{FullName} <{Email}> C.C: {PersonalID}";
    }
}
