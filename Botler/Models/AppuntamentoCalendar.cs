using System;
using System.Collections.Generic;

namespace Botler.Models
{
    public class AppuntamentoCalendar
    {
        public AppuntamentoCalendar()
        {
            Location = "Sede di Reti S.p.A ";
            Partecipanti = new List<string>();
            Inizio = DateTime.Now.TimeOfDay;
            Fine = DateTime.Now.AddHours(2).TimeOfDay;
            Descrizione = "Appuntamento Calendar";

        }

        public DateTime Date { get; set; }

        public TimeSpan Inizio { get; set; }

        public TimeSpan Fine { get; set; }

        public string Location { get; set; } = string.Empty;

        public string Titolo { get; set; }

        public string Descrizione { get; set; }

        public ICollection<string> Partecipanti { get; set; }
    }
}