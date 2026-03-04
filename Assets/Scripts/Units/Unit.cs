using UnityEngine;

public enum MoveType { Walking, Mounted, Flying }
public enum Team { Player, Enemy }
public class Unit : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private UnitDefinition definition;
    public UnitDefinition Definition => definition;

    [SerializeField] private Team team = Team.Player;
    public Team Team => team;

    [Header("Runtime Stats")]
    [SerializeField] private UnitStats stats;
    public UnitStats Stats => stats;

    [Header("Runtime")]
    public GridCoord currentCoord;
    public bool hasActed;

    public int CurrentHP { get; private set; }

    private void Awake()
    {
        InitializeFromDefinitionIfNeeded();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Editor convenience: keep runtime copy aligned when you change definition
        // (Optional: comment out if you want to override per-unit in-scene)
        if (definition != null)
        {
            stats = definition.baseStats;
            CurrentHP = Mathf.Clamp(CurrentHP == 0 ? stats.maxHP : CurrentHP, 0, stats.maxHP);
            moveType = definition.moveType;
            movePoints = stats.move; // if you still use movePoints elsewhere
        }
    }
#endif

    // If you still have these fields used by pathfinding/UI, keep them:
    [Header("Legacy (optional)")]
    public MoveType moveType = MoveType.Walking;
    public int movePoints = 6;

    public void InitializeFromDefinitionIfNeeded()
    {
        if (definition == null) return;

        // If you prefer always overwrite, just assign directly.
        stats = definition.baseStats;
        CurrentHP = stats.maxHP;

        moveType = definition.moveType;
        movePoints = stats.move;
    }

    public void SetCoord(GridCoord coord) => currentCoord = coord;
    public void SetTeam(Team newTeam) => team = newTeam;
}