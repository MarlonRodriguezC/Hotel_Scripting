using HotelManagement.Models;
using System;

namespace HotelManagement.Services
{
    public static class EmployeeFactory 
    {
        private static int _nextEmployeeId = 100; 

        
        public static Employee CreateEmployee(string fullName, EmployeeRole role, string email, string assignedArea)
        {
            

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
            {
               
                throw new ArgumentException("El nombre y el correo del empleado son obligatorios.");
            }

            return new Employee
            {
                Id = _nextEmployeeId++,
                FullName = fullName,
                Email = email,
                Role = role,
                Area = assignedArea
            };
        }
    }
}