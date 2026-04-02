using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class RegisterRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$" , ErrorMessage = "Invalid email format")]
    [DefaultValue("")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Phone is required")]
    [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$",
        ErrorMessage = "Invalid Egyptian phone number")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$",
        ErrorMessage = "Password must contain uppercase, lowercase, number and special character"
    )]
    public string Password { get; set; }
}