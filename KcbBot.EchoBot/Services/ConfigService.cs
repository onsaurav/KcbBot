using System.Collections.Generic;
using System.IO;
using KcbBot.EchoBot.Model.Config;
using Newtonsoft.Json;

namespace KcbBot.EchoBot.Services
{
    public class ConfigService
    {
        public List<SurveyQuestion> EndingSurveyQuestions { get; private set; }
        public List<InitialQuestion> InitialQuestions { get; private set; }

        public ConfigService()
        {
            var configJson = File.ReadAllText("Config.json");
            var config = JsonConvert.DeserializeObject<ChatbotConfig>(configJson);
            EndingSurveyQuestions = config.EndingSurveyQuestions;
            InitialQuestions = config.InitialQuestions;
        }

        public SurveyQuestion GetEndingSurveyQuestion(int index)
        {
            if (index >= 0 && index < EndingSurveyQuestions.Count)
            {
                return EndingSurveyQuestions[index];
            }
            return null;
        }
        public InitialQuestion GetInitialQuestion(int index)
        {
            if (index >= 0 && index < InitialQuestions.Count)
            {
                return InitialQuestions[index];
            }
            return null;
        }
        public string GetInitialQuestionAsString(int index)
        {
            var initialQuestion = GetInitialQuestion(index);
            if (initialQuestion != null)
            {
                return initialQuestion.Question;
            }
            return null;
        }
    }
}


