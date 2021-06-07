using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Cache
{
    /// <summary>
    /// Redis Cache 資料提供者
    /// </summary>
    public class RedisCacheProvider : IRedisCacheProvider
    {
        private ConnectionMultiplexer _redis = null;

        //是否在序列化的 JSON 結果裡保留型別
        private JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects
        };

        private static JsonSerializerSettings JsonSerializerSettings = null;

        /// <summary>
        /// 建立執行個體
        /// 必須設定好 Redis Server for master and slave.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public RedisCacheProvider()
        {
            _redis = RedisState.GetRedisConnection;

            RedisCacheProvider.JsonSerializerSettings = _jsonSerializerSettings;
        }
        /// <summary>
        /// 代表包含 Redis Seerver 或 Cluster 的功能定義
        /// </summary>
        private IDatabase Current
        {
            get
            {
                return RedisState.Db;
            }
        }
        /// <summary>
        /// 使用 key 作為條件，從 Redis Cache Server 中取得一個物件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            object result = null;
            try
            {
                RedisValue cacheResult = Current.StringGet(key);
                result = JsonConvert.DeserializeObject(cacheResult, _jsonSerializerSettings);
            }
            catch (RedisConnectionException rex)
            {
                int reTry = 0;
                do
                {
                    //延遲 1 秒鐘.
                    Thread.Sleep(100);
                    try
                    {
                        RedisValue cacheResult = Current.StringGet(key);
                        result = JsonConvert.DeserializeObject(cacheResult, _jsonSerializerSettings);
                        //成功即跳出迴圈.
                        break;
                    }
                    catch
                    {
                        RedisState.CloseMultiplexer();

                        _redis = RedisState.CreateMultiplexer();
                        //失敗則在迴圈中繼續 ReTry
                    }
                    reTry++;

                } while (reTry < 5);

                if (reTry == 5)
                {
                    //紀錄 Log，請修改為 e 保網紀錄 Log 的方式.
                    Trace.WriteLine(string.Format("{0}, {1}", rex.GetType().Name, rex.Message));
                }
            }
            catch (Exception ex)
            {
                //紀錄 Log，請修改為 e 保網紀錄 Log 的方式.
                Debug.WriteLine(string.Format("Fail to Get(\"{0}\")! Treat as a null result.\nError Detail:\n", key, ex.ToString()));
            }

            return result;
        }
        /// <summary>
        /// 將一個物件存放進 Redis Cache Server 中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void Put(string key, object data)
        {
            Put(key, data, DateTime.Now.AddMinutes(10) - DateTime.Now);
        }
        /// <summary>
        /// 將一個物件存放進 Redis Cache Server 中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="liveTime"></param>
        public void Put(string key, object data, TimeSpan? liveTime)
        {
            try
            {
                Current.StringSet(key, JsonConvert.SerializeObject(data, _jsonSerializerSettings), liveTime);
            }
            catch (RedisConnectionException rex)
            {
                int reTry = 0;
                do
                {
                    //延遲 1 秒鐘.
                    Thread.Sleep(200);
                    try
                    {
                        Current.StringSet(key, JsonConvert.SerializeObject(data, _jsonSerializerSettings), liveTime);
                        //成功即跳出迴圈.
                        break;
                    }
                    catch
                    {
                        RedisState.CloseMultiplexer();

                        _redis = RedisState.CreateMultiplexer();
                        //失敗則在迴圈中繼續 ReTry
                    }
                    reTry++;

                } while (reTry < 5);

                if (reTry == 5)
                {
                    //紀錄 Log，請修改為 e 保網紀錄 Log 的方式.
                    Trace.WriteLine(string.Format("{0}, {1}", rex.GetType().Name, rex.Message));
                }
            }
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }
    }
}
