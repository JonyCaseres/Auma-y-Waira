using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class barradeestamina : MonoBehaviour
{
    [Header("UI (Slider)")]
    [SerializeField] private Slider staminaSlider;      // asigna el Slider del Canvas
    [SerializeField] private TMP_Text valueText;        // opcional: texto que muestra valores

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenRate = 15f;     // por segundo
    [SerializeField] private float regenDelay = 1.5f;   // segundos tras gastar antes de regenerar

    [Header("Visual")]
    [SerializeField] private Color fullColor = Color.cyan;
    [SerializeField] private Color emptyColor = Color.red;
    [SerializeField, Range(1f, 20f)] private float uiSmoothSpeed = 8f;

    private float currentStamina;
    private float displayedValue;    // valor suavizado (en unidades de stamina)
    private float lastUseTime = -999f;

    private Image fillImage;

    private void Awake()
    {
        currentStamina = maxStamina;
        displayedValue = maxStamina;

        if (staminaSlider != null)
        {
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = displayedValue;

            if (staminaSlider.fillRect != null)
            {
                fillImage = staminaSlider.fillRect.GetComponent<Image>();
            }
        }

        UpdateVisualsImmediate();
    }

    private void Update()
    {
        // Regeneración tras delay
        if (Time.time - lastUseTime >= regenDelay && currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + regenRate * Time.deltaTime);
        }

        // Suavizar la UI hacia el valor actual
        displayedValue = Mathf.MoveTowards(displayedValue, currentStamina, uiSmoothSpeed * Time.deltaTime);

        if (staminaSlider != null)
        {
            staminaSlider.value = displayedValue;
        }

        // Actualizar color del fill (si existe)
        if (fillImage != null)
        {
            float t = Mathf.Clamp01(displayedValue / maxStamina);
            fillImage.color = Color.Lerp(emptyColor, fullColor, t);
        }

        UpdateText();
    }

    private void UpdateText()
    {
        if (valueText != null)
        {
            valueText.text = $"{Mathf.CeilToInt(currentStamina)} / {Mathf.CeilToInt(maxStamina)}";
        }
    }

    private void UpdateVisualsImmediate()
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = displayedValue;
        }

        if (fillImage != null)
        {
            float t = Mathf.Clamp01(displayedValue / maxStamina);
            fillImage.color = Color.Lerp(emptyColor, fullColor, t);
        }

        UpdateText();
    }

    // Intentar gastar stamina; devuelve true si se pudo gastar
    public bool TryUse(float amount)
    {
        if (amount <= 0f) return true;
        if (currentStamina >= amount)
        {
            Use(amount);
            return true;
        }
        return false;
    }

    // Gastar stamina internamente
    private void Use(float amount)
    {
        currentStamina = Mathf.Max(0f, currentStamina - amount);
        lastUseTime = Time.time;
        // Actualizar visual inmediatamente para evitar lag en la UI
        displayedValue = currentStamina;
        UpdateVisualsImmediate();
    }

    // Rellenar completamente
    public void Refill()
    {
        currentStamina = maxStamina;
        lastUseTime = -999f;
        displayedValue = maxStamina;
        UpdateVisualsImmediate();
    }

    // getters útiles
    public float GetCurrent() => currentStamina;
    public float GetMax() => maxStamina;
}
