using System;

namespace Botler.Dialogs.RisorseApi
{
    // Modello (oggetto) per gestire la prenotazione a livello di API
    // come è formato e da cos'è composto.
    public class PrenotazioneModel
    {
        public int id_lotto { get; set; }

        public int id_user { get; set; }

        public int id_posto { get; set; }

        public string nomeLotto { get; set; }

        public string posizioneLotto { get; set; }

        public DateTime scadenza { get; set; }

        public bool posto_occupato { get; set; }

        public bool prenotazione_confermata { get; set; }

        public PrenotazioneModel(int id_user, int id_posto, int id_lotto, string nomelotto, string posizionelotto, DateTime scadenza, bool posto_occupato, bool prenotazione_confermata)
        {
            this.id_lotto = id_lotto;
            this.id_posto = id_posto;
            this.id_user = id_user;
            this.nomeLotto = nomelotto;
            this.posizioneLotto = posizionelotto;
            this.scadenza = scadenza;
            this.posto_occupato = posto_occupato;
            this.prenotazione_confermata = prenotazione_confermata;
        }
    }
}