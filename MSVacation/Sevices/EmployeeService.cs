using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBot.MSVacation.Models;

namespace CoreBot.MSVacation.Sevices
{
    public class EmployeeService
    {
        public static EmployeeService Instance { get; } = new EmployeeService();

        public Employee GetById(Guid employeeId)
        {
            return new Employee
            {
                Id = employeeId,
                FirstName = "John",
                LastName = "Doe",
                Manager = null
            };
        }

        public IReadOnlyCollection<Employee> GetDirectReports(Guid employeeId)
        {
            return GetDirectReports(GetById(employeeId));
        }

        public IReadOnlyCollection<Employee> GetDirectReports(Employee employee)
        {
            return new List<string> { "Alice", "Bob", "C" }
                .Select(name => new Employee
                {
                    Id = Guid.NewGuid(),
                    FirstName = name,
                    LastName = name,
                    Manager = employee
                })
                .ToList()
                .AsReadOnly();
        }
    }
}
