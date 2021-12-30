using FreeRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedId.Helper
{
    public class RedisLock
    {
        private readonly RedisClient _redis;
        public RedisLock(RedisClient redis)
        {
            _redis = redis;
        }
        /// <summary>
        /// 锁key
        /// </summary>
        private readonly string lockKey = "RedisLock";
        /// <summary>
        /// 锁的过期秒数
        /// </summary>
        private readonly int lockTime = 1;
        /// <summary>
        /// 续命线程取消令牌
        /// </summary>
        private CancellationTokenSource tokenSource = new CancellationTokenSource();


        /// <summary>
        /// 获取锁
        /// </summary>
        /// <param name="requestId">请求id保证释放锁时的客户端和加锁的客户端一致</param>
        /// <returns></returns>
        public bool GetLock(string requestId)
        {
            //设置key 设置过期时间20s
            while (DateTime.Now.Subtract(DateTime.Now).TotalSeconds < (double)lockTime)
            {
                _redis.Lock(lockKey, lockTime);
                //设置key Redis2.6.12以上版本，可以用set获取锁。set可以实现setnx和expire，这个是原子操作
                if (_redis.SetNx(lockKey, requestId, lockTime))
                {
                    //设置成功后开启子线程为key续命
                    CreateThredXm();
                    return true;
                }
            }
            return false;   
        }

        /// <summary>
        /// 为锁续命(防止业务操作时间大于锁自动释放时间，锁被自动释放掉)
        /// </summary>
        void CreateThredXm()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                    //外部取消 退出子线程
                    if (tokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                    //查询key还有多少秒释放
                    var Seconds = _redis.PTtl(lockKey) / 1000;
                    //key还剩1/3秒时重设过期时间
                    if (Seconds < (lockTime / 3))
                    {
                        //小于5秒则自动 重设过期时间
                        _redis.Expire(lockKey, lockTime);
                    }
                }
            }, tokenSource.Token);
        }

        /// <summary>
        /// 释放锁操作
        /// </summary>
        /// <param name="requestId">请求id保证释放锁时的客户端和加锁的客户端一致</param>
        public void ReleaseLock(string requestId)
        {
            //这里使用Lua脚本保证原子性操作
            string script = "if  redis.call('get', KEYS[1]) == ARGV[1] then " +
                    "return redis.call('del', KEYS[1]) " +
                    "else return 0 end";
            _redis.Eval(script, new string[] { lockKey }, requestId);
            //取消续命线程
            tokenSource.Cancel();
        }

    }
}
