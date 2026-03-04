using TMPro;
using UnityEngine;

public class UnitStatsPanelUI : MonoBehaviour
{
    [Header("Top")]
    [SerializeField] private TMP_Text classNameText;
    [SerializeField] private TMP_Text hpText;

    [Header("Stats")]
    [SerializeField] private TMP_Text moveText;
    [SerializeField] private TMP_Text strText;
    [SerializeField] private TMP_Text magText;
    [SerializeField] private TMP_Text defText;
    [SerializeField] private TMP_Text resText;
    [SerializeField] private TMP_Text spdText;
    [SerializeField] private TMP_Text skillText;
    [SerializeField] private TMP_Text luckText;
    [SerializeField] private TMP_Text critText;

    // Add more fields when you add more UI rows.

    public void Show(Unit unit)
    {
        if (unit == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        // Name / Class
        if (classNameText != null)
        {
            string className = unit.Definition != null ? unit.Definition.displayName : unit.name;
            classNameText.text = $"{className} ({unit.Team})";
        }

        // If you have CurrentHP + Stats:
        var s = unit.Stats;

        if (hpText != null)
            hpText.text = $"HP {unit.CurrentHP} / {s.maxHP}";

        if (moveText != null)
            moveText.text = $"Move {s.move}";

        if (strText != null) strText.text = $"Str {s.str}";
        if (magText != null) magText.text = $"Mag {s.mag}";
        if (defText != null) defText.text = $"Def {s.def}";
        if (resText != null) resText.text = $"Res {s.res}";
        if (spdText != null) spdText.text = $"Spd {s.spd}";
        if (skillText != null) skillText.text = $"Skl {s.skill}";
        if (luckText != null) luckText.text = $"Lck {s.luck}";
        if (critText != null) critText.text = $"Crit {s.crit}%";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}