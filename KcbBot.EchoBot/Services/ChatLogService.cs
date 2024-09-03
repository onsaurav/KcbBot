using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using KcbBot.EchoBot.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace KcbBot.EchoBot.Services
{
    public class ChatLogService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;

        public ChatLogService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["ApiSettings:DbApiKey"];
            _apiUrl = configuration["ApiSettings:DbApiUrl"];
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
        }

        public async Task<bool> SaveChatLogAsync(ChatLog chatLog)
        {
            var json = JsonConvert.SerializeObject(chatLog);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiUrl, content);

            return response.IsSuccessStatusCode;
        }
    }
}
