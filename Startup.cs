using Serilog;
using Serilog.Core;

namespace serverless_image_AspNetCoreWebAPI;

public class Startup
{
  private readonly string _rootPath = "";
  public IConfiguration Configuration { get; }
  public IWebHostEnvironment Environment { get; }
  private readonly Logger _logger;

  public Startup(IConfiguration configuration, IWebHostEnvironment environment)
  {
    Configuration = configuration;
    Environment = environment;
    _rootPath = Environment.ContentRootPath;
    _logger = new LoggerConfiguration().CreateLogger();
  }

  // This method gets called by the runtime. Use this method to add services to the container
  public void ConfigureServices(IServiceCollection services) { services.AddControllers(); }

  // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    var prefix = string.IsNullOrEmpty(_rootPath) ? null : $"/{_rootPath.Split('\\').LastOrDefault()}";
    var appName = "AWS SAM WebAPI";

    _logger.Information($"{appName} Started. Environment: {env.EnvironmentName}, prefix: {prefix}");

    if (env.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();
    app.UseEndpoints(endpoints =>
    {
      endpoints.MapControllers();
      endpoints.MapGet("/", async context =>
      {
        var response = context.Response;

        try
        {
          var dotnetVersion = RuntimeInfo.FrameworkDescription();
          var runtime = RuntimeInfo.RuntimeIdentifier();
          var processArchitecture = RuntimeInfo.ProcessArchitecture();
          var osArchitecture = RuntimeInfo.OsArchitecture();
          var isLinux = RuntimeInfo.IsLinux();
          var cpuInfo = default(string);

          if (isLinux)
          {
            try
            {
              cpuInfo = await File.ReadAllTextAsync(@"/proc/cpuinfo");
            }
            catch (Exception ex)
            {
              _logger.Error($"Could not read /proc/cpuinfo: {ex.Message}");
              await response.WriteAsync($"Error: {ex.Message}");
            }
          }

          await response.WriteAsync($"{appName} ({dotnetVersion}, {runtime}, {processArchitecture}, {osArchitecture}) on AWS Lambda with SAM\n\n{cpuInfo}");
        }
        catch (Exception ex)
        {
          _logger.Error($"Error: {ex.Message}");
          await response.WriteAsync($"Error: {ex.Message}");
        }
      });
    });
  }
}
