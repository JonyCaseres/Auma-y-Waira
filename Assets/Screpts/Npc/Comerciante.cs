using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class Comerciante : MonoBehaviour, IInteractable
{
    [Header("Inventario del comerciante")]
    public List<ItemSO> itemsEnVenta;   // Ítems que ofrece
    public int precioBase = 10;         // Precio genérico si no usas valores propios

    [Header("UI Referencias")]
    public GameObject panelComercio;
    public Transform contenedorSlots;
    public GameObject slotPrefab;
    public TMP_Text textoMonedasJugador;

    private InventarioJugador inventarioJugador;
    private bool comercioActivo = false;

    private void Start()
    {
        inventarioJugador = FindObjectOfType<InventarioJugador>();

        if (inventarioJugador == null)
            Debug.LogError("[Comerciante] No se encontró ningún InventarioJugador en la escena.");

        if (panelComercio == null)
        {
            Debug.LogError("[Comerciante] panelComercio no está asignado en el inspector.");
        }
        else
        {
            panelComercio.SetActive(false);
        }

        if (contenedorSlots == null)
            Debug.LogError("[Comerciante] contenedorSlots no está asignado en el inspector.");

        if (slotPrefab == null)
            Debug.LogError("[Comerciante] slotPrefab no está asignado en el inspector.");

        if (textoMonedasJugador == null)
            Debug.LogError("[Comerciante] textoMonedasJugador no está asignado en el inspector.");

        if (itemsEnVenta == null || itemsEnVenta.Count == 0)
            Debug.LogWarning("[Comerciante] No hay itemsEnVenta asignados para este comerciante.");
    }

    // --- Implementación de IInteractable ---
    public bool CanInteract()
    {
        return !comercioActivo;
    }

    public void Interact(GameObject jugador)
    {
        Debug.Log("[Comerciante] Interact llamado. Abriendo comercio...");
        AbrirComercio();
    }

    private void AbrirComercio()
    {
        if (panelComercio == null || contenedorSlots == null || slotPrefab == null)
        {
            Debug.LogError("[Comerciante] No se puede abrir comercio porque faltan referencias de UI.");
            return;
        }

        comercioActivo = true;
        panelComercio.SetActive(true);

        // Limpiar slots previos
        foreach (Transform hijo in contenedorSlots)
            Destroy(hijo.gameObject);

        // Crear slots para cada ítem en venta
        if (itemsEnVenta == null || itemsEnVenta.Count == 0)
        {
            Debug.LogWarning("[Comerciante] No hay items para mostrar en el comercio.");
        }
        else
        {
            foreach (var item in itemsEnVenta)
            {
                if (item == null)
                {
                    Debug.LogWarning("[Comerciante] Se encontró un item null en itemsEnVenta.");
                    continue;
                }

                var slotGO = Instantiate(slotPrefab, contenedorSlots);
                var slotUI = slotGO.GetComponent<SlotUI>();
                if (slotUI == null)
                {
                    Debug.LogError("[Comerciante] El prefab de slot no tiene SlotUI.");
                    continue;
                }

                slotUI.Configurar(item);

                var boton = slotGO.GetComponent<Button>();
                if (boton == null)
                {
                    Debug.LogError("[Comerciante] El prefab de slot no tiene un componente Button.");
                    continue;
                }

                boton.onClick.AddListener(() => ComprarItem(item));
            }
        }

        ActualizarMonedas();
    }

    private void ComprarItem(ItemSO item)
    {
        int precio = item.valor > 0 ? item.valor : precioBase;

        if (inventarioJugador.monedas >= precio)
        {
            inventarioJugador.monedas -= precio;
            inventarioJugador.AgregarItem(item);
            ActualizarMonedas();
            Debug.Log($"Compraste {item.nombre} por {precio} monedas.");
        }
        else
        {
            Debug.Log("No tienes suficientes monedas.");
        }
    }

    public void VenderItem(ItemSO item)
    {
        int precio = item.valor > 0 ? item.valor : precioBase;

        if (inventarioJugador.TieneItem(item))
        {
            inventarioJugador.RemoverItem(item);
            inventarioJugador.monedas += precio;
            ActualizarMonedas();
            Debug.Log($"Vendiste {item.nombre} por {precio} monedas.");
        }
        else
        {
            Debug.Log("No tienes ese ítem para vender.");
        }
    }

    private void ActualizarMonedas()
    {
        if (textoMonedasJugador == null)
        {
            Debug.LogError("[Comerciante] textoMonedasJugador no está asignado.");
            return;
        }

        if (inventarioJugador == null)
        {
            Debug.LogError("[Comerciante] InventarioJugador no encontrado.");
            return;
        }

        textoMonedasJugador.text = $"Monedas: {inventarioJugador.monedas}";
    }

    public void CerrarComercio()
    {
        comercioActivo = false;
        panelComercio.SetActive(false);
    }
}



