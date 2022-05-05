using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.IO;

namespace Gnoss.Web.Documents.Services
{
    public class ConfigService
    {
        #region Propiedades
        public IConfigurationRoot Configuration { get; set; }
        private string azureStorageConnectionString;
        private string logstashEndpoint;
        private string implementationKey;
        private string logLocation;
        #endregion

        #region Constructor
        public ConfigService()
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }
        #endregion

        #region Metodos
        public string GetAzureStorageConnectionString()
        {
            if (string.IsNullOrEmpty(azureStorageConnectionString))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("AzureStorageConnectionString"))
                {
                    azureStorageConnectionString = environmentVariables["AzureStorageConnectionString"] as string;
                }
                else
                {
                    azureStorageConnectionString = Configuration["AzureStorageConnectionString"];
                }
            }
            return azureStorageConnectionString;
        }

        public string GetLogstashEndpoint()
        {
            if (string.IsNullOrEmpty(logstashEndpoint))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("LogstashEndpoint"))
                {
                    logstashEndpoint = environmentVariables["LogstashEndpoint"] as string;
                }
                else
                {
                    logstashEndpoint = Configuration["LogstashEndpoint"];
                }
            }
            return logstashEndpoint;
        }

        public string GetImplementationKey()
        {
            if (string.IsNullOrEmpty(implementationKey))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("ImplementationKey"))
                {
                    implementationKey = environmentVariables["ImplementationKey"] as string;
                }
                else
                {
                    implementationKey = Configuration["ImplementationKey"];
                }
            }
            return implementationKey;
        }

        public string GetLogLocation()
        {
            if (string.IsNullOrEmpty(logLocation))
            {
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("LogLocation"))
                {
                    logLocation = environmentVariables["LogLocation"] as string;
                }
                else
                {
                    logLocation = Configuration["LogLocation"];
                }
            }
            return logLocation;
        }
        #endregion
    }
}
