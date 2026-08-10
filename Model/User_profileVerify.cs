namespace OtpAuthServices.Model
{
    public class User_profileVerify
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string MobileNo { get; set; }
        public string EmailId { get; set; }
        public bool IsMobileNumberValidate { get; set; }
        public bool IsEmailIdValidate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDt { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public string ProfileType { get; set; }
    }
}
