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

    // 🔹 囲まれた時に呼び出される
    public void OpenChest()
    {
        if (isOpened) return;
        isOpened = true;

        // 🎞 アニメーション再生
        animator.SetTrigger("Open");

        // ⏳ 数秒待ってからハートを出す
        StartCoroutine(DelayedHeartSpawn());
    }

    // 💫 一定時間待ってからハートを生成
    private IEnumerator DelayedHeartSpawn()
    {
        yield return new WaitForSeconds(heartDelay);
        SpawnHeart();

        // 🕓 ハートを出したあと宝箱を削除
        StartCoroutine(DestroyAfterDelay());
    }

    // ❤️ ハート生成＆ポンッと飛ばす
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

    // 💀 削除処理（ふわっと消えるようにしてもOK）
    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);

        // 🔸 削除前にアニメーションやエフェクトを入れたい場合はここに追加
        Destroy(gameObject);
    }
}
