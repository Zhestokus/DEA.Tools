using System;

namespace DEA.Core
{
    public class ConsoleLogger : ILogger
    {
        public void Log(String text)
        {
            Console.WriteLine(text);
        }

        public void Log(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
