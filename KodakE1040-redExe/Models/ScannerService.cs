using System;
using System.ServiceProcess;
using Microsoft.Owin.Hosting;

namespace KodakE1040_redExe
{
    public class ScannerService : ServiceBase
    {
        private IDisposable _webApp;
        private readonly string _baseAddress = "http://localhost:5000/";

        public ScannerService()
        {
            ServiceName = "KodakScannerService";
        }

        protected override void OnStart(string[] args)
        {
            _webApp = WebApp.Start<Startup>(_baseAddress);
        }

        protected override void OnStop()
        {
            _webApp?.Dispose();
        }
    }
}
