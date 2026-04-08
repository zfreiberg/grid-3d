using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Builds and manages the map UI entirely at runtime.
/// Attach to the MapRoot GameObject inside the map scene's Canvas.
/// </summary>
public class MapController : MonoBehaviour
{
    // ---- Layout ----
    const float COL_SPACING   = 120f;
    const float ROW_SPACING   = 150f;
    const float NODE_SIZE     = 64f;
    const float MAP_PAD_X     = 80f;
    const float MAP_HEIGHT    = 480f;
    const float HEADER_HEIGHT = 80f;

    [SerializeField] MapVisualConfig visualConfig;

    TextMeshProUGUI actLabel;
    TextMeshProUGUI seedLabel;
    RectTransform   mapContent;

    // Cached fallback — Unity's built-in Knob (circle) sprite
    static Sprite KnobSprite => Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");

    Sprite NodeSprite(NodeType type) =>
        (visualConfig != null ? visualConfig.GetNodeSprite(type) : null) ?? KnobSprite;

    Sprite DotSprite() =>
        (visualConfig != null && visualConfig.dotSprite != null ? visualConfig.dotSprite : null) ?? KnobSprite;

    void ApplyMaterial(Image img)
    {
        if (visualConfig != null && visualConfig.defaultNodeMaterial != null)
            img.material = visualConfig.defaultNodeMaterial;
    }

    // -------------------------------------------------------------------------

    void Start()
    {
        EnsureRunManager();
        BuildUI();
        DrawMap();
    }

    // -------------------------------------------------------------------------
    // Bootstrap
    // -------------------------------------------------------------------------

    void EnsureRunManager()
    {
        if (RunManager.Instance != null && RunManager.Instance.CurrentMap != null) return;

        var go = new GameObject("RunManager");
        var rm = go.AddComponent<RunManager>();
        rm.StartNewRun(new MapSettings());
    }

    // -------------------------------------------------------------------------
    // UI construction (runs once per scene load)
    // -------------------------------------------------------------------------

    void BuildUI()
    {
        // --- Header ---
        var header     = MakeRect("Header", transform);
        header.anchorMin = new Vector2(0, 1);
        header.anchorMax = new Vector2(1, 1);
        header.pivot     = new Vector2(0.5f, 1);
        header.sizeDelta = new Vector2(0, HEADER_HEIGHT);
        header.anchoredPosition = Vector2.zero;

        actLabel  = MakeTMP("ActLabel",  header, "Act 1", 36,
                            new Vector2(0,    0), new Vector2(0.5f, 1), TextAlignmentOptions.Left);
        seedLabel = MakeTMP("SeedLabel", header, "",       18,
                            new Vector2(0.5f, 0), new Vector2(1,    1), TextAlignmentOptions.Right);

        // Back-to-menu button (top-right corner)
        var backBtn = MakeButton("MenuButton", header, "Menu",
                                 new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                                 new Vector2(-80, 0), new Vector2(120, 48));
        backBtn.onClick.AddListener(() => {
            Destroy(RunManager.Instance?.gameObject);
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        });

        // --- Scroll view ---
        var scrollGO   = new GameObject("MapScroll");
        scrollGO.transform.SetParent(transform, false);
        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        var scrollRt   = scrollGO.GetComponent<RectTransform>();
        scrollRt.anchorMin = Vector2.zero;
        scrollRt.anchorMax = Vector2.one;
        scrollRt.offsetMin = new Vector2(0, 0);
        scrollRt.offsetMax = new Vector2(0, -HEADER_HEIGHT);

        // Viewport
        var vpGO   = new GameObject("Viewport");
        vpGO.transform.SetParent(scrollGO.transform, false);
        var vpImg  = vpGO.AddComponent<Image>();
        vpImg.color = new Color(0, 0, 0, 0);
        var vpMask = vpGO.AddComponent<Mask>();
        vpMask.showMaskGraphic = false;
        var vpRt   = vpGO.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
        vpRt.offsetMin = Vector2.zero; vpRt.offsetMax = Vector2.zero;

        // Map content (nodes and lines go here)
        var contentGO = new GameObject("MapContent");
        contentGO.transform.SetParent(vpGO.transform, false);
        mapContent = contentGO.AddComponent<RectTransform>();
        mapContent.anchorMin        = new Vector2(0, 0.5f);
        mapContent.anchorMax        = new Vector2(0, 0.5f);
        mapContent.pivot            = new Vector2(0, 0.5f);
        mapContent.anchoredPosition = Vector2.zero;
        mapContent.sizeDelta        = new Vector2(2000, MAP_HEIGHT);

        scrollRect.content          = mapContent;
        scrollRect.viewport         = vpRt;
        scrollRect.horizontal       = true;
        scrollRect.vertical         = false;
        scrollRect.movementType     = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 20f;
    }

    // -------------------------------------------------------------------------
    // Map drawing
    // -------------------------------------------------------------------------

    void DrawMap()
    {
        var map   = RunManager.Instance.CurrentMap;
        var state = RunManager.Instance.State;

        actLabel.text  = $"Act {map.act}  |  {map.totalColumns - 1} nodes";
        seedLabel.text = $"Seed {map.seed}";

        float contentWidth = map.totalColumns * COL_SPACING + MAP_PAD_X * 2f;
        mapContent.sizeDelta = new Vector2(Mathf.Max(contentWidth, 1800f), MAP_HEIGHT);

        // Connections drawn first so they sit behind nodes
        foreach (var node in map.nodes)
        {
            foreach (var nextId in node.nextNodeIds)
            {
                var toNode = map.GetNode(nextId);
                if (toNode != null)
                    DrawDots(NodePos(node), NodePos(toNode), state.IsCompleted(node.id));
            }
        }

        foreach (var node in map.nodes)
            DrawNode(node, state);
    }

    Vector2 NodePos(NodeData node)
    {
        float x = MAP_PAD_X + node.column * COL_SPACING;
        float y = (node.row - 1f) * ROW_SPACING;   // row 1 = vertical centre
        return new Vector2(x, y);
    }

    void DrawNode(NodeData node, MapState state)
    {
        bool accessible = state.IsAccessible(node.id);
        bool completed  = state.IsCompleted(node.id);

        var go  = new GameObject($"Node_{node.id}");
        go.transform.SetParent(mapContent, false);

        // Accessible glow outline (drawn behind the node image)
        if (accessible)
        {
            var glow    = new GameObject("Glow");
            glow.transform.SetParent(go.transform, false);
            glow.transform.SetAsFirstSibling();
            var glowImg = glow.AddComponent<Image>();
            glowImg.sprite = NodeSprite(node.type);
            glowImg.color = new Color(1f, 1f, 1f, 0.25f);
            glowImg.raycastTarget = false;
            ApplyMaterial(glowImg);
            var gr = glow.GetComponent<RectTransform>();
            gr.anchorMin = Vector2.zero; gr.anchorMax = Vector2.one;
            gr.offsetMin = new Vector2(-6, -6); gr.offsetMax = new Vector2(6, 6);
        }

        var img = go.AddComponent<Image>();
        img.sprite = NodeSprite(node.type);
        img.color  = NodeColor(node.type, accessible, completed);
        ApplyMaterial(img);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.interactable  = accessible;

        var cb = btn.colors;
        cb.highlightedColor = Color.Lerp(img.color, Color.white, 0.25f);
        cb.pressedColor     = Color.Lerp(img.color, Color.black, 0.25f);
        cb.disabledColor    = img.color;
        btn.colors = cb;

        if (accessible)
        {
            string id = node.id;
            btn.onClick.AddListener(() => RunManager.Instance.EnterNode(id));
        }

        var r = go.GetComponent<RectTransform>();
        r.sizeDelta        = new Vector2(NODE_SIZE, NODE_SIZE);
        r.anchoredPosition = NodePos(node);

        // Type symbol label
        var labelGO  = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var label    = labelGO.AddComponent<TextMeshProUGUI>();
        label.text           = NodeSymbol(node.type);
        label.fontSize       = 24;
        label.alignment      = TextAlignmentOptions.Center;
        label.color          = completed ? new Color(1, 1, 1, 0.35f) : Color.white;
        label.raycastTarget  = false;
        var lr = labelGO.GetComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;
    }

    void DrawDots(Vector2 from, Vector2 to, bool completed)
    {
        float dist     = Vector2.Distance(from, to);
        int   dotCount = Mathf.Max(3, (int)(dist / 22f));
        var   color    = completed
            ? new Color(0.40f, 0.75f, 0.40f, 0.70f)
            : new Color(0.55f, 0.55f, 0.65f, 0.55f);

        for (int i = 1; i < dotCount; i++)
        {
            float t   = (float)i / dotCount;
            var dotGO = new GameObject("Dot");
            dotGO.transform.SetParent(mapContent, false);
            var img  = dotGO.AddComponent<Image>();
            img.sprite        = DotSprite();
            img.color         = color;
            img.raycastTarget = false;
            ApplyMaterial(img);
            var dr = dotGO.GetComponent<RectTransform>();
            dr.sizeDelta        = new Vector2(6f, 6f);
            dr.anchoredPosition = Vector2.Lerp(from, to, t);
        }
    }

    // -------------------------------------------------------------------------
    // UI helpers
    // -------------------------------------------------------------------------

    static RectTransform MakeRect(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    static TextMeshProUGUI MakeTMP(string name, RectTransform parent, string text, int size,
        Vector2 anchorMin, Vector2 anchorMax, TextAlignmentOptions align)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = Color.white;
        tmp.alignment = align;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = anchorMin; r.anchorMax = anchorMax;
        r.offsetMin = new Vector2(16, 0); r.offsetMax = new Vector2(-16, 0);
        return tmp;
    }

    static Button MakeButton(string name, RectTransform parent, string label,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.18f, 0.22f, 0.42f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = anchorMin; r.anchorMax = anchorMax;
        r.pivot     = new Vector2(1, 0.5f);
        r.sizeDelta = size; r.anchoredPosition = anchoredPos;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 20;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        var tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;

        return btn;
    }

    // -------------------------------------------------------------------------
    // Node visual data
    // -------------------------------------------------------------------------

    static Color NodeColor(NodeType type, bool accessible, bool completed)
    {
        if (completed)   return new Color(0.22f, 0.22f, 0.28f);
        if (!accessible) return new Color(0.14f, 0.14f, 0.20f);
        return type switch
        {
            NodeType.Battle => new Color(0.85f, 0.20f, 0.20f),
            NodeType.Event  => new Color(0.50f, 0.28f, 0.85f),
            NodeType.Shop   => new Color(0.85f, 0.72f, 0.10f),
            NodeType.Rest   => new Color(0.20f, 0.75f, 0.40f),
            NodeType.Boss   => new Color(0.95f, 0.10f, 0.50f),
            _               => Color.white
        };
    }

    static string NodeSymbol(NodeType type) => type switch
    {
        NodeType.Battle => "ATK",
        NodeType.Event  => "?",
        NodeType.Shop   => "$",
        NodeType.Rest   => "+",
        NodeType.Boss   => "!!!",
        _               => "o"
    };
}
