using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

using System.Threading.Tasks;
using Botler.Models;
using Newtonsoft.Json;

using static Botler.Dialogs.Utility.BotConst;

namespace Botler.Controllers
{
    public class TicketAPICaller
    {
        public static async Task<Utente> UtenteGetAPIAsync(Utente utente)
        {
            HttpClient clientUtente = new HttpClient();

            var contentUtente = new StringContent(JsonConvert.SerializeObject(utente), Encoding.UTF8, "application/json");
            var uriPutUtente = new Uri(InsertUtenteURI+utente.Email);

            var responseGetUtente = await clientUtente.GetAsync(uriPutUtente);

            if (responseGetUtente.IsSuccessStatusCode) // Se esiste l'utente con questa mail lo restituisce
            {
                return utente;
            }

            else // Lo crea prima e ritorna 'utente'
            {
               return  await UtentePostAPIAsync(utente);
            }

        }

        private static async Task<Utente> UtentePostAPIAsync(Utente utente)
        {
            HttpClient clientUtente = new HttpClient();

            var uriPostUtente = new Uri(InsertUtenteURI);
            var contentUtente= new StringContent(JsonConvert.SerializeObject(utente), Encoding.UTF8, "application/json");

            var responsePostTicket = await clientUtente.PostAsync(uriPostUtente, contentUtente);

            if (responsePostTicket.IsSuccessStatusCode)
            {
                return utente;
            }

            else
            {
                return null;
            }

        }

        public static async Task<bool> TicketPostAPIAsync(Ticket ticket)
        {
            HttpClient clientTicket = new HttpClient();

            var uriPostTicket = new Uri(InsertTicketURI);
            var contentTicket = new StringContent(JsonConvert.SerializeObject(ticket), Encoding.UTF8, "application/json");

            var responsePostTicket = await clientTicket.PostAsync(uriPostTicket, contentTicket);

            if (responsePostTicket.IsSuccessStatusCode)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        public static async Task<ICollection<Ticket>> GetAllUtenteTicketAPIAsync(Utente utente)
        {
            HttpClient clientTickets = new HttpClient();

            var uriGetAllTickets = new Uri(InsertTicketURI);

            var responseAllTicket = await clientTickets.GetAsync(uriGetAllTickets);

            if (responseAllTicket.IsSuccessStatusCode && utente != null)
            {
                var allTicket = await responseAllTicket.Content.ReadAsAsync<ICollection<Ticket>>();
                IEnumerable<Ticket> queryEmail = allTicket.Where(Email => Email.Equals(utente.Email));
                return queryEmail.ToList();
            }

            else
            {
                return null;
            }
        }
    }

}