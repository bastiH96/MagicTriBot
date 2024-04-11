using DiscordBot.helpers;
using DiscordBot.models;
using SQLite;

namespace DiscordBot.services.dataAccess;

public static class PersonDataAccess
{
    public static bool CreatePersonTable()
    {
        const string query = """
                             CREATE TABLE IF NOT EXISTS Person (
                                 Id INTEGER NOT NULL,
                                 Name VARCHAR(30),
                                 Alias VARCHAR(5),
                                 ShiftpatternStartDate BIGINT,
                                 LastEditedBy VARCHAR(30),
                                 LastEditedDate BIGINT,
                                 ShiftsystemId INTEGER,
                                 FOREIGN KEY(ShiftsystemId) REFERENCES Shiftsystem(Id),
                                 PRIMARY KEY(Id AUTOINCREMENT))
                             """;
        var connection = new SQLiteConnection(Constants.fullPath);
        connection.Execute(query);
        connection.Close();
        return true;
    }

    public static bool InsertOne(PersonModel obj)
    {
        var connection = new SQLiteConnection(Constants.fullPath);
        connection.Insert(obj);
        connection.Close();
        return true;
    }
}