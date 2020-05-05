﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WcSync.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables("WcSync")
                .Build();
        }
    }
}
