using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Resources;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Botler.Dialogs.Dialoghi;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Botler.Dialogs.Utility
{
    public static class Responses
    {
        private static ResourceSet _resourseSet = new ResourceManager("Botler.Dialogs.Welcome.Resources.Responses-it", Assembly.GetExecutingAssembly())
        .GetResourceSet(CultureInfo.CurrentUICulture, true, true);

        /// <summary>
        ///  Prende in input un array precedentemente inizializzato
        ///  e ritorna un elemento random tra questi.
        /// </summary>
        /// <param name="responses">Lista di possibile risposte.</param>
        /// <returns>Una stringa casuale della lista.</returns>
        public static string RandomResponses(IList<string> responses)
        {
            Random rnd = new Random();
            int i = rnd.Next(0, responses.Count());
            return responses.ElementAt(i);
        }

        /// <summary>
        /// Prende dal file Response-it.resx tutti i valori, che hanno
        /// come chiave "name".
        /// </summary>
        /// <param name="name">Chiave dei valori che si vuole prendere.</param>
        /// <returns>IList<string> di valori assocciati alla chiave "name".</string></returns>
        public static IList<string> GenerateResponsesFromName(string name)
        {
            IList<string> responses = new List<string>();
            
            string regexPattern = @name+"_\\d*";
            Regex regex = new Regex(regexPattern);

            foreach (DictionaryEntry entry in _resourseSet)
            {
                string resourceKey = entry.Key.ToString();
                object resource = entry.Value;
                var match = regex.Match(resourceKey);

                if (match.Success)
                {
                    responses.Add((string)resource);
                }
            }

            return responses;
         }
    }
}