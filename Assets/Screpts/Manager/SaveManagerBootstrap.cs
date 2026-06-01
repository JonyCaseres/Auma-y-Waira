using UnityEngine;

public class SaveManagerBootstrap : MonoBehaviour
{
    // Si prefieres auto-create sin usar RuntimeInitializeOnLoadMethod, añade este componente
    // a un GameObject de la escena inicial (por ejemplo "GameManager").
    private void Awake()
    {
        if (SaveManager.Instance == null)
        {
            GameObject go = new GameObject("SaveManager");
            go.AddComponent<SaveManager>();
            // SaveManager.Awake hará DontDestroyOnLoad
        }

        // Este bootstrap no necesita permanecer
        Destroy(this);
    }
}