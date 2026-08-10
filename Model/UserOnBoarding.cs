namespace OtpAuthServices.Model
{
    public class UserOnBoarding
    {
         public Guid UserId { get; set; }

        public string id { get; set; }
        public  String Date { get; set; }
        public string? UserName { get; set; }
        public string? UserPassword { get; set; }
        public string MobileNo { get; set; }
        public string? EmailId { get; set; }
        public bool IsMobileNumberValidate { get; set; }
        public bool? IsEmailValidate { get; set; }
        public string ProfileType { get; set; }
    }
}
