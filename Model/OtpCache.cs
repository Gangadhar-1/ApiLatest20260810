namespace OtpAuthServices.Model
{
    public class OtpCache
    {

        public string id { get; set; }
        public string senderValue { get; set; }
        public string otp { get; set; }
        public DateTime expiryTime { get; set; }
    }
}
