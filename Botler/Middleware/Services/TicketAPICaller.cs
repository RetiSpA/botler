using System;
using System.Collections.Generic;
using System.Linq;
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

        public static async Task<bool> TicketPostAPIAsync(Ticket ticket)
        {
            HttpClient clientTicket = new HttpClient();

            var uriPostTicket = new Uri(InsertTicketURI);
            var contentTicket = new StringContent(JsonConvert.SerializeObject(ticket), Encoding.UTF8, "application/json");
            var ticketC = await contentTicket.ReadAsStringAsync();
            Console.WriteLine(ticketC);

            var responsePostTicket = await clientTicket.PostAsync(uriPostTicket, contentTicket);

            if (responsePostTicket.IsSuccessStatusCode)
            {
                return true;
            }

            else
            {
                Console.WriteLine(responsePostTicket.StatusCode + " " + responsePostTicket.ReasonPhrase + " " + responsePostTicket.Content.Headers);
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