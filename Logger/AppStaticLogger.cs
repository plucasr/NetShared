using System.Net.Http.Headers;
using System.Text;
using Shared.EnvVarLoader;

namespace Shared.Logger;

public static class AppStaticLogger
{
    public static async Task Log(string message, LogLevelType level)
    {
        try
        {
            HttpClient httpClient = new();

            var content = new StringContent(
                "{\"query\":\"query CustomLogger($message:String!,$logLevel:LogLevelType!){customLogger(message:$message,level:$logLevel)}\",\"variables\":{\"message\":\""
                    + message
                    + "\",\"logLevel\":\""
                    + level
                    + "\"}}",
                Encoding.UTF8,
                "application/json"
            );
            if (string.IsNullOrEmpty(Env.STATIC_LOGGER_URL))
            {
                return;
            }
            httpClient.BaseAddress = new Uri(Env.STATIC_LOGGER_URL);
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "graphql")
            {
                Content = content,
            };


            requestMessage.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            var response = await httpClient.SendAsync(requestMessage);
            Console.WriteLine(response.StatusCode);
            return;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return;
        }
    }
}
