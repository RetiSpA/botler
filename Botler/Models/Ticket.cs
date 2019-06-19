namespace Botler.Models
{
    using System;

    public class Ticket
    {
        public Ticket()
        {
            Priorita = 0;
            Oggetto = "Richiesta Supporto";
            Chiuso = false;
            ID = 0;
            Descrizione = string.Empty;
        }

        public short ID { get; set; }

        public short Priorita { get; set; }

        public string Oggetto { get; set; }

        public string Descrizione { get; set; }

        public short ID_TipoTicket { get; set; }

        public string Email_Utente { get; set; }

        public bool Chiuso { get; set; }

        public override string ToString()
        {
            string prioritàString = string.Empty;

            if (Priorita == 0)
            {
                prioritàString = "normale";
            }
            
            if (Priorita == 1)
            {
                prioritàString = "alta";
            }

            if (Priorita == 2)
            {
                prioritàString = "urgente";
            }
            return "\nCreato Ticekt: " + "\n Priorità: " + prioritàString + "\n Descrizione : " + Descrizione + " Utente " + Email_Utente;
        }

    }
}