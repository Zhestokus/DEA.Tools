using DEA.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEA.Tools.MessageStore.Minio
{
    public static class Extensions
    {
        public static IDeaConnector UseMinioStore(this IDeaConnector deaConnetor, String endpoint, String accessKey, String secretKey, bool secure)
        {
            var instance = deaConnetor.UseStore(() => new MinioMessageStore(endpoint, accessKey, secretKey, secure));
            return instance;
        }

        public static IDeaProcessor UseMinioStore(this IDeaProcessor deaProcessor, String endpoint, String accessKey, String secretKey, bool secure)
        {
            var instance = deaProcessor.UseStore(() => new MinioMessageStore(endpoint, accessKey, secretKey, secure));
            return instance;
        }
    }
}
