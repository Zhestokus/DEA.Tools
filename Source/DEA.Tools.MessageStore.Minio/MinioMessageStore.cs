using Minio;
using DEA.Core;
using System;
using System.Threading.Tasks;
using Minio.Exceptions;
using System.IO;

namespace DEA.Tools.MessageStore.Minio
{
    public class MinioMessageStore : MessageStoreBase
    {
        private string _endpoint;
        private string _accessKey;
        private string _secretKey;

        private bool _secure;

        private MinioClient _minio;

        public MinioMessageStore(string endpoint, string accessKey, string secretKey, bool secure)
        {
            _endpoint = endpoint;
            _accessKey = accessKey;
            _secretKey = secretKey;
            _secure = secure;
        }

        public override void Begin()
        {
            _minio?.Dispose();

            _minio = new MinioClient()
                        .WithEndpoint(_endpoint)
                        .WithCredentials(_accessKey, _secretKey)
                        .WithSSL(_secure)
                        .Build();
        }

        public override byte[] PopMessage(string eventName, Guid messageID)
        {
            var messageKey = Convert.ToString(messageID);

            try
            {
                var buffer = new MemoryStream();

                var getArgs = new GetObjectArgs()
                                  .WithBucket(eventName)
                                  .WithObject(messageKey)
                                  .WithFile(messageKey)
                                  .WithCallbackStream(s => s.CopyTo(buffer));

                var stat = _minio.GetObjectAsync(getArgs).GetAwaiter().GetResult();

                var remArgs = new RemoveObjectArgs()
                              .WithBucket(eventName)
                              .WithObject(messageKey);

                _minio.RemoveObjectAsync(remArgs).GetAwaiter().GetResult();

                return buffer.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Bucket]  Exception: {e}");
            }

            return null;
        }
        public override async Task<byte[]> PopMessageAsync(string eventName, Guid messageID)
        {
            var messageKey = Convert.ToString(messageID);

            try
            {
                var buffer = new MemoryStream();

                var getArgs = new GetObjectArgs()
                                  .WithBucket(eventName)
                                  .WithObject(messageKey)
                                  .WithFile(messageKey)
                                  .WithCallbackStream(s => s.CopyTo(buffer));

                var stat = await _minio.GetObjectAsync(getArgs);

                var remArgs = new RemoveObjectArgs()
                              .WithBucket(eventName)
                              .WithObject(messageKey);

                await _minio.RemoveObjectAsync(remArgs);

                return buffer.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Bucket]  Exception: {e}");
            }

            return null;
        }

        public override byte[] GetMessage(string eventName, Guid messageID)
        {
            var messageKey = Convert.ToString(messageID);

            try
            {
                var buffer = new MemoryStream();

                var args = new GetObjectArgs()
                              .WithBucket(eventName)
                              .WithObject(messageKey)
                              .WithFile(messageKey)
                              .WithCallbackStream(s => s.CopyTo(buffer));

                var stat = _minio.GetObjectAsync(args).GetAwaiter().GetResult();

                return buffer.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Bucket]  Exception: {e}");
            }

            return null;
        }
        public override async Task<byte[]> GetMessageAsync(string eventName, Guid messageID)
        {
            var messageKey = Convert.ToString(messageID);

            try
            {
                var buffer = new MemoryStream();

                var args = new GetObjectArgs()
                              .WithBucket(eventName)
                              .WithObject(messageKey)
                              .WithFile(messageKey)
                              .WithCallbackStream(s => s.CopyTo(buffer));

                var stat = await _minio.GetObjectAsync(args);

                return buffer.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Bucket]  Exception: {e}");
            }

            return null;
        }

        public override void SaveMessage(string eventName, Guid messageID, byte[] messageData)
        {
            var messageKey = Convert.ToString(messageID);

            try
            {
                var beArgs = new BucketExistsArgs().WithBucket(eventName);

                var found = _minio.BucketExistsAsync(beArgs).ConfigureAwait(false).GetAwaiter().GetResult();
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs().WithBucket(eventName);
                    _minio.MakeBucketAsync(mbArgs).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                var putObjectArgs = new PutObjectArgs()
                                        .WithBucket(eventName)
                                        .WithObject(messageKey)
                                        .WithFileName(messageKey)
                                        .WithContentType("application/octet-stream");

                _minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (MinioException e)
            {
                Console.WriteLine("File Upload Error: {0}", e.Message);
            }
        }
        public override async Task SaveMessageAsync(string eventName, Guid messageID, byte[] messageData)
        {
            var messageKey = Convert.ToString(messageID);

            try
            {
                var beArgs = new BucketExistsArgs().WithBucket(eventName);

                var found = await _minio.BucketExistsAsync(beArgs).ConfigureAwait(false);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs().WithBucket(eventName);
                    await _minio.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                }

                var putObjectArgs = new PutObjectArgs()
                                        .WithBucket(eventName)
                                        .WithObject(messageKey)
                                        .WithFileName(messageKey)
                                        .WithContentType("application/octet-stream");

                await _minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
            }
            catch (MinioException e)
            {
                Console.WriteLine("File Upload Error: {0}", e.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _minio?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }
    }
}
