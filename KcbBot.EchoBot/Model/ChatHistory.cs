using Newtonsoft.Json;
using System;

namespace KcbBot.EchoBot.Model
{
    public class ChatHistory
    {
        public ChatHistory(string sender, string conversationId, string message)
        {
            Sender = sender;
            ConversationId = conversationId;
            Message = message;
            chatTime = DateTime.Now;
        }

        [JsonProperty("Sender")]
        public string Sender { get; set; }

        [JsonProperty("ConversationId")]
        public string ConversationId { get; set; }

        [JsonProperty("chatTime")]
        public DateTime chatTime { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }
    }
}
