using FreeRedis;
using System;
using System.Diagnostics;

namespace ConsoleApp1
{
    class Program
    {
        static Lazy<RedisClient> _cliLazy = new Lazy<RedisClient>(() =>
        {
            var r = new RedisClient("127.0.0.1:6379,database=10");
            r.Notice += (s, e) => Trace.WriteLine(e.Log);
            return r;
        });
        static RedisClient cli => _cliLazy.Value;


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            cli.HDel("testkey1", "field1");

            var result1 = cli.HIncrBy("testkey1", "field1", 1);
            var read1 = cli.HGet("testkey1", "field1");
            if (read1 != "1" || read1 != result1.ToString()) throw new Exception("bug");

            var result2 = cli.HIncrBy("testkey1", "field1", 1);
            var read2 = cli.HGet("testkey1", "field1");
            if (read2 != "2" || read2 != result2.ToString()) throw new Exception("bug");

            var result3 = cli.HIncrBy("testkey1", "field1", 1);
            var read3 = cli.HGet("testkey1", "field1");
            if (read3 != "3" || read3 != result3.ToString()) throw new Exception("bug");

            String script = " local current = redis.call('HINCRBY', KEYS[1] , ARGV[1] , ARGV[2]); " +
               " return current ";
            var result4 = cli.Eval(script, new string[] { "testkey1" }, new object[] { "field1", 1 });
            var read4 = cli.HGet("testkey1", "field1");
            if (read4 != "4" || read4 != result4.ToString()) throw new Exception("bug");


        }
    }
}
