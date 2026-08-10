namespace OtpAuthServices.Model
{
    public class UploadFileRequest
    {
       
        public required IFormFile FileContent { get; set; }

        public required string UserId { get; set; }

        public ProfileTypes Type { get; set; }


        public enum ProfileTypes
        {
            Customer = 0,
            Dealer = 1,
            Estimator = 2,
            Builder = 3,
            Technician = 4



        }

    }
}
