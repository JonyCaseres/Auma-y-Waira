using UnityEngine;
using TMPro;

public class NPCDialogo : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelDialogo;
    [SerializeField] private TMP_Text textoDialogo;

    [TextArea(3,5)]
    [SerializeField] private string mensaje;

    [Header("Audio")]
    [SerializeField] private AudioClip sonidoDialogo;
    [SerializeField] private AudioSource audioSource;
    [SerializeField, Range(0f, 1f)] private float volumen = 1f;
    [SerializeField] private bool reproducirAlIniciar = true;
    [SerializeField] private bool reproducirAlCambiar = true;

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

        if (reproducirAlIniciar)
        {
            ReproducirSonido();
        }
    }

    private void OcultarDialogo()
    {
        panelDialogo.SetActive(false);
    }

    // Método público para cambiar el mensaje y reproducir sonido cuando cambie
    public void CambiarMensaje(string nuevoMensaje)
    {
        // Si el panel no está activo, lo activamos (opcional)
        if (!panelDialogo.activeSelf)
        {
            panelDialogo.SetActive(true);
        }

        // Solo actualizar si es distinto para evitar reproducir innecesariamente
        if (textoDialogo.text != nuevoMensaje)
        {
            textoDialogo.text = nuevoMensaje;

            if (reproducirAlCambiar)
            {
                ReproducirSonido();
            }
        }
    }

    private void ReproducirSonido()
    {
        if (sonidoDialogo == null) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(sonidoDialogo, volumen);
        }
        else if (Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(sonidoDialogo, Camera.main.transform.position, volumen);
        }
        else
        {
            AudioSource.PlayClipAtPoint(sonidoDialogo, transform.position, volumen);
        }
    }
}

