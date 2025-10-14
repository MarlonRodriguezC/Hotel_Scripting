using HotelManagement.Models;
using HotelManagement.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace HotelManagement.Tests
{
    [TestFixture]
    public class EmployeeServiceTests
    {
        private List<Employee> GetInitialEmployees()
        {
            // Usamos un Factory que garantice la creación de IDs únicos
            return new List<Employee>
            {
                EmployeeFactory.CreateEmployee("Gerente Alpha", EmployeeRole.Manager, "gm@test.com", "Todas"),
                EmployeeFactory.CreateEmployee("Recepcionista Beta", EmployeeRole.Receptionist, "rb@test.com", "Front")
            };
        }

        // PRUEBA: AUTENTICACIÓN Y ROLES
        [Test]
        public void Authenticate_ShouldReturnEmployee_WhenIdExists()
        {
            var initialEmployees = GetInitialEmployees();
            var service = EmployeeServiceSingleton.GetInstance(initialEmployees);
            var receptionistId = initialEmployees.First(e => e.Role == EmployeeRole.Receptionist).Id;

            var authenticatedUser = service.Authenticate(receptionistId);

            Assert.IsNotNull(authenticatedUser);
            Assert.AreEqual(EmployeeRole.Receptionist, authenticatedUser.Role);
        }

        [Test]
        public void HasRole_ShouldBeTrue_WhenEmployeeHasRequiredRole()
        {
            var initialEmployees = GetInitialEmployees();
            var service = EmployeeServiceSingleton.GetInstance(initialEmployees);
            var managerId = initialEmployees.First(e => e.Role == EmployeeRole.Manager).Id;

            bool result = service.HasRole(managerId, EmployeeRole.Manager);

            Assert.IsTrue(result);
        }

        // PRUEBA: PATRÓN SINGLETON (Instancia única)
        [Test, Order(1)]
        public void Singleton_ShouldReturnSameInstance()
        {
            var firstInstance = EmployeeServiceSingleton.GetInstance(GetInitialEmployees());
            firstInstance.AddNewEmployee("Temporal", EmployeeRole.Receptionist, "t@t.com", "Lobby");

            var secondInstance = EmployeeServiceSingleton.GetInstance(null);

            Assert.AreSame(firstInstance, secondInstance, "Las dos instancias deben ser el mismo objeto Singleton.");
            Assert.IsTrue(secondInstance.GetAllEmployees().Any(e => e.FullName == "Temporal"), "El estado debe persistir.");
        }
    }
}