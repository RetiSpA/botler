using static Botler.Dialogs.Utility.EntitySets;
namespace Botler.Helpers
{
    public class TicketSupportHelper
    {
        public static int  GetTipoTicket(string entityText)
        {
            var ID = -1;

            foreach(var set in SupportMapTipoTicketID.Keys)
            {
                if (set.Contains(entityText))
                {
                    SupportMapTipoTicketID.TryGetValue(set, out ID);
                }
            }

            return ID;
        }
    }
}