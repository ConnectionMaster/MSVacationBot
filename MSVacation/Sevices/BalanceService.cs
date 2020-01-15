using System;
using CoreBot.MSVacation.Models;

namespace CoreBot.MSVacation.Sevices
{
    public class BalanceService
    {
        private readonly EmployeeService _employeeService;

        public static BalanceService Instance { get; } =
            new BalanceService(EmployeeService.Instance);

        public BalanceService(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public BalanceStatus GetStatus(Employee employee)
        {
            return GetStatus(employee.Id);
        }

        public BalanceStatus GetStatus(Guid employeeId)
        {
            return new BalanceStatus
            {
                CompletedApproved = 15,
                FutureApproved = 6,
                RemainingDays = 9
            };
        }
    }
}
