using System.Text.Json.Nodes;
using Newtonsoft.Json;
using SQLite;

namespace DiscordBot.models;

[Table("Shiftsystem")]
public class ShiftsystemModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; }
    [Column("Shiftpattern")]
    public string ShiftpatternSerialized
    {
        get => JsonConvert.SerializeObject(Shiftpattern);
        set => Shiftpattern = JsonConvert.DeserializeObject<List<string>>(value)!;
    }
    [Ignore]public int IteratorIndex { get; set; } = 0;
    [Ignore] public List<string> Shiftpattern { get; set; }

    public ShiftsystemModel()
    {
        
    }

    public ShiftsystemModel(string name, List<string> shiftpattern)
    {
        this.Name = name;
        this.Shiftpattern = shiftpattern;
    }
}