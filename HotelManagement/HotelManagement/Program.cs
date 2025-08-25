using HotelManagement.Models;
using HotelManagement.Services;


var rooms = new List<Room>
{
    new() { Id = 1, Number = "101", Capacity = 2, Price = 80000_000m},
    new() { Id = 2, Number = "102", Capacity = 3, Price = 100000_000m }
};

var customers = new List<Customer>
{
    new() { Id = 1, FullName = "Marlon", Email = "marlon@gmail.com", PersonalID = "123456789", EmergencyContact = "300123456"},
    new() { Id = 2, FullName = "Santiago", Email = "pussydestroyer@gmail.com", PersonalID = "987654321", EmergencyContact = "300987654"}
};

var service = new ReservationService(rooms, customers);

var checkin = new DateOnly(2025, 8, 24);
var checkout = new DateOnly(2025, 8, 29);

var response = service.CreaReservation(1, 1, checkin, checkout);
Console.WriteLine(response);

foreach (var r in service.GetAllReservations())
{
    Console.WriteLine(r);
}