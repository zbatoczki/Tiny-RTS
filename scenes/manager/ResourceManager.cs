using Game.Autoload;
using Godot;

namespace Game.Manager;

public partial class ResourceManager : Node
{
	private int CurrentWoodAmount = 0;
	private int CurrentGoldAmount = 0;
	private int CurrentFoodAmount = 0;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ResourceEvents.Instance.ResourcesModified += OnResourcesModified;
	}

	private void OnResourcesModified(int woodAmt = 0, int goldAmt = 0, int foodAmt = 0)
	{
		CurrentFoodAmount += foodAmt;
		CurrentGoldAmount += goldAmt;
		CurrentWoodAmount += woodAmt;

		GD.Print($"Wood: {CurrentWoodAmount} | Gold: {CurrentGoldAmount} | Food: {CurrentFoodAmount}");
	}
}
