using HotelManagement.Models;
using HotelManagement.Services;
using HotelManagement.Services; // Necesario para las Estrategias de Precios
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace HotelManagement.Tests
{
    // [TestFixture] es el equivalente de la clase de prueba
    [TestFixture]
    public class ReservationServiceTests
    {
        private ReservationService _service;
        private List<Room> _rooms;
        private List<Customer> _customers;

        // [SetUp] es el equivalente al constructor en xUnit, se ejecuta antes de cada [Test] o [TestCase]
        [SetUp]
        public void Setup()
        {
            // 1. Arrange (Configuración de los datos iniciales)
            _rooms = new List<Room>
            {
                new() { Id = 1, Number = "101", Capacity = 2, Price = 100_000m }, // Precio simple para las pruebas
            };
            _customers = new List<Customer>
            {
                new() { Id = 1, FullName = "Test Customer", Email = "test@example.com" }
            };

            // Inicialización del servicio antes de cada prueba
            _service = new ReservationService(_rooms, _customers);
        }

        // =================================================================
        // PRUEBA 1: VALIDACIÓN DE DISPONIBILIDAD (REQUISITO 4.1)
        // =================================================================

        [Test] // [Test] marca un método sin parámetros
        public void CreateReservation_ShouldNotAllowOverlap()
        {
            // Arrange (Ya realizado en el Setup, pero definimos datos específicos de la prueba)
            var standardPricing = new StandardPricingStrategy();
            var checkIn1 = new DateOnly(2025, 10, 10);
            var checkOut1 = new DateOnly(2025, 10, 15); // 5 noches

            // Acción 1: Crear la primera reserva OK
            _service.CreateReservation(roomId: 1, customerId: 1, checkIn1, checkOut1, standardPricing);

            // Act (Ejecución)
            var checkInOverlap = new DateOnly(2025, 10, 14); // Solapa con la noche del 14
            var checkOutOverlap = new DateOnly(2025, 10, 16);

            // Acción 2: Intentar reservar solapando
            var resFail = _service.CreateReservation(roomId: 1, customerId: 1, checkInOverlap, checkOutOverlap, standardPricing);

            // Assert (Aserción)
            // Assert.IsNull es la aserción de NUnit para verificar que el resultado es null.
            Assert.IsNull(resFail, "La reserva solapada NO debió crearse.");
        }

        [Test]
        public void CreateReservation_ShouldNotAllowCheckInInThePast()
        {
            // Arrange
            var standardPricing = new StandardPricingStrategy();
            var pastIn = DateOnly.FromDateTime(DateTime.Today.AddDays(-5));
            var pastOut = DateOnly.FromDateTime(DateTime.Today.AddDays(-2));

            // Act
            var resFail = _service.CreateReservation(roomId: 1, customerId: 1, pastIn, pastOut, standardPricing);

            // Assert
            Assert.IsNull(resFail, "No se deben permitir reservas con Check-In en el pasado.");
        }

        // =================================================================
        // NUEVA PRUEBA: CANCELACIÓN (DELETE)
        // =================================================================

        [Test]
        public void CancelReservation_ShouldRemoveReservationFromList()
        {
            // Arrange
            var pricing = new StandardPricingStrategy();
            var checkIn = new DateOnly(2025, 12, 1);
            var checkOut = new DateOnly(2025, 12, 5);

            var res = _service.CreateReservation(1, 1, checkIn, checkOut, pricing);
            Assert.IsNotNull(res, "La reserva inicial falló.");

            var initialCount = _service.GetAllReservations().Count();

            // Act
            bool success = _service.CancelReservation(res.Id);
            var finalCount = _service.GetAllReservations().Count();

            // Assert
            Assert.IsTrue(success, "La cancelación debe ser exitosa.");
            Assert.AreEqual(initialCount - 1, finalCount, "La reserva debe ser eliminada de la lista.");
            Assert.IsFalse(_service.GetAllReservations().Any(r => r.Id == res.Id), "La reserva cancelada no debe existir.");
        }

        // =================================================================
        // NUEVA PRUEBA: MODIFICACIÓN (UPDATE)
        // =================================================================

        [Test]
        public void UpdateReservation_ShouldChangeDatesAndRecalculateTotal()
        {
            // Arrange
            var initialPricing = new StandardPricingStrategy();
            var newPricing = new HighSeasonPricingStrategy();
            var initialIn = new DateOnly(2025, 1, 1);
            var initialOut = new DateOnly(2025, 1, 4); // 3 noches @ 100k = 300,000

            var newIn = new DateOnly(2025, 1, 10);
            var newOut = new DateOnly(2025, 1, 12); // 2 noches @ 100k * 1.2 = 240,000

            var res = _service.CreateReservation(1, 1, initialIn, initialOut, initialPricing);
            Assert.IsNotNull(res);

            // Act
            var updatedRes = _service.UpdateReservation(res.Id, newIn, newOut, newPricing);

            // Assert
            Assert.IsNotNull(updatedRes, "La modificación debe ser exitosa.");
            Assert.AreEqual(newIn, updatedRes.CheckIn, "La fecha de CheckIn debe ser actualizada.");

            // 2 noches * 100,000 * 1.20 = 240,000
            Assert.AreEqual(240_000m, updatedRes.Total, "El Total debe ser recalculado con la nueva estrategia.");

            // Verificar que solo existe la versión modificada
            Assert.AreEqual(1, _service.GetAllReservations().Count(), "Solo debe haber una reserva en el servicio.");
        }


        [Test]
        public void UpdateReservation_ShouldFailIfNewDatesOverlap()
        {
            // Arrange
            var pricing = new StandardPricingStrategy();

            // Reserva 1: Bloquea del 10 al 15 (Room 1)
            _service.CreateReservation(1, 1, new DateOnly(2025, 10, 10), new DateOnly(2025, 10, 15), pricing);

            // Reserva 2 (a modificar): Del 1 al 5 (Room 1)
            var resToUpdate = _service.CreateReservation(1, 1, new DateOnly(2025, 10, 1), new DateOnly(2025, 10, 5), pricing);

            // Act
            // Intentar mover la Reserva 2 al rango del 12 al 16 (choca con la Reserva 1)
            var updatedRes = _service.UpdateReservation(
                resToUpdate.Id,
                new DateOnly(2025, 10, 12),
                new DateOnly(2025, 10, 16),
                pricing);

            // Assert
            Assert.IsNull(updatedRes, "La modificación debe fallar por solape con otra reserva.");

            // Verificar que la reserva original (1 al 5) sigue existiendo
            var originalRes = _service.GetAllReservations().FirstOrDefault(r => r.Id == resToUpdate.Id);
            Assert.IsNotNull(originalRes, "La reserva original no debe ser eliminada si la actualización falla.");
            Assert.AreEqual(new DateOnly(2025, 10, 1), originalRes.CheckIn);
        }




        // =================================================================
        // PRUEBA 2: CÁLCULO DE FACTURACIÓN (REQUISITO 4.2)
        // =================================================================

        // [TestCase] permite pasar datos directamente, es el equivalente a [Theory] + [InlineData]
        // Parámetros: Tipo de Estrategia, Total Esperado (3 Noches * Precio Base 100k)
        [TestCase(typeof(StandardPricingStrategy), 300_000, TestName = "StandardPrice")]
        [TestCase(typeof(HighSeasonPricingStrategy), 360_000, TestName = "HighSeasonPrice")] // 3 * 100k * 1.2 = 360k
        [TestCase(typeof(CorporateDiscountStrategy), 270_000, TestName = "DiscountPrice")] // 3 * 100k * 0.9 = 270k
        public void CreateReservation_ShouldCalculateTotalCorrectly_BasedOnStrategy(Type strategyType, decimal expectedTotal)
        {
            // Arrange
            var checkIn = new DateOnly(2025, 11, 1);
            var checkOut = new DateOnly(2025, 11, 4); // Periodo de 3 noches

            // Usamos reflection para crear la instancia del Strategy
            var pricingStrategy = (IPricingStrategy)Activator.CreateInstance(strategyType)!;

            // Act
            var res = _service.CreateReservation(roomId: 1, customerId: 1, checkIn, checkOut, pricingStrategy);

            // Assert
            Assert.IsNotNull(res, "La reserva no debió fallar.");
            // Assert.AreEqual es la aserción de NUnit para comparar valores
            Assert.AreEqual(expectedTotal, res.Total, $"El total calculado por {strategyType.Name} es incorrecto.");
        }
    }
}