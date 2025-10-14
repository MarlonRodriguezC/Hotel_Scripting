using HotelManagement.Models;
using HotelManagement.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelManagement.Tests
{
    [TestFixture]
    public class ReservationServiceTests
    {
        private ReservationService _service;
        private List<Room> _rooms;
        private List<Customer> _customers;

        [SetUp]
        public void Setup()
        {
            // Reiniciar datos antes de cada prueba
            _rooms = new List<Room>
            {
                new() { Id = 1, Number = "101", Capacity = 2, Price = 100_000m },
                new() { Id = 2, Number = "102", Capacity = 3, Price = 200_000m }
            };
            _customers = new List<Customer>
            {
                new() { Id = 1, FullName = "Test Customer", Email = "test@example.com" }
            };
            _service = new ReservationService(_rooms, _customers);
        }

        // PRUEBA: DISPONIBILIDAD (CREATE/Overlap)
        [Test]
        public void CreateReservation_ShouldNotAllowOverlap()
        {
            var pricing = new StandardPricingStrategy();
            _service.CreateReservation(roomId: 1, customerId: 1, new DateOnly(2025, 10, 10), new DateOnly(2025, 10, 15), pricing);
            var resFail = _service.CreateReservation(roomId: 1, customerId: 1, new DateOnly(2025, 10, 14), new DateOnly(2025, 10, 16), pricing);

            Assert.IsNull(resFail, "La reserva solapada NO debió crearse.");
        }

        // PRUEBA: CÁLCULO DE FACTURACIÓN (Strategy)
        [TestCase(typeof(StandardPricingStrategy), 300_000, TestName = "StandardPrice")]
        [TestCase(typeof(HighSeasonPricingStrategy), 360_000, TestName = "HighSeasonPrice")]
        public void CreateReservation_ShouldCalculateTotalCorrectly_BasedOnStrategy(Type strategyType, decimal expectedTotal)
        {
            var checkIn = new DateOnly(2025, 11, 1);
            var checkOut = new DateOnly(2025, 11, 4); // 3 noches, Room 1 (100k)
            var pricingStrategy = (IPricingStrategy)Activator.CreateInstance(strategyType)!;

            var res = _service.CreateReservation(roomId: 1, customerId: 1, checkIn, checkOut, pricingStrategy);

            Assert.IsNotNull(res);
            Assert.AreEqual(expectedTotal, res.Total, "El total calculado es incorrecto.");
        }

        // PRUEBA: CANCELACIÓN (DELETE)
        [Test]
        public void CancelReservation_ShouldRemoveReservationFromList()
        {
            var pricing = new StandardPricingStrategy();
            var res = _service.CreateReservation(1, 1, new DateOnly(2025, 12, 1), new DateOnly(2025, 12, 5), pricing);
            var initialCount = _service.GetAllReservations().Count();

            bool success = _service.CancelReservation(res!.Id);

            Assert.IsTrue(success, "La cancelación debe ser exitosa.");
            Assert.AreEqual(initialCount - 1, _service.GetAllReservations().Count(), "La reserva debe ser eliminada.");
        }

        // PRUEBA: MODIFICACIÓN (UPDATE)
        [Test]
        public void UpdateReservation_ShouldChangeDatesAndRecalculateTotal()
        {
            var initialPricing = new StandardPricingStrategy();
            var newPricing = new HighSeasonPricingStrategy();
            var initialIn = new DateOnly(2025, 1, 1);
            var initialOut = new DateOnly(2025, 1, 4); // 3 noches @ 100k = 300,000

            var newIn = new DateOnly(2025, 1, 10);
            var newOut = new DateOnly(2025, 1, 12); // 2 noches @ 100k * 1.2 = 240,000

            var res = _service.CreateReservation(1, 1, initialIn, initialOut, initialPricing);
            var updatedRes = _service.UpdateReservation(res!.Id, newIn, newOut, newPricing);

            Assert.IsNotNull(updatedRes, "La modificación debe ser exitosa.");
            Assert.AreEqual(newIn, updatedRes.CheckIn);
            Assert.AreEqual(240_000m, updatedRes.Total, "El Total debe ser recalculado.");
        }

        [Test]
        public void UpdateReservation_ShouldFailIfNewDatesOverlap()
        {
            var pricing = new StandardPricingStrategy();
            _service.CreateReservation(1, 1, new DateOnly(2025, 10, 10), new DateOnly(2025, 10, 15), pricing);
            var resToUpdate = _service.CreateReservation(1, 1, new DateOnly(2025, 10, 1), new DateOnly(2025, 10, 5), pricing);

            var updatedRes = _service.UpdateReservation(resToUpdate!.Id, new DateOnly(2025, 10, 12), new DateOnly(2025, 10, 16), pricing);

            Assert.IsNull(updatedRes, "La modificación debe fallar por solape.");
            Assert.AreEqual(2, _service.GetAllReservations().Count(), "No debe haber duplicados después del fallo.");
        }
    }
}