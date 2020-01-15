using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.MSVacation.Models
{
    public class BalanceStatus
    {
        public int RemainingDays { get; set; }

        public int CompletedApproved { get; set; }

        public int FutureApproved { get; set; }
    }
}
