using System;
using System.Collections.Generic;
using CoreBot.MSVacation.Models;

namespace CoreBot.MSVacation.Services
{
    public class PublicHolidaysService
    {
        public IReadOnlyCollection<PublicHoliday> GetPublicHolidays()
        {
            return new List<PublicHoliday>
            {
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 1, 1), Name = "New Year"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 12, 25), Name = "Chirstmas"},
            }.AsReadOnly();
        }
    }
}
