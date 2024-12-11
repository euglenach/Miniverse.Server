using MiniverseShared.MessagePackObjects;
using MiniverseShared.Utility;

namespace Miniverse.LogicLooperServer;

public class QuestionSession
{
    public Ulid PlayerUlid {get; private set;}
    public string QuestionText {get; private set;}
    public string[] Choices {get; private set;}
    public DateTime StartTime {get; private set;}
    private readonly Dictionary<Ulid, int> selectedTable = new();
    
    public void Initialize(Ulid playerUlid, string questionText, string[] choices)
    {
        PlayerUlid = playerUlid;
        QuestionText = questionText;
        Choices = choices;
        StartTime = DateTime.Now;
    }

    public bool RegisterSelected(Ulid playerUlid, int index)
    {
        if(index < 0 || Choices.Length <= index) return false;
        if(!selectedTable.TryAdd(playerUlid, index))
        {
            selectedTable[playerUlid] = index;
        }

        return true;
    }

    public MajorityGameResult CreateResult()
    {
        var indexCounts = selectedTable.Values.GroupBy(i => i)
                                       .ToDictionary(g => g.Key, group => group.Count());
        
        var numTable = new int[Choices.Length];
        
        foreach (var item in indexCounts)
        {
            numTable[item.Key] = item.Value;
        }

        var mostIndexCount = indexCounts.Values.Max();
        var mostIndexes = indexCounts.Where(x => x.Value == mostIndexCount)
                                     .Select(x => x.Key).ToArray();

        var majorities = selectedTable.Where(x => mostIndexes.Contains(x.Value)).Select(x => x.Key).ToArray();

        return new(majorities, numTable);
    }
}
