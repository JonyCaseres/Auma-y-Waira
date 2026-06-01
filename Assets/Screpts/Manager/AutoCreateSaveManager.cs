using UnityEngine;

public static class AutoCreateSaveManager
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureSaveManager()
    {
        if (SaveManager.Instance == null)
        {
            GameObject go = new GameObject("SaveManager");
            go.AddComponent<SaveManager>();
            // Awake() de SaveManager hará DontDestroyOnLoad y cargará datos
            Debug.Log("AutoCreateSaveManager: SaveManager creado automáticamente.");
        }
    }
}