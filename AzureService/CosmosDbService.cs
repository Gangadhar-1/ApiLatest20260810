using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Writers;
using Newtonsoft.Json;
using OtpAuthServices.Controllers;
using OtpAuthServices.Model;
using OtpAuthServices.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static OtpAuthServices.Controllers.MarkMessageSeenController;
using Formatting = Newtonsoft.Json.Formatting;


namespace OtpAuthServices.AzureService
{
    public class CosmosDbService<T> : ICosmosDbService<T> where T : class
    {

        private readonly Container _container;
        private QueryDefinition queryDefinition;

        public CosmosDbService(CosmosClient cosmosClient, string databaseName, string containerName)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        

        public async Task UpsertItemAsync(OtpCache otpData)
        {
            await _container.UpsertItemAsync(
                otpData,
                new PartitionKey(otpData.senderValue)
            );
        }

        // Create (Add) an item in Cosmos DB
        public async Task AddItemAsync(T item)
        {
            try
            {
                await _container.CreateItemAsync(item, new PartitionKey(GetPartitionKey(item)));
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error adding item: {ex.Message}");
                throw; // Optional: rethrow the exception if needed
            }
        }

        public async Task<T> GetItemAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<T>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Item not found: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading item: {ex.Message}");
                throw;
            }
        }

        // Read all items
        public async Task<IEnumerable<T>> GetItemsAsync(string query = null)
        {
            var items = new List<T>();

            try
            {
                var iterator = _container.GetItemQueryIterator<T>(new QueryDefinition(query ?? "SELECT * FROM c"));
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    items.AddRange(response.ToList());
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error reading items: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error reading items: {ex.Message}");
            }

            return items;
        }

        // Update an item
        public async Task UpdateItemAsync(T item)
        {
            var id = GetId(item);
            if (id == null)
            {
                throw new ArgumentException("Item does not have a valid ID.");
            }

            try
            {
                await _container.ReplaceItemAsync(item, id, new PartitionKey(GetPartitionKey(item)));
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error updating item: {ex.Message}");
                throw;
            }
        }

        // Delete an item
        public async Task DeleteItemAsync(string id)
        {
            try
            {
                await _container.DeleteItemAsync<T>(id, new PartitionKey(id));
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error deleting item: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateItemAsync(string id, T item, string partitionKey)
        {
            await _container.UpsertItemAsync(item, new PartitionKey(partitionKey));
        }

        public async Task DeleteItemAsync(string id, string partitionKey)
        {
            await _container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey));
        }

        // Helper to get the partition key value from the item
        private string GetPartitionKey(T item)
        {
            var property = item.GetType().GetProperty("id"); // Assuming "Id" is the partition key field
            return property?.GetValue(item)?.ToString();
        }


        // Helper to get the ID from the item for updates
        private string GetId(T item)
        {
            var property = item.GetType().GetProperty("id");
            return property?.GetValue(item)?.ToString();
        }

        // Get user by either MobileNumber or EmailAddress
        public async Task<T> GetUserByEmailOrMobileAsync(string value)
        {
            try
            {
                // Check if mobile number or email address is provided, and build the query
                string query = "SELECT * FROM c WHERE ";
                bool hasCondition = false;

                if (!string.IsNullOrEmpty(value))
                {
                    query += "c.MobileNo = @value";
                    hasCondition = true;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    if (hasCondition)
                    {
                        query += " OR ";
                    }
                    query += "c.EmailId = @value";
                }

                // Execute the first query to get the id based on either mobile number or email address
                var queryDefinition = new QueryDefinition(query)
                    .WithParameter("@value", value ?? value);

                var iterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);
                var response = await iterator.ReadNextAsync();

                // If no results found, return null
                if (!response.Any())
                {
                    return null;
                }

                // Extract the ID from the result
                var userId = response.FirstOrDefault().id.ToString();

                // Query the full user object by ID
                var user = await GetItemAsync(userId);

                return user;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying user: {ex.Message}");
                return null;
            }
        }

        // Get user by useName
        public async Task<T> GetUserByUserIdAsync(string username)
        {
            try
            {
                string query = "SELECT * FROM c WHERE c.UserName = @username";

                var queryDefinition = new QueryDefinition(query)
                    .WithParameter("@username", username);

                var iterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);
                var response = await iterator.ReadNextAsync();

                if (!response.Any())
                {
                    return null;
                }

                // Deserialize the first item in the response to a generic type T
                var userJson = response.FirstOrDefault().ToString();
                var user = JsonConvert.DeserializeObject<T>(userJson);

                return user;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying user: {ex.Message}");
                return null;
            }
        }

        public async Task<T> GetUserProflie(string value, string profileType)
        {
            try
            {
                // Check if mobile number or email address is provided, and build the query
                string query = "SELECT * FROM c WHERE ";
                bool hasCondition = true;

                if (!string.IsNullOrEmpty(value))
                {
                    query += "c.UserId = @value and c.FirstName !=null";
                    hasCondition = true;
                }



                // Execute the first query to get the id based on either mobile number or email address
                var queryDefinition = new QueryDefinition(query)
                    .WithParameter("@value", value);

                var iterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);
                var response = await iterator.ReadNextAsync();

                // If no results found, return null
                if (!response.Any())
                {
                    return null;
                }

                // Extract the ID from the result
                var userdata = response.FirstOrDefault();

                var customer = JsonConvert.DeserializeObject<Customer>(userdata.ToString());

                return customer;

            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying user: {ex.Message}");
                return null;
            }
        }





        public async Task<List<CustomerDTO>> GetCustomerDirectoryDetails(
      string searchQuery = null,
      string firstname = null,
      string State = null,
      string District = null,
      string ZipCode = null

     )
        {
            try
            {
                // Base query
                var query = @"
SELECT 
    c.CustomerId,
    c.FirstName,
    c.LastName,
    c.MobileNumber,
    c.EmailAddress,
    c.Address,
    c.State,
    c.District,
    c.ZipCode,
    c.CustomerPhotoId,
    c.Status,
    c.StateId,
    c.DistrictId
FROM c
WHERE 1=1 AND c.CustomerId !=null AND c.FirstName !=null";

                if (!string.IsNullOrEmpty(firstname))
                    query += " AND (LOWER(c.FirstName) = LOWER(@firstname) OR c.FirstName=@firstname)";

                if (!string.IsNullOrEmpty(State))
                    query += " AND (LOWER(c.State) = LOWER(@State) OR c.StateId = @State)";

                if (!string.IsNullOrEmpty(District))
                    query += " AND (LOWER(c.District) = LOWER(@District) OR c.DistrictId = @District)";

                if (!string.IsNullOrEmpty(ZipCode))
                    query += " AND c.ZipCode = @ZipCode";

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query += @"
    AND (
        CONTAINS(c.FirstName, @searchQuery, true) OR 
        CONTAINS(c.LastName, @searchQuery, true) OR 
        CONTAINS(c.Address, @searchQuery, true) OR
        CONTAINS(c.EmailAddress, @searchQuery, true) OR
        CONTAINS(c.CustomerPhotoId, @searchQuery, true) OR
        CONTAINS(c.Status, @searchQuery, true)
    )";
                }

                query += " order by c.timestamp desc";

                // Create query definition
                var queryDefinition = new QueryDefinition(query);
                if (!string.IsNullOrEmpty(firstname))
                    queryDefinition = queryDefinition.WithParameter("@firstname", firstname);

                if (!string.IsNullOrEmpty(State))
                    queryDefinition = queryDefinition.WithParameter("@State", State);

                if (!string.IsNullOrEmpty(District))
                    queryDefinition = queryDefinition.WithParameter("@District", District);

                if (!string.IsNullOrEmpty(ZipCode))
                    queryDefinition = queryDefinition.WithParameter("@ZipCode", ZipCode);

                if (!string.IsNullOrEmpty(searchQuery))
                    queryDefinition = queryDefinition.WithParameter("@searchQuery", searchQuery);

                // Execute the query
                var queryIterator = _container.GetItemQueryIterator<CustomerDTO>(queryDefinition);
                var results = new List<CustomerDTO>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;

            }
            catch (CosmosException ex)
            {
                // Handle Cosmos DB-specific exceptions
                Console.WriteLine($"Cosmos DB Error: {ex.StatusCode} - {ex.Message}");
                return new List<CustomerDTO>();
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                Console.WriteLine($"General Error: {ex.Message}");
                return new List<CustomerDTO>();
            }
        }



        public async Task<T> GetDealerProflie(string value, string profileType)
        {
            try
            {
                // Check if mobile number or email address is provided, and build the query
                string query = "SELECT * FROM c WHERE ";
                bool hasCondition = true;

                if (!string.IsNullOrEmpty(value))
                {
                    query += "c.UserId = @value and c.DealerFirmName !=null";
                    hasCondition = true;
                }



                // Execute the first query to get the id based on either mobile number or email address
                var queryDefinition = new QueryDefinition(query)
                    .WithParameter("@value", value);

                var iterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);
                var response = await iterator.ReadNextAsync();

                // If no results found, return null
                if (!response.Any())
                {
                    return null;
                }

                // Extract the ID from the result
                var userdata = response.FirstOrDefault();

                var dealer = JsonConvert.DeserializeObject<Dealer>(userdata.ToString());

                return dealer;


                // Query the full user object by ID
                // var user = await GetItemAsync(userdata);

                // return userdata;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying user: {ex.Message}");
                return null;
            }
        }




        public async Task<T> GetEstimatorProflie(string value, string profileType)
        {
            try
            {
                // Check if mobile number or email address is provided, and build the query
                string query = "SELECT * FROM c WHERE ";
                bool hasCondition = true;

                if (!string.IsNullOrEmpty(value))
                {
                    query += "c.UserId = @value and c.EstimatorFirmName !=null";
                    hasCondition = true;
                }



                // Execute the first query to get the id based on either mobile number or email address
                var queryDefinition = new QueryDefinition(query)
                    .WithParameter("@value", value);

                var iterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);
                var response = await iterator.ReadNextAsync();

                // If no results found, return null
                if (!response.Any())
                {
                    return null;
                }

                // Extract the ID from the result
                var userdata = response.FirstOrDefault();

                var estimator = JsonConvert.DeserializeObject<Estimator>(userdata.ToString());

                return estimator;


                // Query the full user object by ID
                // var user = await GetItemAsync(userdata);

                // return userdata;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying user: {ex.Message}");
                return null;
            }
        }




        public async Task<T> GetTechnicianProflie(string value, string profileType)
        {
            try
            {
                // Check if mobile number or email address is provided, and build the query
                string query = "SELECT * FROM c WHERE ";
                bool hasCondition = true;

                if (!string.IsNullOrEmpty(value))
                {
                    query += "c.UserId = @value and c.TechnicianFullName!=null";
                    hasCondition = true;
                }



                // Execute the first query to get the id based on either mobile number or email address
                var queryDefinition = new QueryDefinition(query)
                    .WithParameter("@value", value);

                var iterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);
                var response = await iterator.ReadNextAsync();

                // If no results found, return null
                if (!response.Any())
                {
                    return null;
                }

                // Extract the ID from the result
                var userdata = response.FirstOrDefault();

                var technician = JsonConvert.DeserializeObject<Technician>(userdata.ToString());

                return technician;



                // Query the full user object by ID
                // var user = await GetItemAsync(userdata);

                // return userdata;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying user: {ex.Message}");
                return null;
            }
        }






        public async Task<T> GetBuilderProflie(string value, string profileType)
        {
            try
            {
                // Check if mobile number or email address is provided, and build the query
                string query = "SELECT * FROM c WHERE ";
                bool hasCondition = true;

                if (!string.IsNullOrEmpty(value))
                {
                    query += "c.UserId = @value and c.BuilderFirmName !=null";
                    hasCondition = true;
                }



                // Execute the first query to get the id based on either mobile number or email address
                var queryDefinition = new QueryDefinition(query)
                    .WithParameter("@value", value);

                var iterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);
                var response = await iterator.ReadNextAsync();

                // If no results found, return null
                if (!response.Any())
                {
                    return null;
                }

                // Extract the ID from the result
                var userdata = response.FirstOrDefault();

                var builder = JsonConvert.DeserializeObject<Builder>(userdata.ToString());

                return builder;

                // Query the full user object by ID
                // var user = await GetItemAsync(userdata);

                // return userdata;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying user: {ex.Message}");
                return null;
            }
        }


        public async Task<List<T>> GetRaiseTicketsAsync(string customerId)
        {
            try
            {
                // Define query with parameter
                var queryDefinition = new QueryDefinition("1")
                    .WithParameter("@customerId", customerId);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);
                var tickets = new List<T>();

                // Iterate through results and add each item to the list
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    tickets.AddRange(response); // Add all items from the current batch
                }

                // Return the list of tickets
                return tickets;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
                // Handle error and return an empty list
                return new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
                // Handle error and return an empty list
                return new List<T>();
            }
        }

        public async Task<T> GetUserByLogin(string username, string password)
        {
            try
            {
                // Check if username and password are provided, and build the query
                string query = "SELECT * FROM c WHERE ";
                List<string> conditions = new List<string>();
                Dictionary<string, object> parameters = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(username))
                {
                    conditions.Add("c.UserName = @username");
                    parameters.Add("@username", username);
                }

                if (!string.IsNullOrEmpty(password))
                {
                    conditions.Add("c.UserPassword = @password");
                    parameters.Add("@password", password);
                }

                if (conditions.Count == 0)
                {
                    // If no conditions are set (both username and password are empty), return null or throw an exception
                    return null;
                }

                query += string.Join(" AND ", conditions);

                // Create the query definition with multiple parameters
                var queryDefinition = new QueryDefinition(query);
                foreach (var param in parameters)
                {
                    queryDefinition = queryDefinition.WithParameter(param.Key, param.Value);
                }

                // Execute the query
                var iterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);
                var response = await iterator.ReadNextAsync();

                // Extract the user data from the result
                var userdata = response.FirstOrDefault();
                if (userdata == null)
                {
                    return null;  // No user found
                }

                // Deserialize the user object
                var user = JsonConvert.DeserializeObject<UserOnBoarding>(userdata.ToString());

                return user;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying user: {ex.Message}");
                return null;
            }
        }


        //public  async Task<Dictionary<string,int>> GetAllRaiseTicketsCounts()
        //{
        //    try
        //    {
        //        string[] TicketStatus = { "open", "Assigned", "Pending","Not Assigned" };
        //        var ticketscounts = new Dictionary<string, int>();
        //        foreach (string ticket in TicketStatus) {

        //            string query = "select value count (1)  from c where  c.internalStatus=@internalStatus";
        //            var queryDefinition = new QueryDefinition(query)
        //            .WithParameter("@internalStatus", TicketStatus);

        //            var iterator = _container.GetItemQueryIterator<int>(queryDefinition);

        //            int count = 0;

        //            while (iterator.HasMoreResults)
        //            {
        //                var response = await iterator.ReadNextAsync();
        //                count += response.FirstOrDefault();
        //            }

        //            ticketscounts[ticket] = count;
        //        }
        //        return ticketscounts;

        //    }
        //    catch(CosmosException ex)
        //    {
        //        Console.WriteLine($"Error querying user count: {ex.Message}");
        //        return null; // Return null to indicate an error
        //    }



        //}


        public async Task<Dictionary<string, int>> GetAllRaiseTicketsCounts()
        {
            try
            {
                string[] ticketStatus = { "Open", "Assigned", "Pending", "Closed" };
                var ticketsCounts = new Dictionary<string, int>();

                foreach (string ticket in ticketStatus)
                {
                    // Modify the query for each status
                    string query = "SELECT VALUE COUNT(1) FROM c WHERE c.internalStatus = @internalStatus";
                    var queryDefinition = new QueryDefinition(query)
                        .WithParameter("@internalStatus", ticket); // Set the status for each iteration

                    var iterator = _container.GetItemQueryIterator<int>(queryDefinition);
                    int count = 0;

                    // Read the result
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        count += response.FirstOrDefault(); // Add the count to the result
                    }

                    // Add the count for the current status to the dictionary
                    ticketsCounts[ticket] = count;
                }

                return ticketsCounts;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying ticket counts: {ex.Message}");
                return null; // Return null if there's an error
            }
        }

        public async Task<Dictionary<string, int>> GetAllUsersCountAsync()
        {
            try
            {
                // Define profile types (case-insensitive)
                string[] profileTypes = { "dealer", "estimator", "technician", "builder", "customer" };
                var userCounts = new Dictionary<string, int>();

                // Loop over each profile type
                foreach (var profileType in profileTypes)
                {
                    // Cosmos DB query to count users by profile type (case-insensitive)
                    string query = "SELECT VALUE COUNT(1) FROM c WHERE LOWER(c.ProfileType) = @ProfileType ";
                    var queryDefinition = new QueryDefinition(query)
                        .WithParameter("@ProfileType", profileType.ToLower()); // Pass the lowercase profile type as a parameter

                    var iterator = _container.GetItemQueryIterator<int>(queryDefinition);

                    int count = 0;

                    // Read all results from the query iterator (Cosmos DB paginates results)
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        count += response.FirstOrDefault(); // COUNT queries return a single integer value
                    }

                    // Add the lowercase profile type count to the dictionary
                    userCounts[profileType.ToLower()] = count;
                }

                return userCounts;
            }
            catch (CosmosException ex)
            {
                // Handle Cosmos DB exception
                Console.WriteLine($"Error querying user count: {ex.Message}");
                return null; // Return null to indicate an error
            }
        }



        public async Task<Dictionary<string, int>> GetAllUsersCountByStateAsync(string state)
        {
            try
            {

                // List of fields to check for non-null values
                var fieldsToCheck = new[] { "EstimatorId", "TechnicianId", "BuilderId", "CustomerId", "DealerId" };
                var userCounts = new Dictionary<string, int>();

                // Loop through each field and construct a query for each one
                foreach (var field in fieldsToCheck)
                {
                    // Dynamically construct the query for each field and State
                    string query = $"SELECT VALUE COUNT(1) FROM c WHERE c.{field} != null AND c.StateId = @State";

                    // Define the query with the State parameter
                    var queryDefinition = new QueryDefinition(query)
                        .WithParameter("@State", state);

                    var iterator = _container.GetItemQueryIterator<int>(queryDefinition);

                    int count = 0;
                    // Read the results from the iterator
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        count += response.FirstOrDefault();
                    }

                    // Store the count for the current field
                    userCounts[field] = count;
                }

                return userCounts;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying user count: {ex.Message}");
                return null; // Return null to indicate an error
            }
        }



        public async Task<Dictionary<string, int>> GetAllUsersCountByStateAndDistrictAsync(string state, string district)
        {
            try
            {

                // List of fields to check for non-null values
                var fieldsToCheck = new[] { "EstimatorId", "TechnicianId", "BuilderId", "CustomerId", "DealerId" };
                var userCounts = new Dictionary<string, int>();

                // Loop through each field and construct a query for each one
                foreach (var field in fieldsToCheck)
                {
                    // Dynamically construct the query for each field and State
                    string query = $"SELECT VALUE COUNT(1) FROM c WHERE c.{field} != null AND c.StateId = @State  AND c.DistrictId=@district";

                    // Define the query with the State parameter
                    var queryDefinition = new QueryDefinition(query)
                        .WithParameter("@State", state)
                         .WithParameter("@district", district);

                    var iterator = _container.GetItemQueryIterator<int>(queryDefinition);

                    int count = 0;
                    // Read the results from the iterator
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        count += response.FirstOrDefault(); // COUNT queries return a single integer value
                    }

                    // Store the count for the current field
                    userCounts[field] = count;
                }

                return userCounts;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying user count: {ex.Message}");
                return null; // Return null to indicate an error
            }
        }

        public async Task<Dictionary<string, int>> GetAllUsersCountByStateAndDistrictAndZipcodeAsync(string state, string district, string zipcode)
        {
            try
            {

                // List of fields to check for non-null values
                var fieldsToCheck = new[] { "EstimatorId", "TechnicianId", "BuilderId", "CustomerId", "DealerId" };
                var userCounts = new Dictionary<string, int>();

                // Loop through each field and construct a query for each one
                foreach (var field in fieldsToCheck)
                {
                    // Dynamically construct the query for each field and State
                    string query = $"SELECT VALUE COUNT(1) FROM c WHERE c.{field} != null  and c.Address !=null and  c.Status !=null AND c.StateId = @State  AND c.DistrictId=@district  AND c.ZipCode=@zipcode";
                    // Define the query with the State parameter
                    var queryDefinition = new QueryDefinition(query)
                        .WithParameter("@State", state)
                         .WithParameter("@district", district)
                         .WithParameter("@zipcode", zipcode);
                    var iterator = _container.GetItemQueryIterator<int>(queryDefinition);

                    int count = 0;
                    // Read the results from the iterator
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        count += response.FirstOrDefault(); // COUNT queries return a single integer value
                    }

                    // Store the count for the current field
                    userCounts[field] = count;
                }

                return userCounts;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying user count: {ex.Message}");
                return null; // Return null to indicate an error
            }
        }




        public async Task<Dictionary<string, int>> GetRaiseTicketCountAsync()
        {
            try
            {
                // Define the status types you're interested in
                string[] statusTypes = { "Open", "pending", "closed", "NotAssigned" };
                var ticketCounts = new Dictionary<string, int>(); // Dictionary to store counts for each status
                int totalCount = 0; // Variable to store the sum of all counts

                foreach (var statusType in statusTypes)
                {
                    // Cosmos DB query to get the count for each status
                    string query = "SELECT VALUE COUNT(1) FROM c WHERE c.TicketId != null AND c.status = @StatusType";
                    var queryDefinition = new QueryDefinition(query)
                        .WithParameter("@StatusType", statusType);

                    var iterator = _container.GetItemQueryIterator<int>(queryDefinition);

                    int count = 0;
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        count += response.FirstOrDefault(); // COUNT queries return a single integer value
                    }

                    // Store the count for this status
                    ticketCounts[statusType] = count;

                    // Add the count for this status to the total count
                    totalCount += count;
                }

                // Optionally, add the total count to the dictionary if needed
                ticketCounts["Total"] = totalCount;

                return ticketCounts;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying ticket counts: {ex.Message}");
                return null; // Return null to indicate an error
            }
        }



        public async Task<Dictionary<string, int>> GetRaiseTicketCountByStateAsync(string state)
        {
            try
            {
                // Define the status types you're interested in
                string[] statusTypes = { "Open", "pending", "closed", "NotAssigned" };
                var ticketCounts = new Dictionary<string, int>(); // Dictionary to store counts for each status
                int totalCount = 0; // Variable to store the sum of all counts

                foreach (var statusType in statusTypes)
                {
                    // Cosmos DB query to get the count for each status
                    string query = "SELECT VALUE COUNT(1) FROM c WHERE c.TicketId != null AND c.status = @StatusType  AND c.State=@state";
                    var queryDefinition = new QueryDefinition(query)
                        .WithParameter("@StatusType", statusType);

                    var iterator = _container.GetItemQueryIterator<int>(queryDefinition);

                    int count = 0;
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        count += response.FirstOrDefault(); // COUNT queries return a single integer value
                    }

                    // Store the count for this status
                    ticketCounts[statusType] = count;

                    // Add the count for this status to the total count
                    totalCount += count;
                }

                // Optionally, add the total count to the dictionary if needed
                ticketCounts["Total"] = totalCount;

                return ticketCounts;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying ticket counts: {ex.Message}");
                return null; // Return null to indicate an error
            }
        }

        public async Task<List<T>> GetProductList(string ProductOwnedBy)
        {
            try
            {

                if (string.IsNullOrEmpty(ProductOwnedBy))
                {
                    throw new ArgumentNullException(nameof(ProductOwnedBy), "ProductOwnedBy Cannot be null or Empty");
                }






                var queryDefinition = new QueryDefinition(

                    "select * from c where  c.ProductOwnedBy =@ProductOwnedBy")

                   .WithParameter("@ProductOwnedBy", ProductOwnedBy);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var product = new List<T>();

                while (queryIterator.HasMoreResults)
                {

                    var response = await queryIterator.ReadNextAsync();
                    product.AddRange(response);
                }





                return product;
            }

            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }



        public async Task<List<T>> GetAdminProductList()
        {
            try
            {

                //if (string.IsNullOrEmpty(ProductOwnedBy))
                //{
                //    throw new ArgumentNullException(nameof(ProductOwnedBy), "ProductOwnedBy Cannot be null or Empty");
                //}






                var queryDefinition = new QueryDefinition(

                    "select * from c where  c.ProductOwnedBy !=null");



                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var product = new List<T>();

                while (queryIterator.HasMoreResults)
                {

                    var response = await queryIterator.ReadNextAsync();
                    product.AddRange(response);
                }





                return product;
            }

            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }





        public async Task<List<T>> GetAllProductList()
        {
            try
            {

                //if (string.IsNullOrEmpty(ProductOwnedBy))
                //{
                //    throw new ArgumentNullException(nameof(ProductOwnedBy), "ProductOwnedBy Cannot be null or Empty");
                //}






                var queryDefinition = new QueryDefinition(

                    "select * from c where  c.ProductOwnedBy !=null and c.ProductStatus='Approved'  and c.Warranty !=null order by c.Date desc");



                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var product = new List<T>();

                while (queryIterator.HasMoreResults)
                {

                    var response = await queryIterator.ReadNextAsync();
                    product.AddRange(response);
                }





                return product;
            }

            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }






        public async Task<List<T>> GetCustomersDetailsByStatey(
    string state,
    string district,
    string zipCode,
    string fullname,
    string mobilleNumber,
    string userId)
        {
            try
            {
                var query = "SELECT * FROM c WHERE c.CustomerId != null  and c.FirstName !=null";

                var parameters = new List<Tuple<string, object>>();

                // Add conditions based on which parameters are not null
                if (!string.IsNullOrEmpty(state))
                {
                    query += " AND c.State = @state";
                    parameters.Add(new Tuple<string, object>("@state", state));
                }
                if (!string.IsNullOrEmpty(district))
                {
                    query += " AND c.District = @district";
                    parameters.Add(new Tuple<string, object>("@district", district));
                }
                if (!string.IsNullOrEmpty(zipCode))
                {
                    query += " AND c.ZipCode = @zipCode";
                    parameters.Add(new Tuple<string, object>("@zipCode", zipCode));
                }
                if (!string.IsNullOrEmpty(mobilleNumber))
                {
                    query += " AND c.MobileNumber = @mobilleNumber";
                    parameters.Add(new Tuple<string, object>("@mobilleNumber", mobilleNumber));
                }
                if (!string.IsNullOrEmpty(userId))
                {
                    query += " AND c.UserId = @userId";
                    parameters.Add(new Tuple<string, object>("@userId", userId));
                }
                if (!string.IsNullOrEmpty(fullname))
                {
                    query += " AND (CONCAT(c.FirstName, ' ', c.LastName) = @fullname OR CONCAT(c.FirstName, c.LastName) = @fullname)";
                    parameters.Add(new Tuple<string, object>("@fullname", fullname));
                }

                // Log the final query (for debugging purposes)
                Console.WriteLine("Final Query: " + query);

                // Prepare the query definition with dynamic parameters
                var queryDefinition = new QueryDefinition(query);
                foreach (var param in parameters)
                {
                    queryDefinition = queryDefinition.WithParameter(param.Item1, param.Item2);
                }

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);
                var customers = new List<T>();

                // Fetch all matching documents
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    customers.AddRange(response);
                }

                return customers;
            }
            catch (CosmosException ex)
            {
                // Handle Cosmos DB exceptions
                return new List<T>();
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return new List<T>();
            }
        }

        //public async Task<List<T>> GetCustomersDetailsByState(string state)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(state))
        //        {
        //            throw new ArgumentException(nameof(state), "state cannot be null or Empty.");
        //        }

        //        var queryDefinition = new QueryDefinition("SELECT * FROM c  where c.CustomerId !=null   and c.FirstName !=null  and c.State=@state")
        //            .WithParameter("@state", state);
        //        var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

        //        var customer = new List<T>();

        //        while (queryIterator.HasMoreResults)
        //        {
        //            var response = await queryIterator.ReadNextAsync();
        //            customer.AddRange(response);
        //        }
        //        return customer;
        //    }
        //    catch (CosmosException ex)
        //    {
        //        return new List<T>();
        //    }
        //    catch (Exception ex)
        //    {
        //        return new List<T>();
        //    }
        //}



        public async Task<List<T>> GetCustomerDetailsByIUserId(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException(nameof(userId), "UserId cannot be null or Empty.");
                }

                var queryDefinition = new QueryDefinition("SELECT * FROM c  where c.CustomerId !=null  and c.UserId=@userId")
                    .WithParameter("@userId", userId);
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var customer = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    customer.AddRange(response);
                }
                return customer;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }






        public async Task<List<T>> GetDealerDetailsByUserId(string userId)

        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException(nameof(userId), "UserId cannot be null or Empty");
                }
                var querDefinition = new QueryDefinition("SELECT * FROM c  where c.DealerId !=null  and c.UserId=@userId")
                    .WithParameter("@userId", userId);

                var queryIterator = _container.GetItemQueryIterator<T>(querDefinition);

                var dealer = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    dealer.AddRange(response);
                }
                return dealer;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }

        public async Task<List<T>> GetTechnicianDetailsByUserId(string userId)

        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException(nameof(userId), "UserId cannot be null or Empty");
                }
                var querDefinition = new QueryDefinition("SELECT * FROM c  where c.TechnicianId !=null  and c.UserId=@userId")
                    .WithParameter("@userId", userId);

                var queryIterator = _container.GetItemQueryIterator<T>(querDefinition);

                var dealer = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    dealer.AddRange(response);
                }
                return dealer;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }

        public async Task<List<T>> GetBuilderDetailsByUserId(string userId)

        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException(nameof(userId), "UserId cannot be null or Empty");
                }
                var querDefinition = new QueryDefinition("SELECT * FROM c  where c.BuilderId !=null  and c.UserId=@userId")
                    .WithParameter("@userId", userId);

                var queryIterator = _container.GetItemQueryIterator<T>(querDefinition);

                var dealer = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    dealer.AddRange(response);
                }
                return dealer;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }



        public async Task<List<T>> GetEstimatorDetailsByUserId(string userId)

        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException(nameof(userId), "UserId cannot be null or Empty");
                }
                var querDefinition = new QueryDefinition("SELECT * FROM c  where c.EstimatorId !=null  and c.UserId=@userId")
                    .WithParameter("@userId", userId);

                var queryIterator = _container.GetItemQueryIterator<T>(querDefinition);

                var dealer = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    dealer.AddRange(response);
                }
                return dealer;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }


        public async Task<List<T>> GetBuyProductdetails(string BuyProductId)
        {
            try
            {
                if (string.IsNullOrEmpty(BuyProductId))
                {
                    throw new ArgumentNullException(nameof(BuyProduct), "BuyProduct Cannot be null or empty.");
                }

                var queryDefinition = new QueryDefinition(
                    "select * from c  where c.BuyProductId=@BuyProductId")
                    .WithParameter("@BuyProductId", BuyProductId);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var buyProduct = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    buyProduct.AddRange(response);
                }

                return buyProduct;

            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }


        public async Task<Dictionary<string, int>> GetTotalCountsOfRaiseTicket()
        {
            {
                try
                {
                    // Define the status types you're interested in
                    string[] statusTypes = { "Open", "pending", "closed", "NotAssigned" };
                    var ticketCounts = new Dictionary<string, int>(); // Dictionary to store counts for each status
                    int totalCount = 0; // Variable to store the sum of all counts

                    foreach (var statusType in statusTypes)
                    {
                        // Cosmos DB query to get the count for each status
                        string query = "SELECT VALUE COUNT(1) FROM c WHERE c.RaiseTicketId != null  AND c.status = @StatusType";
                        var queryDefinition = new QueryDefinition(query)
                            .WithParameter("@StatusType", statusType);

                        var iterator = _container.GetItemQueryIterator<int>(queryDefinition);

                        int count = 0;
                        while (iterator.HasMoreResults)
                        {
                            var response = await iterator.ReadNextAsync();
                            count += response.FirstOrDefault(); // COUNT queries return a single integer value
                        }

                        // Store the count for this status
                        ticketCounts[statusType] = count;

                        // Add the count for this status to the total count
                        totalCount += count;
                    }

                    // Optionally, add the total count to the dictionary if needed
                    ticketCounts["Total"] = totalCount;

                    return ticketCounts;
                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error querying ticket counts: {ex.Message}");
                    return null; // Return null to indicate an error
                }
            }
        }

        public async Task<Dictionary<string, int>> GetTotalCountOfRaiseTicketsByStateWise(string state)
        {
            try
            {
                string[] statusTypes = { "Open", "pending", "closed", "NotAssigned" };
                var ticketCounts = new Dictionary<string, int>(); // Dictionary to store counts for each status
                int totalCount = 0;
                foreach (var statusType in statusTypes)
                {
                    string query = "SELECT VALUE COUNT(1) FROM c WHERE c.RaiseTicketId != null  AND c.State=@state AND c.status = @StatusType";
                    var queryDefinition = new QueryDefinition(query)
                        .WithParameter("@StatusType", statusType)
                        .WithParameter("@state", state);

                    var iterator = _container.GetItemQueryIterator<int>(queryDefinition);

                    int count = 0;
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        count += response.FirstOrDefault(); // COUNT queries return a single integer value
                    }

                    // Store the count for this status
                    ticketCounts[statusType] = count;

                    // Add the count for this status to the total count
                    totalCount += count;
                }

                // Optionally, add the total count to the dictionary if needed
                ticketCounts["Total"] = totalCount;

                return ticketCounts;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying ticket counts: {ex.Message}");
                return null; // Return null to indicate an error
            }
        }



        public async Task<Dictionary<string, int>> GetTotalCountOfRaiseTicketStateWiseAndDistrictWise(string state, string district)
        {
            try
            {
                string[] statusTypes = { "Open", "pending", "closed", "NotAssigned" };
                var ticketCounts = new Dictionary<string, int>(); // Dictionary to store counts for each status
                int totalCount = 0;
                foreach (var statusType in statusTypes)
                {
                    string query = "SELECT VALUE COUNT(1) FROM c WHERE c.RaiseTicketId != null  AND c.State=@state AND c.District=@district  AND c.status = @StatusType";
                    var queryDefinition = new QueryDefinition(query)
                        .WithParameter("@StatusType", statusType)
                        .WithParameter("@state", state)
                        .WithParameter("@district", district);

                    var iterator = _container.GetItemQueryIterator<int>(queryDefinition);

                    int count = 0;
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        count += response.FirstOrDefault(); // COUNT queries return a single integer value
                    }

                    // Store the count for this status
                    ticketCounts[statusType] = count;

                    // Add the count for this status to the total count
                    totalCount += count;
                }

                // Optionally, add the total count to the dictionary if needed
                ticketCounts["Total"] = totalCount;

                return ticketCounts;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying ticket counts: {ex.Message}");
                return null; // Return null to indicate an error
            }
        }



        public async Task<Dictionary<string, int>> GetTotalCountOfRaiseTicketByStateWiseAndDistrictWiseAndZipcodeWise(string state, string district, string zipCode)
        {
            try
            {
                string[] statusTypes = { "Open", "pending", "closed", "NotAssigned" };
                var ticketCounts = new Dictionary<string, int>(); // Dictionary to store counts for each status
                int totalCount = 0;
                foreach (var statusType in statusTypes)
                {
                    string query = "SELECT VALUE COUNT(1) FROM c WHERE c.RaiseTicketId != null  AND c.State=@state AND c.District=@district AND c.ZipCode=@zipcode  AND c.status = @StatusType";
                    var queryDefinition = new QueryDefinition(query)
                        .WithParameter("@StatusType", statusType)
                        .WithParameter("@state", state)
                        .WithParameter("@district", district)
                        .WithParameter("@zipcode", zipCode);

                    var iterator = _container.GetItemQueryIterator<int>(queryDefinition);

                    int count = 0;
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        count += response.FirstOrDefault(); // COUNT queries return a single integer value
                    }

                    // Store the count for this status
                    ticketCounts[statusType] = count;

                    // Add the count for this status to the total count
                    totalCount += count;
                }

                // Optionally, add the total count to the dictionary if needed
                ticketCounts["Total"] = totalCount;

                return ticketCounts;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying ticket counts: {ex.Message}");
                return null; // Return null to indicate an error
            }
        }





        public async Task<Dictionary<string, int>> GetTotalCountOfBuyProducts()
        {

            {
                try
                {
                    // Define the status types you're interested in
                    string[] statusTypes = { "Open", "pending", "closed", "NotAssigned" };
                    var ticketCounts = new Dictionary<string, int>(); // Dictionary to store counts for each status
                    int totalCount = 0; // Variable to store the sum of all counts

                    foreach (var statusType in statusTypes)
                    {
                        // Cosmos DB query to get the count for each status
                        string query = "SELECT VALUE COUNT(1) FROM c WHERE c.BuyProductId  !=null  AND c.status = @StatusType";
                        var queryDefinition = new QueryDefinition(query)
                            .WithParameter("@StatusType", statusType);

                        var iterator = _container.GetItemQueryIterator<int>(queryDefinition);

                        int count = 0;
                        while (iterator.HasMoreResults)
                        {
                            var response = await iterator.ReadNextAsync();
                            count += response.FirstOrDefault(); // COUNT queries return a single integer value
                        }

                        // Store the count for this status
                        ticketCounts[statusType] = count;

                        // Add the count for this status to the total count
                        totalCount += count;
                    }

                    // Optionally, add the total count to the dictionary if needed
                    ticketCounts["Total"] = totalCount;

                    return ticketCounts;
                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error querying ticket counts: {ex.Message}");
                    return null; // Return null to indicate an error
                }
            }
        }


        public async Task<Dictionary<string, int>> GetTotalCountOfBuyProductsByStateWise(string state)
        {




            {
                try
                {
                    // Define the status types you're interested in
                    string[] statusTypes = { "Open", "pending", "closed", "NotAssigned" };
                    var ticketCounts = new Dictionary<string, int>(); // Dictionary to store counts for each status
                    int totalCount = 0; // Variable to store the sum of all counts

                    foreach (var statusType in statusTypes)
                    {
                        // Cosmos DB query to get the count for each status
                        string query = "SELECT VALUE COUNT(1) FROM c WHERE c.BuyProductId  !=null  AND c.State=@state  AND c.status = @StatusType";
                        var queryDefinition = new QueryDefinition(query)
                            .WithParameter("@StatusType", statusType)
                            .WithParameter("@state", state);

                        var iterator = _container.GetItemQueryIterator<int>(queryDefinition);

                        int count = 0;
                        while (iterator.HasMoreResults)
                        {
                            var response = await iterator.ReadNextAsync();
                            count += response.FirstOrDefault(); // COUNT queries return a single integer value
                        }

                        // Store the count for this status
                        ticketCounts[statusType] = count;

                        // Add the count for this status to the total count
                        totalCount += count;
                    }

                    // Optionally, add the total count to the dictionary if needed
                    ticketCounts["Total"] = totalCount;

                    return ticketCounts;
                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error querying ticket counts: {ex.Message}");
                    return null; // Return null to indicate an error
                }
            }
        }

        public async Task<Dictionary<string, int>> GetTotalCountOfBuyProductsByStateWiseAndDistrictWise(string state, string district)
        {




            {
                try
                {
                    // Define the status types you're interested in
                    string[] statusTypes = { "Open", "pending", "closed", "NotAssigned" };
                    var ticketCounts = new Dictionary<string, int>(); // Dictionary to store counts for each status
                    int totalCount = 0; // Variable to store the sum of all counts

                    foreach (var statusType in statusTypes)
                    {
                        // Cosmos DB query to get the count for each status
                        string query = "SELECT VALUE COUNT(1) FROM c WHERE c.BuyProductId  !=null  AND c.District=@district  AND c.State=@state  AND c.status = @StatusType";
                        var queryDefinition = new QueryDefinition(query)
                            .WithParameter("@StatusType", statusType)
                            .WithParameter("@state", state)
                            .WithParameter("@district", district);

                        var iterator = _container.GetItemQueryIterator<int>(queryDefinition);

                        int count = 0;
                        while (iterator.HasMoreResults)
                        {
                            var response = await iterator.ReadNextAsync();
                            count += response.FirstOrDefault(); // COUNT queries return a single integer value
                        }

                        // Store the count for this status
                        ticketCounts[statusType] = count;

                        // Add the count for this status to the total count
                        totalCount += count;
                    }

                    // Optionally, add the total count to the dictionary if needed
                    ticketCounts["Total"] = totalCount;

                    return ticketCounts;
                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error querying ticket counts: {ex.Message}");
                    return null; // Return null to indicate an error
                }
            }

        }

        public async Task<Dictionary<string, int>> GetTotalCountOfBuyProductsByStateWiseAndDistrictWiseAndZipcodeWise(string state, string district, string zipCode)
        {
            {
                try
                {
                    // Define the status types you're interested in
                    string[] statusTypes = { "Open", "pending", "closed", "NotAssigned" };
                    var ticketCounts = new Dictionary<string, int>(); // Dictionary to store counts for each status
                    int totalCount = 0; // Variable to store the sum of all counts

                    foreach (var statusType in statusTypes)
                    {
                        // Cosmos DB query to get the count for each status
                        string query = "SELECT VALUE COUNT(1) FROM c WHERE c.BuyProductId  !=null   AND c.District=@district  AND c.State=@state AND c.ZipCode=@zipCode AND c.status = @StatusType";
                        var queryDefinition = new QueryDefinition(query)
                            .WithParameter("@StatusType", statusType)
                            .WithParameter("@state", state)
                            .WithParameter("@district", district)
                            .WithParameter("@zipCode", zipCode);

                        var iterator = _container.GetItemQueryIterator<int>(queryDefinition);

                        int count = 0;
                        while (iterator.HasMoreResults)
                        {
                            var response = await iterator.ReadNextAsync();
                            count += response.FirstOrDefault(); // COUNT queries return a single integer value
                        }

                        // Store the count for this status
                        ticketCounts[statusType] = count;

                        // Add the count for this status to the total count
                        totalCount += count;
                    }

                    // Optionally, add the total count to the dictionary if needed
                    ticketCounts["Total"] = totalCount;

                    return ticketCounts;
                }
                catch (CosmosException ex)
                {
                    Console.WriteLine($"Error querying ticket counts: {ex.Message}");
                    return null; // Return null to indicate an error
                }
            }

        }



        public async Task<List<T>> GetAddress(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentNullException(nameof(userId), "UserId cannot be null or empty.");
                }

                // Define the query with a parameter for UserId
                var queryDefinition = new QueryDefinition(
                    "SELECT c.id,c.AddressId,c.IsPrimaryAddress,c.UserId,c.Address,c.State,c.District,c.ZipCode," +
                    "c.MobileNumber,c.WalletAmount,c.FirstName,c.LastName,c.EmailAddress, CONCAT (c.FirstName, ' ', c.LastName) AS FullName from c WHERE c.UserId = @userId and c.ZipCode !=null")
                    .WithParameter("@userId", userId);

                // Create a query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var addresses = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    addresses.AddRange(response); // Add all items from the current response
                }

                return addresses; // Return the list of addresses
            }
            catch (CosmosException ex)
            {
                return new List<T>(); // Return an empty list on Cosmos DB-specific errors
            }
            catch (Exception ex)
            {
                return new List<T>(); // Return an empty list on unexpected errors
            }
        }


        public async Task<List<T>> GetBookTechnicianAddress(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentNullException(nameof(userId), "UserId cannot be null or empty.");
                }

                // Define the query with a parameter for UserId
                var queryDefinition = new QueryDefinition(
                    "\r\nSELECT c.id,c.AddressId,c.IsPrimaryAddress,c.Address,c.State,c.District,c.ZipCode, c.TechnicianFullName ,  c.UserId   from c WHERE c.UserId = @userId and c.ZipCode !=null ")
                    .WithParameter("@userId", userId);

                // Create a query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var addresses = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    addresses.AddRange(response); // Add all items from the current response
                }

                return addresses; // Return the list of addresses
            }
            catch (CosmosException ex)
            {
                return new List<T>(); // Return an empty list on Cosmos DB-specific errors
            }
            catch (Exception ex)
            {
                return new List<T>(); // Return an empty list on unexpected errors
            }
        }




        public async Task<List<T>> GetSecondaryAddress(string profileType, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentNullException(nameof(userId), "UserId cannot be null or empty.");
                }

                if (string.IsNullOrEmpty(profileType))
                {
                    throw new ArgumentNullException(nameof(profileType), "ProfileType cannot be null or empty.");
                }

                // Normalize profile type
                profileType = profileType.ToLower();

                // Validate profile type
                var validProfileTypes = new List<string> { "customer", "dealer", "estimator", "builder", "technician" };
                if (!validProfileTypes.Contains(profileType))
                {
                    throw new ArgumentException("Invalid profile type provided.", nameof(profileType));
                }

                // Define the query
                string queryString = @"
            SELECT 
                '1' as AddressId, 
                'true' as IsPrimaryAddress, 
                c.Address as FullAddress, 
                c.State, 
                c.District, 
                c.ZipCode as PinCode, 
                c.UserId,
                c.ProfileType 
            FROM c 
            WHERE c.UserId = @userId AND c.ProfileType = @profileType";

                var queryDefinition = new QueryDefinition(queryString)
                    .WithParameter("@userId", userId)
                    .WithParameter("@profileType", profileType);

                // Create a uery iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var addresses = new List<T>();

                // Fetch all results
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    addresses.AddRange(response); // Add items from the current batch
                }

                return addresses; // Return the list of addresses
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Cosmos DB error while fetching addresses.");
                return new List<T>(); // Return an empty list on Cosmos DB-specific errors
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching addresses.");
                return new List<T>(); // Return an empty list on unexpected errors
            }
        }


        //update existitng data api
        public async Task<string> UpdateDocumentAsync(UpdateDocumentRequest request)
        {
            try
            {
                // Query to fetch the document by id using the correct partition key (id)
                ItemResponse<dynamic> existingDocumentResponse = await _container.ReadItemAsync<dynamic>(
                    request.Id, // The unique document id
                    new PartitionKey(request.Id) // Use 'id' as the partition key for reading the document
                );

                // If the document exists, update it
                dynamic existingDocument = existingDocumentResponse.Resource;

                // Log the existing document for debugging purposes
                Console.WriteLine($"Document found: {JsonConvert.SerializeObject(existingDocument, Formatting.Indented)}");

                // Update fields dynamically
                existingDocument.Status = request.Status;
                existingDocument.Profiletype = request.Profiletype;
                existingDocument.ProfileApprovedby = request.ProfileApprovedby;
                existingDocument.ProfileRequestedby = request.ProfileRequestedby;
                existingDocument.CreatedDate = request.CreatedDate;
                existingDocument.ModifiedDate = request.ModifiedDate;
                existingDocument.Comments = request.Comments;

                // Replace the document in Cosmos DB
                var response = await _container.ReplaceItemAsync(existingDocument, request.Id, new PartitionKey(request.Id));

                // Log success
                Console.WriteLine($"Document updated successfully. New Document: {JsonConvert.SerializeObject(existingDocument, Formatting.Indented)}");

                // Return the updated document as JSON
                return JsonConvert.SerializeObject(existingDocument, Formatting.Indented);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                // Log additional debugging info when document is not found
                Console.WriteLine($"Cosmos DB error: Document not found. Status Code: {cosmosEx.StatusCode}");
                return "Document not found!";
            }
            catch (CosmosException cosmosEx)
            {
                // Log Cosmos DB specific errors and return detailed error information
                Console.WriteLine($"Cosmos DB error: {cosmosEx.Message}, Status code: {cosmosEx.StatusCode}");
                return $"Cosmos DB error: {cosmosEx.Message}, Status code: {cosmosEx.StatusCode}";
            }
            catch (Exception ex)
            {
                // Log any general errors
                Console.WriteLine($"An error occurred: {ex.Message}");
                return $"An error occurred: {ex.Message}";
            }
        }






        public async Task<List<T>> GetRaiseTicketNotificationsByDistrict(string district, string category)
        {
            var raiseticket = new List<T>();
            try
            {

                var queryDefinition = new QueryDefinition("SELECT * FROM c  where c.RaiseTicketId !=null  and  c.District=@district and c.Category=@category and c.LowestBidderTechnicainId !=null")
                    .WithParameter("@district", district)
                    .WithParameter("@category", category);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    raiseticket.AddRange(response);
                }

            }
            catch (CosmosException ex)
            {
                // Log Cosmos DB specific exceptions
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            // Return the list of tickets (empty if exceptions occurred)
            return raiseticket;
        }

        public async Task<List<T>> GetRaiseTicketNotificationsByStateAndDistrict(string district, string category)
        {
            var raiseAQuoteByDealer = new List<T>();
            try
            {

                var queryDefinition = new QueryDefinition("SELECT * FROM c  where c.RaiseTicketId !=null and c.AssignedTo='Dealer/Trader' and  c.District=@district and c.Category=@category and c.LowestBidderTechnicainId !=null")
                    .WithParameter("@district", district)
                    .WithParameter("@category", category);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    raiseAQuoteByDealer.AddRange(response);
                }

            }
            catch (CosmosException ex)
            {
                // Log Cosmos DB specific exceptions
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            // Return the list of tickets (empty if exceptions occurred)
            return raiseAQuoteByDealer;
        }




        //public async Task<List<T>> GetTrackTicketDetailsAsync()
        //{
        //    var supportTicket = new List<T>();

        //    try
        //    {
        //        var queryDefinition = new QueryDefinition("SELECT * FROM c  where c.RaiseTicketId !=null and c.Address !=null ORDER BY c.Date DESC");

        //        // Create query iterator
        //        var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

        //        // Iterate through query results
        //        while (queryIterator.HasMoreResults)
        //        {
        //            var response = await queryIterator.ReadNextAsync();
        //            supportTicket.AddRange(response); // Add current batch of items
        //        }
        //    }
        //    catch (CosmosException ex)
        //    {
        //        // Log Cosmos DB specific exceptions
        //        Console.WriteLine($"Cosmos DB error: {ex.Message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log general exceptions
        //        Console.WriteLine($"Internal server error: {ex.Message}");
        //    }

        //    // Return the list of tickets (empty if exceptions occurred)
        //    return supportTicket;
        //}

        public async Task<List<T>> GetTrackTicketDetailsAsync()
        {
            var supportTicket = new List<T>();

            try
            {
                var queryDefinition = new QueryDefinition(@"
                 SELECT * FROM c  
WHERE c.RaiseTicketId !=null
AND c.Address !=null
AND ( 
    (ARRAY_LENGTH(c.TechnicianList) = 0 AND ARRAY_LENGTH(c.DealerList) = 0 )) 
    OR 
    (ARRAY_LENGTH(c.TechnicianList) > 0 AND ARRAY_LENGTH(c.DealerList) = 0) 
        OR 
        (ARRAY_LENGTH(c.TechnicianList) = 0 AND ARRAY_LENGTH(c.DealerList) > 0)
        OR
    (ARRAY_LENGTH(c.TechnicianList) > 0 AND ARRAY_LENGTH(c.DealerList) > 0  )

ORDER BY c.Date DESC

");

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    supportTicket.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return supportTicket;
        }

        public async Task<List<T>> GetRaiseTicketsForDealer()
        {
            var supportTicket = new List<T>();

            try
            {
                var queryDefinition = new QueryDefinition("SELECT * FROM c  where c.RaiseTicketId !=null and c.AssignedTo ='Dealer/Trader'");

                // Create query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                // Iterate through query results
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    supportTicket.AddRange(response); // Add current batch of items
                }
            }
            catch (CosmosException ex)
            {
                // Log Cosmos DB specific exceptions
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            // Return the list of tickets (empty if exceptions occurred)
            return supportTicket;
        }



        public async Task<List<T>> GetRaiseTicketsForDealerForSMS()
        {
            var supportTicket = new List<T>();

            try
            {
                var queryDefinition = new QueryDefinition("SELECT * FROM c  where c.RaiseTicketId !=null and c.AssignedTo ='Dealer/Trader'");

                // Create query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                // Iterate through query results
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    supportTicket.AddRange(response); // Add current batch of items
                }
            }
            catch (CosmosException ex)
            {
                // Log Cosmos DB specific exceptions
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            // Return the list of tickets (empty if exceptions occurred)
            return supportTicket;
        }

        public async Task<List<T>> GetRaiseTicketsForLowestDealerForSMS()
        {
            var supportTicket = new List<T>();

            try
            {
                var queryDefinition = new QueryDefinition("select * from c where c.RaiseTicketId !=null and   c.LowestBidderDealerId !=null");

                // Create query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                // Iterate through query results
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    supportTicket.AddRange(response); // Add current batch of items
                }
            }
            catch (CosmosException ex)
            {
                // Log Cosmos DB specific exceptions
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            // Return the list of tickets (empty if exceptions occurred)
            return supportTicket;
        }
        public async Task<List<T>> GetRaiseTicketsNotificationsForTechnician()
        {
            var raiseTicketNotification = new List<T>();

            try
            {
                var queryDefinition = new QueryDefinition("SELECT * FROM c  where c.RaiseTicketId !=null and c.AssignedTo='Technical Agency' and c.internalStatus='Pending'");

                // Create query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                // Iterate through query results
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    raiseTicketNotification.AddRange(response); // Add current batch of items
                }
            }
            catch (CosmosException ex)
            {
                // Log Cosmos DB specific exceptions
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            // Return the list of tickets (empty if exceptions occurred)
            return raiseTicketNotification;
        }





        public async Task<List<T>> GetRaiseTicketsNotificationsForTechnicianForSMS()
        {
            var raiseTicketNotification = new List<T>();

            try
            {
                var queryDefinition = new QueryDefinition("SELECT * FROM c  where c.RaiseTicketId !=null and c.AssignedTo='Technical Agency' and c.internalStatus='Assigned'");

                // Create query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                // Iterate through query results
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    raiseTicketNotification.AddRange(response); // Add current batch of items
                }
            }
            catch (CosmosException ex)
            {
                // Log Cosmos DB specific exceptions
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            // Return the list of tickets (empty if exceptions occurred)
            return raiseTicketNotification;
        }







        public async Task<List<T>> GetRaiseTicketsNotificationsForLowestTechnicianForSMS()
        {
            var raiseTicketNotification = new List<T>();

            try
            {
                var queryDefinition = new QueryDefinition("select TOP 1  *  from c where c.RaiseTicketId !=null and   c.LowestBidderTechnicainId !=''order by c.Date desc");

                // Create query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                // Iterate through query results
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    raiseTicketNotification.AddRange(response); // Add current batch of items
                }
            }
            catch (CosmosException ex)
            {
                // Log Cosmos DB specific exceptions
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            // Return the list of tickets (empty if exceptions occurred)
            return raiseTicketNotification;
        }

        public async Task<List<T>> GetRaiseTicketsForCustomer()
        {
            var supportTicket = new List<T>();

            try
            {
                var queryDefinition = new QueryDefinition("select *  from c where   c.RaiseTicketId !=null and  c.internalStatus='Assigned'");

                // Create query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                // Iterate through query results
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    supportTicket.AddRange(response); // Add current batch of items
                }
            }
            catch (CosmosException ex)
            {
                // Log Cosmos DB specific exceptions
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            // Return the list of tickets (empty if exceptions occurred)
            return supportTicket;
        }


        public async Task<List<T>> GetRaiseTicketNotificationsByCustomerId(string customerId)
        {
            var raiseticket = new List<T>();
            try
            {

                var queryDefinition = new QueryDefinition("SELECT * FROM c where c.RaiseTicketId !=null and  c.LowestBidderTechnicainId !=null and c.CustomerId=@customerId and c.AssignedTo='Customer'")
                    .WithParameter("@customerId", customerId);


                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    raiseticket.AddRange(response);
                }

            }
            catch (CosmosException ex)
            {
                // Log Cosmos DB specific exceptions
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            // Return the list of tickets (empty if exceptions occurred)
            return raiseticket;
        }


        public async Task<bool> UpdateAddressAsync(string addressId, AddressModel address)
        {
            try
            {
                // Query the document by AddressId
                var query = "SELECT * FROM c WHERE c.AddressId = @AddressId";
                var queryDefinition = new QueryDefinition(query).WithParameter("@AddressId", addressId);

                var iterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);
                var results = await iterator.ReadNextAsync();

                if (results.Count == 0)
                {
                    return false; // No document found
                }

                // Retrieve the existing document
                var document = results.FirstOrDefault();
                string documentId = document.id; // Extract the document's unique ID
                string partitionKey = document.AddressId; // Assuming AddressId is the partition key

                // Replace the existing document with the updated one
                await _container.ReplaceItemAsync(address, documentId, new PartitionKey(partitionKey));
                return true; // Update successful
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB Exception: {ex.Message}");
                return false; // Update failed
            }
        }


        public async Task CreateItemAsync(BuyProduct buyProduct)
        {
            try
            {
                // Ensure the PartitionKey is set (if needed), here using Category as an example
                var partitionKey = new PartitionKey(buyProduct.Category);

                // Ensure ProductId is assigned to id (required by Cosmos DB)
                buyProduct.id = Guid.NewGuid().ToString();
                var item = new { id = buyProduct.id, buyProduct }; // Assign id to the ProductId field

                // Create item in Cosmos DB
                await _container.CreateItemAsync(item, partitionKey);
            }
            catch (CosmosException ex)
            {
                // Handle any errors, log them as needed
                throw new ApplicationException("Error inserting item into Cosmos DB", ex);
            }
        }



        public async Task<List<T>> GetAllCustomersDetails()

        {
            try
            {
                var queryDefinition = new QueryDefinition("SELECT * FROM c  where c.CustomerId !=null  and c.UserId !=null");

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var customer = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    customer.AddRange(response);
                }
                return customer;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }

        }

        public async Task<List<T>> GetAllDealersDetails()

        {
            try
            {
                var queryDefinition = new QueryDefinition("SELECT * FROM c  where c.DealerId !=null  and c.UserId !=null");
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var dealer = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    dealer.AddRange(response);
                }
                return dealer;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }

        public async Task<List<T>> GetAllTechniciansDetails()
        {
            try
            {
                var queryDefinition = new QueryDefinition("SELECT * FROM c  where c.TechnicianId !=null  and c.UserId !=null");
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var technician = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    technician.AddRange(response);
                }
                return technician;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }

        public async Task<List<T>> GetAllBuildersDetails()

        {
            try
            {

                var querDefinition = new QueryDefinition("SELECT * FROM c  where c.BuilderId !=null  and c.UserId !=null ");


                var queryIterator = _container.GetItemQueryIterator<T>(querDefinition);

                var estimator = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    estimator.AddRange(response);
                }
                return estimator;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }







        public async Task<List<T>> GetAllEstimatorsDetails()

        {
            try
            {

                var querDefinition = new QueryDefinition("SELECT * FROM c  where c.EstimatorId !=null  and c.UserId !=null ");


                var queryIterator = _container.GetItemQueryIterator<T>(querDefinition);

                var estimator = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    estimator.AddRange(response);
                }
                return estimator;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }

        public async Task<List<T>> GetAddMember()
        {
            try
            {
                var queryDefinition = new QueryDefinition("select * from c where c.AddMemberId !=null");

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var addmember = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    addmember.AddRange(response);
                }
                return addmember;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }

        public async Task<List<T>> GetAddTechnicians()
        {
            try
            {

                var queryDefinition = new QueryDefinition(" select * from c where c.AddTechnicianId !=null");

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var addTechnician = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();

                    addTechnician.AddRange(response);
                }
                return addTechnician;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }

        public async Task<List<T>> GetAddMemberDetailsById(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException(nameof(id), "UserId cannot be null or Empty.");
                }

                var queryDefinition = new QueryDefinition("  select * from c where c.AddMemberId !=null and c.id=@id")
                    .WithParameter("@id", id);
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var customer = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    customer.AddRange(response);
                }
                return customer;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }

        public async Task<List<T>> GetAddTechnicianDetailsById(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException(nameof(id), "UserId cannot be null or Empty.");
                }

                var queryDefinition = new QueryDefinition("   select * from c where c.AddTechnicianId !=null and c.id=@id")
                    .WithParameter("@id", id);
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var customer = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    customer.AddRange(response);
                }
                return customer;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }



        public async Task<List<DealerDTO>> GetDealerDirectoryDetails(
    string searchQuery = null,
    string State = null,
    string District = null,
    string ZipCode = null,
    string Status = null)
        {
            try
            {
                // Base query
                var query = @"
SELECT 
    c.DealerId,
    c.DealerFirmName,
    c.PhoneNumber,
    c.EmailAddress,
    c.Address,
    c.State,
    c.District,
    c.ZipCode,
    c.DealerPhotoId,
    c.Status,
    c.StateId,
    c.DistrictId,
    c.IsActive
FROM c
WHERE 1=1 and c.DealerFirmName !=null AND c.DealerId !=null";

                // Apply filters

                if (!string.IsNullOrEmpty(State))
                    query += " AND (LOWER(c.State) = LOWER(@State) OR c.StateId = @State)";

                if (!string.IsNullOrEmpty(District))
                    query += " AND (LOWER(c.District) = LOWER(@District) OR c.DistrictId = @District)";

                if (!string.IsNullOrEmpty(ZipCode) && !string.IsNullOrEmpty(State) && !string.IsNullOrEmpty(District))
                    query += " AND c.ZipCode = @ZipCode";

                if (!string.IsNullOrEmpty(Status))
                    query += " AND c.Status = @Status";

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query += @"
    AND (
        CONTAINS(c.DealerFirmName, @searchQuery, true) OR 
        CONTAINS(c.Address, @searchQuery, true) OR
        CONTAINS(c.EmailAddress, @searchQuery, true) OR 
        CONTAINS(c.DealerPhotoId, @searchQuery, true) OR
        CONTAINS(c.Status, @searchQuery, true)
    )";
                }

                query += " order by c.timestamp desc";

                // Log query for debugging
                Console.WriteLine($"Generated Query: {query}");

                // Create query definition
                var queryDefinition = new QueryDefinition(query);

                if (!string.IsNullOrEmpty(State))
                    queryDefinition = queryDefinition.WithParameter("@State", State);

                if (!string.IsNullOrEmpty(District))
                    queryDefinition = queryDefinition.WithParameter("@District", District);

                if (!string.IsNullOrEmpty(ZipCode))
                    queryDefinition = queryDefinition.WithParameter("@ZipCode", ZipCode);

                if (!string.IsNullOrEmpty(Status))
                    queryDefinition = queryDefinition.WithParameter("@Status", Status);

                if (!string.IsNullOrEmpty(searchQuery))
                    queryDefinition = queryDefinition.WithParameter("@searchQuery", searchQuery);

                // Execute query
                var queryIterator = _container.GetItemQueryIterator<DealerDTO>(queryDefinition);
                var results = new List<DealerDTO>();

                // Read the results
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (Exception ex)
            {
                // Log errors
                Console.WriteLine($"Error: {ex.Message}");
                return new List<DealerDTO>();
            }
        }





        public async Task<List<BUilderDirectoryDTO>> GetBuilderDirectoryDetails(
       string searchQuery = null,
    string State = null,
    string District = null,
    string ZipCode = null,
    string Status = null)
        {
            try
            {
                // Base query
                var query = @"
SELECT 
    c.BuilderId,
    c.BuilderName,
    c.PhoneNumber,
    c.EmailAddress,
    c.Address,
    c.State,
    c.District,
    c.ZipCode,
    c.BuilderPhotoId,
    c.Status,
    c.StateId,
    c.DistrictId,
    c.IsActive
FROM c
WHERE 1=1 AND c.BuilderId !=null AND c.BuilderName !=null"; // Makes appending dynamic conditions easier

                // Apply filters

                if (!string.IsNullOrEmpty(State))
                    query += " AND (LOWER(c.State) = LOWER(@State) OR c.StateId = @State)";

                if (!string.IsNullOrEmpty(District))
                    query += " AND (LOWER(c.District) = LOWER(@District) OR c.DistrictId = @District)";

                if (!string.IsNullOrEmpty(ZipCode) && !string.IsNullOrEmpty(State) && !string.IsNullOrEmpty(District))
                    query += " AND c.ZipCode = @ZipCode";

                if (!string.IsNullOrEmpty(Status))
                    query += " AND c.Status = @Status";

                // Append search query functionality
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query += @"
    AND (
        CONTAINS(c.BuilderName, @searchQuery, true) OR 
        CONTAINS(c.Address, @searchQuery, true) OR
        CONTAINS(c.EmailAddress, @searchQuery, true) OR 
        CONTAINS(c.BuilderPhotoId, @searchQuery, true) OR
        CONTAINS(c.Status, @searchQuery, true)
    )";
                }

                query += " order by c.timestamp desc";

                // Log generated query
                Console.WriteLine($"Generated Query: {query}");

                // Create query definition
                var queryDefinition = new QueryDefinition(query);

                if (!string.IsNullOrEmpty(State))
                    queryDefinition = queryDefinition.WithParameter("@State", State);

                if (!string.IsNullOrEmpty(District))
                    queryDefinition = queryDefinition.WithParameter("@District", District);

                if (!string.IsNullOrEmpty(ZipCode))
                    queryDefinition = queryDefinition.WithParameter("@ZipCode", ZipCode);

                if (!string.IsNullOrEmpty(Status))
                    queryDefinition = queryDefinition.WithParameter("@Status", Status);

                if (!string.IsNullOrEmpty(searchQuery))
                    queryDefinition = queryDefinition.WithParameter("@searchQuery", searchQuery);

                // Execute query
                var queryIterator = _container.GetItemQueryIterator<BUilderDirectoryDTO>(queryDefinition);
                var results = new List<BUilderDirectoryDTO>();

                // Read the results
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (Exception ex)
            {
                // Log errors
                Console.WriteLine($"Error: {ex.Message}");
                return new List<BUilderDirectoryDTO>();
            }
        }






        public async Task<List<EstimatorDTONew>> GetEstimatorDirectoryDetails(
      string searchQuery = null,
      string State = null,
      string District = null,
      string ZipCode = null,
      string Status = null)
        {
            try
            {
                // Base query
                var query = @"
SELECT 
    c.EstimatorId,
    c.EstimatorName,
    c.PhoneNumber,
    c.EmailAddress,
    c.Address,
    c.State,
    c.District,
    c.ZipCode,
    c.EstimatorPhotoId,
    c.Status,
    c.StateId,
    c.DistrictId,
    c.IsActive
FROM c
WHERE 1=1 c.EstimatorId !=null AND c.EstimatorName !=null"; // Allows appending dynamic conditions easily

                // Apply filters

                if (!string.IsNullOrEmpty(State))
                    query += " AND (LOWER(c.State) = LOWER(@State) OR c.StateId = @State)";

                if (!string.IsNullOrEmpty(District))
                    query += " AND (LOWER(c.District) = LOWER(@District) OR c.DistrictId = @District)";

                if (!string.IsNullOrEmpty(ZipCode) && !string.IsNullOrEmpty(State) && !string.IsNullOrEmpty(District))
                    query += " AND c.ZipCode = @ZipCode";

                if (!string.IsNullOrEmpty(Status))
                    query += " AND c.Status = @Status";

                // Append search query functionality
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query += @"
    AND (
        CONTAINS(c.EstimatorName, @searchQuery, true) OR 
        CONTAINS(c.Address, @searchQuery, true) OR
        CONTAINS(c.EmailAddress, @searchQuery, true) OR 
        CONTAINS(c.EstimatorPhotoId, @searchQuery, true) OR
        CONTAINS(c.Status, @searchQuery, true)
    )";
                }

                query += " order by c.timestamp desc";


                // Log generated query
                Console.WriteLine($"Generated Query: {query}");

                // Create query definition
                var queryDefinition = new QueryDefinition(query);

                if (!string.IsNullOrEmpty(State))
                    queryDefinition = queryDefinition.WithParameter("@State", State);

                if (!string.IsNullOrEmpty(District))
                    queryDefinition = queryDefinition.WithParameter("@District", District);

                if (!string.IsNullOrEmpty(ZipCode))
                    queryDefinition = queryDefinition.WithParameter("@ZipCode", ZipCode);

                if (!string.IsNullOrEmpty(Status))
                    queryDefinition = queryDefinition.WithParameter("@Status", Status);

                if (!string.IsNullOrEmpty(searchQuery))
                    queryDefinition = queryDefinition.WithParameter("@searchQuery", searchQuery);

                // Execute query
                var queryIterator = _container.GetItemQueryIterator<EstimatorDTONew>(queryDefinition);
                var results = new List<EstimatorDTONew>();

                // Read the results
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (Exception ex)
            {
                // Log errors
                Console.WriteLine($"Error: {ex.Message}");
                return new List<EstimatorDTONew>();
            }
        }





        public async Task<List<Technician>> GetTechnicianDirectoryDetails(
      string searchQuery = null,
      string State = null,
      string District = null,
      string ZipCode = null,
      string Status = null)
        {
            try
            {
                // Base query
                var query = @"
SELECT 
    c.TechnicianId,
    c.TechnicianFullName,
    c.PhoneNumber,
    c.EmailAddress,
    c.Address,
    c.State,
    c.District,
    c.ZipCode,
    c.TechnicianPhotoId,
    c.Status,
    c.StateId,
    c.DistrictId,
    c.IsActive
FROM c
WHERE 1=1 AND c.TechnicianId !=null  AND c.TechnicianFullName !=null"; // Allows appending dynamic conditions easily

                // Apply filters

                if (!string.IsNullOrEmpty(State))
                    query += " AND (LOWER(c.State) = LOWER(@State) OR c.StateId = @State)";

                if (!string.IsNullOrEmpty(District))
                    query += " AND (LOWER(c.District) = LOWER(@District) OR c.DistrictId = @District)";

                if (!string.IsNullOrEmpty(ZipCode) && !string.IsNullOrEmpty(State) && !string.IsNullOrEmpty(District))
                    query += " AND c.ZipCode = @ZipCode";

                if (!string.IsNullOrEmpty(Status))
                    query += " AND c.Status = @Status";

                // Append search query functionality
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    query += @"
    AND (
        CONTAINS(c.TechnicianFullName, @searchQuery, true) OR 
        CONTAINS(c.Address, @searchQuery, true) OR
        CONTAINS(c.EmailAddress, @searchQuery, true) OR 
        CONTAINS(c.TechnicianPhotoId, @searchQuery, true) OR
        CONTAINS(c.Status, @searchQuery, true)
    )";
                }

                query += " order by c.timestamp desc";

                // Log generated query
                Console.WriteLine($"Generated Query: {query}");

                // Create query definition
                var queryDefinition = new QueryDefinition(query);

                if (!string.IsNullOrEmpty(State))
                    queryDefinition = queryDefinition.WithParameter("@State", State);

                if (!string.IsNullOrEmpty(District))
                    queryDefinition = queryDefinition.WithParameter("@District", District);

                if (!string.IsNullOrEmpty(ZipCode))
                    queryDefinition = queryDefinition.WithParameter("@ZipCode", ZipCode);

                if (!string.IsNullOrEmpty(Status))
                    queryDefinition = queryDefinition.WithParameter("@Status", Status);

                if (!string.IsNullOrEmpty(searchQuery))
                    queryDefinition = queryDefinition.WithParameter("@searchQuery", searchQuery);

                // Execute query
                var queryIterator = _container.GetItemQueryIterator<Technician>(queryDefinition);
                var results = new List<Technician>();

                // Read the results
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (Exception ex)
            {
                // Log errors
                Console.WriteLine($"Error: {ex.Message}");
                return new List<Technician>();
            }
        }



        public async Task<List<T>> GetProductNamesByCategory(string category)
        {
            try
            {

                if (string.IsNullOrEmpty(category))
                {
                    throw new ArgumentNullException(nameof(category), "categoryName Cannot be null or Empty");
                }

                var queryDefinition = new QueryDefinition(

                    "SELECT * FROM c where  c.ProductId !=null and c.ProductName !=null  and c.ProductStatus='Approved' and c.Category=@categoryName")

                   .WithParameter("@categoryName", category);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var product = new List<T>();

                while (queryIterator.HasMoreResults)
                {

                    var response = await queryIterator.ReadNextAsync();
                    product.AddRange(response);
                }

                return product;
            }

            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }


        public async Task<T> GetItemByIdAsync(string id, string partitionKeyValue)
        {
            try
            {
                var response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKeyValue));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default; // Return null if item is not found
            }
        }

        public async Task<List<T>> GetRaiseTicketForTechnician(string state, string district)
        {
            var notifications = new List<T>();
            try
            {
                var queryDefinition = new QueryDefinition("SELECT * FROM c where c.RaiseTicketId !=null " +
                    " and c.Date !=null and c.State =@state  and c.District =@district")
                    .WithParameter("@state", state)
                    .WithParameter("@district", district);
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    notifications.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error:{ex.Message}");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return notifications;
        }


        public async Task<List<T>> GetRaiseTicketForTechnicians(string state, string district)
        {
            var notifications = new List<T>();
            try
            {
                var queryDefinition = new QueryDefinition("SELECT c.RaiseTicketId,c.Date,c.Subject  FROM c where c.RaiseTicketId !=null " +
                    " and c.Date !=null and c.State =@state  and c.District =@district")
                    .WithParameter("@state", state)
                    .WithParameter("@district", district);
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    notifications.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error:{ex.Message}");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return notifications;
        }



        public async Task<List<T>> GetRecentNotifications()
        {
            var notifications = new List<T>();

            try
            {
                var queryDefinition = new QueryDefinition("SELECT  top 10 c.RaiseTicketId,c.Date,c.Subject FROM c where c.RaiseTicketId !=null  and c.Date !=null   order by c.date desc");

                // Create query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                // Iterate through query results
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    notifications.AddRange(response); // Add current batch of items
                }
            }
            catch (CosmosException ex)
            {
                // Log Cosmos DB specific exceptions
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            // Return the list of tickets (empty if exceptions occurred)
            return notifications;
        }


        public async Task<List<T>> GetRaiseAQuoteDetails()
        {
            var supportTicket = new List<T>();

            try
            {
                var queryDefinition = new QueryDefinition("SELECT * FROM c  where c.RaiseAQuoteId !=null and c.QuotedDate  !=null");

                // Create query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                // Iterate through query results
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    supportTicket.AddRange(response); // Add current batch of items
                }
            }
            catch (CosmosException ex)
            {
                // Log Cosmos DB specific exceptions
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            // Return the list of tickets (empty if exceptions occurred)
            return supportTicket;




        }

        public async Task<List<T>> GetRaiseAQuoteByDealerDetails()
        {
            var raiseAQuoteByDealer = new List<T>();

            try
            {
                var queryDefinition = new QueryDefinition("SELECT * FROM c  where c.RaiseAQuoteByDealerId !=null and c.RaiseAQuoteDate  !=null");


                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                // Iterate through query results
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    raiseAQuoteByDealer.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                // Log Cosmos DB specific exceptions
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            // Return the list of tickets (empty if exceptions occurred)
            return raiseAQuoteByDealer;




        }



        public async Task<List<T>> GetRaiseAQuoteDetailsById(string raiseAQuotetId)
        {
            var notifications = new List<T>();
            try
            {
                var queryDefinition = new QueryDefinition("select * from c where c.RaiseAQuoteId !=null and  c.RaiseTicketId=@raiseAQuotetId")
                    .WithParameter("@raiseAQuotetId", raiseAQuotetId);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    notifications.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error:{ex.Message}");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return notifications;
        }


        public async Task<List<T>> GetRaiseAQuoteLowestDealerByIdAsync(string raiseAQuotetDealerId)
        {
            var raiseAQuoteLowestDealer = new List<T>();

            try
            {
                var queryDefinition = new QueryDefinition(
                    "SELECT * FROM c WHERE c.RaiseAQuoteByDealerId != null AND c.RaiseTicketId = @raiseAQuotetDealerId and c.DealerId != 'Customer Care'")
                    .WithParameter("@raiseAQuotetDealerId", raiseAQuotetDealerId);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    raiseAQuoteLowestDealer.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                // Log the Cosmos DB-specific error
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                // Log the generic error
                Console.WriteLine($"Internal server error: {ex.Message}");
                throw;
            }

            return raiseAQuoteLowestDealer;
        }



        public async Task<List<T>> GetRaiseAQuoteLowestDealerById(string raiseAQuotetDealerId)
        {
            var raiseAQuoteLowestDealer = new List<T>();
            try
            {
                var queryDefinition = new QueryDefinition("select * from c where  c.RaiseAQuoteByDealerId !=null and  c.RaiseTicketId=@raiseAQuotetDealerId")
                    .WithParameter("@raiseAQuotetId", raiseAQuotetDealerId);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    raiseAQuoteLowestDealer.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error:{ex.Message}");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return raiseAQuoteLowestDealer;
        }



        public async Task<List<T>> GetRaiseAQuoteDealerDetailsById(string raiseTicketId, string dealerId)
        {
            var notifications = new List<T>();
            try
            {
                var queryDefinition = new QueryDefinition("select * from c where c.RaiseAQuoteByDealerId !=null and  c. DealerId =@dealerId and c.RaiseTicketId=@raiseTicketId")
                    .WithParameter("@raiseTicketId", raiseTicketId)
                    .WithParameter("@dealerId", dealerId);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    notifications.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error:{ex.Message}");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return notifications;
        }

        public async Task<T> GetRaiseAQuoteDetailsByTechnicianId(string raiseTicketId, string TechnicianId)
        {
            var notifications = new List<T>();
            try
            {
                var queryDefinition = new QueryDefinition("select * from c where c.RaiseAQuoteId !=null and  c.RaiseTicketId=@raiseTicketId and c.TechnicianId=@TechnicianId")
                    .WithParameter("@raiseTicketId", raiseTicketId)
                   .WithParameter("@TechnicianId", TechnicianId);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    notifications.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error:{ex.Message}");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return notifications[0];
        }

        //get dealer deatils by using guid id

        public async Task<List<T>> GetDealerByIdAsync<T>(Guid DealerId)
        {
            var results = new List<T>();
            try
            {
                // Define query with parameter
                var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.DealerId = @dealerId")
                    .WithParameter("@dealerId", DealerId.ToString()); // Convert Guid to string

                // Create query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                // Fetch data
                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                // Log Cosmos DB-specific exceptions
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return results;
        }


        //get estimator details by guidid
        public async Task<List<T>> GetEstimatorByIdAsync<T>(Guid EstimatorId)
        {
            var results = new List<T>();
            try
            {

                var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.EstimatorId = @estimatorId")
                    .WithParameter("@estimatorId", EstimatorId.ToString()); // Convert Guid to string


                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);


                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {

                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return results;
        }


        public async Task<List<T>> GetBuilderByIdAsync<T>(Guid BuilderId)
        {
            var results = new List<T>();
            try
            {

                var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.BuilderId = @builderId")
                    .WithParameter("@builderId", BuilderId.ToString()); // Convert Guid to string


                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);


                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {

                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return results;
        }




        public async Task<List<T>> GetTechnicianByIdAsync<T>(Guid TechnicianId)
        {
            var results = new List<T>();
            try
            {

                var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.TechnicianId = @technicianId")
                    .WithParameter("@technicianId", TechnicianId.ToString()); // Convert Guid to string


                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);


                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {

                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return results;
        }


        public async Task<List<T>> GetCustomerByIdAsync<T>(Guid CustomerId)
        {
            var results = new List<T>();
            try
            {

                var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.CustomerId = @customerId")
                    .WithParameter("@customerId", CustomerId.ToString());


                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);


                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {

                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return results;
        }


        public async Task<bool> UpdateDealerAsync(Dealer dealer)
        {
            try
            {
                // Replace the existing estimator document in Cosmos DB with the updated estimator
                await _container.ReplaceItemAsync(dealer, dealer.id.ToString());

                return true; // Return true if the update was successful
            }
            catch (Exception ex)
            {
                // Log the exception if needed (optional)
                // You can replace this with a logging library (e.g., Serilog, NLog)
                Console.WriteLine($"Error updating dealer: {ex.Message}");

                return false; // Return false if an error occurred
            }
        }

        public async Task UpdateCustomerAsync(T customer)
        {
            try
            {
                // Replace the existing dealer document in Cosmos DB with the updated dealer
                await _container.ReplaceItemAsync(customer, (customer as Customer).id.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating customer in Cosmos DB", ex);
            }
        }


        public async Task<bool> UpdateBuilderAsync(Builder builder)
        {
            try
            {
                // Replace the existing estimator document in Cosmos DB with the updated estimator
                await _container.ReplaceItemAsync(builder, builder.id.ToString());

                return true; // Return true if the update was successful
            }
            catch (Exception ex)
            {
                // Log the exception if needed (optional)
                // You can replace this with a logging library (e.g., Serilog, NLog)
                Console.WriteLine($"Error updating builder: {ex.Message}");

                return false; // Return false if an error occurred
            }
        }

        public async Task<bool> UpdateEstimatorAsync(Estimator estimator)
        {
            try
            {
                // Replace the existing estimator document in Cosmos DB with the updated estimator
                await _container.ReplaceItemAsync(estimator, estimator.id.ToString());

                return true; // Return true if the update was successful
            }
            catch (Exception ex)
            {
                // Log the exception if needed (optional)
                // You can replace this with a logging library (e.g., Serilog, NLog)
                Console.WriteLine($"Error updating estimator: {ex.Message}");

                return false; // Return false if an error occurred
            }
        }


        public async Task<bool> UpdateTechnicianAsync(Technician technician)
        {
            try
            {

                await _container.ReplaceItemAsync(technician, technician.id.ToString());

                return true;
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error updating technician: {ex.Message}");

                return false;
            }
        }



        public async Task<List<T>> VerifyUserApproval(string UserId)
        {
            var approvallist = new List<T>();
            try
            {
                var queryDefinition = new QueryDefinition("select * from c where c.ApprovedBy !=null and c.RequestedDate !=null and c.Userid=@UserId")
                    .WithParameter("@UserId", UserId);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    approvallist.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error:{ex.Message}");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }


            return approvallist;
        }

        public async Task<List<T>> GetPendingActionsAsync(string State = null, string District = null, string ZipCode = null)
        {
            var pendingactions = new List<T>();
            try
            {



                var query = @"
SELECT 
   * 
FROM c
where
c.RaiseTicketId !=null  and c.Date !=null and c.status = 'Pending'"; // WHERE 1=1 allows for appending dynamic conditions easily

                // Apply filters based on input conditions

                // Rule 1: Include State as a mandatory filter if provided
                if (!string.IsNullOrEmpty(State))
                    query += " AND c.State = @State";

                if (!string.IsNullOrEmpty(State))
                    query += " AND c.District = @District";

                var queryDefinition = new QueryDefinition(query);

                if (!string.IsNullOrEmpty(State))
                    queryDefinition = queryDefinition.WithParameter("@State", State);

                if (!string.IsNullOrEmpty(District))
                    queryDefinition = queryDefinition.WithParameter("@District", District);

                if (!string.IsNullOrEmpty(ZipCode))
                    queryDefinition = queryDefinition.WithParameter("@ZipCode", ZipCode);


                // Execute the query
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    pendingactions.AddRange(response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return pendingactions;
        }


        // Rule 2: Include District only if State is also provided
        public async Task<T> GetRaiseTicketInvoice(string RaiseTicketId)
        {
            try
            {
                if (string.IsNullOrEmpty(RaiseTicketId))
                {
                    throw new ArgumentException(nameof(RaiseTicketId), "RaiseTicketId cannot be null or Empty.");
                }

                var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.RaiseTicketId != null AND c.CustomerName != null AND c.id = @RaiseTicketId")
                    .WithParameter("@RaiseTicketId", RaiseTicketId);
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        return item;
                    }
                }

                return default;
            }
            catch (CosmosException ex)
            {

                return default;
            }
            catch (Exception ex)
            {

                return default;
            }
        }


        public async Task<T> GetTechnicianDetailsForInvoice(string TechnicianId)
        {
            try
            {
                if (string.IsNullOrEmpty(TechnicianId))
                {
                    throw new ArgumentException(nameof(TechnicianId), "TechnicianId cannot be null or Empty.");
                }

                var queryDefinition = new QueryDefinition("SELECT * FROM c where c.TechnicianId !=null  and c.NumberOfTechnicians !=null  and c.UserId=@TechnicianId")
                    .WithParameter("@TechnicianId", TechnicianId);
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        return item;
                    }
                }

                return default;
            }
            catch (CosmosException ex)
            {

                return default;
            }
            catch (Exception ex)
            {

                return default;
            }
        }
        public async Task<T> GetDealerDetailsForInvoice(string DealerId)
        {
            try
            {
                if (string.IsNullOrEmpty(DealerId))
                {
                    throw new ArgumentException(nameof(DealerId), "DealerId cannot be null or Empty.");
                }

                var queryDefinition = new QueryDefinition("  SELECT * FROM c where c.DealerId !=null  and c.DealerFirmName !=null  and c.OwnershipName !=null and c.UserId=@DealerId")



                    .WithParameter("@DealerId", DealerId);
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        return item;
                    }
                }

                return default;
            }
            catch (CosmosException ex)
            {

                return default;
            }
            catch (Exception ex)
            {

                return default;
            }
        }


        public async Task<T> GetRaiseTicketDetailsForTrader(string RaiseTicketId)
        {
            try
            {
                if (string.IsNullOrEmpty(RaiseTicketId))
                {
                    throw new ArgumentException(nameof(RaiseTicketId), "RaiseTicketId cannot be null or Empty.");
                }

                var queryDefinition = new QueryDefinition("SELECT * FROM c  where c.DeliveryNoteId !=null  and  c.Option1Day !=null and  c.TicketId=@RaiseTicketId")
                    .WithParameter("@RaiseTicketId", RaiseTicketId);
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        return item;
                    }
                }

                return default;
            }
            catch (CosmosException ex)
            {

                return default;
            }
            catch (Exception ex)
            {

                return default;
            }
        }

        public async Task<T> GetPaymentDetailsByRaiseTicketId(string RaiseTicketId)
        {
            try
            {
                if (string.IsNullOrEmpty(RaiseTicketId))
                {
                    throw new ArgumentException(nameof(RaiseTicketId), "RaiseTicketId cannot be null or Empty.");
                }

                var queryDefinition = new QueryDefinition("SELECT * FROM c   where c.PaymentId !=null  and   c.PaymentMode  !=null  and c.RaiseTicketId=@RaiseTicketId")
                    .WithParameter("@RaiseTicketId", RaiseTicketId);
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        return item;
                    }
                }

                return default;
            }
            catch (CosmosException ex)
            {

                return default;
            }
            catch (Exception ex)
            {

                return default;




            }

        }



        public async Task<List<T>> GetRaiseTicketsByTechnicalAgency()
        {
            var approvallist = new List<T>();
            try
            {
                var queryDefinition = new QueryDefinition("SELECT * FROM c where c.RaiseTicketId !=null   and c.AssignedTo='Technical Agency'");


                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    approvallist.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error:{ex.Message}");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }


            return approvallist;
        }



        public async Task<List<T>> GetTechnicianMobileAndEmail(string Category, string District)
        {
            try
            {
                if (string.IsNullOrEmpty(Category))
                {
                    throw new ArgumentException(nameof(Category), "Category cannot be null or Empty.");
                }

                var queryDefinition = new QueryDefinition("SELECT c.EmailAddress, c.PhoneNumber FROM c WHERE c.TechnicianId != null AND c.Address != null AND c.Category = @Category AND c.District = @District")
                    .WithParameter("@Category", Category)
                    .WithParameter("@District", District);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);
                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }


        public async Task<List<T>> GetTechnicianPincodesByCategory(string category)
        {
            try
            {
                if (string.IsNullOrEmpty(category))
                {
                    throw new ArgumentException(nameof(category), "Category cannot be null or Empty.");
                }

                var queryDefinition = new QueryDefinition("select distinct c.ZipCode from  c where c.TechnicianId !=null and c.TechnicianFullName  !=null and c.Category=@category").WithParameter("@category", category);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();

                    results.AddRange(response);
                }

                return results;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }


        }


        public async Task<List<T>> GetTechniciannamesByPincode(string pincode, string category)
        {
            try
            {
                if (string.IsNullOrEmpty(pincode))
                {
                    throw new ArgumentException(nameof(pincode), "pincode cannot be null or Empty.");
                }

                var queryDefinition = new QueryDefinition("select c.TechnicianFullName from  c where c.TechnicianId !=null and c.TechnicianFullName !=null and c.ZipCode=@pincode  and c.Category = @category")
                    .WithParameter("@pincode", pincode)
                    .WithParameter("@category", category);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();

                    results.AddRange(response);
                }

                return results;
            }
            catch (CosmosException ex)
            {
                return new List<T>();
            }
            catch (Exception ex)
            {
                return new List<T>();
            }


        }



        public async Task<List<T>> GetDealerMobileAndEmail(string District)
        {
            try
            {
                if (string.IsNullOrEmpty(District))
                {
                    throw new ArgumentException(nameof(District), "CatDistrictegory cannot be null or Empty.");
                }

                var queryDefinition = new QueryDefinition("SELECT c.EmailAddress, c.PhoneNumber FROM c WHERE c.DealerId != null AND c.Address != null AND c.District = @District")

                    .WithParameter("@District", District);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);
                var results = new List<T>(); // Store all results

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (CosmosException ex)
            {
                return new List<T>(); // Return empty list in case of exception
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }



        public async Task<List<T>> GetRaiseAQuoteDetailsByTechnicianIdAndRiseTicketId(string TicketId, string TechnicianId)
        {
            try
            {
                if (string.IsNullOrEmpty(TicketId))
                {
                    throw new ArgumentException(nameof(TicketId), "TicketId cannot be null or Empty.");
                }

                var queryDefinition = new QueryDefinition("select * from  c where c.RaiseAQuoteId !=null  and c.TicketId !=null   and c.TicketId=@TicketId    and   c.TechnicianId=@TechnicianId")
                    .WithParameter("@TicketId", TicketId)
                    .WithParameter("@TechnicianId", TechnicianId);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);
                var results = new List<T>(); // Store all results

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (CosmosException ex)
            {
                return new List<T>(); // Return empty list in case of exception
            }
            catch (Exception ex)
            {
                return new List<T>();
            }
        }









        //    public async Task<List<RaiseTicket>> GetNotificationsByExistingTechnicianId(
        //string district, string category, string technicianId)

        //    {
        //        var raiseTickets = new List<RaiseTicket>();

        //        try
        //        {
        //            var ticketQuery = new QueryDefinition(@"
        //         SELECT * FROM c 
        //        WHERE c.RaiseTicketId !=null 
        //        AND c.District = @district 
        //        ")
        //                .WithParameter("@district", district)
        //                ;

        //            var ticketIterator = _container.GetItemQueryIterator<RaiseTicket>(ticketQuery);

        //            while (ticketIterator.HasMoreResults)
        //            {
        //                var response = await ticketIterator.ReadNextAsync();
        //                raiseTickets.AddRange(response);
        //            }

        //            var filteredTickets = new List<RaiseTicket>();

        //            foreach (var ticket in raiseTickets)
        //            {
        //                var quoteQuery = new QueryDefinition(@"
        //            SELECT * FROM c 
        //            WHERE c.RaiseAQuoteId !=null AND c.TicketId = @ticketId 
        //            AND c.TechnicianId = @technicianId")
        //                    .WithParameter("@ticketId", ticket.RaiseTicketId)
        //                    .WithParameter("@technicianId", technicianId);

        //                var quoteIterator = _container.GetItemQueryIterator<RaiseAQuote>(quoteQuery);
        //                var quoteResponse = await quoteIterator.ReadNextAsync();

        //                if (quoteResponse.Count > 0)
        //                {
        //                    filteredTickets.Add(ticket);
        //                }
        //            }

        //            return filteredTickets;
        //        }
        //        catch (CosmosException ex)
        //        {
        //            Console.WriteLine($"[ERROR] Cosmos DB error: {ex.Message}");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"[ERROR] Internal server error: {ex.Message}");
        //        }

        //        return new List<RaiseTicket>();
        //    }



        public async Task<List<RaiseTicket>> GetNotificationsByExistingTechnicianId(
string district, string category, string technicianId)
        {
            var tickets = new List<RaiseTicket>();
            try
            {
                var queryDefinition = new QueryDefinition(@"
            SELECT * FROM c 
            WHERE c.RaiseTicketId !=null 
            
            AND c.District = @district 
            AND  ARRAY_CONTAINS(c.TechnicianList, @technicianId)
            ORDER BY c.Date DESC")
                    .WithParameter("@district", district)

                    .WithParameter("@technicianId", technicianId);

                var queryIterator = _container.GetItemQueryIterator<RaiseTicket>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    tickets.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return tickets; // Return results
        }




        //    public async Task<List<RaiseTicket>> GetRaiseTicketNotificationsByNotExistTechnicianId(
        //string district, string category, string technicianId)
        //    {
        //        var raiseTickets = new List<RaiseTicket>();

        //        try
        //        {
        //            var ticketQuery = new QueryDefinition(@"
        //        SELECT * FROM c 
        //        WHERE c.RaiseTicketId !=null AND c.AssignedTo = 'Technical Agency' 
        //        AND c.District = @district 
        //        AND c.Category = @category")
        //                .WithParameter("@district", district)
        //                .WithParameter("@category", category);

        //            var ticketIterator = _container.GetItemQueryIterator<RaiseTicket>(ticketQuery);

        //            while (ticketIterator.HasMoreResults)
        //            {
        //                var response = await ticketIterator.ReadNextAsync();
        //                raiseTickets.AddRange(response);
        //            }

        //            var filteredTickets = new List<RaiseTicket>();

        //            foreach (var ticket in raiseTickets)
        //            {
        //                var quoteQuery = new QueryDefinition(@"
        //            SELECT * FROM c 
        //            WHERE c.RaiseAQuoteId !=null AND c.TicketId = @ticketId 
        //            AND c.TechnicianId = @technicianId")
        //                    .WithParameter("@ticketId", ticket.RaiseTicketId)
        //                    .WithParameter("@technicianId", technicianId);

        //                var quoteIterator = _container.GetItemQueryIterator<RaiseAQuote>(quoteQuery);
        //                var quoteResponse = await quoteIterator.ReadNextAsync();

        //                if (quoteResponse.Count == 0)
        //                {
        //                    filteredTickets.Add(ticket);
        //                }
        //            }

        //            return filteredTickets;
        //        }
        //        catch (CosmosException ex)
        //        {
        //            Console.WriteLine($"[ERROR] Cosmos DB error: {ex.Message}");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"[ERROR] Internal server error: {ex.Message}");
        //        }

        //        return new List<RaiseTicket>();
        //    }



        public async Task<List<RaiseTicket>> GetRaiseTicketNotificationsByNotExistTechnicianId(
string district, string category, string technicianId)
        {
            var tickets = new List<RaiseTicket>();
            try
            {
                var queryDefinition = new QueryDefinition(@"
            SELECT * FROM c 
            WHERE c.RaiseTicketId !=null 
            AND c.Category = @category 
            AND c.District = @district 
            AND NOT ARRAY_CONTAINS(c.TechnicianList, @technicianId)
            ORDER BY c.Date DESC")
                    .WithParameter("@district", district)
                    .WithParameter("@category", category)
                    .WithParameter("@technicianId", technicianId);

                var queryIterator = _container.GetItemQueryIterator<RaiseTicket>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    tickets.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return tickets; // Return results
        }


        //        public async Task<List<RaiseTicket>> GetNotificationsByExistingDealerId(
        //string category, string district, string dealerId)

        //        {
        //            var raiseTickets = new List<RaiseTicket>();

        //            try
        //            {
        //                var ticketQuery = new QueryDefinition(@"
        //            SELECT * FROM c  where c.RaiseTicketId !=null AND
        //c.District=@district  and c.LowestBidderTechnicainId !=null")
        //                    .WithParameter("@district", district);


        //                var ticketIterator = _container.GetItemQueryIterator<RaiseTicket>(ticketQuery);

        //                while (ticketIterator.HasMoreResults)
        //                {
        //                    var response = await ticketIterator.ReadNextAsync();
        //                    raiseTickets.AddRange(response);
        //                }

        //                var filteredTickets = new List<RaiseTicket>();

        //                foreach (var ticket in raiseTickets)
        //                {
        //                    var quoteQuery = new QueryDefinition(@"
        //                SELECT * FROM c 
        //                WHERE c.RaiseAQuoteByDealerId !=null AND c.TicketId = @ticketId 
        //                AND c.DealerId = @dealerId")
        //                        .WithParameter("@ticketId", ticket.RaiseTicketId)
        //                        .WithParameter("@dealerId", dealerId);

        //                    var quoteIterator = _container.GetItemQueryIterator<RaiseAQuoteByDealer>(quoteQuery);
        //                    var quoteResponse = await quoteIterator.ReadNextAsync();

        //                    if (quoteResponse.Count > 0)
        //                    {
        //                        filteredTickets.Add(ticket);
        //                    }
        //                }

        //                return filteredTickets;
        //            }
        //            catch (CosmosException ex)
        //            {
        //                Console.WriteLine($"[ERROR] Cosmos DB error: {ex.Message}");
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"[ERROR] Internal server error: {ex.Message}");
        //            }

        //            return new List<RaiseTicket>();
        //        }



        public async Task<List<RaiseTicket>> GetNotificationsByExistingDealerId(
        string category, string district, string dealerId)
        {
            var tickets = new List<RaiseTicket>();
            try
            {
                var queryDefinition = new QueryDefinition(@"
            SELECT * FROM c 
            WHERE c.RaiseTicketId !=null 
            
            AND c.District = @district 
            AND  ARRAY_CONTAINS(c.DealerList, @dealerId)
            ORDER BY c.Date DESC")
                    .WithParameter("@district", district)

                    .WithParameter("@dealerId", dealerId);

                var queryIterator = _container.GetItemQueryIterator<RaiseTicket>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    tickets.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return tickets; // Return results
        }




        //        public async Task<List<RaiseTicket>> GetRaiseTicketNotificationsByNotExistDealerId(
        //     string category, string district, string dealerId)
        //        {
        //            var raiseTickets = new List<RaiseTicket>();

        //            try
        //            {
        //                var ticketQuery = new QueryDefinition(@"
        //            SELECT * FROM c  where c.RaiseTicketId !=null and c.AssignedTo='Dealer/Trader' and 
        //c.District=@district and c.Category=@category and c.LowestBidderTechnicainId !=null")
        //                    .WithParameter("@district", district)
        //                    .WithParameter("@category", category);

        //                var ticketIterator = _container.GetItemQueryIterator<RaiseTicket>(ticketQuery);

        //                while (ticketIterator.HasMoreResults)
        //                {
        //                    var response = await ticketIterator.ReadNextAsync();
        //                    raiseTickets.AddRange(response);
        //                }

        //                var filteredTickets = new List<RaiseTicket>();

        //                foreach (var ticket in raiseTickets)
        //                {
        //                    var quoteQuery = new QueryDefinition(@"
        //                SELECT * FROM c 
        //                WHERE c.RaiseAQuoteByDealerId !=null AND c.TicketId = @ticketId 
        //                AND c.DealerId = @DealerId")
        //                        .WithParameter("@ticketId", ticket.RaiseTicketId)
        //                        .WithParameter("@DealerId", dealerId);


        //                    var quoteIterator = _container.GetItemQueryIterator<RaiseAQuoteByDealer>(quoteQuery);
        //                    var quoteResponse = await quoteIterator.ReadNextAsync();

        //                    if (quoteResponse.Count == 0)
        //                    {
        //                        filteredTickets.Add(ticket);
        //                    }
        //                }

        //                return filteredTickets;
        //            }
        //            catch (CosmosException ex)
        //            {
        //                Console.WriteLine($"[ERROR] Cosmos DB error: {ex.Message}");
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"[ERROR] Internal server error: {ex.Message}");
        //            }

        //            return new List<RaiseTicket>();
        //        }




        public async Task<List<RaiseTicket>> GetRaiseTicketNotificationsByNotExistDealerId(string category, string district, string dealerId)
        {
            var tickets = new List<RaiseTicket>();
            try
            {
                var queryDefinition = new QueryDefinition(@"
            SELECT * FROM c 
            WHERE c.RaiseTicketId !=null 
            AND c.Category = @category 
            AND c.District = @district 
            AND NOT ARRAY_CONTAINS(c.DealerList, @dealerId)
            ORDER BY c.Date DESC")
                    .WithParameter("@district", district)
                    .WithParameter("@category", category)
                    .WithParameter("@dealerId", dealerId);

                var queryIterator = _container.GetItemQueryIterator<RaiseTicket>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    tickets.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            return tickets; // Return results
        }





        public async Task<List<T>
            > GetTechnicianOrders(string District)
        {
            var approvallist = new List<T>();
            try
            {
                var queryDefinition = new QueryDefinition("SELECT * FROM c Where  c.District=@District and c.internalStatus='Customer Approved'")
                    .WithParameter("@District", District);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    approvallist.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error:{ex.Message}");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }


            return approvallist;
        }














        public async Task<T> GetGSTAccountDetails(string profileType, string category)
        {
            var gstaccountdetails = new List<T>();
            try
            {
                var queryDefinition = new QueryDefinition("select * from c where c.accountid !=null and  c.ProfileType=@profileType and c.Category=@category")
                    .WithParameter("@profileType", profileType)
                   .WithParameter("@category", category);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    gstaccountdetails.AddRange(response);
                }
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB error:{ex.Message}");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            if (gstaccountdetails.Any())
                return gstaccountdetails.First();

            return null;
        }





        public async Task<List<T>> GetTrackTicketsByCustomerId(string customerId)
        {
            var trackTicket = new List<T>();
            try
            {

                var queryDefinition = new QueryDefinition("SELECT * FROM c where c.RaiseTicketId !=null and c.Address !=null and c.internalStatus!='Closed'  and c.CustomerId=@customerId ORDER BY c.Date DESC ")
                    .WithParameter("@customerId", customerId);


                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    trackTicket.AddRange(response);
                }

            }
            catch (CosmosException ex)
            {
                // Log Cosmos DB specific exceptions
                Console.WriteLine($"Cosmos DB error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log general exceptions
                Console.WriteLine($"Internal server error: {ex.Message}");
            }

            // Return the list of tickets (empty if exceptions occurred)
            return trackTicket;
        }



        public async Task<List<T>> GetSelctedJobsByCategory(string Category)

        {
            try
            {
                if (string.IsNullOrEmpty(Category))
                {
                    throw new ArgumentNullException(nameof(Category), "UserId cannot be null or empty.");
                }

                // Define the query with a parameter for UserId
                var queryDefinition = new QueryDefinition(
                    "SELECT * FROM c where c.uploadBookTechnicianId  !=null and c.Category=@Category ")
                    .WithParameter("@Category", Category);

                // Create a query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var addresses = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    addresses.AddRange(response); // Add all items from the current response
                }

                return addresses; // Return the list of addresses
            }
            catch (CosmosException ex)
            {
                return new List<T>(); // Return an empty list on Cosmos DB-specific errors
            }
            catch (Exception ex)
            {
                return new List<T>(); // Return an empty list on unexpected errors
            }

        }

        public async Task<List<T>> GetUploadJobDescriptionDetails<T>()
        {
            try
            {
                // Define the query correctly using IS NOT NULL
                var queryDefinition = new QueryDefinition(
                    "SELECT * FROM c WHERE c.uploadBookTechnicianId !=null AND c.Category !=null"
                );

                // Create a query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB Error: {ex.Message}");
                return new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<T>();
            }
        }


        public async Task<List<T>> GetBookTechnicianListForAdmin<T>()
        {
            try
            {
                // Corrected query with IS NOT NULL
                var queryDefinition = new QueryDefinition(
                    "SELECT * FROM c WHERE c.BookTechnicianId !=null AND c.Category !=null order by  c.Date desc"
                );

                // Create a query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB Error: {ex.Message}");
                return new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<T>();
            }


        }

        public async Task<List<T>> GetBuyProductDetailsForAdminList<T>()
        {
            try
            {
                // Corrected query with IS NOT NULL
                var queryDefinition = new QueryDefinition(
                    "SELECT * FROM c where c.BuyProductId  !=null and c.TotalPaymentAmount !=null  and c.CustomerId !=null  order by  c.Date desc "
                );

                // Create a query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB Error: {ex.Message}");
                return new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<T>();
            }


        }



        public async Task<List<T>> GetBookTechnicianDetailsForUserList<T>(string UserId)
        {
            try
            {
                // Corrected query with IS NOT NULL
                var queryDefinition = new QueryDefinition(
                    "SELECT * FROM c WHERE c.BookTechnicianId !=null AND c.Category !=null and c.CustomerEmail !=null  and c.CustomerId=@userId "
                ).WithParameter("@userId", UserId);

                // Create a query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB Error: {ex.Message}");
                return new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<T>();
            }


        }


        public async Task<List<T>> GetBookTechnicianNotification<T>(string category, string pincode, string technicianName)
        {
            try
            {
                var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.BookTechnicianId !=null and c.Category=@category " +
                    "and c.TechnicianPincode=@pincode AND ARRAY_CONTAINS(c.TechnicianName, @technicianName) order by c.date desc")
                    .WithParameter("@category", category)
                    .WithParameter("@pincode", pincode)
                    .WithParameter("@technicianName", technicianName);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);

                }
                return results;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"CosmosDB Error: {ex.Message}");
                return new List<T>();
            }

            catch (Exception ex)
            {
                Console.WriteLine($"UnExpected Error: {ex.Message}");
                return new List<T>();
            }

        }


        public async Task<List<T>> GetBuyProductDetailsForUserList<T>(string UserId)
        {
            try
            {
                // Corrected query with IS NOT NULL
                var queryDefinition = new QueryDefinition(
                    "SELECT * FROM c where c.BuyProductId  !=null and c.TotalPaymentAmount !=null  and c.CustomerId=@userId "
                ).WithParameter("@userId", UserId);

                // Create a query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB Error: {ex.Message}");
                return new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<T>();
            }


        }


        public async Task<List<T>> GetBuyProductDetailsForAdmin<T>()
        {
            try
            {
                // Corrected query with IS NOT NULL
                var queryDefinition = new QueryDefinition(
                    "SELECT * FROM c  where c.BuyProductId !=null and c.ProductName !=null and c.UTRTransactionNumber !=null"
                );

                // Create a query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB Error: {ex.Message}");
                return new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<T>();
            }

        }


        public async Task<List<T>> ListOfGuestUsers<T>()
        {
            try
            {
                var queryDefinition = new QueryDefinition("select * from c where   c.CustomerPhotoId  !=null and c.FirstName='Guest' ");
                //.WithParameter("@guest", guest);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);


                }

                return results;
            }

            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB Error: {ex.Message}");
                return new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnExpected Error:{ex.Message} ");

                return new List<T>();
            }
        }


        public async Task<List<T>> GuestUserExistingVerification<T>(string mobileNo)
        {
            try
            {
                var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE  c.FirstName  !=null   and c.MobileVerificationCode !=null  and c.MobileNumber =@mobileNo")
                    .WithParameter("@mobileNo", mobileNo);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;

            }

            catch (CosmosException ex)
            {

                Console.WriteLine($"Cosmos DB Error: {ex.Message}");
                return new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnExpected Error:{ex.Message} ");

                return new List<T>();
            }
        }


        public async Task<T> GetGuestUserProfileData(string profileType, string userId)
        {
            try
            {
                var queryDefiniftion = new QueryDefinition("select * from c where  c.FullName !=null and c.OTPVerificationCode !=null and c.UserId=@UserId and c.ProfileType=@profileType")
                    .WithParameter("@UserId", userId)
                    .WithParameter("@profileType", profileType);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefiniftion);

                var response = await queryIterator.ReadNextAsync();

                return response.Resource.FirstOrDefault();
            }

            catch (Exception ex)
            {
                throw new ApplicationException("Error retrieving guest user profile", ex);
            }
        }

        public async Task<List<T>> GetAllTicketsList<T>(string userId, string type)
        {
            try
            {
                string queryString = "";

                if (type == "raiseTicket")
                {
                    queryString = @"
                SELECT * FROM c  
                WHERE c.RaiseTicketId !=null 
                AND c.Address !=null
                AND c.CustomerId = @userId 
                AND (
                    (ARRAY_LENGTH(c.TechnicianList) = 0 AND ARRAY_LENGTH(c.DealerList) = 0) 
                    OR (ARRAY_LENGTH(c.TechnicianList) > 0 AND ARRAY_LENGTH(c.DealerList) = 0) 
                    OR (ARRAY_LENGTH(c.TechnicianList) = 0 AND ARRAY_LENGTH(c.DealerList) > 0) 
                    OR (ARRAY_LENGTH(c.TechnicianList) > 0 AND ARRAY_LENGTH(c.DealerList) > 0)
                )
                ORDER BY c.Date DESC";
                }
                else if (type == "buyProduct")
                {
                    queryString = @"
               SELECT * FROM c  
WHERE c.BuyProductId !=null 
AND c.ProductName !=null 
AND c.UTRTransactionNumber !=null  and c.status !='Draft'  AND c.CustomerId = @userId order by  c.date  desc  ";
                }
                else if (type == "bookTechnician")
                {
                    queryString = @"
                 SELECT * FROM c  
                WHERE c.BookTechnicianId !=null  
                AND c.Category !=null and c.status !='Draft'   AND c.CustomerId = @userId  order by  c.date  desc  ";
                }
                else if (type == "mart")
                {
                    queryString = @"
 select * from c where c.MartId !=null  and c.IsDelivered !=null  and  c.DeliveryPartnerUserId !=null and c.customerId =@userId order by c.Date desc";
                }
                else if (type == "collections")
                {
                    queryString = @"select * from c where c.LakshmiCollectionId !=null  and c.CustomerId =@userId order by c.Date desc";
                }
                else
                {
                    throw new ArgumentException("Invalid type provided");
                }

                var queryDefinition = new QueryDefinition(queryString).WithParameter("@userId", userId);
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB Error: {ex.Message}");
                return new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<T>();
            }
        }


        public async Task<T> GuestUserVerificationByMobileNo(string mobileNo)
        {
            try
            {
                var queryDefiniftion = new QueryDefinition("select * from c where  c.UserName !=null and c.MobileNo=@mobileNo")
                    .WithParameter("@mobileNo", mobileNo);


                var queryIterator = _container.GetItemQueryIterator<T>(queryDefiniftion);

                var response = await queryIterator.ReadNextAsync();

                return response.Resource.FirstOrDefault();
            }

            catch (Exception ex)
            {
                throw new ApplicationException("Error retrieving guest user profile", ex);
            }

        }


        public async Task<MessageSeenCount> GetMarkMessageSeen(string messageId)
        {
            try
            {
                var queryDefinition = new QueryDefinition(@"
            SELECT VALUE COUNT(1)
            FROM (
                SELECT c.UserId
                FROM c
                WHERE c.messageId = @messageId
                GROUP BY c.UserId
            )")
                    .WithParameter("@messageId", messageId);

                var queryIterator = _container.GetItemQueryIterator<int>(
                    queryDefinition,
                    requestOptions: new QueryRequestOptions() // <- don't restrict partition
                );

                var response = await queryIterator.ReadNextAsync();
                var count = response.Resource.FirstOrDefault();

                return new MessageSeenCount { UsersCount = count };
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error Retrieving MarkMessageSeen Data", ex);
            }
        }

        public async Task<T> GetAddressMaintenanceDataByMobileNo(string mobileNo)
        {
            try
            {
                var queryDefiniftion = new QueryDefinition("select * from c where c.ApartmentName !=null and c.ApartmentMaintenanceId !=null and c.MobileNumber=@mobileNo")
                    .WithParameter("@mobileNo", mobileNo);


                var queryIterator = _container.GetItemQueryIterator<T>(queryDefiniftion);

                var response = await queryIterator.ReadNextAsync();

                return response.Resource.FirstOrDefault();
            }

            catch (Exception ex)
            {
                throw new ApplicationException("Error retrieving guest user profile", ex);
            }
        }





        public Task<T> GetApartmentMaintenanceData(string mobileNumber)
        {
            throw new NotImplementedException();
        }


        public async Task<List<T>> GetChatMessages<T>()
        {

            try
            {
                var queryDefinition = new QueryDefinition(

                    "select * from  c where c.ChatBotId !=null");

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);
                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();

                    results.AddRange(response);
                }

                return results;
            }

            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB Error: {ex.Message}");
                return new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<T>();
            }


        }

        public async Task<List<T>> GetChatMessagesByType<T>(string type)
        {

            try
            {
                var queryDefinition = new QueryDefinition("select * from  c where c.ChatBotId !=null and c.ChatType=@type")
                    .WithParameter("@type", type);

                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {

                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }

            catch (CosmosException ex)
            {

                Console.WriteLine($"CosmosDB Error: {ex.Message}");

                return new List<T>();
            }

            catch (Exception ex)
            {
                Console.WriteLine($"UnExpected Error : {ex.Message}");

                return new List<T>();
            }
        }


        public async Task<List<T>> GetApartmentMaintenanceForAdminList<T>()
        {
            try
            {
                // Corrected query with IS NOT NULL
                var queryDefinition = new QueryDefinition(
                    "select * from c where c.ApartmentName !=null and c.ApartmentRaiseTicketId !=null  and c.phoneNumber != null   order by c.date desc"
                );

                // Create a query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB Error: {ex.Message}");
                return new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<T>();
            }


        }

        public async Task<Dictionary<string, int>> GetApartmentRegistrationsCount()
        {
            try
            {
                string[] ticketStatus = { "Open" };
                var ticketsCounts = new Dictionary<string, int>();

                foreach (string ticket in ticketStatus)
                {
                    // Modify the query for each status
                    string query = "select VALUE count (1) from c where c.ApartmentMaintenanceId !=null and c.PaymentId !=null  and c.IsSubscription= 'Yes'  and c.PaidAmount !=null  and c.Status = @internalStatus";
                    var queryDefinition = new QueryDefinition(query)
                        .WithParameter("@internalStatus", ticket); // Set the status for each iteration

                    var iterator = _container.GetItemQueryIterator<int>(queryDefinition);
                    int count = 0;

                    // Read the result
                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        count += response.FirstOrDefault(); // Add the count to the result
                    }

                    // Add the count for the current status to the dictionary
                    ticketsCounts[ticket] = count;
                }

                return ticketsCounts;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error querying ticket counts: {ex.Message}");
                return null; // Return null if there's an error
            }
        }



        public async Task<List<ChatBot>> GetItemsByUserIdAsync(string userId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.ChatBotId !=null and c.UserId = @userId")
                            .WithParameter("@userId", userId);

            var results = new List<ChatBot>();
            using var iterator = _container.GetItemQueryIterator<ChatBot>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<List<UserLikes>> GetUserLikesAsync(string userId)
        {
            var query = new QueryDefinition("select * from c where c.userLikesId !=null and c.userId =@userId")
                .WithParameter("@userId", userId);
            var results = new List<UserLikes>();
            using var iterator = _container.GetItemQueryIterator<UserLikes>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }



        public async Task<List<UploadGrocery>> GetGroceryItemsByCategory(string Category)
        {
            var query = new QueryDefinition("SELECT * from c where c.GroceryItemId !=null and c.Category=@Category")
                .WithParameter("@Category", Category);
            var results = new List<UploadGrocery>();
            using var iterator = _container.GetItemQueryIterator<UploadGrocery>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }


        public async Task<List<UploadGrocery>> GetAllGroceryItems()
        {
            var query = new QueryDefinition("SELECT * from c where c.GroceryItemId !=null and  c.Status='Approved' and c.Category !=null");
            var results = new List<UploadGrocery>();
            using var iterator = _container.GetItemQueryIterator<UploadGrocery>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }



        public async Task<List<UploadGrocery>> GetAllGroceryItemsForAdmin()
        {
            var query = new QueryDefinition("SELECT * from c where c.GroceryItemId !=null  and c.Category !=null");
            var results = new List<UploadGrocery>();
            using var iterator = _container.GetItemQueryIterator<UploadGrocery>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }


        public async Task<List<Lakshmincollection>> GetAllLakshmiCollections()
        {

            var query = new QueryDefinition("select * from c where c.LakshmicollectionId !=null and c.videos !=null");
            var results = new List<Lakshmincollection>();

            using var iterator = _container.GetItemQueryIterator<Lakshmincollection>(query);

            while (iterator.HasMoreResults)
            {

                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }


        public async Task<List<Lakshmincollection>> GetLakshmiCollectionByCategory(string category)
        {
            var query = new QueryDefinition("select * from c where c.LakshmicollectionId !=null and c.videos !=null  and c.Category=@category")
                .WithParameter("@category", category);

            var results = new List<Lakshmincollection>();

            using var iterator = _container.GetItemQueryIterator<Lakshmincollection>(query);
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }


        public async Task<List<LakshmiMart>> GetAllMartItems()
        {
            var query = new QueryDefinition("select * from c where c.MartId !=null   and c.CustomerPhoneNumber !=null and c.customerId !=null");

            var results = new List<LakshmiMart>();
            using var iterator = _container.GetItemQueryIterator<LakshmiMart>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }


        public async Task<List<DeliveryPartner>> GetDeliveryPartnerByUserId(string UserId)
        {
            var query = new QueryDefinition("select * from c where c.DeliveryPartnerId !=null  and c.DeliveryPartnerName !=null and c.UserId=@userId")
                .WithParameter("@userId", UserId);

            var results = new List<DeliveryPartner>();

            using var iterator = _container.GetItemQueryIterator<DeliveryPartner>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());

            }

            return results;
        }



        public async Task<List<T>> GetAllDeliveryPartners<T>()
        {
            try
            {
                // Corrected query with IS NOT NULL
                var queryDefinition = new QueryDefinition(
                    "select * from c where c.DeliveryPartnerId !=null  and c.DeliveryPartnerName !=null and c.UserId !=null"
                );

                // Create a query iterator
                var queryIterator = _container.GetItemQueryIterator<T>(queryDefinition);

                var results = new List<T>();

                while (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    results.AddRange(response);
                }

                return results;
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos DB Error: {ex.Message}");
                return new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<T>();
            }


        }


        public async Task<List<LakshmiMart>> GetMartTicketsByUserId(string UserId)
        {
            var query = new QueryDefinition("select * from c where c.MartId !=null   and c.CustomerPhoneNumber !=null and c.customerId !=null   and c.Status= 'In Progress' and  c.DeliveryPartnerUserId =@userId")
                .WithParameter("@userId", UserId);

            var results = new List<LakshmiMart>();

            using var iterator = _container.GetItemQueryIterator<LakshmiMart>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());

            }

            return results;
        }

        public async Task<List<LakshmiMartProductResponse>> GetMartItemsByProductName(string productName)
        {
            var query = new QueryDefinition(
                "SELECT TOP 1 c.id, cat.CategoryName, p " +
                "FROM c " +
                "JOIN cat IN c.Categories " +
                "JOIN p IN cat.Products " +
                "WHERE p.ProductName = @ProductName " +
                "ORDER BY c.Date DESC"
            ).WithParameter("@ProductName", productName);

            var results = new List<LakshmiMartProductResponse>();

            using var iterator = _container.GetItemQueryIterator<LakshmiMartProductResponse>(query);
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<List<LakshmiMart>> CheckFirstOrder(string CustomerPhoneNumber)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.MartId != null   and c.CustomerPhoneNumber=@CustomerPhoneNumber")
                .WithParameter("@CustomerPhoneNumber", CustomerPhoneNumber);

            var results = new List<LakshmiMart>();
            using var iterator = _container.GetItemQueryIterator<LakshmiMart>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<List<UploadGrocery>> GetGroceryItemsByproductName(string productname)
        {
            //var query = new QueryDefinition("select * from c where c.GroceryItemId !=null and c.Category  !=null and c.Name=@productname")
            //    .WithParameter("@productname", productname);

            var query = new QueryDefinition(
    "SELECT * FROM c WHERE " +
    "c.GroceryItemId != null AND " +
    "c.Category != null AND " +
    "CONTAINS(c.Name, @productname, true)"
)
.WithParameter("@productname", productname.Trim());
            var results = new List<UploadGrocery>();

            using var iterator = _container.GetItemQueryIterator<UploadGrocery>(query);
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }
        

        public async Task<List<ReferralPoints>> GetReferralpointsByUserId(string referreId)
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.referreId = @referreId"
            ).WithParameter("@referreId", referreId);

            var results = new List<ReferralPoints>();
            using var iterator = _container.GetItemQueryIterator<ReferralPoints>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }


        public async Task<List<Collections>> GetAllLakshmiCollectionsByopoen()
        {
            var query = new QueryDefinition("select * from c where c.LakshmiCollectionId !=null and c.Status='Open'");

            var results = new List<Collections>();
            using var iterator = _container.GetItemQueryIterator<Collections>(query);

            while (iterator.HasMoreResults)

            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            return results;
        }

        public async Task<List<Lakshmincollection>> GetcollectionItemsByproductName(string productName)
        {
            var query = new QueryDefinition("select * from c where c.LakshmicollectionId !=null  and c.colour !=null and c.ProductName=@ProductName")
                .WithParameter("@ProductName", productName);

            var results = new List<Lakshmincollection>();

            using var iterator = _container.GetItemQueryIterator<Lakshmincollection>(query);
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());

            }
            return results;
        }


        public async Task<List<OffersTransactions>> GetOfferTransactionByUserId(string userId)
        {
            var queryDefinition = new QueryDefinition(
                "    select * from c where  c.TotalWalletAmount !=null and c.UserId=@userId"
            ).WithParameter("@userId", userId);

            var queryIterator = _container.GetItemQueryIterator<OffersTransactions>(queryDefinition);

            var results = new List<OffersTransactions>();

            while (queryIterator.HasMoreResults)
            {
                var response = await queryIterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }




        public async Task<List<UploadBanners>> GetBanners()
        {
            var query = new QueryDefinition("SELECT * FROM c where  c.Title !=null");

            var results = new List<UploadBanners>();
            using var iterator = _container.GetItemQueryIterator<UploadBanners>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<Customer> GetUserProfilesdata(string userId, string profileType)
        {
            try
            {
                string query = @"
            SELECT *
            FROM c
            WHERE c.UserId = @userId
            AND IS_DEFINED(c.CustomerId)";

                var queryDefinition = new QueryDefinition(query)
                    .WithParameter("@userId", userId);

                var iterator = _container.GetItemQueryIterator<Customer>(
                    queryDefinition);

                List<Customer> customers = new();

                while (iterator.HasMoreResults)
                {
                    FeedResponse<Customer> response =
                        await iterator.ReadNextAsync();

                    customers.AddRange(response);
                }

                return customers.FirstOrDefault();
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Cosmos Error: {ex.Message}");
                throw;
            }
        }






        public Task UpdateItemAsync(LakshmiMart existinglakshmiMarts)
        {
            throw new NotImplementedException();
        }
    }



        public class LakshmiMartProductResponse
    {
        public string id { get; set; }
        public string CategoryName { get; set; }
        public Products p { get; set; }
    }
}



   