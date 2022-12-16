using AzureNamingTool.Models;
using AzureNamingTool.Services;
using System.Runtime.Caching;

namespace AzureNamingTool.Helpers
{
    public class CacheHelper
    {
        public static object GetCacheObject(string cachekey)
        {
            try
            {
                ObjectCache memoryCache = MemoryCache.Default;
                var encodedCache = memoryCache.Get(cachekey);
                if (encodedCache == null)
                {
                    return null;
                }
                else
                {
                    return (object)encodedCache;
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return null;
            }
        }

        public static void SetCacheObject(string cachekey, object cachedata)
        {
            try
            {
                ObjectCache memoryCache = MemoryCache.Default;
                var cacheItemPolicy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(600.0),

                };
                memoryCache.Set(cachekey, cachedata, cacheItemPolicy);
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
        }

        public static void InvalidateCacheObject(string cachekey)
        {
            try
            {
                ObjectCache memoryCache = MemoryCache.Default;
                memoryCache.Remove(cachekey);
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
        }
    }
}
