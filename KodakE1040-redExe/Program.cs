using System;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;

namespace KodakE1040_redExe
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseAddress = "http://localhost:5000/";

            using (WebApp.Start<Startup>(baseAddress))
            {
                Console.WriteLine($"Scanner API is running at {baseAddress}");
                Console.WriteLine("Press [Enter] to exit...");
                Console.ReadLine();
            }
        }
    }

   
}
