using Infrastructure.Config;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Cache
{
    public class RedisState
    {
        protected static AppSettings _appSettings;

        //protected static Lazy<ConnectionMultiplexer> _redisConnection;
        protected static ConnectionMultiplexer _redisConnection = null;

        public static ConnectionMultiplexer GetRedisConnection
        {
            get
            {
                return _redisConnection;
            }
        }

        public static IDatabase Db => (_redisConnection ?? CreateMultiplexer()).GetDatabase();
        //Redis Server
        public static IServer RedisServer;

        private static string _master1 = string.Empty;
        #region Master1 唯讀變數
        public static string Master1
        {
            get
            {
                return _master1;
            }
        }
        #endregion
        private static string _slave1 = string.Empty;
        #region Slave1 唯讀變數
        public static string Slave1
        {
            get
            {
                return _slave1;
            }
        }
        #endregion
        private static int _port;
        #region Port 唯讀變數
        public static int Port
        {
            get
            {
                return _port;
            }
        }
        #endregion
        private static int _portSlave;

        /// <summary>
        /// 取得 Redis Configuration Options 物件.
        /// </summary>
        /// <param name="ssl"></param>
        /// <param name="clientName"></param>
        /// <returns></returns>
        protected static ConfigurationOptions GetConfiguration(string redisConnString, bool ssl)
        {
            var configuration = ConfigurationOptions.Parse(redisConnString);
            configuration.Ssl = ssl;
            //configuration.ClientName = clientName;
            configuration.AbortOnConnectFail = false;
            return configuration;
        }

        static RedisState()
        {
            _appSettings = new AppSettingsContainer().GetAppSettingsIns();

            CreateMultiplexer();
        }

        public static ConnectionMultiplexer Connect2Redis(ConfigurationOptions options)
        {
            ConnectionMultiplexer result = null;

            try
            {
                result = ConnectionMultiplexer.Connect(options);
            }
            catch (RedisConnectionException)
            {
                result = ConnectionMultiplexer.Connect(options);
            }

            _redisConnection = result;

            return result;
        }
        /// <summary>
        /// 取得 Redis ConnectionString.
        /// </summary>
        /// <returns></returns>
        public static string GetRedisConnectionString()
        {
            _master1 = _appSettings.RedisMaster;

            _slave1 = _appSettings.RedisSlave;

            int.TryParse(_appSettings.Port, out _port);
            if(_port == 0)
            {
                _port = 6379;
            }

            int.TryParse(_appSettings.PortSlave, out _portSlave);
            if (_portSlave == 0)
            {
                _portSlave = 6380;
            }

            string redisConnString = $"{_master1}:{_port},password=gelis123,serviceName=master";

            return redisConnString;
        }
        /// <summary>
        /// 取得可操作 Redis 的 Multiplexer 物件.
        /// </summary>
        /// <returns></returns>
        public static ConnectionMultiplexer CreateMultiplexer()
        {
            ConfigurationOptions options = ConfigurationOptions.Parse("127.0.0.1:6379,password=gelis123");

            ConnectionMultiplexer multiplexer = Connect2Redis(options);

            RedisServer = multiplexer.GetServer(options.EndPoints.First());

            return multiplexer;
        }
        /// <summary>
        /// 關閉 Redis Mulitiplexer 物件.
        /// </summary>
        /// <param name="oldMultiplexer"></param>
        public static void CloseMultiplexer(ConnectionMultiplexer oldMultiplexer)
        {
            if (oldMultiplexer != null)
            {
                try
                {
                    oldMultiplexer.Close();
                }
                catch (Exception ex)
                {
                    //如果基礎網路連接已經關閉，再進行關閉就可能出現錯誤，因此如果是因為 Close() 引發的錯誤這裡不進行處理
#if DEBUG
                    Trace.WriteLine($"CloseMultiplexer 時發生錯誤！.詳細錯誤訊息={ex.Message}");
#endif
                }
            }
        }
        /// <summary>
        /// 關閉 Redis Mulitiplexer 物件.
        /// </summary>
        public static void CloseMultiplexer()
        {
            if(_redisConnection.IsConnected)
            {
                CloseMultiplexer(_redisConnection);
            }
        }
    }
}
