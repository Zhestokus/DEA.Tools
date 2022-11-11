using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Core
{

    [AttributeUsage(AttributeTargets.Method)]
    public class DeaEventAttribute : Attribute
    {
        public DeaEventAttribute()
        {
        }

        public DeaEventAttribute(string eventName)
        {
            EventName = eventName;
        }

        public string EventName { get; private set; }
    }
}
