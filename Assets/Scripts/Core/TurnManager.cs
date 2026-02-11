using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private List<Unit> allies = new();

    private int currentIndex = -1;

    public Unit CurrentUnit { get; private set; }

    public void RegisterAlly(Unit unit)
    {
        if (!allies.Contains(unit))
            allies.Add(unit);
    }

    public void Begin()
    {
        // Reset for first turn
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

        // Find next un-acted unit, wrap.
        for (int i = 0; i < allies.Count; i++)
        {
            currentIndex = (currentIndex + 1) % allies.Count;
            if (!allies[currentIndex].hasActed)
            {
                CurrentUnit = allies[currentIndex];
                return;
            }
        }

        // If all acted, start new round
        foreach (var u in allies) u.hasActed = false;
        currentIndex = -1;
        AdvanceToNextUnit();
    }
}
