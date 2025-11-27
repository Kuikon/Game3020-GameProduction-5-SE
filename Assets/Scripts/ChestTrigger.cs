using UnityEngine;

public class ChestTrigger : MonoBehaviour
{
    [SerializeField] private GameObject panel; 

    private void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false); 
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
            if (panel != null)
                panel.SetActive(false);
        }
    }
}
