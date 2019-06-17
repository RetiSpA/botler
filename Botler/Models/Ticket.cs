namespace Botler.Models
{
    using System;

    public partial class Ticket
    {
        public Ticket()
        {
            Priorita = 0;
            Oggetto = "Richiesta Supporto";
            Chiuso = false;
        }
        public short ID { get; set; }

        public short Priorita { get; set; }

        public string Oggetto { get; set; }

        public string Descrizione { get; set; }

        public short ID_TipoTicket { get; set; }

        public string Email_Utente { get; set; }

        public bool Chiuso { get; set; }

        public DateTime DataApertura { get; set; }
    }
}