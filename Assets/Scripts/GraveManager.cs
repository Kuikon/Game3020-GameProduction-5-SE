using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GraveManager : MonoBehaviour
{
    [SerializeField] private GameObject gravePrefab;
    [SerializeField] private GameObject brokenGravePrefab;
    [SerializeField] private Boundry xBoundary;
    [SerializeField] private Boundry yBoundary;
    [SerializeField] private int initialCount = 5;

    private List<GameObject> graveList = new();
    private int lastGraveCount;
   
    public void InitializeGraves()
    {
        lastGraveCount = initialCount;
        SpawnGraves(initialCount);
    }

    private void SpawnGraves(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = GetValidPosition(2f);
            GameObject g = Instantiate(gravePrefab, pos, Quaternion.identity);
            graveList.Add(g);
        }
    }

    public List<Vector3> GetPatrolPoints()
    {
        List<Vector3> points = new();
        foreach (var g in graveList)
        {
            if (g != null && !g.CompareTag("BrokenGrave"))
                points.Add(g.transform.position);
        }
        return points;
    }

    public void ReplaceCapturedGraves(List<GameObject> captured)
    {
        foreach (var g in captured)
        {
            if (g == null) continue;
            Vector3 pos = g.transform.position;
            graveList.Remove(g);
            Destroy(g);
            Instantiate(brokenGravePrefab, pos, Quaternion.identity).tag = "BrokenGrave";
        }
    }

    public IEnumerator RebuildGravesRoutine()
    {
        yield return new WaitForSeconds(2f);
        RemoveAllBroken();
        lastGraveCount++;
        graveList.Clear();
        SpawnGraves(lastGraveCount);
    }

    public bool AllGravesBroken()
    {
        foreach (var g in graveList)
        {
            if (g != null && !g.CompareTag("BrokenGrave"))
                return false;
        }
        return true;
    }

    private void RemoveAllBroken()
    {
        foreach (var g in GameObject.FindGameObjectsWithTag("BrokenGrave"))
            Destroy(g);
    }

    private Vector3 GetValidPosition(float minDist)
    {
        for (int i = 0; i < 100; i++)
        {
            Vector3 pos = new(
                Random.Range(xBoundary.min, xBoundary.max),
                Random.Range(yBoundary.min, yBoundary.max),
                0
            );

            bool valid = true;
            foreach (var existing in graveList)
            {
                if (existing != null && Vector3.Distance(pos, existing.transform.position) < minDist)
                {
                    valid = false;
                    break;
                }
            }

            if (valid) return pos;
        }
        return Vector3.zero;
    }
}
