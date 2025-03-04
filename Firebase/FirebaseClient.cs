using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Shared.Firebase;

public class FirebaseClient
{
    public FirebaseClient(string secretJson)
    {
        GoogleCredential credential = GoogleCredential.FromJson(secretJson);

        FirebaseApp.Create(new AppOptions() { Credential = credential });
    }

    public async Task Notify(string token, string message, string notificationTitle)
    {
        var notification = new Notification { Title = notificationTitle, Body = message };

        var messageToSend = new Message() { Notification = notification, Token = token };

        string response = await FirebaseMessaging.DefaultInstance.SendAsync(messageToSend);
        Console.WriteLine($"Successfully sent message: {response}");
    }
}
