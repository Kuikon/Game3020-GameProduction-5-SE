using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class UIBox : MonoBehaviour
{
    [Header("Box Type")]
    public GhostType boxType;

    [Header("Bounce Area")]
    public BoxCollider2D bounceArea;

    [Header("Particle Effect")]
    public ParticleSystem glowParticle;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.MainModule main;
    private ParticleSystem.NoiseModule noise;
   
    [Header("Stats")]
    public int storedCount = 0;
    public int maxIntensityCount = 100;

    private void Awake()
    {
        if (bounceArea == null)
            bounceArea = GetComponent<BoxCollider2D>();

        if (glowParticle != null)
        {
            emission = glowParticle.emission;
            main = glowParticle.main;
            noise = glowParticle.noise;

            SetupBaseParticleSettings();   
            ApplyColorByType();            
        }
    }

    public void AddBall()
    {
        storedCount++;
        if (!glowParticle.isPlaying)
            glowParticle.Play();
        UpdateParticleIntensity();
    }
    private void SetupBaseParticleSettings()
    {
        var shape = glowParticle.shape;
        shape.enabled = true;

        // 
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.35f;
        main.startSpeed = 0;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, 1.5f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
        main.maxParticles = 1000;
        main.gravityModifier = -0.1f;
        main.loop = true;

        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.8f;
        noise.scrollSpeed = 0.3f;

        glowParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        emission.rateOverTime = 0f;
    }
    private void ApplyColorByType()
    {
        Color baseColor;

        switch (boxType)
        {
            case GhostType.Normal:
                baseColor = new Color(1f, 1f, 1f, 1f); // White
                break;
            case GhostType.Suicide:
                baseColor = new Color(1f, 0.4f, 0.4f, 1f); // Red
                break;
            case GhostType.Quick:
                baseColor = new Color(0.4f, 0.6f, 1f, 1f); // Blue
                break;
            case GhostType.Tank:
                baseColor = new Color(0.9f, 0.2f, 1f, 1f); // Purple
                break;
            case GhostType.Lucky:
                baseColor = new Color(1f, 0.9f, 0.4f, 1f); // Yellow
                break;
            default:
                baseColor = Color.white;
                break;
        }

        main.startColor = new ParticleSystem.MinMaxGradient(baseColor);
    }

    private void UpdateParticleIntensity()
    {
        if (glowParticle == null) return;

        float intensity = Mathf.Clamp01((float)storedCount / maxIntensityCount);

        var emissionRate = Mathf.Lerp(10f, 120f, intensity);
        var currentEmission = emission.rateOverTime.constant;
        emission.rateOverTime = Mathf.Lerp(currentEmission, emissionRate, Time.deltaTime * 3f);

        var currentColor = main.startColor.colorMax;
        var targetColor = currentColor;
        targetColor.a = Mathf.Lerp(0.5f, 1f, intensity);
        Color lerped = Color.Lerp(currentColor, targetColor, Time.deltaTime * 5f);
        main.startColor = new ParticleSystem.MinMaxGradient(lerped);
        float currentSize = main.startSize.constant;
        float targetSize = Mathf.Lerp(0.4f, 1.0f, intensity);
        main.startSize = Mathf.Lerp(currentSize, targetSize, Time.deltaTime * 3f);

        float currentSpeed = main.startSpeed.constant;
        float targetSpeed = Mathf.Lerp(0.5f, 1.5f, intensity);
        main.startSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 3f);
    }

    private void OnDrawGizmos()
    {
        if (bounceArea != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(bounceArea.bounds.center, bounceArea.bounds.size);
        }
    }
}
