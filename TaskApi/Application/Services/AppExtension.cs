using Serilog;

namespace TaskApi.Application.Services
{
    public static class AppExtension
    {
        public static void SerilogConfiguration(this IHostBuilder host)
        {
            host.UseSerilog((context, loggerConfig) =>
            {
                loggerConfig.WriteTo.Console();
            });
        }
    }
}
