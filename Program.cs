using ILB_MCP.McpTools;

var builder = WebApplication.CreateBuilder(args);

// 註冊 HttpClient 服務
builder.Services.AddHttpClient();

builder.Services
.AddMcpServer()
.WithHttpTransport()
.WithToolsFromAssembly();

var app = builder.Build();

// 初始化工具
CrawlTool.InitConfig(builder.Configuration);
GoogleSearchTool.Initialize(app.Services.GetRequiredService<IHttpClientFactory>(), builder.Configuration);

app.MapMcp();

// 健康檢查：給 Docker/監控用
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
