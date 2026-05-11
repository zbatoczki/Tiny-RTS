using Game.Resources;
using Game.Selection;
using Game.Units;
using Godot;

namespace Game.Context.Actions;

/// <summary>
/// Orders selected worker units to gather from a right-clicked tree.
/// Requires a reference to <see cref="TreeTileMapLayerManager"/> so it can
/// resolve the tile-map cell from world position.
/// </summary>
public sealed class GatherTreeAction(TreeTileMapLayerManager _treeTileMapLayer) : IContextAction
{
    private readonly TreeTileMapLayerManager treeTileMapLayer = _treeTileMapLayer;

    public int Priority => 90;

    public bool CanHandle(SelectionContext context, GodotObject target)
        => context.HasWorkers && target is Resources.Tree;

    public void Execute(SelectionContext context, GodotObject target, Vector2 mousePosition)
    {
        Vector2 localPosition = treeTileMapLayer.ToLocal(mousePosition);
        Vector2I cellPosition = treeTileMapLayer.LocalToMap(localPosition);
        Resources.Tree treeData = treeTileMapLayer.GetTreeAt(cellPosition);

        if (treeData == null || treeData.IsDepleted)
        {
            GD.Print("GatherTreeAction: no valid tree at target cell.");
            return;
        }

        foreach (Worker worker in context.Workers)
        {
            worker.GatheringResourceTarget = treeData;
            GD.Print($"GatherTreeAction: worker targeting tree at {treeData.CellCoordinates}");
        }

        FormationHelper.MoveToFormation(context.Workers, mousePosition);
    }
}