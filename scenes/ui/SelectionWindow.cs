using System;
using System.Collections.Generic;
using System.Linq;
using Game.Buildings;
using Game.Globals;
using Game.Resources;
using Game.Resources.Building;
using Game.Resources.Unit;
using Game.SelectionManager;
using Game.Units;
using Godot;
using Godot.Collections;

public partial class SelectionWindow : Control
{
	[Signal] public delegate void BuildRequestedEventHandler(BuildingResource building);

	[Export] private SelectionManager selectionManager;
	[Export] private Array<BuildingResource> buildableBuildings = [];

	private PackedScene unitCard = GD.Load<PackedScene>("res://scenes/ui/UnitCard.tscn");
	private PackedScene resourceCostPanel = GD.Load<PackedScene>("res://scenes/ui/ResourceCostPanel.tscn");

	private PanelContainer actionsPanel;
	private PanelContainer infoPanel;
	private GridContainer actionsContent;
	private VBoxContainer infoContent;

	private readonly List<Unit> selectedUnits = [];

	public override void _Ready()
	{
		actionsPanel = GetNode<PanelContainer>("%ActionsPanel");
		infoPanel = GetNode<PanelContainer>("%InformationPanel");
		actionsContent = GetNode<GridContainer>("%ActionsContent");
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
		ClearPanelContents();
		selectedUnits.Clear();
		if (units.Count == 0) { OnSelectionCleared(); return; }
		Visible = true;

		selectedUnits.AddRange(units);

		infoContent.AddChild(new Label { Text = units.Count == 1 ? units[0].Name : $"{units.Count} units selected" });

		if(units.Count == 1)
		{
			var unit = units[0];
			infoContent.AddChild(new Label { Text = $"Faction: {unit.stats.Faction}" });
			infoContent.AddChild(new Label { Text = $"HP: {unit.CurrentHealth:F0} / {unit.stats.MaxHealth:F0}" });
			infoContent.AddChild(new Label { Text = $"Attack: {unit.stats.AttackDamage}" });
			infoContent.AddChild(new Label { Text = $"Attack Speed: {unit.stats.AttackSpeed}s" });
			infoContent.AddChild(new Label { Text = $"Movement Speed: {unit.stats.MovementSpeed}" });
			infoContent.AddChild(new Label { Text = $"Gather Rate: {unit.stats.GatherRate}" });
		}

		RenderUnitActions();

		ShowPanels();
	}

	private void OnBuildingSelected(Building building)
	{
		ClearPanelContents();
		Visible = true;

		PopulateBuildingInfo(building);
		PopulateBuildingActions(building);

		ShowPanels();
	}

	private void OnResourceSelected(ResourceNode resource)
	{
		ClearPanelContents();
		Visible = true;

		infoContent.AddChild(new Label { Text = resource.Name });
		infoContent.AddChild(new Label { Text = $"Type: {resource.Type}" });
		infoContent.AddChild(new Label { Text = $"Remaining: {resource.RemainingResources} / {resource.TotalResources}" });
		// TODO: resource-specific actions (none for now)

		ShowPanels();
	}

	private void OnSelectionCleared()
	{
		selectedUnits.Clear();
		ClearPanelContents();
		Visible = false;
	}

	private void ShowPanels()
	{
		actionsPanel.Visible = actionsContent.GetChildCount() > 0;
		infoPanel.Visible = infoContent.GetChildCount() > 0;
	}

	private void ClearPanelContents()
	{
		ClearActionsContent();
		foreach (Node child in infoContent.GetChildren())
		{
			infoContent.RemoveChild(child);
			child.QueueFree();
		}
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
			case ArcheryRange archeryRange:
				AddUnitCards(archeryRange.UnitStats, archeryRange.TrainUnit);
				break;
			// TODO: Castle, ArcheryRange, GoldMine action lists drop in here.
		}
	}

	private void RenderUnitActions()
	{
		ClearActionsContent();

		AddActionButton("Move", () => selectionManager.BeginMoveCommand());
		AddActionButton("Attack", () => selectionManager.BeginAttackCommand());
		AddActionButton("Stop", () => selectionManager.StopSelectedUnits());

		bool allWorkers = selectedUnits.Count > 0 && selectedUnits.All(u => u is Worker);
		if (allWorkers)
			AddNavButton("Build", RenderBuildMenu);
	}

	private void RenderBuildMenu()
	{
		ClearActionsContent();

		AddNavButton("< Back", RenderUnitActions);

		int rendered = 0;
		foreach (BuildingResource building in buildableBuildings)
		{
			if (building == null) continue;   // skip empty inspector slots
			string label = string.IsNullOrWhiteSpace(building.Name) ? "(unnamed building)" : building.Name;
			AddActionButton(label, () => OnBuildingChosen(building), building.Description);
			rendered++;
		}

		if (rendered == 0)
			actionsContent.AddChild(new Label { Text = "No buildings available" });
	}

	private void OnBuildingChosen(BuildingResource building)
	{
		EmitSignal(SignalName.BuildRequested, building);
	}

	private void AddActionButton(string text, Action onPressed, string tooltip = null)
	{
		var button = new Button { Text = text };
		if (!string.IsNullOrEmpty(tooltip)) button.TooltipText = tooltip;
		button.Pressed += onPressed;
		actionsContent.AddChild(button);
	}

	/// <summary>
	/// Adds a button that rebuilds the action list when pressed. The rebuild is deferred so the
	/// button finishes consuming its own click before it is freed — otherwise the release event
	/// falls through to the world and re-triggers unit selection.
	/// </summary>
	private void AddNavButton(string text, Action navigate)
		=> AddActionButton(text, () => Callable.From(navigate).CallDeferred());

	private void ClearActionsContent()
	{
		foreach (Node child in actionsContent.GetChildren())
		{
			actionsContent.RemoveChild(child);
			child.QueueFree();
		}
	}

	private void AddUnitCards(Array<UnitStats> unitStats, Func<UnitTypes, bool> onTrain)
	{
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

			actionsContent.AddChild(unitCardInstance);
		}
	}

	private void SetCostsPanel(PanelContainer cardInstance, Godot.Collections.Dictionary<ResourceType, float> costs)
	{
		var costPanel = cardInstance.GetNode<HBoxContainer>("%CostPanel");
		foreach (Node child in costPanel.GetChildren())
		{
			costPanel.RemoveChild(child);
			child.QueueFree();
		}
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
			costPanel.AddChild(costPanelInstance);
		}
	}
}
