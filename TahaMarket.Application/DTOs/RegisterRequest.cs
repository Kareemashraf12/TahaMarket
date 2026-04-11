using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class RegisterRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
    public string Name { get; set; }

    //[Required(ErrorMessage = "Email is required")]
    //[RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$" , ErrorMessage = "Invalid email format")]
    //[DefaultValue("")]
    //public string? Email { get; set; }

    [Required(ErrorMessage = "Phone is required")]
    
        
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8 , ErrorMessage = "Password must be at least 8 digits")]
    [DefaultValue("")]
    public string Password { get; set; }

    
}