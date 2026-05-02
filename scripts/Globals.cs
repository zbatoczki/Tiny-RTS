using Godot;

namespace Game.Globals;

public enum Faction      { Player, Enemy }
public enum GameState    { Playing, PlayerWon, EnemyWon }
public enum ResourceType { Gold, Wood, Food }
public enum UnitTypes  { Worker, Warrior, Archer, Spearman, Monk }

public static class Costs
{
    public static readonly (int gold, int wood) Worker  = (10,  0);
    public static readonly (int gold, int wood) Warrior   = (100, 0);
    public static readonly (int gold, int wood) Archer   = (75,  50);
    public static readonly (int gold, int wood) Spearman   = (50,  75);
    public static readonly (int gold, int wood) Barracks = (150, 100);
    public static readonly (int gold, int wood) Farm     = (50,  100);
}

public static class AttackAnimationDirections
{
    public static readonly StringName AtackUp = "attack_up";
    public static readonly StringName AtackDown = "attack_down";
    public static readonly StringName UpRight = "attack_upright";
    public static readonly StringName DownRight = "attack_downright";
}