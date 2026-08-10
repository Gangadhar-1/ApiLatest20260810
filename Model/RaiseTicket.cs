using Bogus.DataSets;
using System.ComponentModel.DataAnnotations;
using static OtpAuthServices.Model.Comment;

namespace OtpAuthServices.Model
{

    public class RaiseTicket
    {
        public string RaiseTicketId { get; set; }
        public DateTime Date { get; set; }


        public string ApprovedAmount { get; set; }
        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string Address { get; set; }
        public string Subject { get; set; }

        public string Details { get; set; }
        public string Category { get; set; }
        public string AssignedTo { get; set; }

        public string RateQuotedBy { get; set; }

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


        public string PaymentMode { get; set; }

        public string UTRTransactionNumber { get; set; }

        public int IsMaterialType { get; set; }

        public string District { get; set; }
        public string ZipCode { get; set; }
        public string RequestType { get; set; }
        public List<string> Attachments { get; set; } = new List<string>();
        public List<Material> Materials { get; set; }

        public List<Comment> comments { get; set; }

        public string Option1Day { get; set; }
        public string Option1Time { get; set; }
        public string Option2Day { get; set; }
        public string Option2Time { get; set; }

        public string OrderId { get; set; }

        public string OrderDate { get; set; }

        public string PaidAmount { get; set; }

        public string TransactionStatus { get; set; }

        public string TransactionType { get; set; }

        public string InvoiceId { get; set; }


        public string InvoiceURL { get; set; }


    }


    public class Material
    {
        public string material { get; set; }
        public string Quantity { get; set; }
    }

    public class Comment
    {
        public DateTime UpdatedDate { get; set; }
        public string CommentText
        {
            get; set;
        }



    }
}








