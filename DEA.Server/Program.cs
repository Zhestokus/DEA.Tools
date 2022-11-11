using DEA.Tools;
using DEA.Tools.Compression.LZ4;
using DEA.Tools.MessageStore.Redis;
using DEA.Tools.MessageHandler.Redis;
using DEA.Tools.Serialization.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DEA.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var redisServer = "127.0.0.1:6379";

            var method_Act_1 = (Action<String>)OnHelloWorld_Act;
            var method_Act_2 = (Action<String, QuestionModel>)OnHelloWorld_Act;

            var method_Func_1 = (Func<String, Object>)OnHelloWorld_Func;
            var method_Func_2 = (Func<String, QuestionModel, AnswerModel>)OnHelloWorld_Func;

            var processor = new DeaConnector()
                                .UseRedisHandler(redisServer)
                                .UseRedisStore(redisServer)
                                .UseLz4Compression()
                                .UseNewtonsoftJsonSerializer()

                                .MapEventController<EventController>()

                                .MapEventHandler("HelloWorld_Act_01", OnHelloWorld_Act)
                                .MapEventHandler("HelloWorld_Act_02", method_Act_1)
                                .MapEventHandler("HelloWorld_Act_03", method_Act_2)
                                .MapEventHandler("HelloWorld_Func_01", OnHelloWorld_Func)
                                .MapEventHandler("HelloWorld_Func_02", method_Func_1)
                                .MapEventHandler("HelloWorld_Func_03", method_Func_2)

                                .Connect();

            Console.WriteLine("Server running...");
            Console.ReadLine();
        }

        //parameter less handler
        static void OnHelloWorld_Act(String eventName)
        {

            Console.WriteLine($"Question 1");
        }
        static void OnHelloWorld_Act(String eventName, byte[][] model)
        {

            Console.WriteLine($"Question 2 - {model}");
        }
        static void OnHelloWorld_Act(String eventName, QuestionModel model)
        {
            Console.WriteLine($"Question 2 - {model}");
        }

        static Object OnHelloWorld_Func(String eventName)
        {
            Console.WriteLine($"Question 3");

            var asw = new AnswerModel
            {
                ID = Guid.NewGuid(),
                Answer = $"Answer {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                Date = DateTime.Now
            };

            return asw;
        }
        static Object OnHelloWorld_Func(String eventName, byte[][] model)
        {
            Console.WriteLine($"Question 4");

            var asw = new AnswerModel
            {
                ID = Guid.NewGuid(),
                Answer = $"Answer {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                Date = DateTime.Now
            };

            return asw;
        }
        static AnswerModel OnHelloWorld_Func(String eventName, QuestionModel model)
        {
            Console.WriteLine($"Question 5 - {model.Question}");

            var asw = new AnswerModel
            {
                ID = Guid.NewGuid(),
                Answer = $"Answer {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                Date = DateTime.Now
            };

            return asw;
        }
    }
}
