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

    [Header("Stats (can be overridden per-unit if you want)")]
    public MoveType moveType = MoveType.Walking;
    public int movePoints = 6;

    [Header("Runtime")]
    public GridCoord currentCoord;
    public bool hasActed;

    public void SetCoord(GridCoord coord) => currentCoord = coord;
    public void SetTeam(Team newTeam) => team = newTeam;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Optional: if you want definition to set defaults when you assign it
        if (definition != null)
        {
            moveType = definition.moveType;
            movePoints = definition.movePoints;
        }
    }
#endif
}