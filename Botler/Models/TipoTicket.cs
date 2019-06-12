namespace Botler.Models
{
    using System;
    using System.Collections.Generic;

    public partial class TipoTicket
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TipoTicket()
        {
            this.Ticket = new HashSet<Ticket>();
        }

        public short ID { get; set; }
        public string Nome_TipoTicket { get; set; }
        public string Descrizione_TipoTicket { get; set; }
        public short ID_Categoria { get; set; }
        public Nullable<short> ID_Problematica { get; set; }

        public virtual Categoria Categoria { get; set; }
        public virtual Problematica Problematica { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Ticket> Ticket { get; set; }
    }
}