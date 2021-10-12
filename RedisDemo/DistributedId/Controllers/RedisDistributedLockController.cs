using DistributedId.DbContext;
using DistributedId.Entity;
using DistributedId.Helper;
using FreeRedis;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DistributedId.Controllers
{
    /// <summary>
    /// Redis 实现分布式锁
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RedisDistributedLockController : ControllerBase
    {
        readonly IFreeSql<OrderContext> freeSql;
        readonly RedisLock redisLock;

        public RedisDistributedLockController(IFreeSql<OrderContext> _freeSql, RedisLock _redisLock)
        {
            freeSql = _freeSql;
            redisLock = _redisLock;
        }

        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("distributedLock")]
        public int Post()
        {
            //请求id
            string requestId = Guid.NewGuid().ToString();
            try
            {
                //获取锁
                redisLock.GetLock(requestId);
                //业务操作
                var redisLockd = freeSql.Select<RedisLockdTest>().First();
                redisLockd.Num++;
                freeSql.Update<RedisLockdTest>().Set(r => r.Num, redisLockd.Num).Where(r => 1 == 1).ExecuteAffrows();
                return redisLockd.Num;
            }
            finally
            {
                redisLock.ReleaseLock(requestId);
            }
        }


    }

    
}
