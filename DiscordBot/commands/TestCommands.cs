using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace DiscordBot.commands;

public class TestCommands : BaseCommandModule
{
    [Command("test")]
    public async Task TestCommand(CommandContext ctx)
    {
        await ctx.Channel.SendMessageAsync($"hello {ctx.User.Username}");
    }

    [Command("sendFile")]
    public async Task SendFile(CommandContext ctx)
    {
        using var fs = new FileStream(@"/Users/sebastianheyde/Public/testFile/Schichtkalender_2023.xlsx", FileMode.Open, FileAccess.Read);
        var msg = await new DiscordMessageBuilder()
            .WithContent("This is a test file")
            .AddFile("Schichtkalender.xlsx", fs)
            .SendAsync(ctx.Channel);

    }
}