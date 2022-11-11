using System;
using System.Runtime.Serialization;

namespace DEA.Client
{
    [DataContract]
    public class QuestionModel
    {
        [DataMember(Name = "id")]
        public Guid ID { get; set; }

        [DataMember(Name = "question")]
        public String Question { get; set; }

        [DataMember(Name = "date")]
        public DateTime? Date { get; set; }
    }
}
