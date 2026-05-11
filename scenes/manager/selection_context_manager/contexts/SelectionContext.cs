using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Game.Buildings;
using Game.Units;

namespace Game.Selection;

public sealed class SelectionContext
{
    public enum SelectionType
    {
        None,
        Units,
        Building
    }

    public static readonly SelectionContext Empty = new();

    public SelectionType Type { get; }

    //Unit selection
    public IReadOnlyList<Unit> Units { get; }
    public IReadOnlyList<Worker> Workers { get; }
    public bool HasUnits => Units.Count > 0;
    public bool HasWorkers => Workers.Count > 0;

    //Building selection
    public Building SelectedBuilding {get;}
    public bool HasBuilding => SelectedBuilding != null;

    //Empty / No selection
    private SelectionContext()
    {
        Type = SelectionType.None;
        Units = [];
        Workers = []; 
    }

        // Unit selection
    public SelectionContext(IReadOnlyList<Unit> units)
    {
        Type = SelectionType.Units;
        Units = units;
        Workers = [.. units.OfType<Worker>()];
    }
 
    // Building selection
    public SelectionContext(Building building)
    {
        Type = SelectionType.Building;
        Units = [];
        Workers = [];
        SelectedBuilding = building;
    }

}