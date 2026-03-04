using UnityEngine;

[CreateAssetMenu(menuName = "Tactics/Unit Definition", fileName = "UnitDefinition_")]
public class UnitDefinition : ScriptableObject
{
    public string displayName = "Mage";

    [Header("Visuals")]
    public GameObject visualPrefab;
    public Material allyMaterial;
    public Material enemyMaterial;

    [Header("Base Stats")]
    public UnitStats baseStats;

    [Header("Movement Type")]
    public MoveType moveType = MoveType.Walking;
}