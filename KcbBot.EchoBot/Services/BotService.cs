using System;
using System.Collections.Generic;
using System.IO;
using KcbBot.EchoBot.Model.Data;
using Newtonsoft.Json;

namespace KcbBot.EchoBot.Services
{
    public class BotService
    {
        private static Random _random = new Random();
        private List<GreetingData> _greetings;
        private List<GreetingData> _greeting2;

        public BotService()
        {
            LoadData();
        }

        private void LoadData()
        {
            var json = File.ReadAllText("Config.json");
            var greetingsData = JsonConvert.DeserializeObject<GreetingsData>(json);
            _greetings = greetingsData.Greetings;
            _greeting2 = greetingsData.Greeting2;
        }

        public GreetingData GetRandomGreeting()
        {
            int index = _random.Next(_greetings.Count);
            return _greetings[index];
        }

        public GreetingData GetRandomGreeting2()
        {
            int index = _random.Next(_greeting2.Count);
            return _greeting2[index];
        }
    }
}
