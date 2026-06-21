using System;
using System.Collections.Generic;
using Game.Autoload;
using Game.Buildings;
using Game.Groups;
using Game.Manager;
using Game.Selection;
using Game.Units;
using Godot;

namespace Game.Context.Actions;

/// <summary>
/// Orders all selected units to attack a right-clicked enemy.
/// </summary>
public sealed class BuildAction : IContextAction
{
    public int Priority => 95;

    public bool CanHandle(SelectionContext context, GodotObject target)
        => context.HasWorkers
           && target is Building building
           && building.BuildingResource.Faction == Globals.FactionType.Player
           && building.IsUnderConstruction;

    public void Execute(SelectionContext context, GodotObject target, Vector2 mousePosition)
    {
        if(target is not Building building) return;
        
        List<Vector2I> adjacentCells = GameManager.Instance.Grid.GetAdjecentCells(building.GetGridCellPosition(), building.BuildingResource.Dimensions, true);

        // move worker units to open adject tiles and have workers swtich to build state. Replenishs health as build progress
        for (int i = 0; i < context.Workers.Count; i++)
        {
            if (adjacentCells.Count == 0) break;

            var worker = context.Workers[i];

            // pick the open adjacent cell closest to this worker
            int closestIndex = 0;
            float closestDistanceSquared = float.MaxValue;
            for (int j = 0; j < adjacentCells.Count; j++)
            {
                var cellWorldPosition = GridManager.GridCellToWorldPosition(adjacentCells[j]);
                float distanceSquared = worker.GlobalPosition.DistanceSquaredTo(cellWorldPosition);
                if (distanceSquared < closestDistanceSquared)
                {
                    closestDistanceSquared = distanceSquared;
                    closestIndex = j;
                }
            }

            var closestCell = adjacentCells[closestIndex];
            worker.ConstructionTarget = building;
            worker.ConstructionTarget.BuildingConstructed += worker.OnBuldingConstructed;

            worker.MoveTo(GridManager.GridCellToWorldPosition(closestCell));
            adjacentCells.RemoveAt(closestIndex); //avoid having multiple units go to the same cell
        }
    }
}