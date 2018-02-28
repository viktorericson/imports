﻿using System;
using System.Collections.Generic;
using System.IO;
using GirafRest.Setup;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace GirafRest
{
    /// <summary>
    /// An enum to store the user's choice of database option.
    /// </summary>
    public enum DbOption { SQLite, MySQL }


    /// <summary>
    /// The main class for the Giraf REST-api.
    /// </summary>
    public class Program
    {
        public static IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Giraf REST Server.");

            

            //Parse all the program arguments and stop execution if any invalid arguments were found.
            var pa = new ProgramArgumentParser();
            bool validArguments = pa.CheckProgramArguments(args);
            if(!validArguments) return;

            //Build the host from the given arguments.

            try{
                BuildWebHost(args).Run();
            }
            catch(MySqlException e){
                Console.WriteLine("Something went wrong in connecting to the MySql server: " +
                                  $"{e.Message}");              
            }
            catch(Exception e){
                Console.WriteLine("Error: " + e.Message);
            }
        }
        /// <summary>
        /// Builds the host environment from a specified config class.
        /// <see cref="Startup"/> sets the general environment (authentication, logging i.e)
        /// <see cref="StartupLocal"/> sets up the environment for local development.
        /// <see cref="StartupDeployment"/> sets up the environment for deployment.
        /// </summary>
        /// <returns>A <see cref="IWebHost"/> host fit for running the server.</returns>

        public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder()
               .UseKestrel()
               .UseUrls($"http://+:{ProgramOptions.Port}")
               .UseIISIntegration()
               .UseStartup<Startup>()
               .ConfigureAppConfiguration((hostContext, config) =>
               {
                   config.Sources.Clear();
                   var env = hostContext.HostingEnvironment;                
               })
               .UseDefaultServiceProvider(options =>options.ValidateScopes = false)
               .UseApplicationInsights()
               .Build();
    }
}
