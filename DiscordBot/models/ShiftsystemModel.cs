using System.Text.Json.Nodes;
using Newtonsoft.Json;
using SQLite;

namespace DiscordBot.models;

[Table("Shiftsystem")]
public class ShiftsystemModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Column("Shiftpattern")]
    public string ShiftpatternSerialized
    {
        get => JsonConvert.SerializeObject(Shiftpattern);
        set => Shiftpattern = JsonConvert.DeserializeObject<List<string>>(value)!;
    }
    public DateTime StartDate { get; set; }
    [Ignore]public int IteratorIndex { get; set; } = 0;
    [Ignore] public List<string> Shiftpattern { get; set; }

    public ShiftsystemModel()
    {
        
    }

    public ShiftsystemModel(List<string> shiftpattern, DateTime startDate)
    {
        this.Shiftpattern = shiftpattern;
        this.StartDate = startDate;
    }
}