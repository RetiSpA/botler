using System.Collections.Generic;

namespace Botler.Models
{
    public partial class Problematica
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Problematica()
        {
            this.TipoTicket = new HashSet<TipoTicket>();
        }

        public short ID { get; set; }
        public string Nome { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TipoTicket> TipoTicket { get; set; }
    }
}