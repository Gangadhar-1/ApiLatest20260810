using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.Win32;
using OtpAuthServices.AzureService;
using OtpAuthServices.Controllers;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Azure.Cosmos;
using OtpAuthServices.Models;
using OtpAuthServices.Model;
using OtpAuthServices.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;  // Import this for CosmosClient

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache(); // Add MemoryCache service

// Bind Twilio configuration from appsettings.json
builder.Services.Configure<OtpAuthServices.TwilioSettings>(builder.Configuration.GetSection("Twilio"));

// Add CORS services with unrestricted policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddRateLimiter(_ => _.AddFixedWindowLimiter(policyName: "ratepolicy", Options =>
{
    Options.Window = TimeSpan.FromSeconds(2);
    Options.PermitLimit = 1;
    Options.QueueLimit = 0;
    Options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;

}));



// Configure FormOptions for file upload limits
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB limit for file uploads
    options.MultipartHeadersLengthLimit = 16384;  // 16KB limit for multipart headers
});

// Retrieve Blob Storage and Cosmos DB configuration strings from appsettings.json
string blobConnectionString = builder.Configuration.GetConnectionString("BlobStorage");
string cosmosConnectionString = builder.Configuration.GetConnectionString("CosmosDb");

// Register BlobService using the Blob connection string from configuration
builder.Services.AddSingleton(new BlobService(blobConnectionString));

// Register CosmosDbService for Product entities using configuration settings
string databaseName = builder.Configuration["CosmosDb:DatabaseName"];
string containerName = builder.Configuration["CosmosDb:ContainerName"];

// Initialize CosmosClient with the connection string
var cosmosClient = new CosmosClient(cosmosConnectionString);
// Explicitly register CosmosDbService for each entity (Builder, Dealer)
builder.Services.AddSingleton<ICosmosDbService<Builder>>(sp => new CosmosDbService<Builder>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<Dealer>>(sp => new CosmosDbService<Dealer>(cosmosClient, databaseName, containerName));

builder.Services.AddSingleton<ICosmosDbService<Technician>>(sp => new CosmosDbService<Technician>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<Customer>>(sp => new CosmosDbService<Customer>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<TrackTickets>>(sp => new CosmosDbService<TrackTickets>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<SupportTicket>>(sp => new CosmosDbService<SupportTicket>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<RaiseTicket>>(sp => new CosmosDbService<RaiseTicket>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<RaiseAQuote>>(sp => new CosmosDbService<RaiseAQuote>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<RaiseAQuoteByDealer>>(sp=> new CosmosDbService<RaiseAQuoteByDealer>(cosmosClient,databaseName,containerName));
builder.Services.AddSingleton<ICosmosDbService<Estimator>>(sp => new CosmosDbService<Estimator>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<UsersCount>>(sp => new CosmosDbService<UsersCount>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<Product>>(sp => new CosmosDbService<Product>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<BuyProduct>>(sp => new CosmosDbService<BuyProduct>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<UpdateDocumentRequest>>(sp => new CosmosDbService<UpdateDocumentRequest>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<GuestUser>>(sp => new CosmosDbService<GuestUser>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<UserProfileApproval>>(sp => new CosmosDbService<UserProfileApproval>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<MyAccounts>>(sp => new CosmosDbService<MyAccounts>(cosmosClient, databaseName, containerName));

builder.Services.AddSingleton<ICosmosDbService<Payment>>(sp => new CosmosDbService<Payment>(cosmosClient, databaseName, containerName));

builder.Services.AddSingleton<ICosmosDbService<ApartmentMaintenance>>(sp => new CosmosDbService<ApartmentMaintenance>(cosmosClient, databaseName, containerName));

builder.Services.AddSingleton<ICosmosDbService<ApartmentRaiseTicket>>(sp => new CosmosDbService<ApartmentRaiseTicket>(cosmosClient, databaseName, containerName));


builder.Services.AddSingleton<ICosmosDbService<DeliveryNote>>(sp => new CosmosDbService<DeliveryNote>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<RaiseTicketExtention>>(sp => new CosmosDbService<RaiseTicketExtention>(cosmosClient, databaseName, containerName));

builder.Services.AddSingleton<ICosmosDbService<BookTechnician>>(sp => new CosmosDbService<BookTechnician>(cosmosClient, databaseName, containerName));

builder.Services.AddSingleton<ICosmosDbService<UploadJobDescriptionBookTechnician>>(sp => new CosmosDbService<UploadJobDescriptionBookTechnician>(cosmosClient, databaseName, containerName));

builder.Services.AddTransient<DataGeneratorService>();

builder.Services.AddSingleton<ICosmosDbService<RaiseTicketTracker>>(sp => new CosmosDbService<RaiseTicketTracker>(cosmosClient, databaseName, containerName));

builder.Services.AddSingleton<ICosmosDbService<UserOnBoarding>>(sp => new CosmosDbService<UserOnBoarding>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<AddressModel>>(sp => new CosmosDbService<AddressModel>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<AddMember>>(sp => new CosmosDbService<AddMember>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<AddTechnician>>(sp => new CosmosDbService<AddTechnician>(cosmosClient, databaseName, containerName));

builder.Services.AddSingleton<ICosmosDbService<AddToCart>>(sp => new CosmosDbService<AddToCart>(cosmosClient, databaseName, containerName));

builder.Services.AddSingleton<ICosmosDbService<ChatBot>>(sp => new  CosmosDbService<ChatBot>(cosmosClient,databaseName, containerName));

builder.Services.AddSingleton<ICosmosDbService<MarkMessageSeen>>(sp => new CosmosDbService<MarkMessageSeen>(cosmosClient, databaseName, containerName));

builder.Services.AddSingleton<ICosmosDbService<ChatMessageTabSeenByUser>>(sp=>new CosmosDbService<ChatMessageTabSeenByUser>(cosmosClient,databaseName,containerName));

builder.Services.AddSingleton<ICosmosDbService<UserLikes>>(sp => new CosmosDbService<UserLikes>(cosmosClient, databaseName, containerName));

builder.Services.AddSingleton<ICosmosDbService<UploadGrocery>>(sp => new CosmosDbService<UploadGrocery>(cosmosClient, databaseName, containerName));

builder.Services.AddSingleton<ICosmosDbService<Lakshmincollection>>(sp => new CosmosDbService<Lakshmincollection>(cosmosClient, databaseName, containerName));

builder.Services.AddSingleton<ICosmosDbService<Location>>(sp => new CosmosDbService<Location>(cosmosClient, databaseName, containerName));

builder.Services.AddSingleton<ICosmosDbService<ReferralPoints>>(sp => new CosmosDbService<ReferralPoints >(cosmosClient, databaseName,containerName));
builder.Services.AddSingleton<ICosmosDbService<OffersTransactions>>(sp => new CosmosDbService<OffersTransactions>(cosmosClient, databaseName, containerName));
//builder.Services.AddSingleton<ICosmosDbService<DeliveryPartnerForm>>(sp => new CosmosDbService<DeliveryPartnerForm>(cosmosClient, databaseName, containerName));

//builder.Services.AddSingleton<ICosmosDbService<LakshmiMart_v1>>(sp => new CosmosDbService<LakshmiMart_v1>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<DeliveryPartner>>(sp => new CosmosDbService<DeliveryPartner>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<Collections>>(sp => new CosmosDbService<Collections>(cosmosClient,databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<MartController>>(sp => new CosmosDbService<MartController>(cosmosClient,databaseName,containerName));
builder.Services.AddSingleton<ICosmosDbService<LakshmiMart>>(sp => new CosmosDbService<LakshmiMart>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<LmartLogs>>(sp => new CosmosDbService<LmartLogs>(cosmosClient, databaseName, containerName));
builder.Services.AddSingleton<ICosmosDbService<UploadBanners>>(sp => new CosmosDbService<UploadBanners>(cosmosClient, databaseName, containerName));
builder.Services.Configure<bhashsms>(builder.Configuration.GetSection("BhashSms"));

builder.Services.AddSingleton<ICosmosDbService<OtpCache>>(sp => new CosmosDbService<OtpCache>(cosmosClient, databaseName, containerName));
var app = builder.Build();



if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Handy Man Web API");
    });
}

app.UseRateLimiter();
app.UseHttpsRedirection();

// Enable CORS with the "AllowAll" policy
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();

