using FreeRedis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedId.Helper
{
    public class RedisCache
    {
        private readonly RedisClient _redis;
        public RedisCache(RedisClient redis)
        {
            _redis = redis;
        }

        /// <summary>
        ///  添加数据到 SortedSet
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">集合id</param>
        /// <param name="t">数值</param>
        /// <param name="score">排序码</param>
        /// <param name="dbId">库</param>
        public long SortedSet_Add<T>(string key, T t, decimal score, long dbId = 0)
        {
            string value = JsonConvert.SerializeObject(t);
            var result = _redis.ZAdd(key, score, value);
            return result;
        }

        /// <summary>
        /// 移除数据从SortedSet
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">集合id</param>
        /// <param name="t">数值</param>
        /// <param name="dbId">库</param>
        /// <returns></returns>
        public long SortedSet_Remove<T>(string key, T t, long dbId = 0)
        {
            string value = JsonConvert.SerializeObject(t);
            var result = _redis.ZRem(key, value);
            return result;
        }

        /// <summary>
        /// 按Score升序获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键值</param>
        /// <param name="fromScore"></param>
        /// <param name="toScore"></param>
        /// <param name="dbId">库</param>
        /// <returns></returns>
        public IDictionary<T, decimal> SortedSet_GetListByLowestScore<T>(string key, decimal fromScore, decimal toScore, long dbId = 0)
        {
            var list = _redis.ZRangeByScoreWithScores(key, fromScore, toScore);
            if (list != null && list.Length > 0)
            {
                var dict = new Dictionary<T, decimal>();
                foreach (var item in list)
                {
                    var data = JsonConvert.DeserializeObject<T>(item.member);
                    dict.Add(data, item.score);
                }
                return dict;
            }

            return null;
        }

        /// <summary>
        /// key过期订阅
        /// </summary>
        /// <param name="dbId"></param>
        /// <returns></returns>
        public string Subscribe(long dbId)
        {
            _redis.Subscribe($"__keyevent@{dbId}__:expired",(msg,data)=> {
                Console.WriteLine(data);
            });

            return "";
        }
    }
}
