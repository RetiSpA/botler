namespace Botler.Dialogs.RisorseApi
{
    public class PosteggioModel
    {
        public int id_posteggio { get; set; }

        public int stato { get; set; } // 1 = occupato; 2 = prenotato; 3 = libero.

        public int x_offset { get; set; }

        public int y_offset { get; set; }
    }

    // Serve per verificare quale posto preferito da prenotare (posto libero)
    public class PostoPreferito
    {
        public int idLotto { get; set; }

        public int idPosto { get; set; }

        public PostoPreferito(int idLotto, int idPosto)
        {
            this.idLotto = idLotto;
            this.idPosto = idPosto;
        }
    }
}
