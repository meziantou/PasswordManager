using Meziantou.PasswordManager.Client;
//using Microsoft.ApplicationInsights;
//using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
//using System.Diagnostics;

namespace Meziantou.PasswordManager.Windows.Utilities
{
    public static class Telemetry
    {
        //private static TelemetryClient _telemetry = GetAppInsightsClient();
        //private const string TELEMETRY_KEY = "4bbb7b98-78f8-49c3-8ede-da3215b75f43";

        public static bool Enabled { get; set; } = true;

//        private static TelemetryClient GetAppInsightsClient()
//        {
//            var config = new TelemetryConfiguration();
//            config.InstrumentationKey = TELEMETRY_KEY;
//            config.TelemetryChannel = new Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel.ServerTelemetryChannel();
//            //config.TelemetryChannel = new Microsoft.ApplicationInsights.Channel.InMemoryChannel();
//            config.TelemetryChannel.DeveloperMode = Debugger.IsAttached;
//#if DEBUG            
//            config.TelemetryChannel.DeveloperMode = true;
//#endif
//            var client = new TelemetryClient(config);
//            client.Context.Component.Version = App.Version;
//            client.Context.Session.Id = Guid.NewGuid().ToString();
//            client.Context.User.Id = (Environment.UserName + Environment.MachineName).GetHashCode().ToString();
//            client.Context.Device.OperatingSystem = Environment.OSVersion.ToString();

//            return client;
//        }

        public static void SetUser(User user)
        {
            //_telemetry.Context.User.AuthenticatedUserId = user?.Username;
        }

        public static void TrackEvent(string key, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            if (Enabled)
            {
                //_telemetry.TrackEvent(key, properties, metrics);
            }
        }

        public static void TrackException(Exception ex)
        {
            if (ex != null && Enabled)
            {
                //var telex = new Microsoft.ApplicationInsights.DataContracts.ExceptionTelemetry(ex);
                //_telemetry.TrackException(telex);
                Flush();
            }
        }

        internal static void Flush()
        {
            //_telemetry.Flush();
        }
    }

}
