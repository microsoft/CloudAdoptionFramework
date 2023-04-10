using AzureNamingTool.Models;
using AzureNamingTool.Services;
using System.Runtime.Caching;
using System.Text;

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


        public static string GetAllCacheData()
        {
            StringBuilder data = new();
            try
            {
                ObjectCache memoryCache = MemoryCache.Default;
                var cacheKeys = memoryCache.Select(kvp => kvp.Key).ToList();
                foreach (var key in cacheKeys.OrderBy(x => x))
                {                    
                    data.Append("<p><strong>" + key + "</strong></p><div class=\"alert alert-secondary\" style=\"word-wrap:break-word;\">" + MemoryCache.Default[key].ToString() + "</div>");
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                data.Append("<p><strong>No data currently cached.</strong></p>");
            }
            return data.ToString();
        }
    }
}
