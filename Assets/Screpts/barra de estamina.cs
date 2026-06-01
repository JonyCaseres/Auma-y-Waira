using System;
using System.Collections;
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
    [SerializeField, Tooltip("Duración en segundos del parpadeo cuando comienza la recarga desde 0")]
    private float blinkDuration = 1.2f;
    [SerializeField, Tooltip("Frecuencia de parpadeo en Hz")]
    private float blinkFrequency = 8f;

    // Eventos
    public Action OnStaminaDepleted;
    public Action OnStaminaRecoveredFromZero;

    private float currentStamina;
    private float displayedValue;    // valor suavizado (en unidades de stamina)
    private float lastUseTime = -999f;

    private Image fillImage;

    // Estado para detectar transición 0 -> recarga
    private bool wasEmpty = false;
    private bool isBlinking = false;
    private Coroutine blinkCoroutine;

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
            // Si estaba vacío y la recarga va a iniciar, lanzar parpadeo
            if (wasEmpty && !isBlinking)
            {
                StartBlinking();
            }

            currentStamina = Mathf.Min(maxStamina, currentStamina + regenRate * Time.deltaTime);

            // Cuando la stamina supera 0 después de haber estado vacía
            if (wasEmpty && currentStamina > 0f)
            {
                wasEmpty = false;
                OnStaminaRecoveredFromZero?.Invoke();
                // Detener parpadeo si sigue activo
                StopBlinking();
            }
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

    // Intentar gastar stamina; devuelve true si se pudo gastar (mismo comportamiento previo)
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

    // Consume una cantidad (por ejemplo por segundo). Devuelve la stamina restante.
    public float Consume(float amount)
    {
        if (amount <= 0f) return currentStamina;

        float prev = currentStamina;
        float consumed = Mathf.Min(amount, currentStamina);
        currentStamina = Mathf.Max(0f, currentStamina - consumed);
        lastUseTime = Time.time;

        // Actualizar visual inmediatamente para evitar lag perceptible
        displayedValue = currentStamina;
        UpdateVisualsImmediate();

        // Si se acabó la stamina y antes no estaba vacía, notificar
        if (currentStamina <= 0f && !wasEmpty)
        {
            wasEmpty = true;
            OnStaminaDepleted?.Invoke();
        }

        return currentStamina;
    }

    // Gastar stamina internamente (método privado utilizado por TryUse)
    private void Use(float amount)
    {
        currentStamina = Mathf.Max(0f, currentStamina - amount);
        lastUseTime = Time.time;
        // Actualizar visual inmediatamente para evitar lag en la UI
        displayedValue = currentStamina;
        UpdateVisualsImmediate();

        if (currentStamina <= 0f && !wasEmpty)
        {
            wasEmpty = true;
            OnStaminaDepleted?.Invoke();
        }
    }

    // Rellenar completamente
    public void Refill()
    {
        currentStamina = maxStamina;
        lastUseTime = -999f;
        displayedValue = maxStamina;
        UpdateVisualsImmediate();
        wasEmpty = false;
        StopBlinking();
    }

    // getters útiles
    public float GetCurrent() => currentStamina;
    public float GetMax() => maxStamina;

    private void StartBlinking()
    {
        if (isBlinking || fillImage == null) return;
        blinkCoroutine = StartCoroutine(BlinkCoroutine(blinkDuration));
    }

    private void StopBlinking()
    {
        if (!isBlinking) return;
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        isBlinking = false;
        // Restaurar color inmediatamente
        if (fillImage != null)
        {
            float t = Mathf.Clamp01(displayedValue / maxStamina);
            fillImage.color = Color.Lerp(emptyColor, fullColor, t);
            fillImage.enabled = true;
        }
    }

    private IEnumerator BlinkCoroutine(float duration)
    {
        if (fillImage == null) yield break;
        isBlinking = true;
        float elapsed = 0f;
        Color original = fillImage.color;
        Color blinkCol = Color.white;

        while (elapsed < duration)
        {
            // Alternar color
            float phase = Mathf.Sin(elapsed * Mathf.PI * 2f * blinkFrequency) * 0.5f + 0.5f;
            fillImage.color = Color.Lerp(original, blinkCol, phase);
            elapsed += Time.deltaTime;

            // Si la stamina ya subió por encima de 0, terminamos el parpadeo
            if (currentStamina > 0f) break;

            yield return null;
        }

        // Restaurar color
        float t = Mathf.Clamp01(displayedValue / maxStamina);
        fillImage.color = Color.Lerp(emptyColor, fullColor, t);
        isBlinking = false;
    }
}
