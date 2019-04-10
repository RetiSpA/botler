using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Resources;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Bot.Builder.AI.QnA;
using System.Threading.Tasks;
using Botler.Dialogs.Dialoghi;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Botler.Dialogs.Utility
{
    public class Responses
    {
        public IList<string> _noneResponse { get; set; }
        public  IList<string> _anomaliaResponse { get; set; }
        public  IList<string> _ringraziamentoResponse { get; set; }
        public  IList<string> _informazioneResponse { get; set; }
        public  IList<string> _salutoResponse { get; set; }
        public  IList<string> _salutoPositivoResponse { get; set; }
        public IList<string> _presentazioneResponse { get; set; }
        public  IList<string> _salutoNegativoResponse { get; set; }

        private readonly ResourceSet _resourseSet;

        public Responses()
        {
            ResourceManager rm = new ResourceManager("Botler.Dialogs.Utility.Responses-it",
                    Assembly.GetExecutingAssembly());
            _resourseSet = rm.GetResourceSet(CultureInfo.CurrentUICulture,true,true);
            // Loads all responses IList<string>
            LoadAllResponsesList();
        }
        private IList<string> GenerateResponsesFromName(string name)
         {

            IList<string> responses = new List<string>();

            string regexPattern = @name+"_\\d*";
            Regex regex = new Regex (regexPattern);

            foreach(DictionaryEntry entry in _resourseSet)
            {
                string resourceKey = entry.Key.ToString();
                object resource = entry.Value;
                var match = regex.Match(resourceKey);
                if(match.Success)
                    responses.Add((string)resource);
            }
            return responses;
         }

        /// <summary>
        ///  Prende in input un array precedentemente inizializzato
        ///  e ritorna un elemento random tra questi
        /// </summary>
        /// <param name="responses"></param>
        /// <returns></returns>
        public string RandomResponses(IList<string> responses)
        {
            Random rnd = new Random();
            int i = rnd.Next(0, responses.Count());
            return responses.ElementAt(i);
        }

        private void LoadAllResponsesList()
        {
            _noneResponse = GenerateResponsesFromName("None");
            _anomaliaResponse = GenerateResponsesFromName("Anomalia");
            _ringraziamentoResponse = GenerateResponsesFromName("Ringraziamento");
            _informazioneResponse = GenerateResponsesFromName("Informazione");
            _salutoResponse = GenerateResponsesFromName("Saluto");
            _salutoPositivoResponse = GenerateResponsesFromName("SalutoPositivo");
            _salutoNegativoResponse = GenerateResponsesFromName("SalutoNegativo");
            _presentazioneResponse = GenerateResponsesFromName("Presentazione");
        }
    }
}