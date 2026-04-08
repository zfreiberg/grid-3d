using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton that persists across scenes and owns the entire run state:
/// current MapSettings, MapData, and MapState.
///
/// Usage:
///   RunManager.Instance.StartNewRun(settings);   // from MainMenu
///   RunManager.Instance.EnterNode(nodeId);        // from MapController
///   RunManager.Instance.CompleteCurrentNode();    // from each game scene when done
/// </summary>
public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    public MapSettings Settings    { get; private set; }
    public MapData     CurrentMap  { get; private set; }
    public MapState    State       { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void StartNewRun(MapSettings settings = null)
    {
        Settings = settings ?? new MapSettings();
        GenerateActMap();
    }

    /// <summary>Call from a game scene when the player finishes the node.</summary>
    public void CompleteCurrentNode()
    {
        if (State?.currentNodeId == null) return;

        var node = CurrentMap.GetNode(State.currentNodeId);
        State.completedNodeIds.Add(State.currentNodeId);
        State.accessibleNodeIds.Remove(State.currentNodeId);

        if (node.type == NodeType.Boss)
        {
            AdvanceAct();
            return;
        }

        foreach (var nextId in node.nextNodeIds)
            if (!State.completedNodeIds.Contains(nextId))
                State.accessibleNodeIds.Add(nextId);

        State.currentNodeId = null;
        SceneManager.LoadScene("MapScene");
    }

    /// <summary>Called by MapController when the player clicks a node.</summary>
    public void EnterNode(string nodeId)
    {
        if (!State.IsAccessible(nodeId)) return;

        State.currentNodeId    = nodeId;
        NodeSceneContext.CurrentNode = CurrentMap.GetNode(nodeId);

        string sceneName = NodeTypeSceneMap.GetScene(NodeSceneContext.CurrentNode.type);

        if (!SceneExistsInBuild(sceneName))
        {
            Debug.LogWarning($"[RunManager] Scene '{sceneName}' is not in build settings. " +
                             "Completing node and returning to map.");
            CompleteCurrentNode();
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    // -------------------------------------------------------------------------
    // Internal helpers
    // -------------------------------------------------------------------------

    void GenerateActMap()
    {
        CurrentMap = MapGenerator.Generate(Settings);
        State      = new MapState();
        foreach (var node in CurrentMap.GetStartNodes())
            State.accessibleNodeIds.Add(node.id);
    }

    void AdvanceAct()
    {
        Settings.act++;
        if (Settings.act > Settings.totalActs)
        {
            // Run complete
            Destroy(gameObject);
            Instance = null;
            SceneManager.LoadScene("MainMenu");
            return;
        }
        GenerateActMap();
        SceneManager.LoadScene("MapScene");
    }

    static bool SceneExistsInBuild(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (path.Contains(sceneName)) return true;
        }
        return false;
    }
}
