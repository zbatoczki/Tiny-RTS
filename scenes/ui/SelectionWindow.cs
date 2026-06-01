using System;
using System.Collections.Generic;
using Game.Buildings;
using Game.Globals;
using Game.Resources;
using Game.Resources.Unit;
using Game.SelectionManager;
using Game.Units;
using Godot;
using Godot.Collections;

public partial class SelectionWindow : Control
{
	[Export] private SelectionManager selectionManager;

	private PackedScene unitCard = GD.Load<PackedScene>("res://scenes/ui/UnitCard.tscn");
	private PackedScene resourceCostPanel = GD.Load<PackedScene>("res://scenes/ui/ResourceCostPanel.tscn");

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
				AddUnitCards(barracks.UnitStats, barracks.TrainUnit);
				break;
			// TODO: Castle, ArcheryRange, GoldMine action lists drop in here.
		}
	}

	private void AddUnitCards(Array<UnitStats> unitStats, Func<UnitTypes, bool> onTrain)
	{
		List<HBoxContainer> hboxContainerRows = [];
		int rowCount = 0;
		HBoxContainer hboxContainer = new();
		foreach (var stats in unitStats)
		{
			var unitCardInstance = unitCard.Instantiate<PanelContainer>();
			unitCardInstance.GetNode<Label>("%UnitName").Text = stats.Name;
			unitCardInstance.TooltipText = stats.Description;

			var unitIcon = unitCardInstance.GetNode<TextureRect>("%UnitIcon");
			unitIcon.Texture = GD.Load<AtlasTexture>(stats.IconPath);

			SetCostsPanel(unitCardInstance, stats.ResourceCosts);

			var trainButton = unitCardInstance.GetNode<Button>("%TrainButton");
			trainButton.Pressed += () =>
			{
				if (onTrain == null) return;
				onTrain(Enum.Parse<UnitTypes>(stats.Name));
			};

			hboxContainer.AddChild(unitCardInstance);
			rowCount++;

			if (rowCount >= 2)
			{
				hboxContainerRows.Add(hboxContainer);
				hboxContainer = new();
				rowCount = 0;
			}
		}
		if(hboxContainer.GetChildCount() > 0)
		{
			hboxContainerRows.Add(hboxContainer);
		}

		foreach(var row in hboxContainerRows)
		{
			actionsContent.AddChild(row);
		}
	}

	private void SetCostsPanel(PanelContainer cardInstance, Godot.Collections.Dictionary<ResourceType, float> costs)
	{
		foreach (var (resourceType, amount) in costs)
		{
			var costPanelInstance = resourceCostPanel.Instantiate<HBoxContainer>();
			costPanelInstance.GetNode<Label>("ResourceCostLabel").Text = amount.ToString();

			Texture2D resourceIcon = new PlaceholderTexture2D(){Size = new Vector2(32,32)};

			if(resourceType == ResourceType.Gold)
			{
				resourceIcon = GD.Load<Texture2D>("res://assets/resources/gold_atlus.tres");
			}
			else if(resourceType == ResourceType.Wood)
			{
				resourceIcon = GD.Load<Texture2D>("res://assets/resources/wood_atlus.tres");
			}
			
			costPanelInstance.GetNode<TextureRect>("ResourceIcon").Texture = resourceIcon;

			cardInstance.GetNode<HBoxContainer>("%CostPanel").AddChild(costPanelInstance);
		}
	}
}
