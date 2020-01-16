using Microsoft.Bot.Builder;

namespace CoreBot.MSVacation.Services
{
    public class MSVacationService : BaseService
    {
        public MSVacationService(
            IStorage storage,
            EmployeeService employeeService,
            BalanceService balanceService,
            PublicHolidaysService publicHolidaysService,
            RequestService requestService)
            : base(storage)
        {
            Employee = employeeService;
            Balance = balanceService;
            PublicHolidays = publicHolidaysService;
            Requests = requestService;
        }

        public EmployeeService Employee { get; }

        public BalanceService Balance { get; }

        public PublicHolidaysService PublicHolidays { get; }

        public RequestService Requests { get; }
    }
}
