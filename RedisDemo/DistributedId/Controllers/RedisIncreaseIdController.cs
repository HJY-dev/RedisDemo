﻿using DistributedId.Enum;
using DistributedId.Helper;
using FreeRedis;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Yitter.IdGenerator;

namespace DistributedId.Controllers
{
    /// <summary>
    /// Redis 实现自增ID
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RedisIncreaseIdController : ControllerBase
    {
        readonly RedisClient _redis;

        public RedisIncreaseIdController(RedisClient redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// 获取雪花漂移Id
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getid")]
        public long GetSnowId()
        {
            var bussinessId = CommonHelper.GetSerialNumberType((int)GlobalEnumVars.SerialNumberType.服务订单编号);
            var snowId = YitIdHelper.NextId();

            return snowId;
        }


        /// <summary>
        /// Lua 设置自增Id
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("setid")]
        public string Setid()
        {
            String script = " local current = redis.call('incr',KEYS[1]); " +
                " redis.call('expire',KEYS[1],ARGV[1]) " +
                " return current ";
            var rediskey = DateTime.Now.ToString("yyyy-MM-dd");
            var keyexpire = 86400; // 24h
            var result = _redis.Eval(script, new string[] { rediskey }, new object[] { keyexpire });

            return result.ToString();
        }

        /// <summary>
        /// 生成每天的初始Id
        /// </summary>
        /// <param name="hashName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("initPrimaryId")]
        public String initPrimaryId(String hashName)
        {
            var hashCol = DateTime.Now.ToString("yyyy-MM-dd");
            if(!_redis.HExists(hashName,hashCol))
            {
                //自定义编号规则
                int hashColVal = 1;
                _redis.HMSet(hashName, hashCol, hashColVal);
            }
            return hashCol;
        }

        /// <summary>
        /// 获取分布式Id
        /// </summary>
        /// <param name="hashName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("getPrimaryId")]
        public object getPrimaryId(String hashName)
        {
            try
            {
                String hashCol = initPrimaryId(hashName);
                var result = _redis.HIncrBy(hashName, hashCol, 1);

                #region Lua 脚本方式
                //String script = " local current = redis.call('HINCRBY', KEYS[1] , ARGV[1] , ARGV[2]); " +
                //    " return current ";
                //var result = _redis.Eval(script, new string[] { hashName }, new object[] { hashCol, 1 });
                //_redis.Notice += (s, e) => Console.WriteLine(e.Log); //print command log

                #endregion

                return result;
            }
            catch (Exception ex)
            {

            }
            return 0L;
        }

        /// <summary>
        /// 删除多少天之前的cols
        /// </summary>
        /// <param name="hashName"></param>
        /// <param name="lessDay"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("removePrimaryByLessDay")]
        public object removePrimaryByLessDay(String hashName, int lessDay)
        {
            try
            {
                //lessDay前日期
                String hashCol = DateTime.Now.AddDays(-lessDay).ToString("yyyy-MM-dd");

                var dic = _redis.HGetAll(hashName);
                var arr = _redis.HGetAll(hashName).Where(x => DateTime.Parse(x.Key) < DateTime.Parse(hashCol));

                if(arr.Count()>0)
                {
                    String[] removeCols = arr.Select(x => x.Key).ToArray();

                    if (removeCols.Count() > 0)
                    {
                        return _redis.HDel(hashName, removeCols);
                    }
                }
                else
                {
                    return "不存在可删数据";
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return 0L;
        }
    }
}
