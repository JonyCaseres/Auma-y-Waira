using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTrigger : MonoBehaviour
{
    [Header("Condiciones de activación")]
    [Tooltip("Requerir que el jugador haya hablado con la abuela (SaveManager)")]
    [SerializeField] private bool requireTalkedToGrandma = true;
    [Tooltip("Requerir además que haya aceptado la misión")]
    [SerializeField] private bool requireMissionAccepted = false;

    [Header("Detección")]
    [Tooltip("Tag del objeto que activa el trigger (normalmente 'Player')")]
    [SerializeField] private string requiredTag = "Player";
    [Tooltip("Sólo disparar una vez")]
    [SerializeField] private bool singleUse = true;

    [Header("Carga (comportamiento)")]
    [Tooltip("Tiempo en segundos que el jugador debe permanecer dentro del trigger para activar la carga")]
    [SerializeField, Min(0f)] private float requiredStaySeconds = 1.5f;
    [Tooltip("Retraso adicional después de completar el tiempo de permanencia antes de cargar (segundos)")]
    [SerializeField, Min(0f)] private float delayAfterHold = 0.3f;
    [Tooltip("Si true, intenta usar Esenamanager; si no existe, carga la siguiente escena del build index")]
    [SerializeField] private bool useSceneManagerSingleton = true;

    [Header("UI (opcional)")]
    [Tooltip("Image que representará el progreso de permanencia (fillAmount 0..1). Dejar vacío si no se usa.")]
    [SerializeField] private Image progressFill;

    private bool used = false;
    private Coroutine holdCoroutine;

    private void Reset()
    {
        // Asegurar que tiene Collider2D y es trigger para facilitar configuración en Inspector
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

        // Verificar SaveManager condiciones
        if (SaveManager.Instance != null)
        {
            if (requireTalkedToGrandma && !SaveManager.Instance.GetTalkedToGrandma()) return;
            if (requireMissionAccepted && !SaveManager.Instance.GetMissionAccepted()) return;
        }
        else
        {
            // Si SaveManager no existe, sólo permitimos avanzar si no se requieren condiciones
            if (requireTalkedToGrandma || requireMissionAccepted) return;
        }

        // Iniciar coroutine de permanencia
        if (holdCoroutine != null) StopCoroutine(holdCoroutine);
        holdCoroutine = StartCoroutine(HoldAndLoadCoroutine(other.gameObject));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        // Cancelar si sale antes de completar
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
            if (progressFill != null) progressFill.fillAmount = 0f;
            Debug.Log("SceneTrigger: permanencia cancelada (salió del trigger).");
        }
    }

    private IEnumerator HoldAndLoadCoroutine(GameObject activator)
    {
        float elapsed = 0f;
        if (progressFill != null) progressFill.fillAmount = 0f;

        while (elapsed < requiredStaySeconds)
        {
            // Si el activator desaparece o no está en el trigger, abortar (seguridad)
            if (activator == null) yield break;

            elapsed += Time.deltaTime;
            if (progressFill != null) progressFill.fillAmount = Mathf.Clamp01(elapsed / requiredStaySeconds);
            yield return null;
        }

        // Completó permanencia
        if (progressFill != null) progressFill.fillAmount = 1f;
        if (delayAfterHold > 0f) yield return new WaitForSeconds(delayAfterHold);

        // Marcar usado si corresponde
        if (singleUse) used = true;

        // Ejecutar carga mediante Esenamanager o fallback
        if (useSceneManagerSingleton && Esenamanager.Instance != null)
        {
            Esenamanager.Instance.CheckAndLoad();
        }
        else
        {
            // Fallback: cargar siguiente escena en Build Settings
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

    // Método público para reactivar el trigger si lo necesitas
    public void ResetTrigger()
    {
        used = false;
        if (progressFill != null) progressFill.fillAmount = 0f;
    }
}