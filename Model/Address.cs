namespace OtpAuthServices.Model
{
    public class AddressModel
    {

        public string id { get; set; }

        public string ProfileType { get; set; } = null;
        public string AddressId { get; set; }
        public bool? IsPrimaryAddress { get; set; }

        public string Address { get; set; }

        public string State { get; set; }

        public string District { get; set; }


        public string ZipCode { get; set; }
        public  string MobileNumber { get; set; }
        public string EmailAddress { get; set; }

        public string StateId { get; set; }

        public string DistrictId { get; set; }

        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName { get; set; }

        public string WalletAmount { get; set; }
    }
}
