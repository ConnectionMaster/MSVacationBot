using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBot.MSVacation.Models;
using Microsoft.Bot.Builder;

namespace CoreBot.MSVacation.Services
{
    public class EmployeeService : BaseService
    {
        public EmployeeService(IStorage storage)
            : base(storage)
        {
        }

        public Employee GetById(Guid employeeId)
        {
            var employee = new Employee
            {
                Id = employeeId,
                FirstName = "John",
                LastName = "Doe",
                Manager = null
            };
            Storage.WriteAsync(new Dictionary<string, object>
            {
                [employeeId.ToString()] = employee,
            }).Wait();
            var o = Storage.ReadAsync(new string[] { employeeId.ToString() }).Result as Employee;

            return employee;
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
