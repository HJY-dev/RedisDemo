using FreeRedis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DistributedId.Controllers
{
    /// <summary>
    /// Redis 布隆过滤器 场景
    /// 解决 Redis 缓存穿透问题；
    /// 邮件过滤，使用布隆过滤器实现邮件黑名单过滤；
    /// 爬虫爬过的网站过滤，爬过的网站不再爬取；
    /// 推荐过的新闻不再推荐；
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RedisBloomFilterController : ControllerBase
    {
        readonly RedisClient _redis;
        public RedisBloomFilterController(RedisClient redisClient)
        {
            _redis = redisClient;
        }

        [HttpGet]
        public string test()
        {
            _redis.BfReserve("orders", (decimal)0.1, 100);

            _redis.BfAdd("orders", "1");
            _redis.BfAdd("orders2", "1");
            _redis.BfAdd("orders", "2");
            _redis.BfAdd("orders", "3");
            _redis.BfAdd("orders", "101");

            _redis.BfExists("orders","1");


            return string.Empty;
        }
    }
}
