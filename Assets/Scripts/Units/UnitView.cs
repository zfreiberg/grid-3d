using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitView : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Visuals")]
    [SerializeField] private Transform visualRoot; // assign in prefab, or it will use/create "Visual"
    [SerializeField] private bool applyTeamMaterialToAllRenderers = true;

    private Unit unit;
    private GameObject currentVisualInstance;

    private void Awake()
    {
        unit = GetComponent<Unit>();
        EnsureVisualRoot();
        RefreshVisuals();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        unit = GetComponent<Unit>();
        EnsureVisualRoot();
        RefreshVisuals();
    }
#endif

    public void RefreshVisuals()
    {
        if (unit == null || unit.Definition == null) return;
        if (unit.Definition.visualPrefab == null) return;

        // If prefab already matches, you could skip, but simplest is just rebuild
        if (currentVisualInstance != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(currentVisualInstance);
            else Destroy(currentVisualInstance);
#else
            Destroy(currentVisualInstance);
#endif
        }

        currentVisualInstance = Instantiate(unit.Definition.visualPrefab, visualRoot);
        currentVisualInstance.transform.localPosition = Vector3.zero;
        currentVisualInstance.transform.localRotation = Quaternion.identity;
        currentVisualInstance.transform.localScale = Vector3.one;

        ApplyTeamMaterial();
    }

    private void ApplyTeamMaterial()
    {
        if (unit == null || unit.Definition == null) return;

        Material teamMat = unit.Team == Team.Player
            ? unit.Definition.allyMaterial
            : unit.Definition.enemyMaterial;

        if (teamMat == null) return;

        if (!applyTeamMaterialToAllRenderers)
        {
            // Only apply to the first renderer found
            var r = currentVisualInstance != null
                ? currentVisualInstance.GetComponentInChildren<Renderer>(true)
                : null;

            if (r != null) r.sharedMaterial = teamMat;
            return;
        }

        if (currentVisualInstance == null) return;

        foreach (var r in currentVisualInstance.GetComponentsInChildren<Renderer>(true))
            r.sharedMaterial = teamMat;
    }

    private void EnsureVisualRoot()
    {
        if (visualRoot != null) return;

        var existing = transform.Find("Visual");
        if (existing != null)
        {
            visualRoot = existing;
            return;
        }

        var go = new GameObject("Visual");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        visualRoot = go.transform;
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