using UnityEngine;

[System.Serializable]
public struct UnitStats
{
    [Header("Core")]
    public int maxHP;
    public int move;

    [Header("Offense")]
    public int str;
    public int mag;
    public int skill;      // Dex
    public int spd;
    public int attackRange; // Tiles reachable in attack (1 = melee, 2 = ranged, etc.)

    [Header("Defense")]
    public int def;
    public int res;        // Mag Defense

    [Header("Utility")]
    public int luck;

    [Header("Derived / Prototype (you can remove later)")]
    [Range(0, 100)] public int crit;   // %
    [Range(0, 200)] public int hit;    // %
    [Range(0, 200)] public int avoid;  // %
}