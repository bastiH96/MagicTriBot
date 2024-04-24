namespace DiscordBot.helpers;

public static class Constants
{
    // MAC
    // public static string shiftCalendarFolderPath = @"/Users/sebastianheyde/Public/testFile";
    // Gaming PC
    // public static string shiftCalendarFolderPath = @"D:\TestData";
    // Server - C:\Data\testData
    private static string shiftCalendarFolderPath = @"C:\Data\testData";
    public static string fullPath = Path.Combine(shiftCalendarFolderPath, "discordBotShiftsystemDb.db");
    public static string excelFileComparisonTablePath = @"C:\Data\testData\discordBot";
}