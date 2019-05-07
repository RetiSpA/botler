
using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Resources;
using System.Globalization;
using System.Collections.Generic;
using static Botler.Dialogs.Utility.Responses;

namespace Botler.Dialogs.Utility
{

    /// <summary>
    /// All supported LUIS Intent
    /// </summary>
    [Serializable]
    public static class LuisIntent
    {
        public const  string PresentazioneIntent = "Presentazione";
        public const string PrenotazioneIntent = "Prenotazione";
        public const string CancellaPrenotazioneIntent = "Cancellazione";
        public const string TempoRimanentePrenotazioneIntent = "TempoRimanentePrenotazione";
        public const string VerificaPrenotazioneIntent = "VerificaPrenotazione";
        public const string SaluteNegativoIntent = "SaluteNegativo";
        public const string SalutePositivoIntent = "SalutePositivo";
        public const string GoodbyeIntent = "Goodbye";
        public const string AnomaliaIntent = "Anomalia";
        public const string RingraziamentiIntent = "Ringraziamenti";
        public const string InformazioniIntent = "Informazioni";
        public const string PossibilitàIntent = "Possibilità";
        public const string NoneIntent = "None";
    }

    /// <summary>
    /// Const string needed for bot setup
    /// </summary>
    [Serializable]
    public static class BotConst
    {
        public static string ConnectionName = "BotLogin";
        public static string LuisConfiguration = "basic-bot-LUIS";
        public static string QnAMakerKey = "botler-qna-pubblica";
    }

    [Serializable]
    public static class ListsResponsesIT
    {
        public static IList<string> NoneResponse { get; } = GenerateResponsesFromName("None");

        public static IList<string> AnomaliaResponse { get; } = GenerateResponsesFromName("Anomalia");

        public static IList<string> RingraziamentoResponse { get; } = GenerateResponsesFromName("Ringraziamento");

        public static IList<string> InformazioneResponse { get; } = GenerateResponsesFromName("Informazione");

        public static IList<string> SalutoResponse { get; } = GenerateResponsesFromName("Saluto");

        public static IList<string> SalutoPositivoResponse { get; } = GenerateResponsesFromName("SalutoPositivo");

        public static IList<string> PresentazioneResponse { get; }  = GenerateResponsesFromName("Presentazione");

        public static IList<string> SalutoNegativoResponse { get; } = GenerateResponsesFromName("SalutoNegativo");

        public static IList<string> PrenotazioneEliminataResponse { get; } = GenerateResponsesFromName("PrenotazioneEliminata");

        public static IList<string> PrenotazioneNonTrovataResponse { get; } = GenerateResponsesFromName("PrenotazioneNonTrovata");

        public static IList<string> PrenotazioneEffettuataResponse { get; } = GenerateResponsesFromName("PrenotazioneEffettuata");

        public static IList<string> PrenotazioneSceltaNoResponse { get; } = GenerateResponsesFromName("PrenotazioneSceltaNo");

        public static IList<string> PrenotazioneScadutaResponse { get; } = GenerateResponsesFromName("PrenotazioneScaduta");

        public static IList<string> PossibilitaParcheggioResponse { get; } = GenerateResponsesFromName("PossibilitaParcheggio");

        public static IList<string> PrenotazioneSuccessoResponse { get; } = GenerateResponsesFromName("PrenotazioneSuccesso");

        public static IList<string> PrenotazioneSessioneScadutaResponse { get; } = GenerateResponsesFromName("PrenotazioneSessioneScaduta");

        public static IList<string> PrenotazioneTempoDisponibileResponse { get; } = GenerateResponsesFromName("PrenotazioneTempoDisponibile");

        public static IList<string> PrenotazioneDataOraResponse { get; } = GenerateResponsesFromName("PrenotazioneDataOra");

        public static IList<string> VisualizzaPrenotazioneResponse { get; } = GenerateResponsesFromName("VisualizzaPrenotazione");

        public static IList<string> AutenticazioneSuccessoResponse { get; } = GenerateResponsesFromName("AutenticazioneSuccesso");

        public static IList<string> AutenticazioneNecessariaResponse { get; } = GenerateResponsesFromName("AutenticazioneNecessaria");

        public static IList<string> AutenticazioneEffettuata { get; } = GenerateResponsesFromName("AutentacazioneEffettuata");

    }

    [Serializable]
    public static class Scenari
    {
        public const string Default = "Default";

        public const string MenuDipedenti = "MenuDipendenti";

        public const string Autenticazione = "Autenticazione";

        public const string Parking = "Parking";

        public const string Welcome = "Welcome";

        public const string News = "News";

    }

    [Serializable]

    public static class Commands
    {
        public const string CommandWelcome = "commandWelcome";

        public const string CommandParking = "commandParking";

        public const string CommandAuthentication = "commandAuthentication";

        public const string CommandAreaRiservata = "commandAreaRiservata";
        
    }
}