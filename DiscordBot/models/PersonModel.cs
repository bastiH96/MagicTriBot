using System.ComponentModel.DataAnnotations.Schema;
using SQLite;

namespace DiscordBot.models;

[SQLite.Table("Person")]
public class PersonModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [MaxLength(30)]
    public string Name { get; set; }
    [MaxLength(5)]
    public string Alias { get; set; }
    public DateTime ShiftpatternStartDate { get; set; }
    [MaxLength(30)]
    public string LastEditedBy { get; set; }
    public DateTime LastEditedDate { get; set; }
    [ForeignKey("ShiftsystemId")]
    public int ShiftsystemId { get; set; }

    public PersonModel()
    {
        
    }

    public PersonModel(string name, string alias, DateTime shiftpatternStartDate, int shiftsystemId)
    {
        this.Name = name;
        this.Alias = alias;
        this.ShiftpatternStartDate = shiftpatternStartDate;
        this.ShiftsystemId = shiftsystemId;
    }
}