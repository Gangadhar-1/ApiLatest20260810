namespace OtpAuthServices.Model
{
    public class DeliveryPartner
    {

        public string id { get; set; }

        public string UserId { get; set; }
        public string Date { get; set; }
        public string DeliveryPartnerId { get; set; }

        public string DeliveryPartnerName { get; set; }

       

        public List<string> Photo { get; set; } = new List<string>();

        public string Address { get; set; }

        public string state { get; set; }

        public string district { get; set; }


        public string Zipcode { get; set; }

        public string PhoneNumber { get; set; }

        public string pancardNumber { get; set; }


        public List<string> pancardAttachment { get; set; } = new List<string>();


        public List<string> AadharAttachment { get; set; } = new List<string>();

        public string AadharCardNumber { get; set; }


        public List<string> DrivingLicense { get; set; } = new List<string>();
        public string DrivingLicenseNumber { get; set; }

        public string Status { get; set; }

        public string   AssignedTo { get; set; }
        public bool IsRegistered { get; set; }
        public bool IsPickup { get; set; }
        public bool IsDelivered { get; set; }







    }
}
