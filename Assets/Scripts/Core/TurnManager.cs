using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("Scene Roots")]
    [SerializeField] private Transform alliesRoot;
    [SerializeField] private Transform enemiesRoot;

    [Header("Runtime Lists")]
    [SerializeField] private List<Unit> allies = new();
    [SerializeField] private List<Unit> enemies = new();

    private int currentIndex = -1;
    public Unit CurrentUnit { get; private set; }

    public void RegisterAlly(Unit unit)
    {
        if (unit == null) return;
        if (!allies.Contains(unit)) allies.Add(unit);
    }

    public void RegisterEnemy(Unit unit)
    {
        if (unit == null) return;
        if (!enemies.Contains(unit)) enemies.Add(unit);
    }

    public void BuildListsFromRoots()
    {
        allies.Clear();
        enemies.Clear();

        if (alliesRoot != null)
        {
            foreach (var u in alliesRoot.GetComponentsInChildren<Unit>(true))
            {
                u.SetTeam(Team.Player);
                RegisterAlly(u);
            }
        }

        if (enemiesRoot != null)
        {
            foreach (var u in enemiesRoot.GetComponentsInChildren<Unit>(true))
            {
                u.SetTeam(Team.Enemy);
                RegisterEnemy(u);
            }
        }
    }

    public void Begin()
    {
        BuildListsFromRoots();
        foreach (var u in allies) u.hasActed = false;
        AdvanceToNextUnit();
    }

    public void AdvanceToNextUnit()
    {
        if (allies.Count == 0)
        {
            CurrentUnit = null;
            return;
        }

        for (int i = 0; i < allies.Count; i++)
        {
            currentIndex = (currentIndex + 1) % allies.Count;
            if (!allies[currentIndex].hasActed)
            {
                CurrentUnit = allies[currentIndex];
                return;
            }
        }

        foreach (var u in allies) u.hasActed = false;
        currentIndex = -1;
        AdvanceToNextUnit();
    }
}