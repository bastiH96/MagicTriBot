using System.Text.RegularExpressions;
using DiscordBot.models;
using DiscordBot.services.dataAccess;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;

namespace DiscordBot.commands;

public class ShiftsystemCommands : BaseCommandModule
{
    [Command("createPattern")]
    public async Task TestCommand(CommandContext ctx)
    {
        var shiftpatternRegex = new Regex(@"^(F|N|S|F12|N12|[-])");
        var dateRegex = new Regex(@"^\d{2}\.\d{2}\.\d{4}$");
        var shiftpattern = new List<string>();
        var date = new DateTime();
        var interactivity = Program.Client.GetInteractivity();
        ulong messageId = 0;
        var patternFinished = false;
        while (!patternFinished)
        {
            var messageToReact = await interactivity.WaitForMessageAsync(message => message.Content != "");
            if (dateRegex.IsMatch(messageToReact.Result.Content))
            {
                var content = messageToReact.Result.Content;
                var year = Convert.ToInt32(content[6..10]);
                var month = Convert.ToInt32(content[3..5]);
                var day = Convert.ToInt32(content[..2]);
                date = new DateTime(year, month, day);
                patternFinished = true;
            }
            else if (messageToReact.Result.Id == messageId)
            {
                // sometimes the bot gets one message multiple times
                // this statement is used to prevent the bot from adding the same content multiple times to the pattern
            }
            else if(shiftpatternRegex.IsMatch(messageToReact.Result.Content))
            {
                messageId = messageToReact.Result.Id;
                shiftpattern.Add(messageToReact.Result.Content);
            }
            else
            {
                await ctx.Channel.SendMessageAsync("your input doesn't match the terms for a pattern");
                return;
            }
        }
        await ctx.Channel.SendMessageAsync(
            $"so your final pattern is: {CreatePatternAsMessageForUser(shiftpattern)}");
        await ctx.Channel.SendMessageAsync("How do you want to name the shiftsystem");
        var messageWithShiftsystemName = await interactivity.WaitForMessageAsync(message => message.Content != "");
        var shiftsystemName = messageWithShiftsystemName.Result.Content;
        await ctx.Channel.SendMessageAsync("If the pattern, the startdate and the name are correct type 'YES'.");
        var messageToConfirmPattern = await interactivity.WaitForMessageAsync(message => message.Content.ToLower() == "yes");
        if (messageToConfirmPattern.Result.Content.ToLower() == "yes")
        {
            var shiftsystem = new ShiftsystemModel(shiftsystemName, shiftpattern, date);
            ShiftsystemDataAccess.InsertOne(shiftsystem);
            await ctx.Channel.SendMessageAsync("Successfully created shiftsystem!");
        }
    }

    public string CreatePatternAsMessageForUser(List<string> shiftpattern)
    {
        string message = string.Empty;
        foreach (var shift in shiftpattern)
        {
            message += $"| {shift} ";
        }
        message += "|";
        return message;
    }
}