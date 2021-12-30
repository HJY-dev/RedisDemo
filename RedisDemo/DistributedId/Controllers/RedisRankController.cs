using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FreeRedis;

namespace DistributedId.Controllers
{
    /// <summary>
    /// Redis 排名
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RedisRankController : ControllerBase
    {
        readonly RedisClient _redis;
        public RedisRankController(RedisClient redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("test")]
        public ActionResult test()
        {
            return Ok();
        }

    }
}
