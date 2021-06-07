using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Config
{
    public class AppSettingsContainer
    {
        string _basePath = string.Empty;

        public AppSettingsContainer()
        {
            _basePath = System.AppContext.BaseDirectory;
        }

        public AppSettings GetAppSettingsIns()
        {
            AppSettings result = new AppSettings();

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(_basePath)
                .AddJsonFile("appSettings.json")
                .Build();

            result.RedisMaster = configuration.GetSection("AppSettings").GetSection("RedisMaster").Value;
            result.RedisSlave = configuration.GetSection("AppSettings").GetSection("RedisSlave").Value;
            result.Port = configuration.GetSection("AppSettings").GetSection("Port").Value;
            result.PortSlave = configuration.GetSection("AppSettings").GetSection("PortSlave").Value;
            result.expiryTimeForCache = configuration.GetSection("AppSettings").GetSection("expiryTimeForCache").Value;

            return result;
        }
    }
}
