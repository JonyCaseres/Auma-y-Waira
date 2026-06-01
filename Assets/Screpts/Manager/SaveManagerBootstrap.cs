using UnityEngine;

public class SaveManagerBootstrap : MonoBehaviour
{
    private void Awake()
    {
        if (SaveManager.Instance == null)
        {
            GameObject go = new GameObject("SaveManager");
            go.AddComponent<SaveManager>();
        }

        Destroy(this);
    }
}   