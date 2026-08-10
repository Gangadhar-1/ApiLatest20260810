using Microsoft.Azure.Cosmos.Core.Networking;

namespace OtpAuthServices.Model
{
    public class Lakshmincollection
    {

        public string id {  get; set; } 

        public string LakshmicollectionId { get; set; }
        public string Date { get; set; }
        public string ProductName { get; set; }

        public string RequestedBy { get; set; }

        public string Status {  get; set; } 

        public string Category { get; set; }

        public string Catalogue { get; set; }

        public string size { get; set; }

        public string colour { get; set; }

        public List<string> Images { get; set; }=new List<string>();

        public List<string> videos { get; set; } = new List<string>();  

        public string Rate { get; set; }

        public string Discount { get; set; }

         public string AfterDiscount { get; set; }  


        public List<Description> Descriptions { get; set; }  =new List<Description>();   
        public string Optional {  get; set; }   

        public string MoreInfo { get; set; }    

        public string DeliveryInDays    { get; set; }

        public string StockLeft     { get; set; }
}
public class  Description
    {
        public string Name { get; set; }    

        public string value     { get; set; }


    }
}
