using Microsoft.AspNetCore.SignalR;

namespace AllmonNet
{
    public static class AppConfig
    {
        private static IConfiguration appConfig;

        static AppConfig()
        {
            appConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        }

        public static string Get(string key)
        {
            return appConfig.GetValue<string>(key);
        }
    }
}
