using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boleteria.Source
{
    // === Interfaces ===
    public interface IClock { DateTime Now { get; } }

    public interface INotifier
    {
        void Notify(Customer customer, string message);
    }

    public interface IPricingStrategy
    {
        decimal CalculatePrice(Ticket ticket, Customer customer, DateTime date);
    }

    // === Entidades y abstracciones ===
    public record Customer(string Name, bool IsMember);

    public class Show
    {
        public string Title { get; }
        public int Capacity { get; }
        public int Reserved { get; private set; }
        public int SeatsLeft => Capacity - Reserved;

        public Show(string title, int capacity)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title requerido");
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            Title = title;
            Capacity = capacity;
            Reserved = 0;
        }

        public void ReserveOne()
        {
            if (SeatsLeft <= 0) throw new InvalidOperationException("No hay cupos");
            Reserved++;
        }
    }

    public abstract class Ticket
    {
        public string Id { get; }
        public decimal BasePrice { get; }
        public abstract string Kind { get; }
        protected Ticket(string id, decimal basePrice)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id requerido");
            if (basePrice <= 0) throw new ArgumentOutOfRangeException(nameof(basePrice));
            Id = id;
            BasePrice = basePrice;
        }

        // Fee sobre el subtotal calculado por la estrategia
        public abstract decimal ServiceFee(decimal subtotal);
    }

    public sealed class StandardTicket : Ticket
    {
        public StandardTicket(string id, decimal basePrice) : base(id, basePrice) { }
        public override string Kind => "Standard";
        public override decimal ServiceFee(decimal subtotal) => Math.Round(subtotal * 0.05m, 2);
    }

    public sealed class VipTicket : Ticket
    {
        public VipTicket(string id, decimal basePrice) : base(id, basePrice) { }
        public override string Kind => "VIP";
        public override decimal ServiceFee(decimal subtotal) => Math.Round(subtotal * 0.20m, 2);
    }

    // === Servicios ===
    public class WeekendAndMemberPricingStrategy : IPricingStrategy
    {
        private readonly IClock _clock;
        public WeekendAndMemberPricingStrategy(IClock clock) => _clock = clock;

        public decimal CalculatePrice(Ticket ticket, Customer customer, DateTime date)
        {
            // Regla:
            // 1) Subtotal parte de BasePrice
            // 2) Fin de semana (+10%)
            // 3) Miembro (-15%) después del recargo
            var subtotal = ticket.BasePrice;
            var day = date.DayOfWeek;
            var isWeekend = day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;

            if (isWeekend) subtotal *= 1.10m;
            if (customer.IsMember) subtotal *= 0.85m;

            return Math.Round(subtotal, 2);
        }
    }

    public class BookingService
    {
        private readonly IClock _clock;
        private readonly IPricingStrategy _pricing;
        private readonly INotifier _notifier;

        public BookingService(IClock clock, IPricingStrategy pricing, INotifier notifier)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _pricing = pricing ?? throw new ArgumentNullException(nameof(pricing));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        }

        public decimal Book(Show show, Ticket ticket, Customer customer)
        {
            if (show is null) throw new ArgumentNullException(nameof(show));
            if (ticket is null) throw new ArgumentNullException(nameof(ticket));

            if (show.SeatsLeft <= 0)
                throw new InvalidOperationException("No hay cupos");

            // calcular subtotal por estrategia
            var subtotal = _pricing.CalculatePrice(ticket, customer, _clock.Now);
            // aplicar fee del ticket
            var fee = ticket.ServiceFee(subtotal);
            var total = Math.Round(subtotal + fee, 2);

            // reservar
            show.ReserveOne();

            // notificar
            var message = $"CONFIRMADO: 1 x {ticket.Kind} para {show.Title} por {total:C}";
            _notifier.Notify(customer, message);

            return total;
        }
    }
}
