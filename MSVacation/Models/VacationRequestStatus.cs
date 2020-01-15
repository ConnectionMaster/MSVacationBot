using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.MSVacation.Models
{
    public enum VacationRequestStatus
    {
        Approved = 0,
        Cancellation,
        PendingApproval,
        PendingCancellation,
        Refused,
    }
}
