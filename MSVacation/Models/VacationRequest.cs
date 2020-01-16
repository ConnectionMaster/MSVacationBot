using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.MSVacation.Models
{
    public class VacationRequest
    {
        public Guid RequestId { get; set; }

        public Employee Employee { get; set; }

        public VacationType Type { get; set; }

        public VacationRequestStatus Status { get; set; }

        public DateTime StartData { get; set; }

        public DateTime EndData { get; set; }

        public TimeZoneInfo TimeZone { get; set; }

        public VacationRequest Clone =>
            (VacationRequest)MemberwiseClone();
    }
}
