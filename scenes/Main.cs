using Game.Autoload;
using Godot;

namespace Game;

public partial class Main : Node
{
    private TileMapLayer treeLayer;

    public override void _Ready()
    {

        treeLayer = GetNode<TileMapLayer>("TreeTileMapLayer");
        CallDeferred(nameof(RegisterTreesInGrid));
    }

    private void RegisterTreesInGrid()
    {
        GameManager.Instance.Grid.RegisterTrees(treeLayer);
        GameManager.Instance.Grid.DebugPrintGrid();
    }
}
