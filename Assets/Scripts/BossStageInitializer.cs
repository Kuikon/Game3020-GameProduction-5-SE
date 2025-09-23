using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossStageInitializer : MonoBehaviour
{
    [Header("Fireball Settings")]
    public GameObject fireballPrefab;     
    public Sprite fireballSprite;                     
    public int fireballCount = 12;
    public float fireballInterval = 0.2f;  
    public float circleRadius = 5f;        

    [Header("Meteor Settings")]
    public int meteorCount = 6;
    public float spawnRadius = 8f;         
    public float spawnHeight = 10f;        
    public float meteorFallSpeed = 5f;     
    public float circleYOffset = -3f;

    [Header("Timing Settings")]
    public float spreadDelay = 1f;         
    public float fireballLifetime = 1.5f;  
    public float meteorDelay = 1.5f;      

    private List<GameObject> spawnedFireballs = new();
    private int fireballShotIndex = 0; 
    void Start()
    {
        StartCoroutine(StageIntroSequence());
    }

    IEnumerator StageIntroSequence()
    {
        for (int i = 0; i < fireballCount; i++)
        {
            GameObject fb = CreateFireball(Fireball.FireballType.Normal, fireballSprite, transform.position);
            spawnedFireballs.Add(fb);
            yield return new WaitForSeconds(fireballInterval);
        }

        yield return new WaitForSeconds(meteorDelay);
        DropMeteors();
    }

    GameObject CreateFireball(Fireball.FireballType type, Sprite sprite, Vector2 position)
    {
        GameObject obj = Instantiate(fireballPrefab, position, Quaternion.identity);
        Fireball fb = obj.GetComponent<Fireball>();
        fb.type = type;
        fb.SetSprite(sprite);

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2[] directions = new Vector2[]
            {
            new Vector2(-0.5f, 1f).normalized, 
            Vector2.up,                        
            new Vector2(0.5f, 1f).normalized   
            };

            Vector2 dir = directions[fireballShotIndex];
            fireballShotIndex = (fireballShotIndex + 1) % directions.Length;

            float speed = 5f; 
            rb.linearVelocity = dir * speed;
            rb.gravityScale = 0; 
        }

        Destroy(obj, 3f);

        return obj;
    }

    void ClearFireballs()
    {
        foreach (var fb in spawnedFireballs)
        {
            if (fb != null) Destroy(fb);
        }
        spawnedFireballs.Clear();
        Debug.Log("🔥 火の玉が消えた！");
    }

    void DropMeteors()
    {
        StartCoroutine(DropMeteorsRandomRoutine());
    }

    IEnumerator DropMeteorsRandomRoutine()
    {
        List<Vector2> targetPositions = new List<Vector2>();
        Vector2 center = (Vector2)transform.position + Vector2.up * circleYOffset;

        for (int i = 0; i < meteorCount; i++)
        {
            float angle = i * Mathf.PI * 2 / meteorCount;
            Vector2 targetPos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
            targetPositions.Add(targetPos);
        }

        while (targetPositions.Count > 0)
        {
            int index = Random.Range(0, targetPositions.Count);
            Vector2 targetPos = targetPositions[index];
            targetPositions.RemoveAt(index);

            Vector2 spawnPos = targetPos + Vector2.up * spawnHeight;
            GameObject obj = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
            StartCoroutine(MeteorFall(obj, targetPos));

            yield return new WaitForSeconds(Random.Range(0.2f, 1f));
        }
    }

    IEnumerator MeteorFall(GameObject meteor, Vector2 targetPos)
    {
        while (meteor != null && (Vector2)meteor.transform.position != targetPos)
        {
            meteor.transform.position = Vector2.MoveTowards(
                meteor.transform.position,
                targetPos,
                meteorFallSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (meteor != null)
        {
            Rigidbody2D rb = meteor.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Static;
        }
    }
}
