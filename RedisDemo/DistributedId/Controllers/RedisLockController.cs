using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FreeRedis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using DistributedId.DbContext;
using DistributedId.Entity;
using System;

namespace DistributedId.Controllers
{
    /// <summary>
    /// Redis 实现分布式锁
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RedisLockController : ControllerBase
    {
        readonly RedisClient _redis;
        readonly ILogger _logger;
        readonly IFreeSql<OrderContext> _freeSql;
        readonly string _distributedLockKey = "DISTRIBUTEDLOCKKEY";

        public RedisLockController(IFreeSql<OrderContext> freeSql, RedisClient redis, ILoggerFactory loggerFactory)
        {
            _freeSql = freeSql;
            _redis = redis;
            _logger = loggerFactory.CreateLogger<RedisLockController>();
        }

        /// <summary>
        /// 初始化商品库存
        /// </summary>
        /// <returns></returns>
        [HttpPost("initStock")]
        public bool InitStock()
        {
            var id = 1; //写死商品Id测试
            var stockCount = _freeSql.Select<RedisLockdTest>().Where(x => x.Id == id).First().Num;
            _redis.Set($"{id}:StockCount", stockCount);
            return true;
        }

        /// <summary>
        /// 执行抢购
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("rushToBuy")]
        public bool RushToBuy()
        {
            Parallel.For(0, 20, (i) => { Task.Run(() => { Post(1,1, _distributedLockKey); }); });
            return true;
        }

        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="id">商品Id</param>
        /// <param name="goodsCount">购买数量</param>
        /// <param name="uid">请求Id</param>
        /// <returns></returns>
        private int Post(int id,int goodsCount,string uid)
        {
            // 取锁
            var lockObj = _redis.Lock(uid, 1);
            if (lockObj != null)
            {
                try
                {
                    var stockCount = _redis.Get<int>($"{id}:StockCount");
                    if (stockCount > 0 && stockCount >= goodsCount)
                    {
                        stockCount -= goodsCount;
                        _redis.Set($"{id}:StockCount", stockCount);
                        _logger.LogInformation($"线程Id:{Thread.CurrentThread.ManagedThreadId}，抢购成功！库存数：{stockCount}");
                        var result = _freeSql.Update<RedisLockdTest>().Set(r => r.Num, stockCount).Where(r => r.Id == id).ExecuteAffrows();
                        return result;
                    }
                    _logger.LogWarning($"线程Id:{Thread.CurrentThread.ManagedThreadId}，抢购失败！");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"程序异常:{ex.Message}，抢购失败！");
                }
                finally
                {
                    lockObj.Unlock(); // 解锁
                }
            }

            return 0;
        }

    }
}
