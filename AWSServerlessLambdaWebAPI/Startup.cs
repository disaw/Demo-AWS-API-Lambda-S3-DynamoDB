using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AWSLambdaWebAPI.Services;
using Domain.Interfaces;
using Domain.Models;
using Domain.Repositories;
using Data.FileSystem;
using System.IO;
using Microsoft.Extensions.Options;

namespace AWSLambdaWebAPI
{
    public class Startup
    {
        public static IConfiguration Configuration { get; private set; }
        public const string AppS3BucketKey = "AppS3Bucket";

        public Startup(IHostEnvironment environment)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
        
        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Add S3 to the ASP.NET Core dependency injection framework.
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", Configuration["AWS:AccessKey"]);
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", Configuration["AWS:SecretKey"]);
            Environment.SetEnvironmentVariable("AWS_REGION", Configuration["AWS:Region"]);
            services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
            services.AddAWSService<Amazon.S3.IAmazonS3>();

            services.Configure<Configuration>(Configuration.GetSection("Configuration"));

            services.AddTransient<ILP, LP>();
            services.AddTransient<ITOU, TOU>();
            services.AddTransient<IMeterData, MeterData>();
            //services.AddTransient<ICSVRepository, Data.FileSystem.CSVRepository>();
            services.AddTransient<ICSVRepository, Data.AWS.S3.CSVRepository>();
            services.AddSingleton<IDataRepository, Data.AWS.DynamoDB.DataRepository>();
            services.AddSingleton<IMeterService, MeterService>();           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
