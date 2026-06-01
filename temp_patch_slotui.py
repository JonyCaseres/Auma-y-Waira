from pathlib import Path
p = Path('Assets/Screpts/UI/SlotUI.cs')
text = p.read_text(encoding='utf-8')
old = '''using UnityEngine;
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
}'''
new = '''using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image icono;
    public TMP_Text nombre;
    public TMP_Text precio;
    public TMP_Text cantidad;

    private ItemSO item;

    public void Configurar(ItemSO nuevoItem)
    {
        item = nuevoItem;
        
        if (icono != null) icono.sprite = item.icono;
        
        if (nombre != null) nombre.text = item.nombre;
        
        if (precio != null)
        {
            if (item.valor > 0)
                precio.text = $"Precio: {item.valor}";
            else
                precio.text = "Precio: N/A";
        }
        
        if (cantidad != null)
        {
            cantidad.text = $"Stock: {item.cantidadMax}";
        }
    }

    public ItemSO ObtenerItem()
    {
        return item;
    }
}'''
if old not in text:
    raise SystemExit('pattern not found')
text = text.replace(old, new)
p.write_text(text, encoding='utf-8')
print('patched SlotUI.cs')
