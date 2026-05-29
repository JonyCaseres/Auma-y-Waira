using UnityEngine;
using TMPro;

public class NPCDialogo : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelDialogo;
    [SerializeField] private TMP_Text textoDialogo;

    [TextArea(3,5)]
    [SerializeField] private string mensaje;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cuando el jugador entra en el trigger
        if (other.CompareTag("Player"))
        {
            MostrarDialogo();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Cuando el jugador sale del trigger
        if (other.CompareTag("Player"))
        {
            OcultarDialogo();
        }
    }

    private void MostrarDialogo()
    {
        panelDialogo.SetActive(true);
        textoDialogo.text = mensaje;
    }

    private void OcultarDialogo()
    {
        panelDialogo.SetActive(false);
    }
}

