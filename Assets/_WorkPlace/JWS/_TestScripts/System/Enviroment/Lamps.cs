using UnityEngine;

public class Lamps : MonoBehaviour
{
    [SerializeField] private Renderer lampRenderer;
    private Material lampMaterial;
    private Color originalColor;
    private float emissionIntensity = 1.0f;
    private bool isEmitting = false;

    private void Awake()
    {
        if (lampRenderer == null)
        {
            Debug.LogError("Lamp Renderer가 할당되지 않았습니다!");
            return;
        }

        lampMaterial = lampRenderer.material;
        originalColor = lampMaterial.GetColor("_EmissionColor");
        UpdateEmission();
    }

    private void Update()
    {
        ToggleEmission();
    }

    private void ToggleEmission()
    {
        if (EnvironmentManager.Instance == null) return;
        if (EnvironmentManager.Instance.timeOfDay >= 6 && EnvironmentManager.Instance.timeOfDay <= 18)
        {
            if (isEmitting)
            {
                isEmitting = false;
                UpdateEmission();
            }
        }
        else
        {
            if (!isEmitting)
            {
                isEmitting = true;
                UpdateEmission();
            }
        }
    }

    private void ChangeEmissionIntensity(float delta)
    {
        emissionIntensity = Mathf.Clamp(emissionIntensity + delta, 0f, 10f);
        UpdateEmission();
    }

    private void UpdateEmission()
    {
        Color controllColor = originalColor;
        if (isEmitting)
        {
            lampRenderer.transform.GetChild(0).gameObject.SetActive(true);
            lampRenderer.transform.GetChild(1).gameObject.SetActive(true);
            lampMaterial.EnableKeyword("_EMISSION");
            lampMaterial.SetColor("_EmissionColor", controllColor * emissionIntensity);
        }
        else
        {
            lampMaterial.DisableKeyword("_EMISSION");
            lampRenderer.transform.GetChild(0).gameObject.SetActive(false);
            lampRenderer.transform.GetChild(1).gameObject.SetActive(false);
        }
        emissionIntensity = 1.2f;
    }
}
