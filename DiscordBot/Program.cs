using DiscordBot.commands;
using DiscordBot.config;
using DiscordBot.models;
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
            // DatabaseTest();
            
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
            // var pattern = new List<string>() { "F12", "F12", "N12", "N12", "-", "-", "-", "-" };
            // var shiftsystem = new ShiftsystemModel("Corna System Basti", pattern);
            // ShiftsystemDataAccess.InsertOne(shiftsystem);
            // Thread.Sleep(2000);
            
            var person = new PersonModel("Basti", "BAS", new DateTime(2023, 4, 10), 1);
            PersonDataAccess.InsertOne(person);
        }
    }
}
