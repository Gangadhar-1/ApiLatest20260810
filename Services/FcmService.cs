using Google.Apis.Auth.OAuth2;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class FcmService
{
    private readonly string _projectId = "handyman-234"; // from Firebase console
    private readonly string _jsonPath = "C:\\Users\\Admin\\source\\repos\\APILatestTesting\\APILatest20250325\\Services\\handyman-234-66a01e1cc8a1.json";

    public async Task SendNotificationAsync(string deviceToken, string title, string body)
    {
        GoogleCredential credential;
        using (var stream = new FileStream(_jsonPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
        }

        var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

        var message = new
        {
            message = new
            {
                token = deviceToken,
                notification = new { title, body }
            }
        };

        var jsonMessage = JsonSerializer.Serialize(message);

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.PostAsync(
            $"https://fcm.googleapis.com/v1/projects/{_projectId}/messages:send",
            new StringContent(jsonMessage, Encoding.UTF8, "application/json")
        );

        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine(result);
    }
}
