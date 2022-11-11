using System;
using System.Runtime.Serialization;

namespace DEA.Tools.Models
{
    [DataContract]
    [Serializable]
    public class ResponseModel
    {
        [DataMember]
        public Guid RequestID { get; set; }

        [DataMember]
        public String SourceHost { get; set; }

        [DataMember]
        public String EventName { get; set; }

        [DataMember]
        public byte[] ResultData { get; set; }
    }
}
