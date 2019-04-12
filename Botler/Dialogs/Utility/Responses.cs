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
    public class Responses
    {
        private readonly ResourceSet _resourseSet;

        public Responses()
        {
            ResourceManager rm = new ResourceManager("Botler.Dialogs.Utility.Responses-it", Assembly.GetExecutingAssembly());

            _resourseSet = rm.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

            NoneResponse = GenerateResponsesFromName("None");
            AnomaliaResponse = GenerateResponsesFromName("Anomalia");
            RingraziamentoResponse = GenerateResponsesFromName("Ringraziamento");
            InformazioneResponse = GenerateResponsesFromName("Informazione");
            SalutoResponse = GenerateResponsesFromName("Saluto");
            SalutoPositivoResponse = GenerateResponsesFromName("SalutoPositivo");
            SalutoNegativoResponse = GenerateResponsesFromName("SalutoNegativo");
            PresentazioneResponse = GenerateResponsesFromName("Presentazione");
            PrenotazioneSuccessoResponse = GenerateResponsesFromName("PrenotazioneSuccesso");
            PrenotazioneEliminataResponse = GenerateResponsesFromName("PrenotazioneEliminata");
            PrenotazioneEffettuataResponse = GenerateResponsesFromName("PrenotazioneEffettuata");
            PrenotazioneNonTrovataResponse = GenerateResponsesFromName("PrenotazioneNonTrovata");
            PrenotazioneSceltaNoResponse = GenerateResponsesFromName("PrenotazioneSceltaNo");
            PrenotazioneScadutaResponse = GenerateResponsesFromName("PrenotazioneScaduta");
            PossibilitaParcheggioResponse = GenerateResponsesFromName("PossibilitaParcheggio");
            PrenotazioneSessioneScadutaResponse = GenerateResponsesFromName("PrenotazioneSessioneScaduta");
            PrenotazioneTempoDisponibileResponse = GenerateResponsesFromName("PrenotazioneTempoDisponibile");
            PrenotazioneDataOraResponse = GenerateResponsesFromName("PrenotazioneDataOra");
            VisualizzaPrenotazioneResponse = GenerateResponsesFromName("VisualizzaPrenotazione");

        }

        public IList<string> NoneResponse { get; }

        public IList<string> AnomaliaResponse { get; }

        public IList<string> RingraziamentoResponse { get; }

        public IList<string> InformazioneResponse { get; }

        public IList<string> SalutoResponse { get; }

        public IList<string> SalutoPositivoResponse { get; }

        public IList<string> PresentazioneResponse { get; }

        public IList<string> SalutoNegativoResponse { get; }

        public IList<string> PrenotazioneEliminataResponse { get; }

        public IList<string> PrenotazioneNonTrovataResponse { get; }

        public IList<string> PrenotazioneEffettuataResponse { get; }

        public IList<string> PrenotazioneSceltaNoResponse { get; }

        public IList<string> PrenotazioneScadutaResponse { get; }

        public IList<string> PossibilitaParcheggioResponse { get; }

        public IList<string> PrenotazioneSuccessoResponse { get; }

        public IList<string> PrenotazioneSessioneScadutaResponse { get; }

        public IList<string> PrenotazioneTempoDisponibileResponse { get; }

        public IList<string> PrenotazioneDataOraResponse { get; }

        public IList<string> VisualizzaPrenotazioneResponse { get; }

        /// <summary>
        ///  Prende in input un array precedentemente inizializzato
        ///  e ritorna un elemento random tra questi.
        /// </summary>
        /// <param name="responses">Lista di possibile risposte.</param>
        /// <returns>Una stringa casuale della lista.</returns>
        public string RandomResponses(IList<string> responses)
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
        private IList<string> GenerateResponsesFromName(string name)
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