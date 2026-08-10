namespace OtpAuthServices.Model
{
    public class ChatMessageTabSeenByUser
    {

        public string id {  get; set; }

        public string DateTime { get; set; }

        public string ChatMessageTabSeenByUserId { get; set; }

        public string UserId { get; set; }

        public bool ChatTabNews { get; set; }

        public bool ChatTabBuySell { get; set; }

        public bool ChatTabTolet { get; set; }
    }
}
