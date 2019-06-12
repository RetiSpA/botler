// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Web;
using Botler.Controller;
using Botler.Dialogs.Scenari;
using Botler.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.LuisIntent;
using Newtonsoft.Json;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using Botler.Models;
using Botler.Commands;
using Microsoft.Bot.Builder;
using Botler.Middleware.Services;
using Botler.Helpers;
using Botler.Builders;

namespace Botler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
            
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    // Add Azure Logging
                    logging.AddAzureWebAppDiagnostics();
                    // Other Loggers.
                    // There are other logging options available:
                    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.1
                    // logging.AddDebug();
                    // logging.AddConsole();
                })
                .CaptureStartupErrors(false)
                // Application Insights.
                // An alternative logging and metrics service for your application.
                // https://azure.microsoft.com/en-us/services/application-insights/
                // .UseApplicationInsights()
                .UseStartup<Startup>();
    }
}
