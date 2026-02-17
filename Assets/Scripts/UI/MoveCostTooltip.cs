using TMPro;
using UnityEngine;

public class MoveCostTooltip : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Vector2 screenOffset = new Vector2(16f, 16f);

    void Awake()
    {
        if (text == null) text = GetComponentInChildren<TMP_Text>(true);
        Hide();
    }

    public void Show(int cost, Vector3 worldPos, Camera cam)
    {
        if (cam == null || text == null) return;

        // Update label
        text.text = $"Cost: {cost}";

        // Position near the hovered tile (screen space)
        Vector3 screen = cam.WorldToScreenPoint(worldPos);
        transform.position = screen + (Vector3)screenOffset;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
}
