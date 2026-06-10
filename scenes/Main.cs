using System.Linq;
using Game.Autoload;
using Game.Buildings;
using Game.Groups;
using Godot;

namespace Game;

public partial class Main : Node
{
    private TileMapLayer treeLayer;
    private Button resetButton;
    public override void _Ready()
    {

        treeLayer = GetNode<TileMapLayer>("TreeTileMapLayer");

        resetButton = GetNode<Button>("%ResetButton");
        resetButton.Pressed += () => GetTree().ReloadCurrentScene();

        CallDeferred(nameof(RegisterTreesInGrid));
        CallDeferred(nameof(RegisterGoldminesInGrid));
        CallDeferred(nameof(PrintGridAfterRegistration));
    }

    private void RegisterTreesInGrid()
    {
        GameManager.Instance.Grid.RegisterTrees(treeLayer);
    }

    private void RegisterGoldminesInGrid()
    {
        var goldMines = GetTree().GetNodesInGroup(GlobalGroups.GOLD_RESOURCE).Cast<GoldMine>();
        foreach(var goldMine in goldMines)
        {
            GameManager.Instance.Grid.RegisterGoldmine(goldMine);
        }
    }

    private void PrintGridAfterRegistration()
    {
        GameManager.Instance.Grid.DebugPrintGrid();
    }
}
