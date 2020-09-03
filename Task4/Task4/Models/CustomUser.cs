using Microsoft.AspNet.Identity;
using ORM.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Task4.Models
{
    [Table("CustomUser")]
    public class CustomUser : IUser<int>
    {
        [PK]
        [Member]
        public int Id { get; set; }

        [Member]
        public string UserName { get; set; }

        [Member]
        [Required]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Required]
        [Member]
        public string Password { get; set; }

        [Member]
        public string NormalizedEmail { get; set; }

        [Member]
        public string NormalizedUserName { get; set; } 
    }
}