using Botler.Controller;
using static Botler.Dialogs.Utility.Scenari;

namespace Botler.Helper.Attachment
{
    public class SendAttachmentFactory
    {
        public static ISendAttachment FactoryMethod(string menu)
        {
            if (menu.Equals(Parking))
            {
                return new SendMenuParkingCard();
            }

            if (menu.Equals(MenuDipedenti))
            {
                return new SendMenuDipendentiCard();
            }

            if (menu.Equals(Welcome))
            {
                return new SendWelcomeCard();
            }

            if (menu.Equals(News))
            {
                return new SendMenuNewsCard();
            }

            return null;
        }
    }
}