using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class InteractUITrigger : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI hintText; 
    public GameObject actionButton;  

    [Header("Settings")]
    public float blinkSpeed = 2f;

    private Coroutine blinkRoutine;

    private void Start()
    {
        if (hintText != null) hintText.gameObject.SetActive(false);
        if (actionButton != null) actionButton.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (hintText != null)
            {
                hintText.gameObject.SetActive(true);
                blinkRoutine = StartCoroutine(BlinkText());
            }

            if (actionButton != null)
                actionButton.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            if (hintText != null)
            {
                hintText.gameObject.SetActive(false);

                if (blinkRoutine != null)
                    StopCoroutine(blinkRoutine);
            }

            if (actionButton != null)
                actionButton.SetActive(false);
        }
    }

    private IEnumerator BlinkText()
    {
        Color baseColor = hintText.color;

        while (true)
        {
            float t = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f; // 0〜1
            Color c = baseColor;
            c.a = Mathf.Lerp(0.3f, 1f, t); 
            hintText.color = c;

            yield return null;
        }
    }
}
