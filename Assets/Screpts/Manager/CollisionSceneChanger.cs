using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionSceneChanger : MonoBehaviour
{
    public enum Mode { Trigger2D, Collision2D }

    [Header("Detector")]
    [SerializeField] private Mode mode = Mode.Trigger2D;
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private bool singleUse = true;

    [Header("Carga de escena")]
    [SerializeField] private bool loadNextScene = true;
    [SerializeField] private string sceneName = "";
    [SerializeField, Min(0f)] private float delaySeconds = 0.5f;
    [SerializeField] private bool useEsenamanagerIfAvailable = true;

    private bool triggered = false;

    private void Reset()
    {
        // Facilita la configuración: añadir BoxCollider2D si no existe
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = (mode == Mode.Trigger2D);
        }
        else
        {
            col.isTrigger = (mode == Mode.Trigger2D);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (mode != Mode.Trigger2D) return;
        TryHandleEnter(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (mode != Mode.Collision2D) return;
        TryHandleEnter(collision.gameObject);
    }

    private void TryHandleEnter(GameObject other)
    {
        if (triggered && singleUse) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        if (delaySeconds > 0f)
            StartCoroutine(DelayedLoad());
        else
            PerformLoad();

        if (singleUse) triggered = true;
    }

    private IEnumerator DelayedLoad()
    {
        yield return new WaitForSeconds(delaySeconds);
        PerformLoad();
    }

    private void PerformLoad()
    {
        // Si existe Esenamanager y está habilitado, pedirle que compruebe/ejecute la carga.
        if (useEsenamanagerIfAvailable && Esenamanager.Instance != null)
        {
            Esenamanager.Instance.CheckAndLoad();
            return;
        }

        // Fallback: cargar por nombre o cargar siguiente en Build Settings
        if (!loadNextScene)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("CollisionSceneChanger: sceneName vacío y loadNextScene desactivado.");
            }
        }
        else
        {
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            if (next < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(next);
            }
            else
            {
                Debug.LogWarning("CollisionSceneChanger: No hay escena siguiente en Build Settings.");
            }
        }
    }

    // Permite reactivar manualmente el trigger desde otros scripts
    public void ResetTrigger() => triggered = false;
}