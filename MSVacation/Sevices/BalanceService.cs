using System;
using CoreBot.MSVacation.Models;
using Microsoft.Bot.Builder;

namespace CoreBot.MSVacation.Services
{
    public class BalanceService : BaseService
    {
        private readonly EmployeeService _employeeService;

        public BalanceService(IStorage storage, EmployeeService employeeService)
            : base(storage)
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
