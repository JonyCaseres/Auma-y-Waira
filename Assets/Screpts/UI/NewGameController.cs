using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameController : MonoBehaviour
{
    [Header("Opciones de nuevo juego")]
    [Tooltip("Nombre de la escena del juego que se debe cargar al pulsar Nuevo Juego.")]
    public string sceneName = "GameScene";
    [Tooltip("Si true, usa sceneName; si false usará startSceneIndex.")]
    public bool useSceneName = true;
    [Tooltip("Índice de escena en Build Settings si no usas sceneName.")]
    public int startSceneIndex = 1;

    [Header("Reset de save")]
    [Tooltip("Si true, reseteará las flags talkedToGrandma y missionAccepted al iniciar nuevo juego.")]
    public bool resetFlags = true;

    // Método público para asignar al Button -> OnClick
    public void StartNewGame()
    {
        // Reset de flags si corresponde
        if (resetFlags && SaveManager.Instance != null)
        {
            SaveManager.Instance.SetTalkedToGrandma(false);
            SaveManager.Instance.SetMissionAccepted(false);
#if UNITY_EDITOR
            Debug.Log("NewGameController: Save flags reseteadas.");
#endif
        }

        // Suscribirse al evento de carga para arrancar el cómic en la escena destino
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Cargar escena
        if (useSceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("NewGameController: sceneName vacío.");
                return;
            }
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(startSceneIndex);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Desuscribirse (solo queremos una vez)
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Buscar controlador de cómic y arrancarlo
        var camCtrl = FindObjectOfType<CamaraControllerNuevo>();
        if (camCtrl != null)
        {
            camCtrl.StartComic();
#if UNITY_EDITOR
            Debug.Log("NewGameController: Iniciando comic en la escena cargada.");
#endif
            return;
        }

        // Alternativa: si usas el otro controlador llamado CamaraController
        var camCtrlOld = FindObjectOfType<CamaraController>();
        if (camCtrlOld != null)
        {
            camCtrlOld.StartComic();
#if UNITY_EDITOR
            Debug.Log("NewGameController: Iniciando comic (CamaraController) en la escena cargada.");
#endif
            return;
        }

        // Si no hay controlador, puedes opcionalmente activar playOnStart en el componente manualmente
#if UNITY_EDITOR
        Debug.LogWarning("NewGameController: No se encontró CamaraControllerNuevo ni CamaraController en la escena.");
#endif
    }
}