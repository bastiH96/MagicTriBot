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

    public static PersonModel GetOne(int id)
    {
        var connection = new SQLiteConnection(Constants.fullPath);
        var result = connection.Query<PersonModel>($"SELECT * FROM Person WHERE Id = {id}").Single();
        connection.Close();
        result.Shiftsystem = ShiftsystemDataAccess.GetOne(result.ShiftsystemId);
        return result;
    }

    public static List<PersonModel> GetAll()
    {
        var connection = new SQLiteConnection(Constants.fullPath);
        var result = connection.Query<PersonModel>($"SELECT * FROM Person");
        connection.Close();
        foreach (var person in result)
        {
            person.Shiftsystem = ShiftsystemDataAccess.GetOne(person.ShiftsystemId);
        }
        return result;
    }
}