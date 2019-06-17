
using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Resources;
using System.Globalization;
using System.Collections.Generic;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.LuisEntity;

namespace Botler.Dialogs.Utility
{
    [Serializable]
    public static class LuisIntent
    {
        public const string PresentazioneIntent = "Presentazione";
        public const string PrenotazioneIntent = "Prenotazione";
        public const string PrenotazioneSalaRiunioniIntent = "PrenotazioneSalaRiunioni";
        public const string PrenotazioneParcheggioIntent = "PrenotazioneParcheggio";
        public const string CancellaPrenotazioneParcheggioIntent = "CancellazioneParcheggio";
        public const string TempoRimanentePrenotazioneParcheggioIntent = "TempoRimanentePrenotazioneParcheggio";
        public const string VerificaPrenotazioneParcheggioIntent = "VerificaPrenotazioneParcheggio";
        public const string SaluteNegativoIntent = "SaluteNegativo";
        public const string SalutePositivoIntent = "SalutePositivo";
        public const string GoodbyeIntent = "Goodbye";
        public const string AnomaliaIntent = "Anomalia";
        public const string RingraziamentiIntent = "Ringraziamenti";
        public const string InformazioniIntent = "Informazioni";
        public const string PossibilitàIntent = "Possibilità";
        public const string NoneIntent = "None";
        public const string RisataIntent = "Risata";
        public const string LeggiMailIntent = "LeggiMail";
        public const string AutenticazioneIntent = "Autenticazione";
        public const string VisualizzaAppuntamentiCalendarIntent = "VisualizzaAppuntamentiCalendar";
        public const string CreaAppuntamentoCalendarIntent = "CreaAppuntamentoCalendar";
        public const string RichiestaSupportoIntent = "RichiestaSupporto";

    }
    [Serializable]
    public static class LuisEntity
    {
        public const string Number = "number";
        public const string Datetime = "datetime";
        public const string DatetimeTextual = "datetime_textual";
        public const string DatetimeRegex = "datetime_regex";
        public const string DatetimeBuiltin = "builtin.datetime.date";
        public const string DatetimeRegexFormat = "datatime_regex_format";
        public const string Auto = "auto";
        public const string Time = "builtin.datetime.time";
        public const string TimeRegex = "time_regex";
        public const string Email = "builtin.email";
        public const string Mese = "mese";
        public const string MailUnread = "mail_unread";
        public const string Date = "date";
        public const string SalaRiunioni = "sala_riunioni";
        public const string SaleRiunioni = "sale_riunioni";
        public const string Appuntamento = "appuntamento";
        public const string ListaAssistenza = "lista_entita_assistenza";
        public const string PC = "pc";
        public const string Computer = "computer";
        public const string Badge = "badge";
        public const string Tesserino = "tesserino";
        public const string Multifunzionale = "multifunzionale";
        public const string R101 = "r101";
        public const string R102 = "r102";
        public const string Utenza = "utenza";
        public const string  PostaElettronica = "posta elettronica";
        public const string AbilitaUtente = "abilita utente";
        public const string DisabilitaUtente = "disabilita utente";
        public const string Dispositivo = "dispositivo";
        public const string Server = "server";
        public const string Rete = "rete";
        public const string Connessione = "connessione";
        public const string Wifi = "wifi";
        public const string LAN = "lan";
        public const string Wi_Fi = "wi-fi";
        public const string Progetto = "progetto";
        public const string NuovoProgetto = "nuovo progetto";
        public const string Infrastruttura = "infrastruttura";




    }

    [Serializable]
    public static class BotConst
    {
        public static string ConnectionName = "BotLogin";
        public static string LuisConfiguration = "basic-bot-LUIS";
        public static string QnAPublicKey = "botler-qna-pubblica";
        public static string QnAKey = "botler-qna";
        public const string ResourcesIT = "Botler.Dialogs.Resources.Responses-it";
        public const string ResourceEN = "Botler.Dialogs.Resources.Responses-en";
        public const string InsertTicketURI = "http://ticketsupportapi.azurewebsites.net/api/Ticket";
        public const string InsertUtenteURI = "http://ticketsupportapi.azurewebsites.net/api/Utente";
        public const string MongoDBCollection = "botler-states";
        public const string MongoDatabase = "botler-data-context";


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
        public static IList<string> AutenticazioneEffettuataResponse { get; } = GenerateResponsesFromName("AutentacazioneEffettuata");
        public static IList<string> PossibilitaResponse { get; } = GenerateResponsesFromName("Possibilita");
        public static IList<string> DomandaResponse { get; } = GenerateResponsesFromName("Domanda");
        public static IList<string> QnAPublicResponse { get; } = GenerateResponsesFromName("QnAPublic");
        public static IList<string> QnAResponse { get; } = GenerateResponsesFromName("QnA");
        public static IList<string> LogoutEffettuatoResponse { get; } = GenerateResponsesFromName("LogoutEffettuato");
        public static IList<string> LogoutImpossibileResponse { get; } = GenerateResponsesFromName("LogoutImpossibile");
        public static IList<string> ContinuaQnAResponse { get; } = GenerateResponsesFromName("QnAContinua");
        public static IList<string> AutenticazioneErroreResponse { get; } = GenerateResponsesFromName("AutenticazioneErrore");
        public static IList<string> RisataResponse { get; } = GenerateResponsesFromName("Risata");
        public static IList<string> ComandiResponse { get; } = GenerateResponsesFromName("ListaComandi");
        public static IList<string> MailGenericaRicevutaResponse { get; } = GenerateResponsesFromName("MailGenericaRicevuta");
        public static IList<string> MailGenericaInviataResponse { get; } = GenerateResponsesFromName("MailGenericaInviata");
        public static IList<string> PrimoAppuntamentoResponse { get; } = GenerateResponsesFromName("PrimoAppuntamento");
        public static IList<string> AppuntamentoGenericoResponse { get; } = GenerateResponsesFromName("AppuntamentoGenerico");
        public static IList<string> GiornoLiberoResponse { get; } = GenerateResponsesFromName("GiornoLibero");
    }
    [Serializable]
    public static class IntentsSets
    {
        public static HashSet<string> ExecutionIntents = new HashSet<string>() {  PrenotazioneIntent };
        public static HashSet<string> ParkingIntents = new HashSet<string>() { VerificaPrenotazioneParcheggioIntent, PrenotazioneParcheggioIntent,
            CancellaPrenotazioneParcheggioIntent, TempoRimanentePrenotazioneParcheggioIntent };
        public static HashSet<string> OutlookIntents = new HashSet<string> () { LeggiMailIntent, PrenotazioneSalaRiunioniIntent, VisualizzaAppuntamentiCalendarIntent, CreaAppuntamentoCalendarIntent};
        public static HashSet<string> SupportIntents = new HashSet<string> () { RichiestaSupportoIntent };

    }

    [Serializable]
    public static class EntitySets
    {
        public static HashSet<string> OutlookEntities = new HashSet<string>() { Datetime, MailUnread, DatetimeRegex, DatetimeBuiltin, SalaRiunioni, Appuntamento, SaleRiunioni, Time, TimeRegex, Email };
        public static HashSet<string> ParkingEntities = new HashSet<string>() { Auto };
        public static HashSet<string> SupportEntities = new HashSet<string>() { PC, Computer, Badge, Tesserino, ListaAssistenza};

        public static HashSet<string> UtenzeSupportEntities = new HashSet<string>() { Utenza, PostaElettronica , AbilitaUtente, DisabilitaUtente };

        public static HashSet<string> DispositiviSupportEntities = new HashSet<string>() { PC, Computer, Dispositivo};

        public static HashSet<string> NetworkSupportEntities = new HashSet<string>() { Rete, Connessione, Wifi, Wi_Fi, LAN};

        public static HashSet<string> InfrastructureSupportEntities = new HashSet<string> () { Server, Infrastruttura};

        public static HashSet<string> ProjectSupportEntities = new HashSet<string> () { Progetto, NuovoProgetto };

        public static HashSet<string> BadgeAccessiEntities= new HashSet<string>() { Badge, "accessi" };

        public static HashSet<string> DomoticaEntities = new HashSet<string> () { "domotica", "luci" };

        public static HashSet<string> SiWebEntities = new HashSet<string>() { "siweb", "gestionale reti", "gestionale" };

        public static HashSet<string> RegiEntities = new HashSet<string>() { "regia" };

        public static Dictionary<HashSet<string>, int> SupportMapTipoTicketID = new Dictionary<HashSet<string>, int>() { [UtenzeSupportEntities] = 0, [DispositiviSupportEntities] = 1,
        [NetworkSupportEntities] = 2,  [InfrastructureSupportEntities] = 3, [ProjectSupportEntities] = 4 , [BadgeAccessiEntities] = 7, [DomoticaEntities] = 9 , [SiWebEntities] = 11, [RegiEntities] = 13};

        public static HashSet<string> SaleRiunioniSet = new HashSet<string>() { Multifunzionale, R101, R102 };

        public static HashSet<string> DatesEntitiesSet = new HashSet<string>() { Datetime, DatetimeBuiltin, DatetimeRegex, DatetimeRegexFormat, DatetimeTextual };
        public static Dictionary<string, int> MonthEntities = new Dictionary<string, int>() { ["gennaio"] = 01, ["febbraio"] = 02,["marzo"] = 03, ["aprile"] = 04, ["maggio"] = 05,
             ["giugno"] = 06, ["luglio"] = 07, ["agosto"] = 08, ["settembre"] = 09, ["ottobre"] = 10, ["novembre"] = 11,["dicembre"] = 12 };
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

        public const string QnA = "QnA";

        public const string QnARiservata = "QnARiservata";

        public const string QnAPublic = "QnAPublic";

        public const string Outlook = "Outlook";

        public const string OutlookDescription = "OutlookDescription";

        public const string Supporto = "Supporto";

        public const string SupportoDescription = "SupportoDescription";

        public static HashSet<string> ExecutionScenarios = new HashSet<string>() { Parking, Autenticazione, Outlook };

        public static HashSet<string> DescriptionScenarios = new HashSet<string>() { OutlookDescription };
    }

    [Serializable]
    public static class Commands
    {
        public const string CommandWelcome = "commandWelcome";

        public const string CommandParking = "commandParking";

        public const string CommandAuthentication = "commandAuth";

        public const string CommandAreaRiservata = "commandAreaRiservata";

        public const string CommandQnA = "commandQnA";

        public const string CommandQnAPublic = "commandQnAPublic";

        public const string CommandQnARiservata = "commandQnARiservata";

        public const string CommandLogout = "commandLogout";

        public const string CommandExit = "commandExit";

        public const string CommandHelp = "commandHelp";
    }

    [Serializable]
    public static class RegularExpressions
    {
        public const string RegexFindDateValue = "(:)(.*)";
        public const string RegexFixYearDateTime = "XXXX-\\d{2}-\\d{2}";
        public const string RegexEntityType = "(.*)(\\[)";
        public const string RegexEntityText = @"\""(.+?)\""";
        public const string RegexEntityFound = "(.*)(},)(.*)(.*)(}|},)";
        public const string RegexEntityValueFound = "(\\[\\s)(.*)(\\s\\])";
        public const string RegexGetTextFromEntity = @"(\""text\"":)(.*)";
        public const string RegexGetTypeFromEntity = @"(\""type\"":)(.*)";
        public const string RegexCheckYear = "([1970-2019])(\\s*|.*\\s*|\\s*.*)";

        public const string RegexDeserilizeEntityValue =
        "(\\[\\s*)(\\{\\s*.*\\s*.*\\s*.*\\s*.*\\s*})";
        public const string RegexDeserilizeComplexEntityValue =
         "(\\[\\s*)(\\{\\s*.*\\s.*\\s*.*\\s*.*\\s*.*\\s*},\\s*\\{\\s*.*\\s.*\\s*.*\\s*.*\\s*.*\\s*})";
        public const string RegexFindDateTimeFromText =
        "(.*\\s*|.*)(0[1-9]|1[0-9]|2[0-9]|3[0-9])(\\s*)(gennaio|febbraio|marzo|aprile|maggio|giugno|luglio|agosto|settembre|ottobre|novembre|dicembre)(\\s*\\d{4}\\s*|\\s*.*|.*\n|.*\\s*)(\\s*.*)+";
        public const string RegexTimeFound_1 = "(dalle|dalle ore) ((0[0-9]|1[0-9]|2[0-3]|[0-9]):[0-5][0-9]) (alle|alle ore) ((0[0-9]|1[0-9]|2[0-3]|[0-9]):[0-5][0-9])"; // [2] AND [5]

        public const string RegexTimeFound_2 = "((0[0-9]|1[0-9]|2[0-3]|[0-9]):[0-5][0-9]) (\\s*|-\\s*) ((0[0-9]|1[0-9]|2[0-3]|[0-9]):[0-5][0-9])";
    }

    [Serializable]
    public static class IntentNeedsEntityPhrases
    {
        public const string PrenotaSalaEntityToCollect = "Vedo che vuoi prenotare una sala riunioni, le informazioni utili che puoi darmi sono: \n Data, Ora e nome Meeting Rooms(opzionale) :)";
        public const string LeggiMailEntityNeedsToCollect = "Vuoi inserire una data o vuoi leggere le ultime mail ?";
        public const string SupportoEntityNeedsToCollect = "Per aprire un ticket correttamente mi serve sapere almeno su cosa hai bisogno di assistenza.\n - PC, Badge, Accessi, Domotica...\n Insieme ad una descrizione, priorità (opzionali) :)";
        public const string CreaAppuntamentoEntityNeedsToCollect = "Per creare un appuntamento, inserisci: \nUna Location, una Data, orario di inizo e fine, e dei partecipanti (email partecipanti)";
    }
}