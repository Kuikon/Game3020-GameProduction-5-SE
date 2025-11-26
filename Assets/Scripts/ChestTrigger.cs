using UnityEngine;

public class ChestTrigger : MonoBehaviour
{
    [SerializeField] private GameObject panel;   // Inspector でセット

    private void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);  // 最初は非表示
        }
    }

    private void OnTriggerEnter2D(UnityEngine.Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player detected");
            if (panel != null)
                panel.SetActive(true);
        }
    }

    private void OnTriggerExit2D(UnityEngine.Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // プレイヤーがゴリラ圏内から出たとき → パネル非表示
            if (panel != null)
                panel.SetActive(false);
        }
    }
}
