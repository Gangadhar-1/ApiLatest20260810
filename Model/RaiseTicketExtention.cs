using Bogus.DataSets;
using System.ComponentModel.DataAnnotations;
using static OtpAuthServices.Model.Comment;

namespace OtpAuthServices.Model
{

    public class RaiseTicketExtention
    {
        public string RaiseTicketId { get; set; }
        public string RaiseTicketIdVideoRef { get; set; }
        public DateTime Date { get; set; }


        public string ApprovedAmount { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string Subject { get; set; }

        public string Details { get; set; }
        public string Category { get; set; }
        public string AssignedTo { get; set; }

        public string id { get; set; }
        public string status { get; set; }

        public string internalStatus { get; set; }
        public string CustomerId { get; set; }

        public string State { get; set; }

        public string Rating { get; set; }

        public List<string> TechnicianList { get; set; } = new List<string>();
        public List<string> DealerList { get; set; } = new List<string>();

        public string LowestBidderTechnicainId { get; set; }
        public string LowestBidderDealerId { get; set; }




        public int IsMaterialType { get; set; }

        public string District { get; set; }
        public string ZipCode { get; set; }
        public string RequestType { get; set; }
        public List<string> Attachments { get; set; } = new List<string>();
        public List<Materialss> Materials { get; set; }

        public List<Comments> comments { get; set; }

        public string Option1Day { get; set; }
        public string Option1Time { get; set; }
        public string Option2Day { get; set; }
        public string Option2Time { get; set; }

    }


    public class Materialss
    {
        public string material { get; set; }
        public string Quantity { get; set; }
    }

    public class Comments
    {
        public DateTime UpdatedDate { get; set; }
        public string CommentText
        {
            get; set;
        }



    }
}








