namespace OtpAuthServices.Model
{
    public class ChatBotUpdateModel
    {

        public string UserId { get; set; }

        public bool? ChatTypeNews { get; set; }
        public bool? ChatTypeBuySell { get; set; }
        public bool? ChatTypeTolet { get; set; }
    }
}
