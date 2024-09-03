using System;

namespace KcbBot.EchoBot.Model
{
    public class ChatLog
    {
        public string Ip { get; set; }
        public string ChatId { get; set; }
        public string Transcript { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
