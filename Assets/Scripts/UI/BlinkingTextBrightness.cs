using UnityEngine;
using TMPro;

public class BlinkTextBrightness : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float speed = 3f;         
    public float intensity = 0.5f;   

    private Color baseColor;

    void Start()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();

        baseColor = text.color;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) * 0.5f + 0.5f); 
        float brightness = Mathf.Lerp(1f - intensity, 1f, t);

        Color newColor = baseColor * brightness;
        newColor.a = baseColor.a; 
        text.color = newColor;
    }
}
