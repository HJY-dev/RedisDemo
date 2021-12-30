using DistributedId.Helper;
using FreeRedis;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DistributedId.Controllers
{
    /// <summary>
    /// Redis 实现延时队列
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RedisDelayedQueueController : ControllerBase
    {
        readonly RedisCache _redisCache;
        readonly RedisClient _redis;

        public RedisDelayedQueueController(RedisCache redisCache, RedisClient redisClient)
        {
            _redisCache = redisCache;
            _redis = redisClient;
        }


        /// <summary>
        /// 延时队列
        /// </summary>
        [HttpGet]
        [Route("DelayedQueueTest")]
        public void Test()
        {
            #region 方式一 redis timer 检测通知

            DelayedQueue<String> queue = new DelayedQueue<string>(_redisCache, "delayedQueue")
                .SetAction(s =>
                Console.WriteLine(s)
            );
            queue.Push("A", TimeSpan.FromMinutes(20));
            queue.Push("B", TimeSpan.FromMinutes(2));
            queue.Push("C", TimeSpan.FromMinutes(1));
            queue.Push("D", TimeSpan.FromSeconds(25));
            queue.Listen();

            #endregion


            #region 方式二 redis 订阅通知
            // 将redis.conf中 notify-keyspace-events "" 更改成 notify-keyspace-events Ex

            #region 实现
            //_redis.Set("A", DateTime.Now.Add(TimeSpan.FromMinutes(20)).Ticks);
            //_redis.Expire("A", 20);
            //Thread.Sleep(30);

            //_redis.Set("B", DateTime.Now.Add(TimeSpan.FromMinutes(20)).Ticks);
            //_redis.Expire("B", 20);
            //Thread.Sleep(30);

            //_redis.Set("C", DateTime.Now.Add(TimeSpan.FromMinutes(20)).Ticks);
            //_redis.Expire("C", 20);
            //Thread.Sleep(30);

            //_redis.Set("D", DateTime.Now.Add(TimeSpan.FromMinutes(20)).Ticks);
            //_redis.Expire("D", 20);
            //Thread.Sleep(30);

            // //key 过期通知
            //_redis.Subscribe("__keyevent@11__:expired", (msg, data) =>
            //{

            //    Console.WriteLine(data);
            //});
            #endregion

            #endregion

        }

    }
}
