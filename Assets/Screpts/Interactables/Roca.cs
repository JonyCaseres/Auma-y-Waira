using UnityEngine;

public enum TipoRoca { Blanda, Dura }

public class Roca : MonoBehaviour, IInteractable
{
    [Header("Configuración de la roca")]
    public TipoRoca tipo = TipoRoca.Blanda;
    public ItemSO mineralDrop;
    public int cantidadDrop = 5;

    [Header("Herramientas requeridas")]
    [Tooltip("Herramienta necesaria para romper rocas blandas (ej. Pico)")]
    public ItemSO herramientaParaRocaBlanda;
    [Tooltip("Herramienta necesaria para romper rocas duras (ej. Pico resistente)")]
    public ItemSO herramientaParaRocaDura;

    private bool rota = false;

    public bool CanInteract() => !rota;

    public void Interact(GameObject jugador)
    {
        if (rota) return;

        InventarioJugador inv = jugador.GetComponent<InventarioJugador>();

        // Determinar herramienta requerida
        ItemSO requerida = null;
        if (tipo == TipoRoca.Blanda) requerida = herramientaParaRocaBlanda;
        else requerida = herramientaParaRocaDura;

        if (requerida != null)
        {
            if (inv == null || !inv.TieneItem(requerida))
            {
                Debug.Log($"Necesitas '{requerida.nombre}' para interactuar con esta roca.");
                return;
            }
        }

        // Entregar minerales
        if (inv != null)
        {
            inv.AgregarItem(mineralDrop, cantidadDrop);
            rota = true;
            gameObject.SetActive(false); // Desaparece la roca
            Debug.Log("La roca se rompió y soltó minerales.");
        }
        else
        {
            Debug.LogWarning("Jugador sin InventarioJugador; no se pudo agregar el mineral.");
        }
    }
}

