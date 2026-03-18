#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom inspector for GridManager that adds a tile painter tool.
/// In the inspector: toggle Paint Mode, pick a tile type, then click/drag
/// on the grid in the Scene view to paint tiles.
/// Painted data is saved to the TileMapData ScriptableObject (Undo-aware).
/// </summary>
[CustomEditor(typeof(GridManager))]
public class GridPainterEditor : Editor
{
    private TileType selectedType = TileType.Grass;
    private bool     paintMode    = false;

    // Per-type display info: label, semi-transparent overlay, button tint
    private static readonly string[] TypeNames = { "Grass", "Road", "Forest", "Water", "Sand" };

    private static readonly Color[] TypeOverlay =
    {
        new Color(0.13f, 0.55f, 0.13f, 0.40f), // Grass
        new Color(0.55f, 0.47f, 0.35f, 0.40f), // Road
        new Color(0.00f, 0.27f, 0.00f, 0.40f), // Forest
        new Color(0.10f, 0.30f, 0.80f, 0.40f), // Water
        new Color(0.86f, 0.76f, 0.46f, 0.40f), // Sand
    };

    // ── Inspector ────────────────────────────────────────────────────────────

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var gm = (GridManager)target;

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Tile Painter", EditorStyles.boldLabel);

        var tileMapDataProp = serializedObject.FindProperty("tileMapData");
        if (tileMapDataProp?.objectReferenceValue == null)
            EditorGUILayout.HelpBox(
                "Assign a TileMapData asset to enable painting.\n" +
                "Right-click in the Project window → Create → Tactics → Tile Map Data",
                MessageType.Warning);

        EditorGUI.BeginChangeCheck();
        paintMode = GUILayout.Toggle(
            paintMode,
            paintMode ? "Paint Mode: ON" : "Paint Mode: OFF",
            "Button",
            GUILayout.Height(28));
        if (EditorGUI.EndChangeCheck())
            SceneView.RepaintAll();

        if (!paintMode) return;

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Brush:", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < TypeNames.Length; i++)
        {
            bool isSelected = (int)selectedType == i;
            Color oldBg = GUI.backgroundColor;
            GUI.backgroundColor = isSelected ? TypeOverlay[i] * 2f : TypeOverlay[i];
            if (GUILayout.Toggle(isSelected, TypeNames[i], "Button"))
                selectedType = (TileType)i;
            GUI.backgroundColor = oldBg;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);
        if (GUILayout.Button("Apply Tile Map to Scene"))
        {
            gm.ApplyTileMap();
            EditorUtility.SetDirty(gm);
        }

        EditorGUILayout.HelpBox(
            "Click or drag tiles in the Scene view to paint. Changes are saved to TileMapData.",
            MessageType.Info);
    }

    // ── Scene GUI ────────────────────────────────────────────────────────────

    private void OnSceneGUI()
    {
        var gm = (GridManager)target;
        if (gm == null) return;

        var tileMapData = serializedObject.FindProperty("tileMapData")
                              ?.objectReferenceValue as TileMapData;

        DrawTileOverlays(gm, tileMapData);

        if (paintMode)
            HandlePainting(gm, tileMapData);
    }

    // Draws a coloured semi-transparent quad over every tile to show its type.
    private void DrawTileOverlays(GridManager gm, TileMapData tileMapData)
    {
        bool hasData = tileMapData != null
                    && tileMapData.tiles != null
                    && tileMapData.width  == gm.Width
                    && tileMapData.height == gm.Height;

        float hs = gm.TileSize * 0.49f; // slight inset so grid lines remain visible

        for (int z = 0; z < gm.Height; z++)
        {
            for (int x = 0; x < gm.Width; x++)
            {
                TileType type = hasData ? tileMapData.Get(x, z) : TileType.Grass;
                int      idx  = Mathf.Clamp((int)type, 0, TypeOverlay.Length - 1);

                Vector3 c = gm.CoordToWorldCenter(new GridCoord(x, z));
                Vector3[] corners =
                {
                    c + new Vector3(-hs, 0.01f, -hs),
                    c + new Vector3( hs, 0.01f, -hs),
                    c + new Vector3( hs, 0.01f,  hs),
                    c + new Vector3(-hs, 0.01f,  hs),
                };

                Color fill    = TypeOverlay[idx];
                Color outline = new Color(fill.r * 0.5f, fill.g * 0.5f, fill.b * 0.5f, 0.85f);
                Handles.DrawSolidRectangleWithOutline(corners, fill, outline);
            }
        }
    }

    // Intercepts left-mouse clicks/drags in the Scene view and paints tiles.
    private void HandlePainting(GridManager gm, TileMapData tileMapData)
    {
        if (tileMapData == null) return;

        Event  e      = Event.current;
        int    ctrlId = GUIUtility.GetControlID(FocusType.Passive);

        // Claim the default control so the click doesn't deselect the GameObject.
        if (e.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(ctrlId);
            return;
        }

        // Keep scene repainting so the hover overlay updates.
        if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
            HandleUtility.Repaint();

        bool isPaintEvent = (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
                         && e.button == 0
                         && !e.alt;
        if (!isPaintEvent) return;

        // Raycast against the Y = 0 plane.
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Mathf.Abs(ray.direction.y) < 0.0001f) return;

        float   t        = -ray.origin.y / ray.direction.y;
        if (t < 0f) return;

        Vector3   worldHit = ray.origin + ray.direction * t;
        GridCoord coord    = gm.WorldToCoord(worldHit);

        if (!gm.IsInBounds(coord)) return;

        // Auto-resize data if grid dimensions changed.
        if (tileMapData.width != gm.Width || tileMapData.height != gm.Height)
        {
            Undo.RecordObject(tileMapData, "Resize Tile Map");
            tileMapData.Resize(gm.Width, gm.Height);
        }

        if (tileMapData.Get(coord.x, coord.z) == selectedType) return; // no change

        Undo.RecordObject(tileMapData, "Paint Tile");
        tileMapData.Set(coord.x, coord.z, selectedType);
        EditorUtility.SetDirty(tileMapData);

        // Immediately update the scene visual.
        gm.SetTileType(coord, selectedType, updateData: false);

        e.Use();
    }
}
#endif
