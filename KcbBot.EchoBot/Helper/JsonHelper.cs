using System.IO;

namespace KcbBot.EchoBot.Helper
{
    public class JsonHelper
    {
        public static string ReadCardJson(string fileName)
        {
            // Construct the full path to the file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "cards", fileName);

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file '{fileName}' was not found in the 'cards' directory.");
            }

            // Read the content of the file and return it as a string
            return File.ReadAllText(filePath);
        }
    }
}
