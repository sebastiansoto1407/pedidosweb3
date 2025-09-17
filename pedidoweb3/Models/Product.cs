using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pedidoweb3.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(160)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(400)]
        public string? Descripcion { get; set; }

        [StringLength(80)]
        public string? Categoria { get; set; }

        [Range(0.01, 9999999)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; }
    }
}
