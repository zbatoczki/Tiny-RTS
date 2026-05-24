using System.Collections.Generic;
using System.Linq;
using Game.Buildings;
using Game.Globals;
using Game.Manager;
using Game.Units;
using Godot;
using Game.Autoload;


namespace Game.Autoload;

public partial class GameManager: Node
{
    public static GameManager Instance {get; private set;}

    [Signal] public delegate void GameOverEventHandler(bool playerWon);
    [Signal] public delegate void PopulationChangedEventHandler(int faction, int current, int max);

    public GameState State {get; private set;} = GameState.Playing;

    private readonly Dictionary<Faction, int> currentPopulation = [];
    private readonly Dictionary<Faction, int> maxPopulation = [];

    public readonly HashSet<Unit> AllUnits = [];
    public readonly HashSet<Building> AllBuildings = [];

    public GridManager Grid { get; private set; }

    public override void _Ready()
    {
        if(Instance != null)
        {
            QueueFree();
            return;
        }
        Instance = this;

        Grid = new GridManager(50, 50);

        foreach(Faction faction in System.Enum.GetValues<Faction>())
        {
            currentPopulation[faction] = 0;
            maxPopulation[faction] = 5;
        }
    }

    #region UNITS

    public void AddUnitToPopulation(Unit unit)
    {
        AllUnits.Add(unit);
        Faction faction = unit.stats.Faction;
        currentPopulation[faction]++;
        EmitSignal(SignalName.PopulationChanged, (int)faction, currentPopulation[faction], maxPopulation[faction]);
    }

     public void RemoveUnitFromPopulation(Unit unit)
    {
        AllUnits.Remove(unit);
        Faction faction = unit.stats.Faction;
        currentPopulation[faction] = Mathf.Max(0, currentPopulation[faction] - 1);
        EmitSignal(SignalName.PopulationChanged, (int)faction, currentPopulation[faction], maxPopulation[faction]);
    }

    #endregion

    #region BUILDINGS

    public void RegisterBuilding(Building building)
    {
        AllBuildings.Add(building);
        Grid?.RegisterBuilding(building);
    }

    public void UnregisterBuilding(Building building)
    {
        AllBuildings.Remove(building);
        Grid?.UnregisterBuilding(building);
    }

    #endregion

    #region POPULATION

    public int GetCurrentPopulation(Faction faction) => currentPopulation[faction];

    public int GetMaxPopulation(Faction faction) => maxPopulation[faction];

    public bool CanTrain(Faction faction) => currentPopulation[faction] < maxPopulation[faction];

    public void IncreaseMaxPopulation(Faction faction, int amount)
    {
        maxPopulation[faction] += amount;
        EmitSignal(SignalName.PopulationChanged, (int)faction, currentPopulation[faction], maxPopulation[faction]);
    }

    public void DecreaseMaxPopulation(Faction faction, int amount)
    {
        maxPopulation[faction] = Mathf.Max(5, maxPopulation[faction] - amount);
        EmitSignal(SignalName.PopulationChanged, (int)faction, currentPopulation[faction], maxPopulation[faction]);
    }

    #endregion

    public List<Unit> GetUnits(Faction faction) => [.. AllUnits.Where(unit => unit.stats.Faction == faction)];

    public List<Building> GetBuildings(Faction faction) => [.. AllBuildings.Where(building => building.Faction == faction)];

    private void CheckWinLoss()
    {
        if(State != GameState.Playing) return;

        var castles = AllBuildings.Where(building => building is Castle);
        bool playerCastleExists = castles.Any(building => building.Faction == Faction.Player);
        bool enemyCastleExists = castles.Any(building => building.Faction == Faction.Enemy);

        if(!playerCastleExists)
        {
            State = GameState.EnemyWon;
            EmitSignal(SignalName.GameOver, false);
        }
        else if (!enemyCastleExists)
        {
            State = GameState.PlayerWon;
            EmitSignal(SignalName.GameOver, true);
        }

    }
}