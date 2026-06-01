using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image icono;
    public TMP_Text nombre;

    private ItemSO item;

    public void Configurar(ItemSO nuevoItem)
    {
        item = nuevoItem;
        if (icono != null) icono.sprite = item.icono;
        if (nombre != null) nombre.text = item.nombre;
    }

    public ItemSO ObtenerItem()
    {
        return item;
    }
}
