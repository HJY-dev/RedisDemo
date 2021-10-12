using FreeRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedId.Helper
{
    /// <summary>
    /// 延时队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelayedQueue<T>
    {
        #region 私有变量
        private readonly RedisCache _redisCache;
        private Func<T, bool> checkData = t => true;
        private Action<T> action = t => { Console.WriteLine("请调用SetAction配置具体操作"); };
        private string queueName = null;
        private long dbId = 0;
        private Timer timer = null;
        private int dueTime = 1000;
        private int period = 500;
        #endregion

        #region 构建一个新的延时队列

        /// <summary>
        /// 构建一个新的延时队列
        /// </summary>
        /// <param name="redisCache">Redis</param>
        /// <param name="queueName">队列名称</param>
        /// <param name="dbId">库Id</param>
        public DelayedQueue(RedisCache redisCache, string queueName, long dbId = 0)
        {
            _redisCache = redisCache;
            this.queueName = queueName;
            this.dbId = dbId;
        }
        #endregion

        #region 向队列中添加一个记录
        /// <summary>
        /// 向队列中添加一个记录
        /// </summary>
        /// <param name="t">添加的内容</param>
        /// <param name="delayed">延时时间</param>
        /// <returns></returns>
        public long Push(T t, TimeSpan delayed)
        {
            return _redisCache.SortedSet_Add<T>(queueName, t, DateTime.Now.Add(delayed).Ticks, dbId);
        }
        #endregion

        #region 将数据移出队列
        /// <summary>
        /// 将数据移出队列
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public long Remove(T t)
        {
            return _redisCache.SortedSet_Remove(queueName, t, dbId);
        }
        #endregion

        #region 设置检查是否需要执行默认操作
        /// <summary>
        /// 设置检查是否需要执行默认操作
        /// </summary>
        /// <returns>当返回True时，执行默认操作</returns>
        public DelayedQueue<T> SetCheck(Func<T, bool> func)
        {
            checkData = func;
            return this;
        }
        #endregion

        #region 设置操作
        /// <summary>
        /// 设置操作
        /// </summary>
        public DelayedQueue<T> SetAction(Action<T> action)
        {
            this.action = action;
            return this;
        }
        #endregion

        #region 更改Time的调度时间
        /// <summary>
        /// 更改Time的调度时间
        /// </summary>
        /// <param name="dueTime"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public DelayedQueue<T> ChangeTime(int dueTime = 1000, int period = 500)
        {
            this.dueTime = dueTime;
            this.period = period;
            timer?.Change(dueTime, period);
            return this;
        }
        #endregion

        #region 开始监听队列
        /// <summary>
        /// 开始监听数据
        /// </summary>
        public void Listen()
        {
            Stop();
            timer = new Timer(TimerAction, null, dueTime, period);
        }

        /// <summary>
        /// 获取到期记录
        /// </summary>
        /// <returns></returns>
        private IEnumerable<T> GetDelayeds()
        {
            var dict = _redisCache.SortedSet_GetListByLowestScore<T>(queueName, 0, DateTime.Now.Ticks, dbId);
            return dict?.Keys;
        }

        private int isRunning = 0;
        /// <summary>
        /// Timer定期触发
        /// </summary>
        /// <param name="obj"></param>
        private void TimerAction(object obj)
        {
            if (Interlocked.Exchange(ref isRunning, 1) == 0)
            {
                try
                {
                    IEnumerable<T> list = null;
                    do
                    {
                        list = GetDelayeds();
                        if (!(list?.Any() ?? false)) break;

                        list.ToList().ForEach(item =>
                        {
                            if (checkData(item))
                            {
                                action(item);
                                Remove(item);
                            }
                        });
                    } while (true);
                }
                finally
                {
                    Interlocked.Exchange(ref isRunning, 0);
                }
            }
        }
        #endregion

        #region 停止监听队列
        /// <summary>
        /// 停止监听队列
        /// </summary>
        public void Stop()
        {
            timer?.Dispose();
            timer = null;
        }
        #endregion
    }

}
