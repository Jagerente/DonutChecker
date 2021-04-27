using DonutChecker.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.IO;
using System.Text;
using VkNet;
using VkNet.Model;

namespace DonutChecker
{
    class Program
    {
        private const string ConfigPath = "configuration/";
       
        private const string VkConfigPath = ConfigPath + "vkConfig.json";
        public static VkApi Vk { get; private set; }

        static void Main(string[] args)
        {
            Setup();
            var vkConfig = JsonStorage.RestoreObject<Configuration.VkConfiguration>(VkConfigPath);
            
            //Logger
        //    Log.Logger = new LoggerConfiguration()
        //.MinimumLevel
        //.Debug()
        //.WriteTo
        //.Console()
        //.WriteTo
        //.File("log.txt",
        //    rollingInterval: RollingInterval.Day,
        //    rollOnFileSizeLimit: true)
        //.CreateLogger();

        //    var services = new ServiceCollection();

        //    services.AddLogging(builder =>
        //    {
        //        builder.ClearProviders();
        //        builder.SetMinimumLevel(LogLevel.Trace);
        //        builder.AddSerilog(dispose: true);
        //    });


            Vk = new VkApi(/*services*/);

            Vk.Authorize(new ApiAuthParams
            {
                AccessToken = vkConfig.Token
            });

            Vk.VkApiVersion.SetVersion(5, 130);

            var donuts = Vk.Groups.GetMembers(
                new VkNet.Model.RequestParams.GroupsGetMembersParams()
                {
                    Filter = VkNet.Enums.SafetyEnums.GroupsMemberFilters.Donut,
                    GroupId = vkConfig.GroupId,
                    Fields = VkNet.Enums.Filters.UsersFields.All
                });

            var k = 0;
            var sb = new StringBuilder();
            foreach (var donut in donuts)
            {
                var txt = $"{++k}. {donut.FirstName} {donut.LastName}  - vk.com/id{donut.Id}";
                Console.WriteLine(txt);
                sb.AppendLine(txt);
            }
            File.WriteAllText("output.txt", sb.ToString());
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine($"Saved to output.txt");
        }

        private static void Setup()
        {
            if (!Directory.Exists(ConfigPath)) Directory.CreateDirectory(ConfigPath);

            if (!File.Exists(VkConfigPath))
            {
                var cfg = new Configuration.VkConfiguration()
                {
                    Token = string.Empty,
                    GroupId = string.Empty
                };
                JsonStorage.StoreObject(cfg, VkConfigPath);
                foreach (var property in JObject.Parse(JsonConvert.SerializeObject(cfg)))
                {
                    Console.WriteLine($"Set VK {property.Key}:");
                    JsonStorage.SetValue(VkConfigPath, property.Key, Console.ReadLine());
                }
            }

        }
    }
}
