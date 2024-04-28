namespace DotnetAPI.Models
{
    public partial class UserForRegistrationDTO
    {
        public string FirstName {get; set;} = "";
        public string LastName {get; set;} = "";
        public string Email {get; set;} = "";
        public string Gender {get; set;} = "";
        public string Password { get; set; } = "";
        public string PasswordConfirm { get; set; } = "";
    }
}
