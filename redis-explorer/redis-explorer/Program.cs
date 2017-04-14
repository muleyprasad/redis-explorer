using ServiceStack.Redis;
using System;
using System.Linq;

namespace redis_explorer
{
    class Program
    {
        public static string redisHost = "127.0.0.1"; //Provide your Redis Ip Here
        public static string redisPort = "6379"; //Provide your Redis port Here

        static void Main(string[] args)
        {
            Findlen();
            var timer = new System.Threading.Timer((e) =>
            {
                Findlen();
            }, null, 0, 5000);
            Console.Read();
        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        static double ConvertKiloBytesToMegabytes(double bytes)
        {
            return (bytes / 1024f);
        }

        static double ConvertBytesToKilobytes(long bytes)
        {
            return (bytes / 1024f);
        }

        static void Findlen()
        {
            using (var redisClient = new RedisClient(redisHost, Convert.ToInt16(redisPort)))
            {
                double totalsize = 0;
                var keys = redisClient.GetAllKeys();
                Console.WriteLine("-----------------------------------------------------------------------------");
                foreach (string key in keys)
                {
                    
                    try
                    {
                        double kblen = ConvertBytesToKilobytes(GetBytesCount(redisClient, key));
                        Console.WriteLine("* " + key + " : " + kblen.ToString("F") + " kb");
                        totalsize = totalsize + kblen;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("*--* Error retirving key : " + key);
                    }
                }
                Console.WriteLine("** Total size : " + totalsize.ToString("F") + " kb **");
            }
        }

        private static long GetBytesCount(RedisClient redisClient, string key)
        {
            long len;
            switch (redisClient.Type(key))
            {
                case "string":
                    len = redisClient.Get(key).Length;
                    break;
                case "list":
                    len = redisClient.LRange(key, 0, -1).Sum(a => a.Length);
                    break;
                case "set":
                    len = redisClient.SMembers(key).Sum(a => a.Length);
                    break;
                case "zset":
                    len = redisClient.ZRange(key, 0, -1).Sum(a => a.Length);
                    break;
                case "hash":
                    len = redisClient.HGetAll(key).Sum(a => a.Length);
                    break;
                default:
                    len = 0;
                    break;
            }

            return len;
        }
    }
}