using System.ComponentModel;
using ModelContextProtocol.Server;
using System.Text;
using System.Text.Json;

namespace ILB_MCP.McpTools
{
    [McpServerToolType]
    public static class CrawlTool
    {
        public static string? CrawlUrl { get; private set; }

        public static void InitConfig(IConfiguration configuration)
        {
            CrawlUrl = configuration["Crawl:Url"];
        }

        [McpServerTool, Description("Crawls the given URL and returns the content.")]
        public static async Task<string> Crawl(string url, HttpClient? httpClient = null)
        {
            var browserConfig = new
            {
                type = "BrowserConfig",
                @params = new { headless = true }
            };

            var crawlerConfig = new
            {
                type = "CrawlerRunConfig",
                @params = new
                {
                    stream = false,
                    cache_mode = "bypass",
                    deep_crawl_strategy = new
                    {
                        type = "dict",
                        value = new
                        {    
                            max_depth = 2,
                            include_external = false,
                            max_pages = 25,
                        }
                    },
                    verbose = true,
                    semaphore_count = 2,
                    exclude_all_images = true
                }
            };

            var payload = new
            {
                urls = new[] { url },
                browser_config = browserConfig,
                crawler_config = crawlerConfig
            };

            var json = JsonSerializer.Serialize(payload);

            using HttpClient client = httpClient ?? new HttpClient();

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 如果需要 JWT，請取消下行註解並補上 token
            // client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync($"{CrawlUrl}/crawl", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return responseBody;
            }
            else
            {
                throw new Exception($"Error: {response.StatusCode}, {responseBody}");
            }
        }
    }
}