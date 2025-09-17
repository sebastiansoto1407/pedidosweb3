using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace pedidoweb3.Models
{
    public class User
    {
        public int Id { get; set; } //id

        [Required, StringLength(120)]
        public string Nombre { get; set; } = string.Empty; //string para nombre

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty; //string para el email pq es formato texto

        //guardaremos hash (no en texto plano)
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // admin | cliente | empleado
        [Required, StringLength(20)]
        public string Rol { get; set; } = "cliente";
        

        public ICollection<Order>? Orders { get; set; }
    }
}
