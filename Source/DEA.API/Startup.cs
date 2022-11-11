using DEA.Tools;
using DEA.Tools.MessageHandler.Redis;
using DEA.Tools.MessageStore.Redis;
using DEA.Tools.Serialization.NewtonsoftJson;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DEA.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton(options =>
            {
                var redisServer = Configuration.GetValue<String>("AppSettings:RedisServer");
                //var kafkaServer = Configuration.GetValue<String>("AppSettings:KafkaServer");

                var deaProcessor = new DeaProcessor()
                                      .UseRedisHandler(redisServer)
                                      .UseRedisStore(redisServer)
                                      //.UseKafka(kafkaServer)
                                      .UseNewtonsoftJsonSerializer()
                                      .Connect();

                return deaProcessor;
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "DEA.API",
                    Description = "DEA.API"
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                options.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Error");

            if (env.IsProduction())
            {
                var defaultFile = new DefaultFilesOptions();
                defaultFile.DefaultFileNames.Clear();
                defaultFile.DefaultFileNames.Add("Index.html");

                app.UseDefaultFiles(defaultFile);

                app.UseStaticFiles();
            }

            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("swagger/v1/swagger.json", "DEA.API");
                options.RoutePrefix = String.Empty;
            });

            app.UseMvc();
        }
    }
}
