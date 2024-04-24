using System.Drawing;
using System.Text.RegularExpressions;
using DiscordBot.helpers;
using DiscordBot.models;
using DiscordBot.services;
using DiscordBot.services.dataAccess;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace DiscordBot.commands;

public class ShiftsystemCommands : BaseCommandModule
{
    private readonly InteractivityExtension _interactivity = Program.Client.GetInteractivity();
    [Command("createPattern")]
    public async Task CreatePatternCommand(CommandContext ctx)
    {
        var shiftpatternRegex = new Regex(@"^(F|N|S|F12|N12|[-])");
        var shiftpattern = new List<string>();
        // var interactivity = Program.Client.GetInteractivity();
        ulong messageId = 0;
        var patternFinished = false;
        while (!patternFinished)
        {
            var messageToReact = await _interactivity.WaitForMessageAsync(message => message.Content != "");
            if (messageToReact.Result.Content.Equals("done", StringComparison.CurrentCultureIgnoreCase))
            {
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
        var messageWithShiftsystemName = await _interactivity.WaitForMessageAsync(message => message.Content != "");
        var shiftsystemName = messageWithShiftsystemName.Result.Content;
        
        await ctx.Channel.SendMessageAsync("If the pattern and the name is correct type 'YES'.");
        var messageToConfirmPattern = await _interactivity.WaitForMessageAsync(message => message.Content.ToLower() == "yes");
        if (messageToConfirmPattern.Result.Content.Equals("yes", StringComparison.CurrentCultureIgnoreCase))
        {
            var shiftsystem = new ShiftsystemModel(shiftsystemName, shiftpattern);
            ShiftsystemDataAccess.InsertOne(shiftsystem);
            await ctx.Channel.SendMessageAsync("Successfully created shiftsystem!");
        }
    }

    [Command("createPerson")]
    public async Task CreatePersonCommand(CommandContext ctx)
    {
        string name, alias;
        ShiftsystemModel? shiftsystem;
        var startDate = new DateTime();
        var embedMessage = new DiscordEmbedBuilder()
        {
            Color = DiscordColor.Green
        };

        name = GetNewPersonName(ctx, embedMessage).Result;
        if(name.Equals(string.Empty)) return;
        alias = GetNewPersonAlias(ctx, embedMessage).Result;
        if (alias.Equals(string.Empty)) return;
        shiftsystem = GetNewPersonsShiftpattern(ctx, embedMessage).Result;
        if (shiftsystem == null) return;
        startDate = GetNewPersonsShiftpatternStartDate(ctx, embedMessage).Result;
        if(startDate == new DateTime()) return;

        var person = new PersonModel(name, alias, startDate, shiftsystem.Id);

        await AddNewPerson(ctx, embedMessage, person, shiftsystem);

    }

    [Command("createTable")]
    public async Task CreateComparisonTable(CommandContext ctx)
    {
        var embedMessage = new DiscordEmbedBuilder()
        {
            Color = DiscordColor.Green
        };
        var personsForTable = await GetListOfSelectedPersons(ctx, embedMessage);
        if (personsForTable == null) return;
        var year = await GetYearForComparisonTable(ctx, embedMessage);
        if (year == 0) return;
        var fileName = $"Schichtkalender {year}";
        fileName = personsForTable.Aggregate(fileName, (current, person) => current + $" {person?.Alias}");
        var excelService = new ExcelCalendarService(personsForTable, fileName,
            Constants.excelFileComparisonTablePath, year);
        excelService.CreateComparingTableInCsvFile();
        await SendComparisonTableAsExcelFile(ctx, fileName);
    }

    private string CreatePatternAsMessageForUser(List<string> shiftpattern)
    {
        string message = string.Empty;
        foreach (var shift in shiftpattern)
        {
            message += $"| {shift} ";
        }
        message += "|";
        return message;
    }

    private async Task<string> GetNewPersonName(CommandContext ctx, DiscordEmbedBuilder embedMessage)
    {
        embedMessage.Description = "What's the name of the person?";
        await ctx.Channel.SendMessageAsync(embed: embedMessage);
        
        var messageWithPersonName = await _interactivity.WaitForMessageAsync(message => message.Content != "");
        if (messageWithPersonName.Result.Content.Equals("stop", StringComparison.CurrentCultureIgnoreCase)) return string.Empty;
        var name = messageWithPersonName.Result.Content;

        embedMessage.Description = $"So the name of the new person is '{name}', right? \nCONFIRM WITH 'YES'.";
        await ctx.Channel.SendMessageAsync(embed: embedMessage);
        var confirmationMessage = await _interactivity.WaitForMessageAsync(message => message.Content != "");
        return !confirmationMessage.Result.Content.Equals("yes", StringComparison.CurrentCultureIgnoreCase) ? string.Empty : name;
    }

    private async Task<string> GetNewPersonAlias(CommandContext ctx, DiscordEmbedBuilder embedMessage)
    {
        embedMessage.Description = """
                                   Please enter the alias of the person. The alias will form the header in each
                                   column of the comparison table, in which the respective shift system of the
                                   person is shown.
                                   Therefore you can only use 5 letters for the alias.
                                   """;
        await ctx.Channel.SendMessageAsync(embed: embedMessage);

        var messageWithPersonsAlias = await _interactivity.WaitForMessageAsync(message => message.Content != "");
        if (messageWithPersonsAlias.Result.Content.Equals("stop", StringComparison.CurrentCultureIgnoreCase))
            return string.Empty;
        if (messageWithPersonsAlias.Result.Content.Length > 5)
        {
            embedMessage.Description = "You entered to many letters. The max. number of letters is 5.\nABORT!";
            return string.Empty;
        }

        var alias = messageWithPersonsAlias.Result.Content;
        embedMessage.Description = $"So the alias of the new person is '{alias}', right? \nCONFIRM WITH 'YES'.";
        await ctx.Channel.SendMessageAsync(embed: embedMessage);
        var confirmationMessage = await _interactivity.WaitForMessageAsync(message => message.Content != "");
        return !confirmationMessage.Result.Content.Equals("yes", StringComparison.CurrentCultureIgnoreCase)
            ? string.Empty
            : alias;
    }

    private async Task<ShiftsystemModel?> GetNewPersonsShiftpattern(CommandContext ctx, DiscordEmbedBuilder embedMessage)
    {
        var shiftsystems = ShiftsystemDataAccess.GetAll();
        embedMessage.Description = CreateShiftpatternMessage(shiftsystems);
        await ctx.Channel.SendMessageAsync(embed: embedMessage);
        var messageWithPersonsShiftpattern = await _interactivity.WaitForMessageAsync(message => message.Content != "");
        try
        {
            var selectedShiftpattern = Convert.ToInt32(messageWithPersonsShiftpattern.Result.Content);
            return shiftsystems[selectedShiftpattern - 1];
        }
        catch (FormatException e)
        {
            embedMessage.Description = "You didn't enter a number.";
            await ctx.Channel.SendMessageAsync(embed: embedMessage);
        }
        catch (IndexOutOfRangeException e)
        {
            embedMessage.Description = "You entered a number that wasn't in the list of shiftsystems.";
            await ctx.Channel.SendMessageAsync(embed: embedMessage);
        }
        return null;
    }

    private string CreateShiftpatternMessage(List<ShiftsystemModel> shiftsystems)
    {
        string messageString = string.Empty;
        int count = 1;
        foreach (var shiftsystem in shiftsystems)
        {
            if (messageString != string.Empty) messageString += "\n";
            messageString += $"{count} - {shiftsystem.Name}: ";
            messageString = shiftsystem.Shiftpattern.Aggregate(messageString, (current, shift) => current + $"| {shift} ");
            count++;
        }
        messageString += @"
                         Type in the number of the shiftsystem.
                         If you can't find a correct shiftsystem, you have to create one.
                         Therefore type '!createShiftpattern' and I will guide you through it.";
        return messageString;
    }

    private async Task<DateTime> GetNewPersonsShiftpatternStartDate(CommandContext ctx,
        DiscordEmbedBuilder embedMessage)
    {
        var dateRegex = new Regex(@"^\d{1,2}\.\d{1,2}\.\d{4}$");
        var date = new DateTime();
        embedMessage.Description = """
                                   When does the chosen shiftpattern start? Enter a Date to set a start point for
                                   the shiftpattern, so the program is able to calculate shift calendars for years in
                                   the past and in the future.
                                   The Date needs to have a the format 'DD.MM.YYYY'
                                   """;
        await ctx.Channel.SendMessageAsync(embed: embedMessage);
        var messageWithPersonsShiftpatternStartDate =
            await _interactivity.WaitForMessageAsync(message => message.Content != "");
        if (!dateRegex.IsMatch(messageWithPersonsShiftpatternStartDate.Result.Content)) return date;
        var content = messageWithPersonsShiftpatternStartDate.Result.Content;
        try
        {
            var dateParts = content.Split(".");
            var year = Convert.ToInt32(dateParts[2]);
            var month = Convert.ToInt32(dateParts[1]);
            var day = Convert.ToInt32(dateParts[0]);
            date = new DateTime(year, month, day);
        }
        catch (FormatException e)
        {
            Console.WriteLine($"{content} isn't a valid format.");
        }

        embedMessage.Description =
            $"So the start date of the new persons shiftpattern is '{date:dd.MM.yyyy}', right? \nCONFIRM WITH 'YES'.";
        await ctx.Channel.SendMessageAsync(embed: embedMessage);
        var confirmationMessage = await _interactivity.WaitForMessageAsync(message => message.Content != "");
        return !confirmationMessage.Result.Content.Equals("yes", StringComparison.CurrentCultureIgnoreCase)
            ? new DateTime()
            : date;
    }

    private async Task AddNewPerson(CommandContext ctx, DiscordEmbedBuilder embedMessage, PersonModel person, ShiftsystemModel shiftsystem)
    {
        embedMessage.Title = "Person:";
        embedMessage.Description = $"""
                                   Name: {person.Name}
                                   Alias: {person.Alias}
                                   Shiftsystem: {shiftsystem.Name}
                                   Shiftsystem Start Date: {person.ShiftpatternStartDate}
                                   
                                   Do you want to create this person?
                                   Confirm with 'YES'!
                                   """;
        await ctx.Channel.SendMessageAsync(embed: embedMessage);
        var confirmationMessage = await _interactivity.WaitForMessageAsync(message => message.Content != "");
        if(!confirmationMessage.Result.Content.Equals("yes", StringComparison.CurrentCultureIgnoreCase)) return;
        person.LastEditedBy = ctx.User.Username;
        person.LastEditedDate = DateTime.Now;
        if (PersonDataAccess.InsertOne(person))
        {
            embedMessage.Title = string.Empty;
            embedMessage.Description = $"Person '{person.Name}' successfully created!";
            await ctx.Channel.SendMessageAsync(embed: embedMessage);
        }
    }

    private async Task<List<PersonModel?>> GetListOfSelectedPersons(CommandContext ctx, DiscordEmbedBuilder embedMessage)
    {
        var persons = PersonDataAccess.GetAll();
        var selectedPersons = new List<PersonModel?>();
        embedMessage.Description = @"Type in the number of the person you want to add to the table.
                                    If you can't find the person you want, you have to create it first.
                                    Therefore type '!createPerson' and I will guide you through it." + "\n" + CreateListOfPersonsMessage(persons);
        await ctx.Channel.SendMessageAsync(embed: embedMessage);
        while (true)
        {
            var numberOfSelectedPersonMessage = await _interactivity.WaitForMessageAsync(message => message.Content != "");
            try
            {
                var numberOfSelectedPerson = Convert.ToInt32(numberOfSelectedPersonMessage.Result.Content);
                if (0 < numberOfSelectedPerson &&
                    numberOfSelectedPerson <= persons.Count)
                {
                    embedMessage.Description =
                        @$"You selected {persons[numberOfSelectedPerson - 1].Name}. Is that right?
                                                Confirm with 'YES'.";
                    await ctx.Channel.SendMessageAsync(embed: embedMessage);
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }

                var confirmationMessage = await _interactivity.WaitForMessageAsync(message => message.Content != "");
                if (!confirmationMessage.Result.Content.Equals("yes", StringComparison.CurrentCultureIgnoreCase))
                {
                    embedMessage.Description = "Creating a comparison table aborted.";
                    await ctx.Channel.SendMessageAsync(embed: embedMessage);
                    return null;
                }

                selectedPersons.Add(persons[numberOfSelectedPerson - 1]);
                embedMessage.Description = @$"'{persons[numberOfSelectedPerson - 1].Name}' added to comparison list.
                                            Do you want to add another person?
                                            YES/NO";
                await ctx.Channel.SendMessageAsync(embed: embedMessage);
                confirmationMessage = await _interactivity.WaitForMessageAsync(message => message.Content != "");
                if (confirmationMessage.Result.Content.Equals("yes", StringComparison.CurrentCultureIgnoreCase))
                {
                    embedMessage.Description = "Which additional person do you want to add?";
                    await ctx.Channel.SendMessageAsync(embed: embedMessage);
                }
                else
                {
                    return selectedPersons;
                }
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync($"Aborted 'creating comparison table' due:\n'{e.Message}'");
                Console.WriteLine($"Creating comparison table aborted due: {e.Message}");
            }
        }
    }
    
    private string CreateListOfPersonsMessage(List<PersonModel> persons)
    {
        var messageString = string.Empty;
        var count = 1;
        foreach (var person in persons)
        {
            if (messageString != string.Empty) messageString += "\n";
            messageString += $"{count} - {person.Name}";
            count++;
        }
        return messageString;
    }

    private async Task<int> GetYearForComparisonTable(CommandContext ctx, DiscordEmbedBuilder embedMessage)
    {
        embedMessage.Description = "Enter the year for the comparison table to be created.";
        await ctx.Channel.SendMessageAsync(embed: embedMessage);

        var yearOfTableMessage = await _interactivity.WaitForMessageAsync(message => message.Content != "");
        try
        {
            var year = Convert.ToInt32(yearOfTableMessage.Result.Content);
            if (year is >= 1900 and <= 2500)
            {
                return year;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Getting year from user for comparison table aborted due:\n{e.Message}");
        }

        return 0;
    }

    private async Task SendComparisonTableAsExcelFile(CommandContext ctx, string fileName)
    {
        await using var fs = new FileStream(Path.Combine(Constants.excelFileComparisonTablePath, $"{fileName}.xlsx"), FileMode.Open, FileAccess.Read);
        var msg = await new DiscordMessageBuilder()
            .WithContent("This is a test file")
            .AddFile($"{fileName}.xlsx", fs)
            .SendAsync(ctx.Channel);
    }
}