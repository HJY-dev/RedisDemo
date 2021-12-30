using DistributedId.DbContext;
using DistributedId.Entity;
using DistributedId.Helper;
using FreeRedis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedId.Controllers
{
    /// <summary>
    /// Redis 实现分布式锁
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RedisDistributedLockController : ControllerBase
    {
        readonly IFreeSql<OrderContext> _freeSql;
        readonly RedisLock _redisLock;
        readonly ILogger _logger;
        readonly string _distributedLockKey = "DISTRIBUTEDLOCKKEY";

        public RedisDistributedLockController(IFreeSql<OrderContext> freeSql, RedisLock redisLock, ILoggerFactory loggerFactory)
        {
            _freeSql = freeSql;
            _redisLock = redisLock;
            _logger = loggerFactory.CreateLogger<RedisDistributedLockController>();
        }


        /// <summary>
        /// 执行抢购
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("rushToBuy")]
        public bool RushToBuy()
        {
            Parallel.For(0, 20, (i) => { Task.Run(() => { Post(1,1, Guid.NewGuid().ToString("N")); }); });
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
            try
            {
                //获取锁
                var lockObj = _redisLock.GetLock(uid);
                if(lockObj)
                {
                    //业务操作
                    var stockCount = _freeSql.Select<RedisLockdTest>().Where(x => x.Id == id).First().Num;
                    if (stockCount > 0 && stockCount >= goodsCount)
                    {
                        stockCount -= goodsCount;
                        _logger.LogInformation($"线程Id:{Thread.CurrentThread.ManagedThreadId}，抢购成功！库存数：{stockCount}");
                        var result = _freeSql.Update<RedisLockdTest>().Set(r => r.Num, stockCount).Where(r => r.Id == id).ExecuteAffrows();
                        _redisLock.ReleaseLock(uid);
                        return result;
                    }
                    else
                    {
                        _logger.LogWarning($"线程Id:{Thread.CurrentThread.ManagedThreadId}，抢购失败！");
                        _redisLock.ReleaseLock(uid);
                    }
                }
            }
            catch (Exception ex)
            {
                _redisLock.ReleaseLock(uid);
            }
            finally
            {
                //_redisLock.ReleaseLock(uid);
            }
            return 0;
        }


    }

    
}
