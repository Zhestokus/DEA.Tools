using System;
using System.Runtime.Serialization;

namespace DEA.Client
{
    [DataContract]
    public class AnswerModel
    {
        [DataMember(Name = "id")]
        public Guid ID { get; set; }

        [DataMember(Name = "answer")]
        public String Answer { get; set; }

        [DataMember(Name = "date")]
        public DateTime? Date { get; set; }
    }
}
