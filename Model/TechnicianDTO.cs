using Newtonsoft.Json;

namespace OtpAuthServices.Model
{
    public class TechnicianDTO
    {

        [JsonProperty("technicianId")]
        public Guid TechnicianId { get; set; }

        [JsonProperty("technicianFullName")]
        public string TechnicianFullName { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("zipCode")]
        public string ZipCode { get; set; }

        [JsonProperty("technicianPhotoId")]
        public string TechnicianPhotoId { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("IsActive")]
        public bool IsActive { get; set; }

        [JsonProperty("StateId")]
        public string StateId { get; set; }

        [JsonProperty("DistrictId")]
        public string DistrictId { get; set; }

    }
}
