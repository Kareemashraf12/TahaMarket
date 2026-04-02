namespace TahaMarket.Application.DTOs
{
    public class VerifyOtpDto
    {
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
        public string Purpose { get; set; }
    }
}