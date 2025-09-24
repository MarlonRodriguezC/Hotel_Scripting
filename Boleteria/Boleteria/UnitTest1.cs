using Boleteria.Source;

namespace Boleteria
{
    public class FakeClock : IClock
    {
        public DateTime Now { get; set; }
        public FakeClock(DateTime now) => Now = now;
    }

    public class SpyNotifier : INotifier
    {
        public int TimesCalled { get; private set; }
        public string? LastMessage { get; private set; }

        public void Notify(Customer customer, string message)
        {
            TimesCalled++;
            LastMessage = message;
        }
    }

    [TestFixture]
    public class BookingServiceTests
    {
        private static DateTime Weekday() => new DateTime(2025, 9, 17); // Miércoles
        private static DateTime Weekend() => new DateTime(2025, 9, 21); // Domingo

        [Test]
        public void ReservaExitosa_DisminuyeCupoYNotifica()
        {
            // Arrange
            var show = new Show("Concierto Ambrosía", capacity: 2);
            var clock = new FakeClock(Weekday());
            var notifier = new SpyNotifier();
            var pricing = new WeekendAndMemberPricingStrategy(clock);
            var sut = new BookingService(clock, pricing, notifier);

            var customer = new Customer("Valeria", IsMember: false);
            var ticket = new StandardTicket(id: "S1", basePrice: 100_000m);

            // Act
            var total = sut.Book(show, ticket, customer);

            // Assert
            Assert.That(show.SeatsLeft, Is.EqualTo(1));
            Assert.That(total, Is.EqualTo(100_000m /*subtotal*/ + 0.05m * 100_000m).Within(0.01m));
            Assert.That(notifier.TimesCalled, Is.EqualTo(1));
            Assert.That(notifier.LastMessage, Does.Contain("CONFIRMADO: 1 x Standard"));
            Assert.That(notifier.LastMessage, Does.Contain("Concierto Ambrosía"));
        }

        [Test]
        public void Sobreventa_LanzaInvalidOperation()
        {
            var show = new Show("Show Íntimo", capacity: 1);
            var clock = new FakeClock(Weekday());
            var notifier = new SpyNotifier();
            var pricing = new WeekendAndMemberPricingStrategy(clock);
            var sut = new BookingService(clock, pricing, notifier);

            var c = new Customer("Juan", IsMember: false);
            var t = new StandardTicket("S1", 50_000m);

            sut.Book(show, t, c); // ocupa el único cupo

            Assert.Throws<InvalidOperationException>(() => sut.Book(show, t, c));
        }

        [TestCase(true, 100_000, 1.10 * 0.85, TestName = "VIP FinDeSemana+Miembro: 10%+ y -15%")] // falla
        [TestCase(false, 100_000, 1.00 * 0.85, TestName = "VIP Semana+Miembro: 15%-")]
        [TestCase(true, 80_000, 1.10 * 1.00, TestName = "VIP FinDeSemana+NoMiembro: 10%+")]
        public void EstrategiaPrecio_VIP_AplicaReglas(bool weekend, decimal basePrice, double factor)
        {
            var date = weekend ? Weekend() : Weekday();
            var clock = new FakeClock(date);
            var notifier = new SpyNotifier();
            var pricing = new WeekendAndMemberPricingStrategy(clock);
            var sut = new BookingService(clock, pricing, notifier);

            var show = new Show("VIP Night", 5);
            var vip = new VipTicket("V1", basePrice);
            var customer = new Customer("Santiago", IsMember: factor <= 1.0 * 0.85 + 1e-9); // heurística simple

            var subtotalEsperado = basePrice * (decimal)factor;
            var feeEsperada = vip.ServiceFee(subtotalEsperado);
            var totalEsperado = Math.Round(subtotalEsperado + feeEsperada, 2);

            var total = sut.Book(show, vip, customer);

            Assert.That(total, Is.EqualTo(totalEsperado).Within(0.01m));
        }

        [Test]
        public void TarifasDeServicio_DiferencianStandardVsVip()
        {
            var clock = new FakeClock(Weekday());
            var notifier = new SpyNotifier();
            var pricing = new WeekendAndMemberPricingStrategy(clock);
            var sut = new BookingService(clock, pricing, notifier);

            var show = new Show("Comparativa", 10);
            var c = new Customer("Cliente", IsMember: false);

            var st = new StandardTicket("S1", 100_000m);
            var vip = new VipTicket("V1", 100_000m);

            var totalStandard = sut.Book(show, st, c);
            var totalVip = sut.Book(show, vip, c);

            // Con mismo subtotal, VIP debe ser más caro por fee (20% vs 5%)
            Assert.That(totalVip, Is.GreaterThan(totalStandard));
        }
    }
}