using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class UnitView : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Visuals")]
    [SerializeField] private Transform visualRoot; // optional: assign, otherwise finds/creates "Visual"

    private Unit unit;
    private GameObject visualInstance;

#if UNITY_EDITOR
    private bool refreshQueued;
#endif

    private void Awake()
    {
        unit = GetComponent<Unit>();
        EnsureVisualRoot();
        RefreshVisualsImmediate();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Avoid doing Instantiate/Destroy inside OnValidate.
        unit = GetComponent<Unit>();
        EnsureVisualRoot();
        QueueEditorRefresh();
    }

    private void QueueEditorRefresh()
    {

        if (Application.isPlaying) return;
        if (refreshQueued) return;

        refreshQueued = true;
        EditorApplication.delayCall += () =>
        {
            refreshQueued = false;
            if (this == null) return;          // object may have been deleted
            if (!gameObject) return;
            RefreshVisualsImmediate();
        };
    }
#endif

    private void EnsureVisualRoot()
    {
        if (visualRoot != null) return;

        var t = transform.Find("Visual");
        if (t != null)
        {
            visualRoot = t;
            return;
        }

        var go = new GameObject("Visual");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        visualRoot = go.transform;
    }

    // Public if you want to call it after SetTeam / swapping definition at runtime.
    public void RefreshVisualsImmediate()
    {
        if (unit == null) unit = GetComponent<Unit>();
        if (unit == null || unit.Definition == null) return;
        if (unit.Definition.visualPrefab == null) return;
#if UNITY_EDITOR
        // Skip prefab assets in the Project window (persistent objects)
        if (EditorUtility.IsPersistent(gameObject)) return;
#endif

        // 1) If we already have the right prefab instance, just re-apply team material.
        // We'll detect it by storing the instance and also checking name.
        // (Optional optimization, but helps avoid churn.)
        if (visualInstance != null)
        {
            ApplyTeamMaterial();
            return;
        }

        // 2) Cleanup any existing children under Visual (from previous runs)
        CleanupVisualChildren();

        // 3) Instantiate exactly ONE instance
        visualInstance = Instantiate(unit.Definition.visualPrefab, visualRoot);
        visualInstance.name = unit.Definition.visualPrefab.name; // remove "(Clone)" noise
        visualInstance.transform.localPosition = Vector3.zero;
        visualInstance.transform.localRotation = Quaternion.identity;
        visualInstance.transform.localScale = Vector3.one;

        ApplyTeamMaterial();
    }

    private void CleanupVisualChildren()
    {
        // If there are any existing visuals under VisualRoot, destroy them safely.
        // This runs in editor via delayCall, so DestroyImmediate is safe here.
        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            var child = visualRoot.GetChild(i).gameObject;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(child);
            else
                Destroy(child);
#else
            Destroy(child);
#endif
        }

        visualInstance = null;
    }

    private void ApplyTeamMaterial()
    {
        if (unit == null || unit.Definition == null) return;

        Material mat = unit.Team == Team.Player
            ? unit.Definition.allyMaterial
            : unit.Definition.enemyMaterial;

        if (mat == null) return;

        // If visualInstance got cleared somehow, attempt to find the first child as the visual.
        if (visualInstance == null && visualRoot.childCount > 0)
            visualInstance = visualRoot.GetChild(0).gameObject;

        if (visualInstance == null) return;

        foreach (var r in visualInstance.GetComponentsInChildren<Renderer>(true))
            r.sharedMaterial = mat;
    }

    public Coroutine MoveAlongPath(List<Vector3> worldPoints)
    {
        return StartCoroutine(MoveRoutine(worldPoints));
    }

    private IEnumerator MoveRoutine(List<Vector3> pts)
    {
        if (pts == null || pts.Count == 0) yield break;

        for (int i = 0; i < pts.Count; i++)
        {
            Vector3 target = new Vector3(pts[i].x, transform.position.y, pts[i].z);
            while ((transform.position - target).sqrMagnitude > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }
}