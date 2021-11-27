using System.ComponentModel.DataAnnotations;

namespace Ae.Galeriya.Web.Models
{
    public class EditUserModel
    {
        [Required]
        [DataType(DataType.Text)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
