using UnityEngine;
using UnityEngine.SceneManagement;

public class Esenamanager : MonoBehaviour
{
    public static Esenamanager Instance { get; private set; }

    [Header("Cambio de escena")]
    [SerializeField] private bool loadNextScene = true;
    [SerializeField] private string sceneName = "";
    [SerializeField, Range(0f, 10f)] private float delayBeforeLoad = 0.5f;
    [SerializeField] private bool requireMissionAccepted = false;

    private bool hasTriggered = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Comprobar al iniciar por si ya está guardado
        CheckAndLoad();
    }

    private void Update()
    {
        // Polling ligero hasta activarse; puedes desactivar si llamas desde NPCEspecial/SceneTrigger
        if (!hasTriggered)
            CheckAndLoad();
    }

    // Método público para verificar el guardado y cargar la escena si corresponde.
    public void CheckAndLoad()
    {
        if (hasTriggered) return;
        if (SaveManager.Instance == null) return;

        bool talked = SaveManager.Instance.Data != null && SaveManager.Instance.Data.talkedToGrandma;
        bool mission = SaveManager.Instance.Data != null && SaveManager.Instance.Data.missionAccepted;

        if (talked && (!requireMissionAccepted || mission))
        {
            hasTriggered = true;
            if (delayBeforeLoad > 0f)
                Invoke(nameof(PerformSceneLoad), delayBeforeLoad);
            else
                PerformSceneLoad();
        }
    }

    // Forzar la carga (ignora flags de SaveManager)
    public void ForceLoadImmediate()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        PerformSceneLoad();
    }

    private void PerformSceneLoad()
    {
        if (loadNextScene)
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                Debug.LogWarning($"Esenamanager: No hay escena siguiente en Build Settings (index {nextIndex}).");
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("Esenamanager: sceneName vacío y loadNextScene desactivado. No se cargó ninguna escena.");
            }
        }
    }
}
