using System.Numerics;

namespace OtpAuthServices.Model
{
    public class SupportTicket
    {
        public string SupportTicketId { get; set; }
        public DateTime Date { get; set; }
        public string Address { get; set; }
        public string Subject { get; set; }

        public string Details { get; set; }
        public string Category { get; set; }
        public string AssignedTo { get; set; }

        public string id { get; set; }
        public string status { get; set; }
        public string CustomerId { get; set;}

        public string State  { get; set; }


        public int IsMaterialType { get; set; } 

        public string District { get; set; }
        public string ZipCode { get; set; }
        public string RequestType { get; set; }
        public List<string> Attachments { get; set; } = new List<string>(); // Store filenames (GUIDs) of uploaded files
    }








   
}
