using HotelManagement.Models;
using HotelManagement.Services;
using System;
using System.Linq;

//===================================================================
// SETUP INICIAL
//===================================================================

// Rooms
var rooms = new List<Room>
{
    new() { Id = 1, Number = "101", Capacity = 2, Price = 150_000m },
    new() { Id = 2, Number = "102", Capacity = 3, Price = 220_000m }
};

// Customers
var customers = new List<Customer>
{
    new() { Id = 1, FullName = "Juan Perez", Email = "juan@example.com", PersonalID = "123456" },
    new() { Id = 2, FullName = "Ana Gómez", Email = "ana@example.com", PersonalID = "654321" }
};

// Employees
var employees = new List<Employee>
{
    new() { Id = 10, FullName = "Marta Recepción", Role = EmployeeRole.Receptionist, Area = "Front Desk" },
    new() { Id = 20, FullName = "Carlos Limpieza", Role = EmployeeRole.Housekeeping, Area = "Piso 1" },
    new() { Id = 30, FullName = "Gerente General", Role = EmployeeRole.Manager, Area = "Todas" }
};

// Inicialización de Servicios
var reservationService = new ReservationService(rooms, customers);
var taskService = new TaskManagementService();

// Helper local functions
static void Pause() { Console.WriteLine("\nPresiona una tecla para continuar..."); Console.ReadKey(true); }
EmployeeRole? ReadEmployeeRole()
{
    Console.WriteLine("Seleccione rol:");
    Console.WriteLine("1) Receptionist");
    Console.WriteLine("2) Housekeeping");
    Console.WriteLine("3) Manager");
    Console.Write("Opción: ");
    var r = Console.ReadLine();
    return r switch
    {
        "1" => EmployeeRole.Receptionist,
        "2" => EmployeeRole.Housekeeping,
        "3" => EmployeeRole.Manager,
        _ => null
    };
}
DateOnly? ReadDate(string prompt)
{
    Console.Write($"{prompt} (yyyy-MM-dd): ");
    if (DateOnly.TryParse(Console.ReadLine(), out var d)) return d;
    Console.WriteLine("Fecha inválida.");
    return null;
}
void ListRooms()
{
    Console.WriteLine("\n--- Habitaciones ---");
    foreach (var r in rooms) Console.WriteLine($"Id: {r.Id}  Nº: {r.Number}  Cap: {r.Capacity}  Precio: {r.Price:N0}");
}
void ListCustomers()
{
    Console.WriteLine("\n--- Clientes ---");
    foreach (var c in customers) Console.WriteLine($"Id: {c.Id}  {c.FullName}  Email: {c.Email}  CI: {c.PersonalID}");
}
void ListEmployees()
{
    Console.WriteLine("\n--- Empleados ---");
    foreach (var e in employees) Console.WriteLine($"Id: {e.Id}  {e.FullName}  Rol: {e.Role}  Area: {e.Area}");
}

// Menú principal
while (true)
{
    Console.Clear();
    Console.WriteLine("=============================================");
    Console.WriteLine("== HOTEL MANAGEMENT - MENÚ INTERACTIVO    ==");
    Console.WriteLine("=============================================");
    Console.WriteLine("1) Listar habitaciones");
    Console.WriteLine("2) Listar clientes");
    Console.WriteLine("3) Listar empleados");
    Console.WriteLine("4) Crear reserva");
    Console.WriteLine("5) Ejecutar demos rápidos (reservas + tareas)");
    Console.WriteLine("6) Listar tareas pendientes");
    Console.WriteLine("7) Añadir tarea");
    Console.WriteLine("8) Obtener siguiente tarea para rol");
    Console.WriteLine("9) Completar siguiente tarea para rol");
    Console.WriteLine("0) Salir");
    Console.Write("Opción: ");

    var opt = Console.ReadLine();
    Console.WriteLine();

    if (opt == "0") break;

    switch (opt)
    {
        case "1":
            ListRooms();
            Pause();
            break;

        case "2":
            ListCustomers();
            Pause();
            break;

        case "3":
            ListEmployees();
            Pause();
            break;

        case "4":
            // Crear reserva interactiva
            try
            {
                ListRooms();
                Console.Write("\nId habitación: ");
                if (!int.TryParse(Console.ReadLine(), out var roomId)) { Console.WriteLine("Id inválido."); Pause(); break; }

                ListCustomers();
                Console.Write("\nId cliente: ");
                if (!int.TryParse(Console.ReadLine(), out var customerId)) { Console.WriteLine("Id inválido."); Pause(); break; }

                var inDate = ReadDate("Fecha entrada");
                if (inDate is null) { Pause(); break; }
                var outDate = ReadDate("Fecha salida");
                if (outDate is null) { Pause(); break; }
                if (outDate <= inDate) { Console.WriteLine("La fecha de salida debe ser posterior a la de entrada."); Pause(); break; }

                Console.WriteLine("Estrategia de precio:");
                Console.WriteLine("1) Standard");
                Console.WriteLine("2) High Season");
                Console.Write("Opción: ");
                var stratOpt = Console.ReadLine();
                IPricingStrategy strategy = stratOpt switch
                {
                    "1" => new StandardPricingStrategy(),
                    "2" => new HighSeasonPricingStrategy(),
                    _ => new StandardPricingStrategy()
                };

                var res = reservationService.CreateReservation(roomId, customerId, inDate.Value, outDate.Value, strategy);
                if (res == null) Console.WriteLine("No se pudo crear la reserva (posible solape o datos inválidos).");
                else Console.WriteLine($"Reserva creada: {res}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creando reserva: {ex.Message}");
            }
            Pause();
            break;

        case "5":
            // Demo rápido (similar al existente)
            Console.WriteLine("Ejecutando demo de reservas y tareas...");

            var demoIn = new DateOnly(2025, 11, 1);
            var demoOut = new DateOnly(2025, 11, 4);
            var std = new StandardPricingStrategy();
            var high = new HighSeasonPricingStrategy();

            var d1 = reservationService.CreateReservation(1, 1, demoIn, demoOut, std);
            Console.WriteLine($"\n[Standard] Reserva 101 ({demoIn}→{demoOut}): {(d1 is null ? "Fallo" : d1.ToString())}");

            var d2 = reservationService.CreateReservation(2, 2, demoIn, demoOut, high);
            Console.WriteLine($"[High Season] Reserva 102 ({demoIn}→{demoOut}): {(d2 is null ? "Fallo" : d2.ToString())}");

            var solapeIn = new DateOnly(2025, 11, 3);
            var solapeOut = new DateOnly(2025, 11, 5);
            var d3 = reservationService.CreateReservation(1, 2, solapeIn, solapeOut, std);
            Console.WriteLine($"\n[Solape] Intentando solape en 101: {(d3 is null ? "Correcto: No se reservó." : "ERROR: Se reservó.")}");

            // Tareas demo
            taskService.AddTask("Limpieza profunda de salida", "Room 101", EmployeeRole.Housekeeping);
            taskService.AddTask("Chequear iluminación", "Lobby", EmployeeRole.Manager);
            taskService.AddTask("Atender llamada de emergencia", "Front Desk", EmployeeRole.Receptionist);

            Console.WriteLine($"\nTareas pendientes ({taskService.GetPendingTasks().Count()}):");
            foreach (var t in taskService.GetPendingTasks()) Console.WriteLine($"- {t}");
            Pause();
            break;

        case "6":
            var pending = taskService.GetPendingTasks().ToList();
            Console.WriteLine($"Tareas pendientes ({pending.Count}):");
            foreach (var t in pending) Console.WriteLine($"- {t}");
            Pause();
            break;

        case "7":
            Console.Write("Descripción de la tarea: ");
            var desc = Console.ReadLine() ?? "";
            Console.Write("Ubicación: ");
            var loc = Console.ReadLine() ?? "";
            var role = ReadEmployeeRole();
            if (role == null) Console.WriteLine("Rol inválido.");
            else
            {
                taskService.AddTask(desc, loc, role.Value);
                Console.WriteLine("Tarea añadida correctamente.");
            }
            Pause();
            break;

        case "8":
            var roleForGet = ReadEmployeeRole();
            if (roleForGet == null) { Console.WriteLine("Rol inválido."); Pause(); break; }
            var next = taskService.GetNextTask(roleForGet.Value);
            Console.WriteLine(next is null ? "No hay tareas para ese rol." : $"Siguiente tarea: {next}");
            Pause();
            break;

        case "9":
            var roleForComplete = ReadEmployeeRole();
            if (roleForComplete == null) { Console.WriteLine("Rol inválido."); Pause(); break; }
            var completed = taskService.CompleteNextTask(roleForComplete.Value);
            Console.WriteLine(completed is null ? "No se completó ninguna tarea (no había)." : $"Tarea completada: {completed}");
            Pause();
            break;

        default:
            Console.WriteLine("Opción inválida.");
            Pause();
            break;
    }
}

Console.WriteLine("Saliendo...");
