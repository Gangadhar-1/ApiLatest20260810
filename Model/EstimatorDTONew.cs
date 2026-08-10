using Newtonsoft.Json;

namespace OtpAuthServices.Model
{
    public class EstimatorDTONew
    {
        [JsonProperty("estimatorId")]
        public Guid EstimatorId { get; set; }

        
        [JsonProperty("estimatorName")]
        public string EstimatorName { get; set; }

       
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

       
        [JsonProperty("estimatorPhotoId")]
        public string EstimatorPhotoId { get; set; }

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
