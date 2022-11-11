using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Core
{
    public interface ILogger
    {
        void Log(String text);
        void Log(Exception ex);
    }
}
