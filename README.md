# DEA.Tools

Distributed Event Architecture

DEA is tools library which allows to develope fault-tolerant and Load Balanced application. (.NET Alternative of Python Celery)

How it works?

Let's imagine we have one Web API application which is load balanced using web server (IIS web farm or web garden)
what if we transmit all received (on the Web server) request to some message broker (like Redis Pub/Sub, Apache Kafka, RebbitMQ, etc...)
and then we write a simple Console Application which consume/subscribe channels in which we transfer requests and handle all requests using Console Application (create/develope console application is much easy then Web API) and then we can run this Console Application on few servers and run few instances on each server.
as a result we get Load Balancing + Failover.

How to use DEA.Tools?

**on Web API side** add singleton of DeaProcessor in Program.cs or Startup.cs (in ConfigureServices method)
```csharp
            services.AddSingleton(options =>
            {
                var redisServer = Configuration.GetValue<String>("AppSettings:RedisServer");

                var deaProcessor = new DeaProcessor()
                                      .UseRedisHandler(redisServer)
                                      .UseNewtonsoftJsonSerializer()
                                      .Connect();

                return deaProcessor;
            });
```
it is also possible to use data compression
```csharp
            services.AddSingleton(options =>
            {
                var redisServer = Configuration.GetValue<String>("AppSettings:RedisServer");

                var deaProcessor = new DeaProcessor()
                                      .UseRedisHandler(redisServer)
                                      .UseRedisStore(redisServer)
                                      .UseGZipCompression()
                                      .UseNewtonsoftJsonSerializer()
                                      .Connect();

                return deaProcessor;
            });
```
**SetCompressionLimit** is could be used to set minimum data size (default is 1024) 

it means thet only messages size of which is more then compression limit will be compressed.

and then use it in Controller code
```csharp
        private readonly DeaProcessor _deaProcessor;

        public ValuesController(DeaProcessor deaProcessor)
        {
            _deaProcessor = deaProcessor;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            var eventName = $"{nameof(ValuesController)}.{nameof(Get)}";
            var result = await _deaProcessor.ProcessAsync<String>(eventName, id);

            return result;
        }
```

also it is possible to add DeaEventAttribute to API method and specify event name as a parameter of attribute
```csharp
        private readonly DeaProcessor _deaProcessor;

        public ValuesController(DeaProcessor deaProcessor)
        {
            _deaProcessor = deaProcessor;
        }

        [HttpGet("{id}")]
        [DeaEvent]
        public async Task<ActionResult<string>> Get(int id)
        {
            var result = await _deaProcessor.ProcessAsync<String>(id);
            return result;
        }
```
or
```csharp
        private readonly DeaProcessor _deaProcessor;

        public ValuesController(DeaProcessor deaProcessor)
        {
            _deaProcessor = deaProcessor;
        }

        [HttpGet("{id}")]
        [DeaEvent("ValuesController.Get")]
        public async Task<ActionResult<string>> Get(int id)
        {
            var result = await _deaProcessor.ProcessAsync<String>(id);
            return result;
        }
```
**NOTE:** if DeaEventAttribute constructor parameter is not set then default name of event will be use.
Default event name is name of class separated using "." (dot) and name of method. (e.g if name of class is ProductHelper and method name is SetProductQuentity then default event name is ProductHelper.SetProductQuentity)

**Please consider** that DEA.Tools implements two type of call
1. With Void type - should be used when you don't care about result/response and you don't want to wait for complete 
2. With Return type - should be used when we need to wait and receive result/response

e.g
```csharp
        [HttpPost]
        [DeaEvent]
        public async Task<ActionResult> Post([FromBody] String value)
        {
            //in this case we don't care about result/response (this type of call is like Fire and Forget)
            await _deaProcessor.ProcessAsync(value);
            return Ok();
        }
        
        [HttpPost]
        [DeaEvent]
        public async Task<ActionResult> Post([FromBody] String value)
        {
            //if return type is specified (generic parameter) then DeaProcessor will wait for response
            //because of this it is possible to set timeout using SetDefaultTimeout (default timeout is 30 sec)
            var result = await _deaProcessor.ProcessAsync<String>(value);
            return Ok(result);
        }
```

**on Console Application side** define single instance of DeaConnector and map events
```csharp
        static void Main(string[] args)
        {
            var redisServer = "localhost:6379";

            var getMethod = (Func<String, int, String>)Get;

            var connector = new DeaConnector()
                                .UseRedisHandler(redisServer)
                                .UseRedisStore(redisServer)
                                .UseGZipCompression()
                                .UseNewtonsoftJsonSerializer()
                                .MapEventHandler("ValuesController.Get", getMethod)
                                .Connect();

            Console.WriteLine("Server running...");
            Console.ReadLine();
        }

        static String Get(String eventName, int id)
        {
            Console.WriteLine($"Request from {eventName}");
            return "Hello";
        }
```
also it is possible to create event handler class (like Controller class in Web API)
```csharp
    class Program
    {
        static void Main(string[] args)
        {
            var redisServer = "localhost:6379";

            var connector = new DeaConnector()
                                .UseRedisHandler(redisServer)
                                .UseRedisStore(redisServer)
                                .UseGZipCompression()
                                .UseNewtonsoftJsonSerializer()
                                .MapEventController<EventController>()
                                .Connect();

            Console.WriteLine("Server running...");
            Console.ReadLine();
        }
    }

    class EventController
    {
        [DeaEvent("ValuesController.Get")]
        public String Get(int id)
        {
            Console.WriteLine($"ValuesController.Get(id={id}) Processed");

            return "Value1";
        }
    }
```

also it is possible to specify threading mode on Console Application side (SingleThread, MultiThread, ThreadPool, TaskHandled)
e.g
```csharp
            var connector = new DeaConnector()
                                .UseRedisHandler(redisServer)
                                .UseRedisStore(redisServer)
                                .UseGZipCompression()
                                .UseNewtonsoftJsonSerializer()
                                .SetThreadingMode(ThreadingMode.MultiThread)
                                .MapEventController<EventController>()
                                .Connect();
```
**SingleThread** - all requests will be handled in one thread.

**MultiThread** - new thread will be started for each request.

**ThreadPool** - ThreadPool.QueueUserWorkItem will be called for each request.

**TaskHandled** - new Task will be created for each request.


**NOTE:** Unfortunately some message brokers have limitations of max message size (Redis Pub/Sub **32KiB**, Kafka **1MiB**, RabbitMQ **128MiB**)
and if size of Request or Response may be larger then max size of message of message broker which we use, then we need to use Message Store.
for that purpose (for Message Store) we can use Redis, Memcached, SQL Server, MongoDb, Minio or implement IMessageStore interface and create your own message store.
```csharp
            services.AddSingleton(options =>
            {
                var redisServer = Configuration.GetValue<String>("AppSettings:RedisServer");

                var deaProcessor = new DeaProcessor()
                                      .UseRedisHandler(redisServer)
                                      .UseRedisStore(redisServer)
                                      .UseGZipCompression()
                                      .UseNewtonsoftJsonSerializer()
                                      .Connect();

                return deaProcessor;
            });
```
also it is possible to implement IMessageHandler, IDataSerializer, IDataCompression interfaces and develope your own.

**IMessageHandler** - Class which connects and receives/send messages from Message Broker (example see in source Code)

**IDataSerializer** - Class which serializes/deserializes data

**IDataCompression** - Class which compress/decompress data

**IMessageStore** - Class which saves or/and get message data from message store

and then use those classes directly

e.g
```csharp
            var processor = new DeaProcessor()
                                .UseHandler(() => new RedisMessageHandler(redisServer))
                                .UseStore(() => new RedisMessageStore(redisServer))
                                .UseCompression(() => new GZipCompressor())
                                .UseSerializer(() => new NewtonsoftJsonSerializer())
                                .Connect();

            var connector = new DeaConnector()
                                .UseHandler(() => new RedisMessageHandler(redisServer))
                                .UseStore(() => new RedisMessageStore(redisServer))
                                .UseCompression(() => new GZipCompressor())
                                .UseSerializer(() => new NewtonsoftJsonSerializer())
                                .MapEventController<EventController>()
                                .Connect();
```

**IMPORTANT:** Both side of Dea applications (Web API side and Console Application side) should be configured same
(e.g if on API side used message store then same message store must be used on Console Application side too,
if on API side used data compression then same data compression must be used on Console Application side too)
