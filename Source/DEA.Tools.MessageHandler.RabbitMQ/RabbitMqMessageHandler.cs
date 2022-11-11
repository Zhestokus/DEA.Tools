using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.MessageHandler.RabbitMQ
{
    public class RabbitMqMessageHandler : MessageHandlerBase
    {
        private String _connectionString;

        private ISet<String> _subscribes;

        public RabbitMqMessageHandler(String connectionString)
        {
            _connectionString = connectionString;
            _subscribes = new HashSet<String>();
        }

        public override void Send(string channel, byte[] data)
        {
            throw new NotImplementedException();
        }

        public override Task SendAsync(string channel, byte[] data)
        {
            throw new NotImplementedException();
        }

        public override bool Subscribe(string channel)
        {
            throw new NotImplementedException();
        }

        public override bool Unsubscribe(string channel)
        {
            throw new NotImplementedException();
        }
    }
}
