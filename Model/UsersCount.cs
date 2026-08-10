using Microsoft.AspNetCore.Mvc;

namespace OtpAuthServices.Model
{
    public class UsersCount
    {

        [BindProperty]
        public int TotalCount { get; set; }
        [BindProperty]
        public int CustomerCount { get; set; }
        //[BindProperty]
        //public int TechnicalAgencyCount { get; set; }

        [BindProperty]
        public int TechnicianCount { get; set; }

        [BindProperty]
        public int DealerCount { get; set; }

        [BindProperty]
        public int BuilderCount { get; set; }

        [BindProperty]
        public int EstimatorCount { get; set; }

        public string State { get; set; }

        public string District { get; set; }


        public  string ZipCode { get; set; }
        public void CalculateTotalCount()
        {
            TotalCount = CustomerCount + DealerCount + BuilderCount + TechnicianCount + EstimatorCount;
        }

      

    }

}
