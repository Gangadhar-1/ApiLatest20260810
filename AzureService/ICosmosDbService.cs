using Microsoft.AspNetCore.Mvc;
using OtpAuthServices.Controllers;
using OtpAuthServices.Model;
using OtpAuthServices.Models;
using static OtpAuthServices.Controllers.MarkMessageSeenController;

namespace OtpAuthServices.AzureService
{
    public interface ICosmosDbService<T> where T : class
    {
        // Create (Add) an item in Cosmos DB
        Task AddItemAsync(T item);

        Task UpdateItemAsync(string id, T item, string partitionKey);
        Task DeleteItemAsync(string id, string partitionKey);

        // Read an item by id
        Task<T> GetItemAsync(string id);

        // Read all items
        Task<IEnumerable<T>> GetItemsAsync(string query = null);

        Task<List<T>> GetCustomersDetailsByStatey(
        string state,
        string district,
        string zipCode,
        string fullname,
        string mobilleNumber,
        string userId);
        Task<List<T>> GetAllCustomersDetails();


        Task<List<CustomerDTO>> GetCustomerDirectoryDetails(
       string searchQuery = null,
       string firstname = null,
       string State = null,
       string District = null,
       string ZipCode = null

      );
        Task<List<T>> GetAllDealersDetails();

        Task<List<T>> GetAllTechniciansDetails();
        Task<List<T>> GetAllBuildersDetails();
        Task<List<T>> GetAllEstimatorsDetails();
        Task<List<T>> GetCustomerDetailsByIUserId(string userId);

        Task<List<T>> GetDealerDetailsByUserId(string userId);
        Task<string> UpdateDocumentAsync(UpdateDocumentRequest request);

        Task<List<T>> GetTechnicianDetailsByUserId(string userId);
        Task<List<T>> GetBuilderDetailsByUserId(string userId);

        Task<List<Technician>> GetTechnicianDirectoryDetails(
      string searchQuery = null,
      string State = null,
      string District = null,
      string ZipCode = null,
      string Status = null);
        Task<List<T>> GetEstimatorDetailsByUserId(string userId);

        // Update an item
        Task UpdateItemAsync(T item);

        // Delete an item
        Task DeleteItemAsync(string id);

        Task<T> GetUserByEmailOrMobileAsync(string value);

        Task<T> GetUserByLogin(string username, string password);
        Task<T> GetUserByUserIdAsync(string username);

        Task<T> GetUserProflie(string value, string ProfileType);
        Task<T> GetDealerProflie(string value, string ProfileType);

        Task<T> GetEstimatorProflie(string value, string ProfileType);

        Task<T> GetTechnicianProflie(string value, string ProfileType);

        Task<T> GetBuilderProflie(string value, string ProfileType);
        Task<Dictionary<string, int>> GetAllUsersCountAsync();
        Task<Dictionary<string, int>> GetAllUsersCountByStateAsync(string state);
        Task<Dictionary<string, int>> GetAllUsersCountByStateAndDistrictAsync(string state, string district);
        Task<Dictionary<string, int>> GetAllUsersCountByStateAndDistrictAndZipcodeAsync(string state, string district, string zipcode);

        Task<List<T>> GetRaiseTicketsAsync(string customerId);

        Task<List<T>> GetAddress(string userId);

        Task<List<T>> GetSecondaryAddress(string ProfileType, string userId);


        Task<List<T>> GetBuyProductdetails(string BuyProductId);


        //Task<Dictionary<string, int>> GetTotalCountsOfRaiseTicket();
        Task<Dictionary<string, int>> GetTotalCountOfRaiseTicketsByStateWise(string state);
        Task<Dictionary<string, int>> GetTotalCountOfRaiseTicketStateWiseAndDistrictWise(string state, string district);
        Task<Dictionary<string, int>> GetTotalCountOfRaiseTicketByStateWiseAndDistrictWiseAndZipcodeWise(string state, string district, string zipCode);

        Task<Dictionary<string, int>> GetTotalCountOfBuyProducts();

        Task<Dictionary<string, int>> GetTotalCountOfBuyProductsByStateWise(string state);

        Task<Dictionary<string, int>> GetTotalCountOfBuyProductsByStateWiseAndDistrictWise(string state, string district);

        Task<Dictionary<string, int>> GetTotalCountOfBuyProductsByStateWiseAndDistrictWiseAndZipcodeWise(string state, string district, string zipCode);
        Task<Dictionary<string, int>> GetRaiseTicketCountAsync();


        Task<Dictionary<string, int>> GetRaiseTicketCountByStateAsync(string state);


        Task<List<T>> GetProductList(string ProductOwnedBy);

        //Task<bool> UpdateAddressAsync(string addressId, Address address);

        Task<List<T>> GetTrackTicketDetailsAsync();
        Task CreateItemAsync(BuyProduct buyProduct);

        Task<List<T>> GetAdminProductList();
        Task<List<T>> GetProductNamesByCategory(string category);

        Task<List<T>> GetAddMember();

        Task<List<T>> GetAddTechnicians();
        Task<List<T>> GetAddMemberDetailsById(string id);
        Task<List<T>> GetAddTechnicianDetailsById(string id);
        Task<List<EstimatorDTONew>> GetEstimatorDirectoryDetails(
     string searchQuery = null,
     string State = null,
     string District = null,
     string ZipCode = null,
     string Status = null);


        //Task<List<T>> GetBuildersDirectoryDetails(string searchQuery);

        Task<List<BUilderDirectoryDTO>> GetBuilderDirectoryDetails(
     string searchQuery = null,
     string State = null,
     string District = null,
     string ZipCode = null,
     string Status = null);

        Task<List<DealerDTO>> GetDealerDirectoryDetails(
   string searchQuery = null,
   string State = null,
   string District = null,
   string zipcode = null,
    string Status = null);


        Task<List<T>> GetRecentNotifications();

        Task<List<T>> GetRaiseTicketForTechnician(string state, string district);
        Task<List<T>> GetRaiseTicketForTechnicians(string state, string district);
        Task<List<T>> GetRaiseAQuoteDetails();
        Task<List<T>> GetRaiseTicketNotificationsByDistrict(string district, string category);


        Task<List<T>> GetRaiseAQuoteDetailsById(string raiseAQuotetId);

        Task<T> GetRaiseAQuoteDetailsByTechnicianId(string raiseAQuotetId, string TechnicianId);


        Task<Dictionary<string, int>> GetAllRaiseTicketsCounts();

        Task<List<T>> GetRaiseTicketsForCustomer();

        Task<List<T>> GetRaiseTicketNotificationsByCustomerId(string customerId);
        Task<List<T>> GetDealerByIdAsync<T>(Guid DealerId);
        Task<List<T>> GetEstimatorByIdAsync<T>(Guid EstimatorId);

        Task<List<T>> GetBuilderByIdAsync<T>(Guid BuilderId);
        Task<List<T>> GetTechnicianByIdAsync<T>(Guid TechnicianId);
        Task<List<T>> GetCustomerByIdAsync<T>(Guid CustomerId);
        Task<bool> UpdateDealerAsync(Dealer dealer);
        Task<List<T>> GetRaiseAQuoteByDealerDetails();
        Task<List<T>> GetRaiseTicketNotificationsByStateAndDistrict(string district, string category);

        Task<List<T>> GetRaiseAQuoteDealerDetailsById(string raiseTicketId, string dealerId);


        Task<List<T>> GetRaiseAQuoteLowestDealerByIdAsync(string raiseAQuotetDealerId);

        Task<List<T>> GetRaiseTicketsForDealer();
        Task<List<T>> GetRaiseTicketsNotificationsForTechnician();
        Task<List<T>> VerifyUserApproval(string UserId);
        Task UpdateCustomerAsync(T customer);
        Task<bool> UpdateBuilderAsync(Builder builder);
        Task<bool> UpdateEstimatorAsync(Estimator estimator);
        Task<bool> UpdateTechnicianAsync(Technician technician);

        Task<List<T>> GetPendingActionsAsync(string State = null, string District = null, string ZipCode = null);

        Task<T> GetRaiseTicketInvoice(string RaiseTicketId);

        Task<T> GetTechnicianDetailsForInvoice(string TechnicianId);
        Task<T> GetDealerDetailsForInvoice(string DealerId);



        Task<T> GetRaiseTicketDetailsForTrader(string RaiseTicketId);
        Task<T> GetPaymentDetailsByRaiseTicketId(string RaiseTicketId);
        Task<List<T>> GetRaiseTicketsByTechnicalAgency();
        Task<List<T>> GetTechnicianMobileAndEmail(string Category, string District);


        Task<List<T>> GetRaiseAQuoteDetailsByTechnicianIdAndRiseTicketId(string TicketId, string TechnicianId);




        Task<List<RaiseTicket>> GetNotificationsByExistingTechnicianId(
string district, string category, string technicianId);

        Task<List<RaiseTicket>> GetRaiseTicketNotificationsByNotExistTechnicianId(
    string district, string category, string technicianId);

        Task<List<RaiseTicket>> GetRaiseTicketNotificationsByNotExistDealerId(
    string category, string district, string dealerId);

        Task<List<RaiseTicket>> GetNotificationsByExistingDealerId(string category, string district, string dealerId);




        Task<T> GetGSTAccountDetails(string profileType, string category);

        Task<List<T>> GetTechnicianOrders(string District);




        Task<List<T>> GetTrackTicketsByCustomerId(string customerId);

        Task<List<T>> GetRaiseTicketsNotificationsForTechnicianForSMS();

        Task<List<T>> GetDealerMobileAndEmail(string District);

        Task<List<T>> GetRaiseTicketsForDealerForSMS();



        Task<List<T>> GetRaiseTicketsNotificationsForLowestTechnicianForSMS();


        Task<List<T>> GetBookTechnicianAddress(string userId);

        Task<List<T>> GetSelctedJobsByCategory(string Category);

        Task<List<T>> GetUploadJobDescriptionDetails<T>();
        Task<List<T>> GetBookTechnicianListForAdmin<T>();
        //Task<List<T>> GetBookTechnicianListForAdmin();

        Task<List<T>> GetBuyProductDetailsForAdmin<T>();

        Task<List<T>> GetAllTicketsList<T>(string userId, string type);

        Task<List<T>> GetBookTechnicianDetailsForUserList<T>(string UserId);

        Task<List<T>> GetBuyProductDetailsForAdminList<T>();
        Task<List<T>> GetBuyProductDetailsForUserList<T>(string UserId);

        Task<List<T>> GetTechnicianPincodesByCategory(string category);

        Task<List<T>> GetTechniciannamesByPincode(string pincode, string category);




        Task<List<T>> GetAllProductList();

        Task<List<T>> GetBookTechnicianNotification<T>(string category, string pincode, string technicianName);

        Task<List<T>> GuestUserExistingVerification<T>(string mobileNo);

        Task<T> GetGuestUserProfileData(string profileType, string userId);


        Task<T> GuestUserVerificationByMobileNo(string mobileNo);

        Task<T> GetApartmentMaintenanceData(string mobileNumber);

        Task<T> GetAddressMaintenanceDataByMobileNo(string mobileNo);

        Task<List<T>> GetApartmentMaintenanceForAdminList<T>();

        Task<Dictionary<string, int>> GetApartmentRegistrationsCount();


        Task<List<T>> ListOfGuestUsers<T>();


        Task<List<T>> GetChatMessages<T>();



        Task<List<T>> GetChatMessagesByType<T>(string type);


        Task<MessageSeenCount> GetMarkMessageSeen(string messageId);


        Task<List<ChatBot>> GetItemsByUserIdAsync(string userId);

        Task<List<UserLikes>> GetUserLikesAsync(string userId);

        Task<List<UploadGrocery>> GetGroceryItemsByCategory(string Category);

        Task<List<UploadGrocery>> GetAllGroceryItems();
        Task<List<Lakshmincollection>> GetAllLakshmiCollections();


        Task<List<Lakshmincollection>> GetLakshmiCollectionByCategory(string category);

        Task<List<LakshmiMart>> GetAllMartItems();


        Task<List<DeliveryPartner>> GetDeliveryPartnerByUserId(string UserId);



        Task<List<T>> GetAllDeliveryPartners<T>();
        Task<List<LakshmiMart>> GetMartTicketsByUserId(string UserId);


        Task<List<LakshmiMartProductResponse>> GetMartItemsByProductName(string productName);

        Task<List<UploadGrocery>> GetGroceryItemsByproductName(string productname);


        Task<List<ReferralPoints>> GetReferralpointsByUserId(string referreId);

        Task<List<Collections>> GetAllLakshmiCollectionsByopoen();
        Task<List<LakshmiMart>> CheckFirstOrder(string CustomerPhoneNumber);
        Task<List<Lakshmincollection>> GetcollectionItemsByproductName(string productName);

        Task<List<UploadGrocery>> GetAllGroceryItemsForAdmin();

        Task<List<UploadBanners>> GetBanners();
        Task UpsertItemAsync(OtpCache otpData);

        Task<Customer> GetUserProfilesdata(string userId, string profileType);
        Task<List<OffersTransactions>> GetOfferTransactionByUserId(string userId);
        Task UpdateItemAsync(LakshmiMart existinglakshmiMarts);
    }
}
