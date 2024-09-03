using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace KcbBot.EchoBot.Services
{
    public interface IExternalApiService
    {
        Task<string> CallPredictionApiAsync(string userResponses, string chatId);
    }

    public class ExternalApiService : IExternalApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _bearerToken;

        public ExternalApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiUrl = configuration["ApiSettings:PredictionApiUrl"];
            _bearerToken = configuration["ApiSettings:AuthorizationToken"];
        }

        public async Task<string> CallPredictionApiAsync(string userResponses, string chatId)
        {
            try
            {
                var payload = new
                {
                    question = userResponses,
                    chatId = chatId
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _bearerToken);

                var response = await _httpClient.PostAsync(_apiUrl, content);

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error calling external API: " + ex.Message);
            }
        }
    }
}
