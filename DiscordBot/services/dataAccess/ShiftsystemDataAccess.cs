using DiscordBot.helpers;
using DiscordBot.models;
using SQLite;

namespace DiscordBot.services.dataAccess;

public static class ShiftsystemDataAccess
{
    public static bool CreateTableShiftsystem()
    {
        var connection = new SQLiteConnection(Constants.fullPath);
        connection.CreateTable<ShiftsystemModel>();
        connection.Close();
        return true;
    }

    public static List<ShiftsystemModel> GetAll()
    {
        var connection = new SQLiteConnection(Constants.fullPath);
        var results = connection.Query<ShiftsystemModel>("SELECT * FROM Shiftsystem");
        connection.Close();
        return results.ToList();
    }

    public static bool InsertOne(ShiftsystemModel obj)
    {
        var connection = new SQLiteConnection(Constants.fullPath);
        connection.Insert(obj);
        connection.Close();
        return true;
    }
}