using UnityEngine;

[CreateAssetMenu(menuName = "Tactics/Unit Definition", fileName = "UnitDefinition_")]
public class UnitDefinition : ScriptableObject
{
    [Header("Identity")]
    public string displayName = "Knight";

    [Header("Visuals")]
    public GameObject visualPrefab;     // e.g. MageVisual prefab (cylinder model etc.)
    public Material allyMaterial;       // e.g. AllyMageMat (or generic AllyUnitMaterial)
    public Material enemyMaterial;      // e.g. EnemyMageMat (or generic EnemyUnitMaterial)

    [Header("Optional: Stats defaults later")]
    public MoveType moveType = MoveType.Walking;
    public int movePoints = 6;
}