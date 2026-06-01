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
        panelComercio.SetActive(false);
    }

    // --- Implementación de IInteractable ---
    public bool CanInteract()
    {
        return !comercioActivo;
    }

    public void Interact(GameObject jugador)
    {
        AbrirComercio();
    }

    private void AbrirComercio()
    {
        comercioActivo = true;
        panelComercio.SetActive(true);

        // Limpiar slots previos
        foreach (Transform hijo in contenedorSlots)
            Destroy(hijo.gameObject);

        // Crear slots para cada ítem en venta
        foreach (var item in itemsEnVenta)
        {
            var slotGO = Instantiate(slotPrefab, contenedorSlots);
            var slotUI = slotGO.GetComponent<SlotUI>();
            slotUI.Configurar(item);

            // Botón de compra
            var boton = slotGO.GetComponent<Button>();
            boton.onClick.AddListener(() => ComprarItem(item));
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
        textoMonedasJugador.text = $"Monedas: {inventarioJugador.monedas}";
    }

    public void CerrarComercio()
    {
        comercioActivo = false;
        panelComercio.SetActive(false);
    }
}



