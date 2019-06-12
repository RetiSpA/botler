using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.ListsResponsesIT;
namespace Botler.Helpers
{
    public class GraphAPIHelper
    {
        public static async Task GetMailsFromInboxAsync(ITurnContext turn, BotlerAccessors accessors, string token, DateTime dateTimeRange, int lastCountMail, bool unread)
        {
            var graphClient = GetGraphClient(token);

            var messages = await graphClient.Me.MailFolders.Inbox.Messages.Request().GetAsync();

            foreach (var mail in messages)
            {
                if (mail.IsRead == unread)
                {
                    if (DateTimeIsInRange(dateTimeRange, mail.ReceivedDateTime.GetValueOrDefault().DateTime))
                    {
                        await SendGenericMailTextAsync(turn, mail);
                    }

                }
            }
        }

        public static async Task GetAppointmentOnCalendarAsync(ITurnContext turn, BotlerAccessors accessors,string token, DateTime dateTimeRange)
        {
            IList<Event> listEvent = await GetEventCalendarListAsync(token);

            DateTime today = DateTime.Today;

            if (dateTimeRange == null) // take all the possibile appointment on outlook calendar.
            {
                foreach (var e in listEvent)
                {
                    if (EventIsFuture(e))
                    {
                        await SendGenericAppointmentOnCalendar(turn, e);
                    }
                }
            }
            else // we have a range of DateTime(s) to check
            {
                foreach(var e in listEvent)
                {
                    if (DateTimeIsInRange(dateTimeRange, ConvertDateTimeToCentralEurope(e.Start)))
                    {
                        await SendGenericAppointmentOnCalendar(turn, e);
                    }
                }
            }

        }

        public static async Task GetFirstAppointmentOfDayAsync(ITurnContext turn, BotlerAccessors accessors, string token)
        {
            IList<Event> listEvent = await GetEventCalendarListAsync(token);

            foreach (var e in listEvent)
            {
                DateTime eventDateStart = ConvertDateTimeToCentralEurope(e.Start);
                DateTime eventDateEnd = ConvertDateTimeToCentralEurope(e.End);

                if (eventDateStart.ToShortDateString().Equals(DateTime.Today.ToShortDateString()))
                {
                    var appointment = RandomResponses(PrimoAppuntamentoResponse);
                    await turn.SendActivityAsync(string.Format(@appointment, e.Subject , e.BodyPreview , eventDateStart.ToShortTimeString(),
                    eventDateEnd.ToShortTimeString(), e.Location.DisplayName));

                    return;
                }
            }

            await turn.SendActivityAsync(RandomResponses(GiornoLiberoResponse));
        }

        public static async Task SharePointTest(ITurnContext turn, string token)
        {
            var graphClient = GetGraphClient(token);
            var me = await graphClient.Sites["reti.sharepoint.com"].Request().GetAsync();
        }

        private static async Task SendGenericMailTextAsync(ITurnContext turn, Message mail)
        {
            string mailResponse = RandomResponses(MailGenericaRicevutaResponse);

            await turn.SendActivityAsync(string.Format(@mailResponse, mail.Subject, mail.BodyPreview));
        }

        private static async Task SendGenericAppointmentOnCalendar(ITurnContext turn, Event e)
        {
            DateTime dateTimeEventStart = ConvertDateTimeToCentralEurope(e.Start);
            DateTime dateTimeEventEnd = ConvertDateTimeToCentralEurope(e.End);

            string startDate = dateTimeEventStart.ToShortDateString();
            string endDate = dateTimeEventEnd.ToShortDateString();
            string startTime = dateTimeEventStart.ToShortTimeString();
            string endTime = dateTimeEventEnd.ToShortTimeString();
            string appointment = RandomResponses(AppuntamentoGenericoResponse);

            await turn.SendActivityAsync(string.Format(@appointment, e.Subject, startDate, startTime, endTime, e.Location.DisplayName));

        }

        private static bool EventIsFuture(Event e)
        {
            DateTime today = DateTime.Today;
            DateTime eventStart = ConvertDateTimeToCentralEurope(e.Start);

            if( DateTime.Compare(eventStart, today) >= 0) return true;
            return false;
        }

        private static bool DateTimeIsInRange(DateTime dateTimeRange, DateTime dateTime)
        {
            if (DateTime.Compare(dateTime, dateTimeRange) >= 0 )
            {
                return true;
            }

            return false;
        }

        private static async Task<List<Event>> GetEventCalendarListAsync(string token)
        {
            var graphClient = GetGraphClient(token);
            var events = await graphClient.Me.Events.Request().GetAsync();
            var listEvent = events.ToList();
            listEvent.Sort((x, y) => DateTime.Compare(ConvertDateTimeToCentralEurope(x.Start), ConvertDateTimeToCentralEurope(y.Start)));
            return listEvent;
        }

        private static DateTime ConvertDateTimeToCentralEurope(DateTimeTimeZone date)
        {
            TimeZoneInfo infotime = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            DateTime start = DateTime.Parse(date.DateTime).ToUniversalTime();
            return TimeZoneInfo.ConvertTimeFromUtc(start,  infotime);
        }

        private static GraphServiceClient GetGraphClient(string token)
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