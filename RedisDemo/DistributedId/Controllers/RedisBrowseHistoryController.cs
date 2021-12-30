using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FreeRedis;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using DistributedId.Helper;
using DistributedId.Entity;
using DistributedId.DbContext;
using FreeSql;
using DistributedId.Dto;

namespace DistributedId.Controllers
{
    /// <summary>
    /// Redis 浏览记录
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RedisBrowseHistoryController : ControllerBase
    {
        readonly RedisClient _redis;
        readonly IFreeSql<PalaceContext> _freeSql;

        public RedisBrowseHistoryController(RedisClient redis, IFreeSql<PalaceContext> freesql)
        {
            _redis = redis;
            _freeSql = freesql;
        }

        /// <summary>
        /// Redis添加用户浏览历史
        /// 保存最新的101条数据
        /// </summary>
        /// <param name="uId">用户Id</param>
        /// <param name="optionId">操作Id</param>
        /// <param name="optionType">操作类型 1宫观 2神仙</param>
        /// <returns></returns>
        [HttpPost("addBrowseHistoryByUserId")]
        public bool AddBrowseHistoryByUserId(string uId, string optionId, string optionType)
        {
            var key = uId;
            _redis.LPush(key, JsonConvert.SerializeObject(new Option { optionId = optionId, optionType = optionType }));
            _redis.LTrim(key, 0, 100);

            _redis.ZIncrBy(RedisKey.ViewHistory, 1, optionId);
            return true;
        }

        /// <summary>
        /// Redis获取用户浏览历史
        /// </summary>
        /// <param name="uId"></param>
        /// <returns></returns>
        [HttpGet("getBrowseHistoryByUserId")]
        public List<Option> GetBrowseHistoryByUserId(string uId)
        {
            var key = uId;
            var data = _redis.LRange(key, 0, 100);
            var list = data.Distinct().ToList();
            List<Option> result = new List<Option>();
            foreach (var item in list)
            {
                result.Add(JsonConvert.DeserializeObject<Option>(item));
            }
            return result;
        }

        /// <summary>
        /// Redis获取人气访问列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        [HttpGet("getViewHistoryPalace")]
        public List<PalaceRank> GetHistoryPalaceRanks(int page, int size, out long total)
        {
            total = 0;
            var list = new List<PalaceRank>();
            var listIds = _redis.ZRevRange(RedisKey.ViewHistory, (page - 1) * size, size * page - 1);
            if (listIds == null || listIds.Length == 0) return list;
            total = listIds.Length;
            
            foreach (var item in listIds)
            {
                list.Add(_freeSql.Select<PalaceRank>().Where(x => x.PalaceId == item).First());
            }

            return list;
        }

    }
}
