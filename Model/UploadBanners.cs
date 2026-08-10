namespace OtpAuthServices.Model
{
    public class UploadBanners
    {

        public string id { get; set; }

        public string Title { get; set; }

        public DateTime CreatedDate { get; set; }

        public  DateTime UpdatedDate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }


        public string Description { get; set; }
        public List<Image> Image { get; set; } = new List<Image>();
    }

    public class Image
    {
        public string Images { get; set; }
    }

}
