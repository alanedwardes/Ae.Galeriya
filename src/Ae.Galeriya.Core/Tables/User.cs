using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ae.Galeriya.Core.Tables
{
    public sealed class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }

        public string Username { get; set; }
        public string Password { get; set; } // TEMPORARY FOR TESTING
    }
}
