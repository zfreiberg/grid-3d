using UnityEngine;

public class ActionMenuController : MonoBehaviour
{
    [SerializeField] private RectTransform root; // ActionMenuRoot
    [SerializeField] private UnitStatsPanelUI statsPanel;

    private TacticalStateMachine stateMachine;
    private Unit unit;

    void Awake()
    {
        if (root == null) root = GetComponent<RectTransform>();
        if (statsPanel == null) statsPanel = GetComponentInParent<Canvas>().GetComponentInChildren<UnitStatsPanelUI>(true);
        Hide();
    }

    public void ShowForUnit(Unit u, TacticalStateMachine sm)
    {
        unit = u;
        stateMachine = sm;
        statsPanel?.Show(unit);

        if (root != null) root.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (root != null) root.gameObject.SetActive(false);
        statsPanel?.Hide();
        unit = null;
        stateMachine = null;
    }

    public void OnMovePressed() => stateMachine.StartMoveSelection();
    public void OnAttackPressed() => stateMachine.StartAttackSelection();
    public void OnWaitPressed() => stateMachine.Wait();
}