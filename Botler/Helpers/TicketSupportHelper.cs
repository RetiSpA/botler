using static Botler.Dialogs.Utility.EntitySets;

namespace Botler.Helpers
{
    public static class TicketSupportHelper
    {
        /// <summary>
        /// Find the TipoTicket_ID in the Dictionary<HashSet<string>, int> SupportMapTipoTicketID.
        /// </summary>
        /// <param name="entityText"> Entity text coming from LUIS</param>
        /// <returns> TipoTicket_ID mapped to the entity</returns>
        public static int GetTipoTicket(string entityText)
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