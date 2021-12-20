using System.ComponentModel.DataAnnotations;

namespace Ae.Galeriya.Web.Models
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }

    }
}
