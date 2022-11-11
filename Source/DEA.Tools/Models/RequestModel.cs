using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.Models
{
    [DataContract]
    [Serializable]
    public class RequestModel
    {
        [DataMember]
        public Guid RequestID { get; set; }

        [DataMember]
        public String SourceHost { get; set; }

        [DataMember]
        public String EventName { get; set; }

        [DataMember]
        public byte[][] Parameters { get; set; }
    }
}
