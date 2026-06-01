using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTrigger : MonoBehaviour
{
    [Header("Condiciones de activación")]
    [Tooltip("Si true, requiere que SaveManager.talkedToGrandma sea true para permitir avanzar.")]
    [SerializeField] private bool requireTalkedToGrandma = true;
    [Tooltip("Si true, invierte la condición anterior: requiere que talkedToGrandma sea false.")]
    [SerializeField] private bool invertTalkRequirement = false;

    [Tooltip("Si true, requiere que SaveManager.missionAccepted sea true para permitir avanzar.")]
    [SerializeField] private bool requireMissionAccepted = false;
    [Tooltip("Invertir la condición de misión (requiere false si se activa).")]
    [SerializeField] private bool invertMissionRequirement = false;

    [Header("Detección")]
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private bool singleUse = true;

    [Header("Carga (comportamiento)")]
    [SerializeField, Min(0f)] private float requiredStaySeconds = 1.5f;
    [SerializeField, Min(0f)] private float delayAfterHold = 0.3f;
    [SerializeField] private bool useSceneManagerSingleton = true;

    [Header("UI (opcional)")]
    [SerializeField] private Image progressFill;

    private bool used = false;
    private Coroutine holdCoroutine;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used && singleUse) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        // Intentar recargar los datos de disco justo antes de comprobar
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Load();
        }
        else
        {
used = false;            Debug.LogWarning("SceneTrigger: SaveManager no presente en escena. Asegúrate de añadirlo manualmente a la escena inicial.");
        }

        // Depuración: mostrar estado actual
        bool talked = SaveManager.Instance != null && SaveManager.Instance.Data != null && SaveManager.Instance.Data.talkedToGrandma;
        bool mission = SaveManager.Instance != null && SaveManager.Instance.Data != null && SaveManager.Instance.Data.missionAccepted;
        Debug.Log($"SceneTrigger: OnTriggerEnter2D detected by '{other.name}'. Save flags -> talkedToGrandma: {talked}, missionAccepted: {mission}");

        // Evaluar condiciones con posibilidad de inversión
        if (!CheckConditions(talked, mission))
        {
            Debug.Log("SceneTrigger: condiciones no cumplidas. No se iniciará la carga.");
            return;
        }

        // Iniciar coroutine de permanencia (hold)
        if (holdCoroutine != null) StopCoroutine(holdCoroutine);
        holdCoroutine = StartCoroutine(HoldAndLoadCoroutine(other.gameObject));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
            if (progressFill != null) progressFill.fillAmount = 0f;
            Debug.Log("SceneTrigger: permanencia cancelada (salió del trigger).");
        }
    }

    private bool CheckConditions(bool talked, bool mission)
    {
        if (requireTalkedToGrandma)
        {
            bool expected = !invertTalkRequirement;
            if (talked != expected) return false;
        }

        if (requireMissionAccepted)
        {
            bool expected = !invertMissionRequirement;
            if (mission != expected) return false;
        }

        return true;
    }

    private IEnumerator HoldAndLoadCoroutine(GameObject activator)
    {
        float elapsed = 0f;
        if (progressFill != null) progressFill.fillAmount = 0f;

        while (elapsed < requiredStaySeconds)
        {
            if (activator == null) yield break;

            elapsed += Time.deltaTime;
            if (progressFill != null) progressFill.fillAmount = Mathf.Clamp01(elapsed / requiredStaySeconds);
            yield return null;
        }

        if (progressFill != null) progressFill.fillAmount = 1f;
        if (delayAfterHold > 0f) yield return new WaitForSeconds(delayAfterHold);

        if (singleUse) used = true;

        // Ejecutar carga mediante Esenamanager o fallback
        if (useSceneManagerSingleton && Esenamanager.Instance != null)
        {
            Esenamanager.Instance.CheckAndLoad();
        }
        else
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                Debug.LogWarning($"SceneTrigger: No hay escena siguiente (index {nextIndex}).");
            }
        }

        holdCoroutine = null;
    }

    public void ResetTrigger()
    {
        used = false;
        if (progressFill != null) progressFill.fillAmount = 0f;
    }
}