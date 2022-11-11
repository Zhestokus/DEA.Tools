using DEA.Core;
using System;
using System.Collections.Generic;

namespace DEA.Server
{
    class EventController
    {
        [DeaEvent("ValuesController.Get")]
        public List<String> Get()
        {
            Console.WriteLine($"ValuesController.Get Processed");

            var result = new List<String> { "Value1", "Value2", "Value2" };
            return result;
        }

        [DeaEvent("ValuesController.Get")]
        public String Get(int id)
        {
            Console.WriteLine($"ValuesController.Get(id={id}) Processed");

            return "Value1";
        }

        [DeaEvent("ValuesController.Post")]
        public String Post(String value)
        {
            Console.WriteLine($"ValuesController.Post(value={value}) Processed");

            return "Ok";
        }

        [DeaEvent("ValuesController.Put")]
        public String Put(int id, String value)
        {
            Console.WriteLine($"ValuesController.Put(id={id}, value={value}) Processed");

            return "Ok";
        }

        [DeaEvent("ValuesController.Delete")]
        public String Delete(int id)
        {
            Console.WriteLine($"ValuesController.Delete(id={id}) Processed");

            return "Ok";
        }

        [DeaEvent("ValuesController.Test")]
        public String Test(byte? @byte, short? @short, int? @int, long? @long, float? @float, double? @double, Guid? @guid, DateTime? @dateTime)
        {
            Console.WriteLine($"ValuesController.Delete(@byte={@byte}, @short={@short}, @@int={@int}, @long={@long}, @float={@float}, @double={@double}, @guid={@guid}, @dateTime={@dateTime}) Processed");

            return "Ok";
        }
    }
}
