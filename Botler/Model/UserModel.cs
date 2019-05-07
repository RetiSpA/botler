using System;
using Microsoft.Bot.Schema;

namespace Botler.Dialogs.RisorseApi
{
    public class UserModel
    {
        public string nome { get; set; }

        public string email { get; set; }

        public int id_utente { get; set; }

        public bool autenticato { get; set; } = false;

        public TokenResponse token { get; set; }

        public UserModel(int id_utente, string email)
        {
            this.id_utente = id_utente;
            this.email = email;
            this.nome = setNameFromEmail(email);
        }

        public static string setNameFromEmail(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var index = email.LastIndexOf('@');
                if (index > 0)
                {
                    return email.Remove(index).Replace('.', ' ');
                }
                else
                {
                    return "name surname";
                }
            }
            else
            {
                return "name surname";
            }
        }
    }
}
