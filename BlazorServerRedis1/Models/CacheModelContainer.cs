using Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorServerRedis1.Models
{
    /// <summary>
    /// Redis 模型容器
    /// </summary>
    public class CacheModelContainer
    {
        private readonly IRedisCacheProvider _redisCacheProvider;

        public CacheModelContainer(IRedisCacheProvider redisCacheProvider)
        {
            _redisCacheProvider = redisCacheProvider;
        }
        /// <summary>
        /// 存入模型資料到 Redis Cache 中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void SetModelData(string key, object data)
        {
            _redisCacheProvider.Put(key, data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetModelData<T>(string key) 
            where T: class
        {
            return _redisCacheProvider.Get(key) as T;
        }
    }
}
