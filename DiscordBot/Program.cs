using DiscordBot.commands;
using DiscordBot.config;
using DiscordBot.models;
using DiscordBot.services;
using DiscordBot.services.dataAccess;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace DiscordBot {
    internal class Program {
        public static DiscordClient? Client { get; set; }
        public static CommandsNextExtension? Commands { get; set; }
        static async Task Main(string[] args)
        {
            CreateTablesIfNotExists();
            //DatabaseTest();
            
            var jsonReader = new JsonReader();
            await jsonReader.ReadJson();

            Client = SetupDiscordClient(jsonReader.Token);
            ActivateInteractivity();

            Client.Ready += Client_Ready;
            
            Commands = SetupCommands(jsonReader.Prefix);
            
            Commands.RegisterCommands<TestCommands>();
            Commands.RegisterCommands<ShiftsystemCommands>();

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }

        private static void CreateTablesIfNotExists()
        {
            ShiftsystemDataAccess.CreateTableShiftsystem();
            PersonDataAccess.CreatePersonTable();
        }

        private static DiscordClient SetupDiscordClient(string token)
        {
            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };
            return new DiscordClient(discordConfig);
        }

        private static CommandsNextExtension SetupCommands(string prefix)
        {
            var commandConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false
            };
            return Client!.UseCommandsNext(commandConfig);
        }

        private static void ActivateInteractivity()
        {
            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2)
            });
        }

        private static void DatabaseTest()
        {
            var excelService = new ExcelCalendarService(PersonDataAccess.GetAll(), "testCalendar", "D:\\TestData", 2023);
            excelService.CreateComparingTableInCsvFile();
        }
    }
}
