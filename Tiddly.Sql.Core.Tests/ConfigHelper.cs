using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tiddly.Sql.Core.Tests
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
