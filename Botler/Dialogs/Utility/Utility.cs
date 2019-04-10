using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Botler.Dialogs.RisorseApi;
using Newtonsoft.Json;

namespace Botler.Dialogs.Utility
{
    public class Utility
    {
        // Costanti per memorizzare informazioni
        public static int bot_id = 8; // Bot id sul db (per la demo!).
        public static int lotto_id = 1; // Lotto id sul db (per la demo!).
        public static string email; // Variabile di salvataggio credenziale email.
        public static string password; // Variabile di salvataggio credenziale password.

        // METODO PER VERIFICARE IL LOGIN
        public static async Task<UserModel> checkLogin()
        {
            string RestUrl = "http://retismartparking.azurewebsites.net/api/loginUtente";
            var uri = new Uri(string.Format(RestUrl));

            // var json = JsonConvert.SerializeObject(utente);
            var json = "{\"email\":\"" + Botler.email + "\",\"password\":\"" + Botler.password + "\"}";
            // var json = "{\"email\":\"andrea.guzzo@reti\",\"password\":\"Password01\"}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();

            response = await client.PostAsync(uri, content);

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("++++ RESPONSE IS SUCCESS STATUS CODE");
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserModel>(responseContent);
            }
            else
            {
                Debug.WriteLine("++++ STATUS CODE FROM LOGIN REQUEST: " + response.StatusCode + "\r\nMessage:" + response.RequestMessage + "\r\n" + response.Content.ReadAsStringAsync().Result);
                return null;
            }
        }

        // Chimata per verificare quale parcheggio è stato assegnato.
        public static string uriParcheggioAutoassegnato(int id_lotto, int id_utente)
        {
            return "http://retismartparking.azurewebsites.net/api/lotto/" + id_lotto + "/utente/" + id_utente + "/parcheggioLibero";
        }

        //?????
        public static async Task<PosteggioModel> setPosteggioAutoassegnato(int id_lotto)
        {
            try
            {
                System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
                var uri = new Uri(string.Format(Utility.uriParcheggioAutoassegnato(id_lotto, bot_id)));
                var response = await client.PutAsync(uri, null);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<PosteggioModel>(content);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        // Metodo per prenotare effettivamente un parcheggio,
        public static async Task<PrenotazioneModel> prenotaPosteggio(int id_lotto, int id_posteggio)
        {
            try
            {
                System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
                var uri = new Uri(string.Format(Utility.uriPrenotaPosteggio(bot_id, id_lotto, id_posteggio)));
                var response = await client.PutAsync(uri, null);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<PrenotazioneModel>(content);

                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        // Chiamata effettiva per le API.
        public static string uriPrenotaPosteggio(int id_utente, int id_lotto, int id_posto)
        {
            return "http://retismartparking.azurewebsites.net/api/lotto/" + id_lotto + "/parcheggio/" + id_posto + "/utente/" + id_utente + "/prenota";
        }

        // Metodo per cancellare la prenotazione.
        public static async Task<bool> cancellaPrenotazione(int id_posto)
        {
            try
            {
                System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
                var uri = new Uri(string.Format(Utility.uriCancellaPrenotazione(bot_id, lotto_id, id_posto)));
                var response = await client.PutAsync(uri, null);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        // Chiamata effettiva alle API per cancellare la prenotazione.
        public static string uriCancellaPrenotazione(int id_utente, int id_lotto, int id_posto)
        {
            return "http://retismartparking.azurewebsites.net/api/lotto/" + id_lotto + "/parcheggio/" + id_posto + "/utente/" + id_utente + "/cancella";
        }

        // Metodo per ottenere le informazioni dalla prenotazione.
        public static async Task<PrenotazioneModel> getPrenotazione(int id_user)
        {
            try
            {
                System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
                var uri = new Uri(string.Format(uriGetPrenotazione(id_user)));
                var response = await client.PutAsync(uri, null);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<PrenotazioneModel>(content);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        // Chiamata effettiva alle API per recuperare le informazioni di prenotazione.
        public static string uriGetPrenotazione(int id_utente)
        {
            return "http://retismartparking.azurewebsites.net/api/prenotazione/utente/" + id_utente;
        }
    }
}
