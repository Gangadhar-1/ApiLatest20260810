
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.Cosmos;
//using OtpAuthServices.AzureService;
//using OtpAuthServices.Model;

//namespace OtpAuthServices.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class UsersCountController : ControllerBase
//    {

//        private readonly ICosmosDbService<UsersCount> _cosmosDbService;

//        public UsersCountController(ICosmosDbService<UsersCount> cosmosDbService)
//        {
//            _cosmosDbService = cosmosDbService;
//        }

//        [HttpGet("GetTotalCount")]
//        public async Task<IActionResult> GetTotalCount()
//        {
//            try
//            {
//                // Fetch individual counts from Cosmos DB
//                var countsFromUser = await _cosmosDbService.GetAllUsersCountAsync();

//                if (countsFromUser == null)
//                {
//                    return StatusCode(500, "Error retrieving counts from Cosmos DB.");
//                }

//                var totalCount = countsFromUser.Values.Sum();
//                return Ok(new
//                {
//                    DealerCount = countsFromUser.GetValueOrDefault("dealer", 0),
//                    EstimatorCount = countsFromUser.GetValueOrDefault("estimator", 0),
//                    TechnicianCount = countsFromUser.GetValueOrDefault("technician", 0),
//                    BuilderCount = countsFromUser.GetValueOrDefault("builder", 0),
//                    CustomerCount = countsFromUser.GetValueOrDefault("customer", 0),
//                    TotalCount = totalCount
//                });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Internal server error: {ex.Message}");
//            }
//        }

//        //[HttpGet("GetCounts")]
//        //public async Task<IActionResult> GetCounts()
//        //{
//        //    try
//        //    {
//        //        // Get UsersCount from Blob Storage
//        //        var countsFromSingleUser = await _cosmosDbService.GetAllUsersCountAsync();
//        //        if (countsFromSingleUser == null)
//        //        {

//        //            return NotFound("Users count data not found in Cosmosdb storage.");
//        //        }              

//        //        return Ok(new
//        //        {
//        //            DealerCount = countsFromSingleUser.GetValueOrDefault("dealer", 0),
//        //            EstimatorCount = countsFromSingleUser.GetValueOrDefault("estimator", 0),
//        //            TechnicianCount = countsFromSingleUser.GetValueOrDefault("technician", 0),
//        //            BuilderCount = countsFromSingleUser.GetValueOrDefault("builder", 0),
//        //            CustomerCount = countsFromSingleUser.GetValueOrDefault("customer", 0),

//        //        });             

//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        _logger.LogError(ex, "Error occurred while fetching counts.");
//        //        return StatusCode(500, "Internal server error. Please try again later.");
//        //    }
//        //}


//        [HttpGet("GetTotalCountByState")]
//        public async Task<IActionResult> GetTotalCountByState(string state)
//        {
//            try  {
//                //string normalizedstate =state.ToUpper();
//                // Fetch individual counts from Cosmos DB (using the dynamic method)
//                var countsFromUser = await _cosmosDbService.GetAllUsersCountByStateAsync(state);

//                if (countsFromUser == null)
//                {
//                    return StatusCode(500, "Error retrieving counts from Cosmos DB.");
//                }

//                // Calculate the total count by summing all individual counts
//                var totalCount = countsFromUser.Values.Sum();

//                // Return the counts for each ID field dynamically
//                return Ok(new
//                {
//                    DealerCount        = countsFromUser.GetValueOrDefault("DealerId", 0),
//                    CustomerCount      = countsFromUser.GetValueOrDefault("CustomerId", 0),
//                    BuilderCount       = countsFromUser.GetValueOrDefault("BuilderId", 0),
//                    TechnicianCount    = countsFromUser.GetValueOrDefault("TechnicianId", 0),
//                    EstimatorCount     =countsFromUser.GetValueOrDefault("EstimatorId",0),


//                    TotalCount         = totalCount
//                });
//            }
//            catch (Exception ex)
//            {
//                // Handle any exceptions and return appropriate error message
//                return StatusCode(500, $"Internal server error: {ex.Message}");
//            }
//        }


//        [HttpGet("GetTotalCountByStateAndDistrict")]
//        public async Task<IActionResult> GetTotalCountByStateAndDistrict(string state,string district)
//        {
//            try
//            {
//                //string normalizedstate = state.ToLower();
//                //string normalizeddistrict=district.ToLower();
//                // Fetch individual counts from Cosmos DB (using the dynamic method)
//                var countsFromUser = await _cosmosDbService.GetAllUsersCountByStateAndDistrictAsync(state, district);

//                if (countsFromUser == null)
//                {
//                    return StatusCode(500, "Error retrieving counts from Cosmos DB.");
//                }

//                // Calculate the total count by summing all individual counts
//                var totalCount = countsFromUser.Values.Sum();

//                // Return the counts for each ID field dynamically
//                return Ok(new
//                {
//                    DealerCount = countsFromUser.GetValueOrDefault("DealerId", 0),
//                    EstimatorCount = countsFromUser.GetValueOrDefault("EstimatorId", 0),
//                    TechnicianCount = countsFromUser.GetValueOrDefault("TechnicianId", 0),
//                    BuilderCount = countsFromUser.GetValueOrDefault("BuilderId", 0),
//                    CustomerCount = countsFromUser.GetValueOrDefault("CustomerId", 0),
//                    TotalCount = totalCount
//                });
//            }
//            catch (Exception ex)
//            {
//                // Handle any exceptions and return appropriate error message
//                return StatusCode(500, $"Internal server error: {ex.Message}");
//            }
//        }


//        [HttpGet("GetTotalCountByStateAndDistrictAndZipcode")]
//        public async Task<IActionResult> GetTotalCountByStateAndDisrictAndDistrict(string state, string district,string zipcode)
//        {
//            try
//            {
//                //string normalizedstate = state.ToUpper();
//                //string normalizeddistrict = district.ToUpper();

//                // Fetch individual counts from Cosmos DB (using the dynamic method)
//                var countsFromUser = await _cosmosDbService.GetAllUsersCountByStateAndDistrictAndZipcodeAsync(state, district, zipcode);

//                if (countsFromUser == null)
//                {
//                    return StatusCode(500, "Error retrieving counts from Cosmos DB.");
//                }

//                // Calculate the total count by summing all individual counts
//                var totalCount = countsFromUser.Values.Sum();

//                // Return the counts for each ID field dynamically
//                return Ok(new
//                {
//                    DealerCount = countsFromUser.GetValueOrDefault("DealerId", 0),
//                    EstimatorCount = countsFromUser.GetValueOrDefault("EstimatorId", 0),
//                    TechnicianCount = countsFromUser.GetValueOrDefault("TechnicianId", 0),
//                    BuilderCount = countsFromUser.GetValueOrDefault("BuilderId", 0),
//                    CustomerCount = countsFromUser.GetValueOrDefault("CustomerId", 0),
//                    TotalCount = totalCount
//                });
//            }
//            catch (Exception ex)
//            {
//                // Handle any exceptions and return appropriate error message
//                return StatusCode(500, $"Internal server error: {ex.Message}");
//            }
//        }


//    }


//}

﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using OtpAuthServices.AzureService;
using OtpAuthServices.Model;

namespace OtpAuthServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersCountController : ControllerBase
    {

        private readonly ICosmosDbService<UsersCount> _cosmosDbService;

        public UsersCountController(ICosmosDbService<UsersCount> cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        [HttpGet("GetTotalCount")]
        public async Task<IActionResult> GetTotalCount()
        {
            try
            {
                // Fetch individual counts from Cosmos DB
                var countsFromUser = await _cosmosDbService.GetAllUsersCountAsync();

                if (countsFromUser == null)
                {
                    return StatusCode(500, "Error retrieving counts from Cosmos DB.");
                }

                var totalCount = countsFromUser.Values.Sum();
                return Ok(new
                {
                    DealerCount = countsFromUser.GetValueOrDefault("dealer", 0),
                    EstimatorCount = countsFromUser.GetValueOrDefault("estimator", 0),
                    TechnicianCount = countsFromUser.GetValueOrDefault("technician", 0),
                    BuilderCount = countsFromUser.GetValueOrDefault("builder", 0),
                    CustomerCount = countsFromUser.GetValueOrDefault("customer", 0),
                    TotalCount = totalCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //[HttpGet("GetCounts")]
        //public async Task<IActionResult> GetCounts()
        //{
        //    try
        //    {
        //        // Get UsersCount from Blob Storage
        //        var countsFromSingleUser = await _cosmosDbService.GetAllUsersCountAsync();
        //        if (countsFromSingleUser == null)
        //        {

        //            return NotFound("Users count data not found in Cosmosdb storage.");
        //        }              

        //        return Ok(new
        //        {
        //            DealerCount = countsFromSingleUser.GetValueOrDefault("dealer", 0),
        //            EstimatorCount = countsFromSingleUser.GetValueOrDefault("estimator", 0),
        //            TechnicianCount = countsFromSingleUser.GetValueOrDefault("technician", 0),
        //            BuilderCount = countsFromSingleUser.GetValueOrDefault("builder", 0),
        //            CustomerCount = countsFromSingleUser.GetValueOrDefault("customer", 0),

        //        });             

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while fetching counts.");
        //        return StatusCode(500, "Internal server error. Please try again later.");
        //    }
        //}


        [HttpGet("GetTotalCountByState")]
        public async Task<IActionResult> GetTotalCountByState(string state)
        {
            try  {
                //string normalizedstate =state.ToUpper();
                // Fetch individual counts from Cosmos DB (using the dynamic method)
                var countsFromUser = await _cosmosDbService.GetAllUsersCountByStateAsync(state);

                if (countsFromUser == null)
                {
                    return StatusCode(500, "Error retrieving counts from Cosmos DB.");
                }

                // Calculate the total count by summing all individual counts
                var totalCount = countsFromUser.Values.Sum();

                // Return the counts for each ID field dynamically
                return Ok(new
                {
                    DealerCount        = countsFromUser.GetValueOrDefault("DealerId", 0),
                    CustomerCount      = countsFromUser.GetValueOrDefault("CustomerId", 0),
                    BuilderCount       = countsFromUser.GetValueOrDefault("BuilderId", 0),
                    TechnicianCount    = countsFromUser.GetValueOrDefault("TechnicianId", 0),
                    EstimatorCount     =countsFromUser.GetValueOrDefault("EstimatorId",0),


                    TotalCount         = totalCount
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return appropriate error message
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("GetTotalCountByStateAndDistrict")]
        public async Task<IActionResult> GetTotalCountByStateAndDistrict(string state,string district)
        {
            try
            {
                //string normalizedstate = state.ToLower();
                //string normalizeddistrict=district.ToLower();
                // Fetch individual counts from Cosmos DB (using the dynamic method)
                var countsFromUser = await _cosmosDbService.GetAllUsersCountByStateAndDistrictAsync(state, district);

                if (countsFromUser == null)
                {
                    return StatusCode(500, "Error retrieving counts from Cosmos DB.");
                }

                // Calculate the total count by summing all individual counts
                var totalCount = countsFromUser.Values.Sum();

                // Return the counts for each ID field dynamically
                return Ok(new
                {
                    DealerCount = countsFromUser.GetValueOrDefault("DealerId", 0),
                    EstimatorCount = countsFromUser.GetValueOrDefault("EstimatorId", 0),
                    TechnicianCount = countsFromUser.GetValueOrDefault("TechnicianId", 0),
                    BuilderCount = countsFromUser.GetValueOrDefault("BuilderId", 0),
                    CustomerCount = countsFromUser.GetValueOrDefault("CustomerId", 0),
                    TotalCount = totalCount
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return appropriate error message
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("GetTotalCountByStateAndDistrictAndZipcode")]
        public async Task<IActionResult> GetTotalCountByStateAndDisrictAndDistrict(string state, string district,string zipcode)
        {
            try
            {
                //string normalizedstate = state.ToUpper();
                //string normalizeddistrict = district.ToUpper();

                // Fetch individual counts from Cosmos DB (using the dynamic method)
                var countsFromUser = await _cosmosDbService.GetAllUsersCountByStateAndDistrictAndZipcodeAsync(state, district, zipcode);

                if (countsFromUser == null)
                {
                    return StatusCode(500, "Error retrieving counts from Cosmos DB.");
                }

                // Calculate the total count by summing all individual counts
                var totalCount = countsFromUser.Values.Sum();

                // Return the counts for each ID field dynamically
                return Ok(new
                {
                    DealerCount = countsFromUser.GetValueOrDefault("DealerId", 0),
                    EstimatorCount = countsFromUser.GetValueOrDefault("EstimatorId", 0),
                    TechnicianCount = countsFromUser.GetValueOrDefault("TechnicianId", 0),
                    BuilderCount = countsFromUser.GetValueOrDefault("BuilderId", 0),
                    CustomerCount = countsFromUser.GetValueOrDefault("CustomerId", 0),
                    TotalCount = totalCount
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return appropriate error message
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }


}
