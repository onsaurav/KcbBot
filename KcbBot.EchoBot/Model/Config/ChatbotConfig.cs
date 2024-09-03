using KcbBot.EchoBot.Services;
using System.Collections.Generic;

namespace KcbBot.EchoBot.Model.Config
{
    public class ChatbotConfig
    {
        public List<SurveyQuestion> EndingSurveyQuestions { get; set; }
        public List<InitialQuestion> InitialQuestions { get; set; }
    }
}
