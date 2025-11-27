using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Chest : MonoBehaviour
{
    [Header("Chest Settings")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private float heartPopForce = 3f;
    [SerializeField] private float heartDelay = 1.5f; 
    [SerializeField] private float destroyDelay = 2.5f;

    private Animator animator;
    private Rigidbody2D rb;
    private bool isOpened = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    public void OpenChest()
    {
        if (isOpened) return;
        isOpened = true;
        animator.SetTrigger("Open");
        StartCoroutine(DelayedHeartSpawn());
    }

    private IEnumerator DelayedHeartSpawn()
    {
        yield return new WaitForSeconds(heartDelay);
        SpawnHeart();
        StartCoroutine(DestroyAfterDelay());
    }

    private void SpawnHeart()
    {
        if (heartPrefab == null) return;

        GameObject heart = Instantiate(heartPrefab, transform.position, Quaternion.identity);
        Rigidbody2D heartRb = heart.GetComponent<Rigidbody2D>();

        if (heartRb != null)
        {
            heartRb.AddForce(Vector2.up * heartPopForce, ForceMode2D.Impulse);
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);
    }
}
