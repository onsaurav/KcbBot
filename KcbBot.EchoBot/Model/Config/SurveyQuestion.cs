using KcbBot.EchoBot.Services;
using System.Collections.Generic;

namespace KcbBot.EchoBot.Model.Config
{
    public class SurveyQuestion
    {
        public string Question { get; set; }
        public List<SurveyOption> Options { get; set; }
    }
}
