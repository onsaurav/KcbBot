using Microsoft.Bot.Builder;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace KcbBot.EchoBot.Storage
{
    public interface IStore
    {
        Task<(JObject content, string etag)> LoadAsync(string key);

        Task<bool> SaveAsync(string key, JObject content, string etag);

        IDictionary<string, (JObject, string)> GetAll();
    }
}
