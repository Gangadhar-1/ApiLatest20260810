using System.ComponentModel.DataAnnotations;

namespace OtpAuthServices.Models
{
    public class Product
    //    {
    //        public string id { get; set; } // Unique ID for Cosmos DB
    //        public string ProductName { get; set; }
    //        public decimal Cost { get; set; }
    //        public string Description { get; set; }
    //        public List<string> ImageUrls { get; set; } = new List<string>(); // List of image URLs
    //        public string Category { get; set; } // Partition key, e.g., category of product

    //        public 
    //    }
    //}
    
    {
            
            public string id { get; set; } // Auto-generated unique identifier
             public string  ProductId { get; set; }
             public string  DeliveryInDays { get; set; }    

             public string  NumberOfStockAvailable  { get; set; }  

            public  DateTime Date { get; set; }

            [Required(ErrorMessage = "Category is required.")]
            public string Category { get; set; }

            [Required(ErrorMessage = "Product Name  is required.")]
            public string ProductName { get; set; }
  
            [Required(ErrorMessage = "Catalogue is required.")]
            public string Catalogue { get; set; }

            [Required(ErrorMessage = "Product size is required.")]
            public string ProductSize { get; set; }

            public string Color { get; set; } // Optional field

            [Required(ErrorMessage = "Units are required.")]
            public string Units { get; set; }

            public List<string> ProductPhotos { get; set; } // File names or URLs for uploaded photos

            [Range(0, double.MaxValue, ErrorMessage = "Rate must be a positive number.")]
            public decimal Rate { get; set; }

            [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100.")]
            public double Discount { get; set; }

            [Required(ErrorMessage = "Product specifications are required.")]
            public List<Specification> Specifications { get; set; }

            public string SpecificationDesc { get; set; } // Optional additional specification description

            public string Warranty { get; set; } // Optional field for warranty period

            public string AdditionalInformation { get; set; } 

            public string ProductStatus { get; set; }

            public string ProductOwnedBy { get; set; }

              

        }

        //public class Category
        //{
        //    public int Id { get; set; }

        //    [Required]
        //    public string Name { get; set; }

        //    public List<Product> Products { get; set; } = new List<Product>();
        //}

        //public class Catalogue
        //{
        //    public int Id { get; set; }

        //    [Required]
        //    public string Name { get; set; }

        //    public List<Product> Products { get; set; } = new List<Product>();
        //}
    public class Specification
    {
        [Required(ErrorMessage = "Specification label is required.")]
        public string Label { get; set; }

        [Required(ErrorMessage = "Specification value is required.")]
        public string Value { get; set; }
    }
    //public class ProductCatalog
    //    {
    //        public List<Product> Products { get; set; } = new List<Product>();
    //        public List<Category> Categories { get; set; } = new List<Category>();
    //        public List<Catalogue> Catalogues { get; set; } = new List<Catalogue>();
    //    }
    }
