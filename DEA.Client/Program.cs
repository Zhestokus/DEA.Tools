using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DEA.Tools;
using DEA.Tools.Compression.LZ4;
using DEA.Tools.MessageStore.Redis;
using DEA.Tools.MessageHandler.Redis;
using DEA.Tools.Serialization.NewtonsoftJson;

namespace DEA.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var redisServer = "127.0.0.1:6379";

            var processor = new DeaProcessor()
                                .UseRedisHandler(redisServer)
                                .UseRedisStore(redisServer)
                                .UseLz4Compression()
                                .UseNewtonsoftJsonSerializer()
                                .Connect();

            Console.WriteLine("Client running...");

            while (true)
            {
                var text = $"Hello World !!! {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                Console.WriteLine($"Push Message - {text}");

                //processor.Process("HelloWorld", text);

                var question = new QuestionModel
                {
                    ID = Guid.NewGuid(),
                    Date = DateTime.Now,
                    Question = $"Question {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                };

                Console.WriteLine();

                processor.Process("HelloWorld_Act_01", question);
                Console.WriteLine($"Answer1 Act");

                processor.Process("HelloWorld_Act_02");
                Console.WriteLine($"Answer2 Act");

                processor.Process("HelloWorld_Act_03", question);
                Console.WriteLine($"Answer2 Act");

                var answer1 = processor.Process<AnswerModel>("HelloWorld_Func_01", question);
                Console.WriteLine($"Answer1 Func - {answer1.Answer}");

                var answer2 = processor.Process<AnswerModel>("HelloWorld_Func_02");
                Console.WriteLine($"Answer2 Func - {answer1.Answer}");

                var answer3 = processor.Process<AnswerModel>("HelloWorld_Func_03", question);
                Console.WriteLine($"Answer3 Func - {answer2.Answer}");

                Console.ReadLine();
            }
        }
    }
}
