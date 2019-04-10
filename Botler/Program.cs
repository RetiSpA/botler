// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Resources;
using System.Globalization;
using System.Collections.Generic;
namespace Botler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateWebHostBuilder(args).Build().Run();
            ResourceManager rm = new ResourceManager("Botler.Dialogs.Utility.Responses-it",
                   Assembly.GetExecutingAssembly());
            var _resourseSet = rm.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            //string regexPattern = @name + "_\\d*";
            //Regex regex = new Regex(regexPattern);
            var prenotazioneNomeLotto = "Lotto nome";
            var prenotazioneIdPosto = "82";
            object resource = rm.GetString("PrenotazioneEffettuata_1");
            Console.WriteLine(resource.ToString());

        }

        public static IWebHostBuilder CreateWebHostBuilder (string[] args) =>
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
