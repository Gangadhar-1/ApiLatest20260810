using Bogus;
using OtpAuthServices.Model;
using System;
using System.Collections.Generic;

namespace OtpAuthServices.Services
{
    public class DataGeneratorService
    {
        private readonly Faker _faker;

        public DataGeneratorService()
        {
            _faker = new Faker(); // This will be used to generate fake data
        }

        // Method to generate a list of dynamic UserOnBoarding data
        //public List<UserOnBoarding> GenerateUsers(int count)
        //{
        //    var users = new List<UserOnBoarding>();

        //    for (int i = 0; i < count; i++)
        //    {
        //        var user = new UserOnBoarding
        //        {
        //            UserId = Guid.NewGuid().ToString(),
        //            id = Guid.NewGuid().ToString(),
        //            UserName = _faker.Internet.UserName(),
        //            UserPassword = _faker.Internet.Password(),
        //            MobileNo = _faker.Phone.PhoneNumber(),
        //            EmailId = _faker.Internet.Email(),
        //            IsMobileNumberValidate = _faker.Random.Bool(),
        //            IsEmailValidate = _faker.Random.Bool(),
        //            ProfileType = _faker.Random.ArrayElement(new[] { "Admin", "User", "Guest" })
        //        };

        //        users.Add(user);
        //    }

        //    return users;
        //}

        // Method to generate a list of dynamic Customer data
        //public List<Customer> GenerateCustomers(int count, List<Guid> userIds)
        //{
        //    var customers = new List<Customer>();

        //    for (int i = 0; i < count; i++)
        //    {
        //        var customer = new Customer
        //        {
        //            CustomerId = Guid.NewGuid(),
        //            id = Guid.NewGuid().ToString(),
        //            FirstName = _faker.Name.FirstName(),
        //            LastName = _faker.Name.LastName(),
        //            MobileNumber = _faker.Phone.PhoneNumber(),
        //            MobileVerificationCode = _faker.Random.Int(100000, 999999).ToString(),
        //            EmailAddress = _faker.Internet.Email(),
        //            EmailVerificationCode = _faker.Random.Int(100000, 999999).ToString(),
        //            AlternativeMobileNumber = _faker.Phone.PhoneNumber(),
        //            GSTNumber = _faker.Random.AlphaNumeric(15), // Fake GST number
        //            Address = _faker.Address.StreetAddress(),
        //            Landmark = _faker.Address.SecondaryAddress(),
        //            State = _faker.Address.State(),
        //            District = _faker.Address.City(),
        //            ZipCode = _faker.Address.ZipCode(),
        //            CustomerPhotoId = _faker.Random.AlphaNumeric(10), // Example photo ID
        //            UserId = userIds[i % userIds.Count] // Bind customer to a random user
        //        };

        //        customers.Add(customer);
        //    }

        //    return customers;
        //}
    }
}
