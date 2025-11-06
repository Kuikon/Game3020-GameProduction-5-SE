using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class UIBox : MonoBehaviour
{
    [Header("Box Type")]
    public GhostType boxType;

    [Header("Stats")]
    public int storedCount = 0;
    public int maxIntensityCount = 100;

    // 🔹 アニメーション用
    private Vector3 baseScale;
    private float flashTimer = 0f;
    private bool isHighlighted = false;
    private SpriteRenderer sr;
    private void Awake()
    {
        baseScale = transform.localScale; // 🔹 初期スケールを保存

        sr = GetComponent<SpriteRenderer>();
        ApplyColorByType();
    }


    private void Update()
    {
        // 🔸 点滅効果（有効中のみ）
        if (isHighlighted)
        {
            flashTimer += Time.deltaTime * 8f;
            float alpha = (Mathf.Sin(flashTimer) * 0.5f + 0.5f); // 0～1の点滅波

           

            // スケール拡大縮小（ポンポンする感じ）
            float scale = Mathf.Lerp(1f, 1.15f, alpha);
            transform.localScale = baseScale * scale;
        }
        else
        {
            // 通常時にスムーズに戻す
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * 5f);
        }
    }

    private void OnMouseOver()
    {
        // 🔹 右クリック押下中のみ反応
        if (Input.GetMouseButton(1))
        {
            if (!isHighlighted)
            {
                isHighlighted = true;
                flashTimer = 0f;
            }
        }
        else
        {
            isHighlighted = false;
        }
    }

    private void OnMouseExit()
    {
        isHighlighted = false;
    }

    // ========================================================
    // 既存のパーティクル・色設定関数
    // ========================================================

    public void AddBall()
    {
        storedCount++;
        
    }

   

    private void ApplyColorByType()
    {
        Color baseColor;

        switch (boxType)
        {
            case GhostType.Normal: baseColor = Color.white; break;
            case GhostType.Suicide: baseColor = new Color(1f, 0.4f, 0.4f); break;
            case GhostType.Quick: baseColor = new Color(0.4f, 0.6f, 1f); break;
            case GhostType.Tank: baseColor = new Color(0.9f, 0.2f, 1f); break;
            case GhostType.Lucky: baseColor = new Color(1f, 0.9f, 0.4f); break;
            default: baseColor = Color.white; break;
        }
        if (sr != null)
            sr.color = baseColor;

    }



 
}
