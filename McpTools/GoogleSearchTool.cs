using System.ComponentModel;
using ModelContextProtocol.Server;

namespace ILB_MCP.McpTools
{
    [McpServerToolType]
    public static class GoogleSearchTool
    {
        private static IHttpClientFactory? _httpClientFactory;
        private static IConfiguration? _configuration;

        // 初始化方法,由應用程式啟動時調用
        public static void Initialize(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [McpServerTool, Description("A tool that uses Google Custom Search API to retrieve up-to-date information from the web based on user queries.")]
        public static async Task<string> GoogleSearch(
            [Description("user search query.")] string query,
            [Description("number of search results to return (default: 10, max: 10)")] int numResults = 10)
        {
            Console.WriteLine($"GoogleSearchTool called with query: {query}, numResults: {numResults}");

            if (string.IsNullOrWhiteSpace(query))
                return "查詢字串不可為空";

            if (_httpClientFactory == null || _configuration == null)
            {
                Console.WriteLine("GoogleSearchTool is not properly initialized");
                return "Google Search API 設定不完整或尚未初始化";
            }

            var apiKey = _configuration["GoogleSearch:ApiKey"];
            var searchEngineId = _configuration["GoogleSearch:SearchEngineId"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(searchEngineId))
            {
                Console.WriteLine("GoogleSearch configuration is missing");
                return "Google Search API 設定不完整";
            }

            // 限制結果數量在 1-10 之間
            if (numResults < 1) numResults = 1;
            if (numResults > 10) numResults = 10;

            try
            {
                // 每次調用時創建新的 HttpClient,避免被回收的問題
                using var httpClient = _httpClientFactory.CreateClient();

                var baseUrl = _configuration["GoogleSearch:SearchUrl"] ?? "https://www.googleapis.com/customsearch/v1";
                var separator = baseUrl.Contains('?') ? "&" : "?";
                var url = baseUrl + separator
                    + "key=" + Uri.EscapeDataString(apiKey)
                    + "&cx=" + Uri.EscapeDataString(searchEngineId)
                    + "&q=" + Uri.EscapeDataString(query)
                    + "&num=" + Uri.EscapeDataString(numResults.ToString());

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Google Search API error: {response.StatusCode}, {errorContent}");
                    return $"搜尋失敗: {response.StatusCode}";
                }

                var content = await response.Content.ReadAsStringAsync();

                // 返回原始 JSON 結果
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GoogleSearchTool: {ex.Message}");
                return $"搜尋時發生錯誤: {ex.Message}";
            }
        }
    }
}
