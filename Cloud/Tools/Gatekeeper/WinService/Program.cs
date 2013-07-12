// -
// <copyright file="Program.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -

namespace HomeOS.Cloud.Platform.Gatekeeper
{
    using System;
    using System.ServiceProcess;
 

    /// <summary>
    /// Represents the program that hosts the service.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main()
        {
            ServiceBase[] servicesToRun;
            servicesToRun = new ServiceBase[] 
            {
                new WindowsService()
            };

#if DEBUG
            foreach (ServiceBase serviceBase in servicesToRun)
            {
                ((WindowsService)serviceBase).DebugStart(null);
            }

            while (true)
            {
                System.Threading.Thread.Sleep(600000);
            }
#else
            ServiceBase.Run(servicesToRun);
#endif
        }
    }
}