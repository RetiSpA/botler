using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Graph;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using Botler.Models;

namespace Botler.Helpers
{
    public class GraphAPIHelper
    {
        /// <summary>
        /// Read the Outlook Mail found in a DateTime
        /// </summary>
        /// <param name="turn">Bot currenct turn</param>
        /// <param name="token">Authentication Token</param>
        /// <param name="dateTimeRange">DateTime taken from user</param>
        /// <param name="unread">True if the user want only the unread mail</param>
        /// <returns> void </returns>
        public static async Task SendMailsFromInboxAsync(ITurnContext turn, string token, DateTime dateTimeRange, bool unread)
        {
            var graphClient = GetGraphClient(token);

            var messages = await graphClient.Me.MailFolders.Inbox.Messages.Request().GetAsync();

            int mailRead = 0;

            foreach (var mail in messages)
            {
                var datetimeMail = mail.ReceivedDateTime.GetValueOrDefault().DateTime;

                var resultDateInRange = DateTimeIsInRange(dateTimeRange, datetimeMail);

                if (resultDateInRange)
                {
                    await SendGenericMailTextAsync(turn, mail);
                    ++mailRead;
                }
            }

            if (mailRead == 0)
            {
                await turn.SendActivityAsync(RandomResponses(MailNonTrovataResponse));
            }
        }
        /// <summary>
        /// Get the appointment on user's calendar within a datetime
        /// </summary>
        /// <param name="turn">Bot current turn</param>
        /// <param name="accessors">Memory Storage manage</param>
        /// <param name="token">Authentication Token</param>
        /// <param name="dateTimeRange"> appointment's date</param>
        /// <returns> void </returns>
        public static async Task GetAppointmentOnCalendarAsync(ITurnContext turn, BotlerAccessors accessors,string token, DateTime dateTimeRange)
        {
            IList<Event> listEvent = await GetEventCalendarListAsync(token);

            DateTime today = DateTime.Today;

            int countEvent = 0 ;

            if (dateTimeRange == today)
            {
                ++countEvent;
                await GetFirstAppointmentOfDayAsync(turn, accessors, token);
            }

            foreach (var e in listEvent)
            {
                if (DateTimeIsInRange(dateTimeRange, ConvertDateTimeToCentralEurope(e.Start)))
                {
                    ++countEvent;
                    await SendGenericAppointmentOnCalendar(turn, e);
                }
            }

            if (countEvent == 0)
            {
                await turn.SendActivityAsync(RandomResponses(GiornoLiberoResponse));
            }

        }

        /// <summary>
        /// Get the first appointment of the day
        /// </summary>
        /// <param name="turn"> current bot turn</param>
        /// <param name="accessors">MemoryStorage manager</param>
        /// <param name="token">Auth token</param>
        /// <returns> void </returns>
        public static async Task GetFirstAppointmentOfDayAsync(ITurnContext turn, BotlerAccessors accessors, string token)
        {
            IList<Event> listEvent = await GetEventCalendarListAsync(token);
            int appointmentFound = 0;
            foreach (var e in listEvent)
            {
                DateTime eventDateStart = ConvertDateTimeToCentralEurope(e.Start);
                DateTime eventDateEnd = ConvertDateTimeToCentralEurope(e.End);

                if (eventDateStart.ToShortDateString().Equals(DateTime.Today.ToShortDateString()))
                {
                    ++appointmentFound;
                    var appointment = RandomResponses(PrimoAppuntamentoResponse);
                    await turn.SendActivityAsync(string.Format(@appointment, e.Subject , e.BodyPreview , eventDateStart.ToShortTimeString(),
                        eventDateEnd.ToShortTimeString(), e.Location.DisplayName));

                    return;
                }
            }

            if (appointmentFound == 0)
            {
                await turn.SendActivityAsync(RandomResponses(GiornoLiberoResponse));
            }
        }

        /// <summary>
        /// Create an Outlook Event with the AppuntamentCalendar Model
        /// </summary>
        /// <param name="turn"></param>
        /// <param name="token"></param>
        /// <param name="appuntamento"></param>
        /// <returns></returns>
        public static async Task CreateAppointmentAsync(ITurnContext turn, string token, AppuntamentoCalendar appuntamento)
        {
            var client = GetGraphClient(token);
            var me = await client.Me.Request().GetAsync();
            var events = await GetEventCalendarListAsync(token);

            Event e = new Event();
            var organizer = new Recipient();
            var location = new Location();
            var start = new DateTimeTimeZone();
            var end = new DateTimeTimeZone();

            start.DateTime = appuntamento.Date.ToShortDateString() + " " + appuntamento.Inizio;
            start.TimeZone = "Central European Standard Time";
            end.DateTime = appuntamento.Date.ToShortDateString() + " " + appuntamento.Fine;;
            end.TimeZone = "Central European Standard Time";
            location.DisplayName = appuntamento.Location;
            organizer.EmailAddress = new EmailAddress();
            organizer.EmailAddress.Address = me.Mail;
            organizer.EmailAddress.Name = me.GivenName + " " + me.Surname;

            e.Start = start;
            e.End = end;
            e.Subject = appuntamento.Titolo;
            e.BodyPreview =  appuntamento.Descrizione;
            e.Organizer = organizer;
            e.BodyPreview = appuntamento.Descrizione;
            // e.IsAllDay = appuntamento.IsAllDay;

            await client.Me.Events
                    .Request()
                    .Header("Prefer", "outlook.timezone=\"Central European Standard Time\"")
                    .AddAsync(e);


            await turn.SendActivityAsync("Creato appuntamento: " + "\n Il giorno " + appuntamento.Date.ToShortDateString() + " Orario : "
                 + appuntamento.Inizio.ToString() + " - " + appuntamento.Fine.ToString() + " \nLocation: " + appuntamento.Location + "Tutto il giorno " + appuntamento.IsAllDay);
        }

        /// <summary>
        /// Create a response with the mail body and subject
        /// </summary>
        /// <param name="turn"> current bot turn </param>
        /// <param name="mail"> mail to read </param>
        /// <returns> void </returns>
        private static async Task SendGenericMailTextAsync(ITurnContext turn, Message mail)
        {
            string mailResponse = RandomResponses(MailGenericaRicevutaResponse);
            mailResponse = " - " + mailResponse;
            await turn.SendActivityAsync(string.Format(@mailResponse, mail.Subject, mail.BodyPreview));
        }

        /// <summary>
        /// Send to the user Subject, TimeSpan and BodyPreview of an Appointment
        /// </summary>
        /// <param name="turn"> current bot turn </param>
        /// <param name="e">Outlook Event</param>
        /// <returns> void </returns>
        private static async Task SendGenericAppointmentOnCalendar(ITurnContext turn, Event e)
        {
            DateTime dateTimeEventStart = ConvertDateTimeToCentralEurope(e.Start);
            DateTime dateTimeEventEnd = ConvertDateTimeToCentralEurope(e.End);

            string startDate = dateTimeEventStart.ToShortDateString();
            string endDate = dateTimeEventEnd.ToShortDateString();
            string startTime = dateTimeEventStart.ToShortTimeString();
            string endTime = dateTimeEventEnd.ToShortTimeString();
            string appointment = RandomResponses(AppuntamentoGenericoResponse);

            await turn.SendActivityAsync(string.Format(@appointment, e.Subject, startDate, startTime, endTime, e.Location.DisplayName, e.Organizer.EmailAddress.Name));

        }

        /// <summary>
        /// Check if the datetime is not a past date
        /// </summary>
        /// <param name="e">Event to check</param>
        /// <returns> void </returns>
        private static bool EventIsFuture(Event e)
        {
            DateTime today = DateTime.Today;
            DateTime eventStart = ConvertDateTimeToCentralEurope(e.Start);

            if (DateTime.Compare(eventStart, today) >= 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Simply equal operation between two DateTime
        /// </summary>
        /// <param name="dateTimeRange"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        private static bool DateTimeIsInRange(DateTime dateTimeRange, DateTime dateTime)
        {
            return dateTimeRange.Date == dateTime.Date;
        }

        /// <summary>
        /// Found and sort all user's Outlook Calendar events
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static async Task<List<Event>> GetEventCalendarListAsync(string token)
        {
            var graphClient = GetGraphClient(token);
            var events = await graphClient.Me.Events.Request().GetAsync();
            var listEvent = events.ToList();
            listEvent.Sort((x, y) => DateTime.Compare(ConvertDateTimeToCentralEurope(x.Start), ConvertDateTimeToCentralEurope(y.Start)));
            return listEvent;
        }

        /// <summary>
        /// Outlook has a different TimeZone so we convert it to Central European Standard
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static DateTime ConvertDateTimeToCentralEurope(DateTimeTimeZone date)
        {
            TimeZoneInfo infotime = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            DateTime start = DateTime.Parse(date.DateTime).ToUniversalTime();
            return TimeZoneInfo.ConvertTimeFromUtc(start,  infotime);
        }

        /// <summary>
        /// Return the GraphClient linked with the Auth Token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static GraphServiceClient GetGraphClient(string token)
        {
            return new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                        return Task.CompletedTask;
                    }));
        }

    }
}
