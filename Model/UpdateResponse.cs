using System.Net;

namespace OtpAuthServices.Model
{
    public class UpdateResponse
    {
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
