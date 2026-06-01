using UnityEngine;

public class TabMenuController : MonoBehaviour
{
    [Header("Paneles de contenido")]
    public GameObject[] panels; // cada panel corresponde a una pestaña

    private int currentIndex = 0;

    public void ShowPanel(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == index);
        }
        currentIndex = index;
    }

    // Avanzar pestaña con Input System (ej. acción Next)
    public void NextTab()
    {
        int nextIndex = (currentIndex + 1) % panels.Length;
        ShowPanel(nextIndex);
    }

    // Retroceder pestaña
    public void PreviousTab()
    {
        int prevIndex = (currentIndex - 1 + panels.Length) % panels.Length;
        ShowPanel(prevIndex);
    }
}

