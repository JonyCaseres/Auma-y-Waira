using UnityEngine;
using System.Collections;

public enum TipoMina { Pequeña, Mediana, Grande }

public class Mina : MonoBehaviour, IInteractable
{
    [Header("Configuración de la mina")]
    public ItemSO mineral;
    public TipoMina tipoMina;

    [SerializeField] private int cantidadPorInteraccion;
    [SerializeField] private int cantidadTotal;

    [Header("Minado")]
    [Tooltip("Segundos que tarda el minado antes de entregar recursos")]
    public float tiempoMinado = 1f;
    private bool isMinando = false;

    private void Start()
    {
        // Ajustar valores según el tipo de mina
        switch (tipoMina)
        {
            case TipoMina.Pequeña:
                cantidadTotal = 10;
                break;
            case TipoMina.Mediana:
                cantidadTotal = 20;
                break;
            case TipoMina.Grande:
                cantidadTotal = 40;
                break;
        }
    }

    public bool CanInteract() => cantidadTotal > 0 && !isMinando;

    public void Interact(GameObject jugador)
    {
        if (!CanInteract()) return;

        StartCoroutine(ProcesarInteraccion(jugador));
    }

    private IEnumerator ProcesarInteraccion(GameObject jugador)
    {
        isMinando = true;

        // Esperar el tiempo de minado (sin animación)
        yield return new WaitForSeconds(tiempoMinado);

        cantidadPorInteraccion = ObtenerCantidadAleatoria();
        cantidadPorInteraccion = Mathf.Min(cantidadPorInteraccion, cantidadTotal);

        if (cantidadPorInteraccion > 0)
        {
            InventarioJugador inv = jugador.GetComponent<InventarioJugador>();
            if (inv != null)
            {
                inv.AgregarItem(mineral, cantidadPorInteraccion);
                Debug.Log($"Recolectado {cantidadPorInteraccion} de {mineral.nombre}. Restante: {cantidadTotal - cantidadPorInteraccion}");
            }
            else
            {
                Debug.LogWarning("El jugador no tiene componente InventarioJugador. No se agregó el mineral.");
            }
        }
        else
        {
            Debug.Log($"La mina de tipo {tipoMina} no devolvió minerales esta vez.");
        }

        // Finalizar y desactivar la mina
        cantidadTotal -= cantidadPorInteraccion;
        cantidadTotal = 0;
        gameObject.SetActive(false);

        isMinando = false;
    }

    private int ObtenerCantidadAleatoria()
    {
        switch (tipoMina)
        {
            case TipoMina.Pequeña:
                return Random.Range(0, 4); // 0 a 3
            case TipoMina.Mediana:
                return Random.Range(2, 6); // 2 a 5
            case TipoMina.Grande:
                return Random.Range(3, 8); // 3 a 7
            default:
                return 0;
        }
    }
}
