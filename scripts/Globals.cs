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