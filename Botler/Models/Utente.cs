using System.Collections.Generic;

namespace Botler.Models
{
    public class Utente
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Utente()
        {
          
        }

        public string Email { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
    }

}