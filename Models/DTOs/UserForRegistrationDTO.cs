namespace DotnetAPI.Models
{
    public partial class UserForRegistrationDTO
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string PasswordConfirm { get; set; } = "";
    }
}