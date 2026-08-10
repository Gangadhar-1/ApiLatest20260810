using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OtpAuthServices.Model;
using System.Data;

[ApiController]
[Route("api/[controller]")]
public class UserProfileController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public UserProfileController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Changed from POST to GET
    //[HttpGet("verify")]
    //public IActionResult VerifyUserProfile([FromQuery] string? mobileNo, [FromQuery] string? emailId, [FromQuery] string type)
    //{
    //    if (string.IsNullOrEmpty(type))
    //    {
    //        return BadRequest("Please provide a valid 'type' query parameter.");
    //    }

    //    using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
    //    {
    //        conn.Open();

    //        using (SqlCommand cmd = new SqlCommand("Usp_Verify_UserProfile", conn))
    //        {
    //            cmd.CommandType = CommandType.StoredProcedure;

    //            // Add parameters based on the query parameters
    //            if (type == "mobileNo" && !string.IsNullOrEmpty(mobileNo))
    //            {
    //                cmd.Parameters.AddWithValue("@MobileNo", mobileNo);
    //                cmd.Parameters.AddWithValue("@EmailId", DBNull.Value);  // Handle null EmailId
    //                cmd.Parameters.AddWithValue("@Type", "mobileNo");
    //            }
    //            else if (type == "emailId" && !string.IsNullOrEmpty(emailId))
    //            {
    //                cmd.Parameters.AddWithValue("@MobileNo", DBNull.Value);  // Handle null MobileNo
    //                cmd.Parameters.AddWithValue("@EmailId", emailId);
    //                cmd.Parameters.AddWithValue("@Type", "emailId");
    //            }
    //            else
    //            {
    //                return BadRequest("Please provide either a valid MobileNo or EmailId based on the Type.");
    //            }

    //            // Execute the stored procedure
    //            using (SqlDataReader reader = cmd.ExecuteReader())
    //            {
    //                if (reader.HasRows)
    //                {
    //                    List<User_profileVerify> users = new List<User_profileVerify>();

    //                    while (reader.Read())
    //                    {
    //                        User_profileVerify user = new User_profileVerify
    //                        {
    //                            UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
    //                            UserName = reader.GetString(reader.GetOrdinal("UserName")),
    //                            Password = reader.GetString(reader.GetOrdinal("Password")),
    //                            MobileNo = reader.GetString(reader.GetOrdinal("MobileNo")),
    //                            EmailId = reader.GetString(reader.GetOrdinal("EmailId")),
    //                            IsMobileNumberValidate = reader.GetBoolean(reader.GetOrdinal("IsMobileNumberValidate")),
    //                            IsEmailIdValidate = reader.GetBoolean(reader.GetOrdinal("IsEmailIdValidate")),
    //                            Status = reader.GetString(reader.GetOrdinal("Status")),
    //                            CreatedDt = reader.GetDateTime(reader.GetOrdinal("CreatedDt")),
    //                            CreatedBy = reader.GetString(reader.GetOrdinal("CreatedBy")),
    //                            UpdatedDt = reader.IsDBNull(reader.GetOrdinal("UpdatedDt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedDt")),
    //                            UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetString(reader.GetOrdinal("UpdatedBy")),
    //                            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
    //                            ProfileType = reader.GetString(reader.GetOrdinal("ProfileType"))
    //                        };

    //                        users.Add(user);
    //                    }

    //                    return Ok(users);  // Return the list of users
    //                }
    //                else
    //                {
    //                    return NotFound("No user found for the given criteria.");
    //                }
    //            }
    //        }
    //    }
    //}
}
