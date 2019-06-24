namespace Botler.Models
{
    using System;
    using System.Collections.Generic;

    public partial class Categoria
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Categoria()
        {}

        public short ID { get; set; }
        public string Nome_Categoria { get; set; }
    }
}