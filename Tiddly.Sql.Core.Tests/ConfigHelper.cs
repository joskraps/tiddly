using Microsoft.Extensions.Configuration;

namespace Tiddly.Sql.Tests
{
    public static class ConfigHelper
    {
        public static IConfigurationRoot GetConfigRoot()
        {
            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables();

            var returnConfig = builder.Build();

            return returnConfig;
        }
    }
}
