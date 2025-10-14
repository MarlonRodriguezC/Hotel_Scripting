using HotelManagement.Models;
using System.Collections.Generic;
using System.Linq;

namespace HotelManagement.Services
{
   
    public sealed class EmployeeServiceSingleton
    {
        private static EmployeeServiceSingleton? _instance; 
        private static readonly object Lock = new();     

        private readonly List<Employee> _employees;

       
        private EmployeeServiceSingleton(List<Employee> initialEmployees)
        {
            _employees = initialEmployees;
        }

       
        public static EmployeeServiceSingleton GetInstance(List<Employee>? initialEmployees = null)
        {
           
            lock (Lock)
            {
                if (_instance == null)
                {
                   
                    _instance = new EmployeeServiceSingleton(initialEmployees ?? new List<Employee>());
                }
                return _instance;
            }
        }

       

        public Employee? AddNewEmployee(string fullName, EmployeeRole role, string email, string assignedArea)
        {
            try
            {
                var newEmployee = EmployeeFactory.CreateEmployee(fullName, role, email, assignedArea);
                _employees.Add(newEmployee);
                return newEmployee;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public Employee? Authenticate(int employeeId)
        {
            return _employees.FirstOrDefault(e => e.Id == employeeId);
        }

        public bool HasRole(int employeeId, EmployeeRole requiredRole)
        {
            var employee = Authenticate(employeeId);
            return employee != null && employee.Role == requiredRole;
        }

        public IEnumerable<Employee> GetAllEmployees() => _employees;
    }
}
