using Microsoft.Azure.Cosmos.Core.Networking;

namespace OtpAuthServices.Model
{
    public class ChatBot
    {
        public string id        { get; set; }

        public string UserName  { get; set; }
        public string? DateTime { get; set; }
        public string ChatType  { get; set; }
        public string UserId    { get; set; }
        public string ChatBotId { get; set; }
        public string Message   { get; set; }

        public string NumberOfLikes { get; set; }

        public List<string> UploadFile { get; set; } = new List<string>();

       
    }
}




