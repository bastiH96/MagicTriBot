using DiscordBot.helpers;
using DiscordBot.models;
using SQLite;

namespace DiscordBot.services.dataAccess;

public static class ShiftsystemDataAccess
{
    private static readonly string Connectionstring = Constants.fullPath;
    public static bool CreateTableShiftsystem()
    {
        var connection = new SQLiteConnection(Connectionstring);
        connection.CreateTable<ShiftsystemModel>();
        return true;
    }

    public static bool InsertOne(ShiftsystemModel obj)
    {
        var connection = new SQLiteConnection(Connectionstring);
        connection.Insert(obj);
        connection.Close();
        return true;
    }
}