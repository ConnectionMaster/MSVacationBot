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
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 1, 1), Name = "New Year Holiday"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 1, 7), Name = "Christmas Day"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 1, 25), Name = "25 January Revolution Day"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 4, 19), Name = "Easter Sunday"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 4, 20), Name = "Sham el Nessim"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 4, 25), Name = "Sinai Liberation Day"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 5, 1), Name = "Labour Day"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 5, 24), Name = "Eid al-Fitr"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 5, 25), Name = "Eid al-Fitr Holiday"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 6, 30), Name = "30 June Revolution Day"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 7, 23), Name = "Revolution Day"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 7, 30), Name = "Arafat Day"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 7, 31), Name = "Eid al-Adha"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 8, 1), Name = "Eid al-Adha Holiday"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 8, 20), Name = "Islamic New Year"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 10, 6), Name = "Armed Forces Day"},
                new PublicHoliday {Date=new DateTime(DateTime.Now.Year, 10, 29), Name = "Prophet Muhammad's Birthday"},
            }.AsReadOnly();
        }
    }
}
