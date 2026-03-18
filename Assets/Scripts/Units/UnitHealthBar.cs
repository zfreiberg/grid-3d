using UnityEngine;

/// <summary>
/// Renders a billboard world-space health bar above the unit.
/// Auto-added by UnitView at runtime — no prefab wiring required.
/// Adjust height per unit class via UnitDefinition.healthBarOffset.
/// </summary>
public class UnitHealthBar : MonoBehaviour
{
    private const float BarWidth   = 0.8f;
    private const float BarHeight  = 0.1f;
    private const float BaseHeight = 2.2f; // default height above unit origin

    private Unit unit;
    private Transform barRoot;     // billboarded parent
    private Transform fillTransform;
    private MeshRenderer fillRenderer;

    private static readonly Color ColorFull = new Color(0.15f, 0.85f, 0.15f);
    private static readonly Color ColorMid  = new Color(0.95f, 0.75f, 0.05f);
    private static readonly Color ColorLow  = new Color(0.85f, 0.15f, 0.15f);

    void Awake()
    {
        unit = GetComponent<Unit>();
        BuildBar();
    }

    void LateUpdate()
    {
        if (unit == null) return;

        // Billboard: face the camera each frame
        if (Camera.main != null)
            barRoot.rotation = Camera.main.transform.rotation;

        // Height: base + per-class offset from the unit definition
        float offset = unit.Definition != null ? unit.Definition.healthBarOffset : 0f;
        barRoot.localPosition = new Vector3(0f, BaseHeight + offset, 0f);

        // HP fill
        if (unit.Stats.maxHP > 0)
            UpdateFill(Mathf.Clamp01((float)unit.CurrentHP / unit.Stats.maxHP));
    }

    private void BuildBar()
    {
        // barRoot is the billboarded pivot — children inherit its rotation
        var rootGO = new GameObject("HealthBarRoot");
        barRoot = rootGO.transform;
        barRoot.SetParent(transform, false);

        // Background quad
        var bgObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bgObj.name = "HealthBarBG";
        bgObj.transform.SetParent(barRoot, false);
        bgObj.transform.localPosition = Vector3.zero;
        bgObj.transform.localScale    = new Vector3(BarWidth, BarHeight, 1f);
        Destroy(bgObj.GetComponent<Collider>());
        SetColor(bgObj.GetComponent<MeshRenderer>(), new Color(0.08f, 0.08f, 0.08f));

        // Fill quad (slightly in front to avoid z-fighting)
        var fillObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        fillObj.name = "HealthBarFill";
        fillObj.transform.SetParent(barRoot, false);
        fillObj.transform.localPosition = new Vector3(0f, 0f, -0.001f);
        fillObj.transform.localScale    = new Vector3(BarWidth, BarHeight, 1f);
        Destroy(fillObj.GetComponent<Collider>());
        fillTransform = fillObj.transform;
        fillRenderer  = fillObj.GetComponent<MeshRenderer>();
        SetColor(fillRenderer, ColorFull);
    }

    private void UpdateFill(float fraction)
    {
        // Scale width and shift left so the bar drains from the right
        float w       = BarWidth * fraction;
        float xOffset = -BarWidth * (1f - fraction) * 0.5f;
        fillTransform.localScale    = new Vector3(w, BarHeight, 1f);
        fillTransform.localPosition = new Vector3(xOffset, 0f, -0.001f);

        // Colour: green → yellow → red
        Color c = fraction > 0.5f
            ? Color.Lerp(ColorMid, ColorFull, (fraction - 0.5f) * 2f)
            : Color.Lerp(ColorLow, ColorMid,  fraction * 2f);
        SetColor(fillRenderer, c);
    }

    private static void SetColor(MeshRenderer r, Color c)
    {
        // URP uses _BaseColor; legacy shaders use _Color — set both
        r.material.SetColor("_BaseColor", c);
        r.material.SetColor("_Color",     c);
    }
}
