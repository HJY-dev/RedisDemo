using FreeRedis;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DistributedId.Controllers
{
    /// <summary>
    /// Redis 实现指定时间内登录次数限制
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RedisLoginLimitController : ControllerBase
    {
        public static RedisClient cli = new RedisClient("127.0.0.1:6379,database=10");

        public RedisLoginLimitController()
        {
                
        }

        /// <summary>
        /// redis实现限制1小时内每用户Id最多只能登录5次
        /// </summary>
        /// <param name="userid">用户id</param>
        /// <param name="password">用户密码</param>
        [HttpGet]
        [Route("userlimitlogin")]
        public string CheckLogin(string userid,string password)
        {
            if (userid =="admin")
            {
                if(password != "123456")
                {
                    //密码错误
                    var isExit = cli.Exists(userid);
                    if (isExit)
                    {
                        if (Convert.ToInt16(cli.Get(userid)) < 5)
                        {
                            cli.Incr(userid);
                        }
                        else
                        {
                            return $"{userid}登录错误次数超过上限，禁止登录";
                        }
                    }
                    else
                    {
                        cli.Set(userid, 1);
                        cli.Expire(userid, 3600);
                    }
                }
                else
                {
                    return $"{userid} 登录成功";
                }
            }
            else
            {
                return $"{userid} 用户不存在";
            }
            return $"{userid} 您还有{5-Convert.ToInt16(cli.Get(userid))} 次登录机会";
        }

    }
}
