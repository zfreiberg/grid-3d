using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitView : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;

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
