using System;
using System.Collections.Generic;
using System.Linq;
using CoreBot.MSVacation.Models;

namespace CoreBot.MSVacation.Services
{
    public class RequestService
    {
        private readonly EmployeeService _employeeService;

        public RequestService(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public Guid CreateRequest(Guid employeeId, VacationType vacationType, DateTime startDate, DateTime endDate, TimeZoneInfo timeZoneInfo = null)
        {
            var request = new VacationRequest
            {
                RequestId = Guid.NewGuid(),
                Employee = _employeeService.GetById(employeeId),
                StartData = startDate,
                EndData = endDate,
                TimeZone = timeZoneInfo ?? TimeZoneInfo.Local,
                Status = VacationRequestStatus.PendingApproval,
                Type = vacationType
            };

            return request.RequestId;
        }

        public IReadOnlyCollection<VacationRequest> GetEmployeeRequests(Guid employeeId)
        {
            var results = new List<VacationRequest>();

            var today = DateTime.Today;

            for (int i = 1; i < 5; i++)
            {
                var startDate = today.Subtract(TimeSpan.FromDays(i * 5));
                var endDate = startDate.AddDays(3);
                var request = new VacationRequest
                {
                    RequestId = Guid.NewGuid(),
                    Employee = _employeeService.GetById(employeeId),
                    StartData = startDate,
                    EndData = endDate,
                    TimeZone = TimeZoneInfo.Local,
                    Status = VacationRequestStatus.Approved,
                    Type = VacationType.PaidHoliday
                };
                results.Add(request);
            }

            for (int i = 1; i < 3; i++)
            {
                var startDate = today.Add(TimeSpan.FromDays(i * 5));
                var endDate = startDate.AddDays(2);
                var request = new VacationRequest
                {
                    RequestId = Guid.NewGuid(),
                    Employee = _employeeService.GetById(employeeId),
                    StartData = startDate,
                    EndData = endDate,
                    TimeZone = TimeZoneInfo.Local,
                    Status = VacationRequestStatus.PendingApproval,
                    Type = VacationType.PaidHoliday
                };
                results.Add(request);
            }

            return results.AsReadOnly();
        }

        public IReadOnlyCollection<VacationRequest> GetEmployeePendingRequests(Guid employeeId)
        {
            return GetEmployeeRequests(employeeId)
                .Where(r => r.Status == VacationRequestStatus.PendingApproval)
                .ToList()
                .AsReadOnly();
        }

        public VacationRequest GetById(Guid requestId)
        {
            var request = new VacationRequest
            {
                RequestId = requestId,
                Employee = _employeeService.GetById(Guid.NewGuid()),
                StartData = DateTime.Today.Subtract(TimeSpan.FromDays(20)),
                EndData = DateTime.Today.Subtract(TimeSpan.FromDays(15)),
                TimeZone = TimeZoneInfo.Local,
                Status = VacationRequestStatus.PendingApproval,
                Type = VacationType.PaidHoliday
            };

            return request;
        }

        public VacationRequest ApproveRequest(Guid requestId)
        {
            return ApproveRequest(GetById(requestId));
        }

        public VacationRequest ApproveRequest(VacationRequest request)
        {
            request = request.Clone;
            request.Status = VacationRequestStatus.Approved;
            return request;
        }

        public VacationRequest UpdateRequest(VacationRequest request, VacationType? vacationType = null, DateTime? startDate = null, DateTime? endDate = null, TimeZoneInfo timeZoneInfo = null)
        {
            request = request.Clone;
            request.Type = vacationType ?? request.Type;
            request.StartData = startDate ?? request.StartData;
            request.EndData = endDate ?? request.EndData;
            request.TimeZone = timeZoneInfo ?? request.TimeZone;
            return request;
        }

        public IReadOnlyCollection<VacationRequest> GetTeamVacations(Guid managerId)
        {
            return GetTeamVacations(_employeeService.GetById(managerId));
        }

        public IReadOnlyCollection<VacationRequest> GetTeamVacations(Employee employee)
        {
            var results = new List<VacationRequest>();
            
            foreach (var e in _employeeService.GetDirectReports(employee))
            {
                results.AddRange(GetEmployeeRequests(e.Id));
            }

            return results.AsReadOnly();
        }
    }
}
