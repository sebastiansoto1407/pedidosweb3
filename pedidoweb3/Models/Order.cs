using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pedidoweb3.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }
        public User? Cliente { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required]
        public OrderStatus Estado { get; set; } = OrderStatus.Pendiente;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
