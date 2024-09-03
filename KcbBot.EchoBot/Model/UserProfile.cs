using System.Collections.Generic;

namespace KcbBot.EchoBot.Model
{
    public class UserProfile
    {
        public string User { get; set; }
        public string ConversationId { get; set; }
        public List<ChatHistory> ChatHistory { get; set; }
        public bool ConversationComplete { get; set; } = false;
    }
}
