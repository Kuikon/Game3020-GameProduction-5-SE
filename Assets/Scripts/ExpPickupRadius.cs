using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ExpPickupRadius : MonoBehaviour
{
    private CircleCollider2D col;
    private Transform player;
    [SerializeField] private Transform spriteObj;
    private SpriteRenderer sr;
    private Coroutine flashRoutine;
    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        player = transform.parent;
        sr = spriteObj.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        CircleCollider2D myCol = GetComponent<CircleCollider2D>();
        Collider2D playerCol = transform.parent.GetComponent<Collider2D>();
        if (flashRoutine == null)
            flashRoutine = StartCoroutine(Flash());
        if (playerCol != null)
            Physics2D.IgnoreCollision(myCol, playerCol);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Dog"))
        {
            Debug.Log("Ghost を無視しました");
            return;
        }

       
    }


    public void AddRadius(float extra)
    {
        spriteObj.localScale += new Vector3(extra, extra, 0f);
      
    }
    private IEnumerator Flash()
    {
        float speed = 2f; // 点滅速度
        Color baseColor = sr.color;   // 元の色(RGB)はそのまま保持

        while (true)
        {
            // 0〜1 を時間で往復
            float t = Mathf.Abs(Mathf.Sin(Time.time * speed));

            // アルファを 0〜1 で変化させる
            float alpha = Mathf.Lerp(0f, 1f, t);

            // RGB は固定、Alpha だけ変更
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

            yield return null;
        }
    }

}
