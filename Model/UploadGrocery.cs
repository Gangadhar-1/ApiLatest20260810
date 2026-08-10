namespace OtpAuthServices.Model
{
    public class UploadGrocery
    {
        public string id { get; set; }

        public string Date { get; set; }


        public string GroceryItemId { get; set; }

       public string Name { get; set; } 

        public string Category { get; set; }

        public List<string> Images { get; set; }=new List<string>();    

        public string MRP { get; set; }

        public string Discount { get; set; }

        public string AfterDiscount { get; set; }

        public string StockLeft { get; set; }

        public string DeliveryIn { get; set; }


        public string RequestedBy { get; set; }
        public string Status { get; set; }  
        public string Code { get; set; }
        
        public string Units { get; set; }

        public string Limit { get; set; }

        //public string selectedZipcodes { get; set; }

    }
}
