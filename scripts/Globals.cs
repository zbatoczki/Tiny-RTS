using Godot;

namespace Game.Globals;

public enum FactionType      { Player, Enemy }
public enum GameState    { Playing, PlayerWon, EnemyWon }
public enum ResourceType { Gold, Wood, Food }
public enum UnitTypes  { Worker, Warrior, Archer, Spearman, Monk }

public static class AttackAnimationDirections
{
    public static readonly StringName AtackUp = "attack_up";
    public static readonly StringName AtackDown = "attack_down";
    public static readonly StringName UpRight = "attack_upright";
    public static readonly StringName DownRight = "attack_downright";
}

public static class GlobalValues
{
    public const int CELL_SIZE = 64;
    public const int HALF_CELL_SIZE = CELL_SIZE / 2;
}