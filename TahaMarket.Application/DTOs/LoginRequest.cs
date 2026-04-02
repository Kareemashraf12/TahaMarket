using System.ComponentModel.DataAnnotations;

namespace TahaMarket.Application.DTOs
{
   
    public class LoginRequest
    {
        [Required(ErrorMessage = "Phone is required")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
