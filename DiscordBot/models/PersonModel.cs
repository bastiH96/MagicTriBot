using SQLite;

namespace DiscordBot.models;

public class PersonModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    [MaxLength(5)]
    public string Alias { get; set; }
    public DateTime ShiftpatternStartDate { get; set; }
    public string LastEditedBy { get; set; }
    public string LastEditedDate { get; set; }
    public int ShiftpatternId { get; set; }
}