using Game.Buildings;
using Game.Globals;
using Game.Resources;
using Game.SelectionManager;
using Game.Units;
using Godot;
using Godot.Collections;

public partial class SelectionWindow : Control
{
	[Export] private SelectionManager selectionManager;

	private VBoxContainer actionsContent;
	private VBoxContainer infoContent;

	public override void _Ready()
	{
		actionsContent = GetNode<VBoxContainer>("%ActionsContent");
		infoContent = GetNode<VBoxContainer>("%InformationContent");

		ConnectSignals();
		Visible = false;
	}

	private void ConnectSignals()
	{
		if (selectionManager == null)
		{
			GD.PrintErr($"{Name}: SelectionManager export not set; panel will not react to selection.");
			return;
		}

		selectionManager.UnitsSelected += OnUnitsSelected;
		selectionManager.BuildingSelected += OnBuildingSelected;
		selectionManager.ResourceSelected += OnResourceSelected;
		selectionManager.SelectionCleared += OnSelectionCleared;
	}

	private void OnUnitsSelected(Array<Unit> units)
	{
		ClearPanels();
		if (units.Count == 0) { OnSelectionCleared(); return; }
		Visible = true;

		infoContent.AddChild(new Label { Text = units.Count == 1 ? units[0].Name : $"{units.Count} units selected" });
		// TODO: per-unit actions (move, attack, gather...) and stats (HP, faction, type)
	}

	private void OnBuildingSelected(Building building)
	{
		ClearPanels();
		Visible = true;

		PopulateBuildingInfo(building);
		PopulateBuildingActions(building);
	}

	private void OnResourceSelected(ResourceNode resource)
	{
		ClearPanels();
		Visible = true;

		infoContent.AddChild(new Label { Text = resource.Name });
		infoContent.AddChild(new Label { Text = $"Type: {resource.Type}" });
		infoContent.AddChild(new Label { Text = $"Remaining: {resource.RemainingResources} / {resource.TotalResources}" });
		// TODO: resource-specific actions (none for now)
	}

	private void OnSelectionCleared()
	{
		ClearPanels();
		Visible = false;
	}

	private void ClearPanels()
	{
		foreach (Node child in actionsContent.GetChildren()) child.QueueFree();
		foreach (Node child in infoContent.GetChildren()) child.QueueFree();
	}

	private void PopulateBuildingInfo(Building building)
	{
		string displayName = building.BuildingResource?.Name ?? building.Name;
		infoContent.AddChild(new Label { Text = displayName });
		infoContent.AddChild(new Label { Text = $"Faction: {building.Faction}" });
		infoContent.AddChild(new Label { Text = $"HP: {building.CurrentHealth:F0} / {building.MaxHealth:F0}" });
	}

	private void PopulateBuildingActions(Building building)
	{
		switch (building)
		{
			case Barracks barracks:
				AddActionButton("Train Warrior", () => barracks.TrainUnit(UnitTypes.Warrior));
				AddActionButton("Train Spearman", () => barracks.TrainUnit(UnitTypes.Spearman));
				break;
			// TODO: Castle, ArcheryRange, GoldMine action lists drop in here.
		}
	}

	private void AddActionButton(string text, System.Action onPressed)
	{
		var btn = new Button { Text = text };
		btn.Pressed += onPressed;
		actionsContent.AddChild(btn);
	}
}
