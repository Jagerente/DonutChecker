using DonutChecker.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
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
            Serilog.Log.Logger = new LoggerConfiguration()
        .MinimumLevel
        .Debug()
        .WriteTo
        .Console()
        .WriteTo
        .File("log.txt",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true)
        .CreateLogger();

            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddSerilog(dispose: true);
            });


            Vk = new VkApi();

            Vk.Authorize(new ApiAuthParams
            {
                AccessToken = vkConfig.Token
            });

            Vk.VkApiVersion.SetVersion(5, 130);

            var donuts = Vk.Groups.GetMembers(
                new VkNet.Model.RequestParams.GroupsGetMembersParams()
                {
                    Filter = VkNet.Enums.SafetyEnums.GroupsMemberFilters.Donut,
                    GroupId = vkConfig.GroupId.ToString(),
                    Fields = VkNet.Enums.Filters.UsersFields.All
                });
            var chatMembers = Vk.Messages.GetConversationMembers(2000000000 + vkConfig.ConversationId, groupId: vkConfig.GroupId);
            var k = 0;
            var sb = new StringBuilder();
            string txt;

            Log("Участников в беседе:" + chatMembers.Count, ref sb);

            Log("\nСписок донов:", ref sb);
            var donList = new List<long>();
            foreach (var donut in donuts)
            {
                donList.Add(donut.Id);
                Log($"{++k}. {donut.FirstName} {donut.LastName}  - vk.com/id{donut.Id}", ref sb);
            }

            Log("\nЗакончилась подписка:", ref sb);
            k = 0;
            var memberList = new List<long>();
            foreach (var member in chatMembers.Profiles)
            {
                memberList.Add(member.Id);

                if (!donList.Contains(member.Id))
                {
                    Log($"{++k}. {member.FirstName} {member.LastName} - vk.com/id{member.Id}", ref sb);
                }
            }

            Log("\nОтсутствует в чате:", ref sb);
            k = 0;
            foreach (var donut in donuts)
            {
                if (!memberList.Contains(donut.Id))
                Log($"{++k}. {donut.FirstName} {donut.LastName}  - vk.com/id{donut.Id}", ref sb);
            }

            File.WriteAllText("output.txt", sb.ToString());
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine($"Saved to output.txt");
            Console.ReadKey();
        }

        private static void Setup()
        {
            if (!Directory.Exists(ConfigPath)) Directory.CreateDirectory(ConfigPath);

            if (!File.Exists(VkConfigPath))
            {
                var cfg = new Configuration.VkConfiguration()
                {
                    Token = string.Empty,
                    GroupId = 0,
                    ConversationId = 0
                };
                JsonStorage.StoreObject(cfg, VkConfigPath);
                foreach (var property in JObject.Parse(JsonConvert.SerializeObject(cfg)))
                {
                    Console.WriteLine($"Set VK {property.Key}:");
                    JsonStorage.SetValue(VkConfigPath, property.Key, Console.ReadLine());
                }
            }
        }

        private static void Log(string txt, ref StringBuilder sb)
        {
            Console.WriteLine(txt);
            sb.AppendLine(txt);
        }
    }
}
