using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using KcbBot.EchoBot.Model;
using KcbBot.EchoBot.Storage;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KcbBot.EchoBot.Services
{
    public class MemoryLogService
    {
        private readonly IStore _MemoryStore;

        public MemoryLogService(IStore memoryStore)
        {
            _MemoryStore = memoryStore;
        }

        public async Task<bool> SaveChatLogAsync(string conversationId, ChatLog chatLog)
        {
            try
            {
                await _MemoryStore.SaveAsync(conversationId, JObject.FromObject(chatLog), Guid.NewGuid().ToString());
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

    }
}
