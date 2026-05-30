using UnityEngine;

public enum TipoItem 
{ 
    Herramienta, 
    Moneda, 
    Consumible, 
    Coleccionable, 
    Mineral 
}


[CreateAssetMenu(fileName = "NuevoItem", menuName = "Inventario/Item")]
public class ItemSO : ScriptableObject
{
    [Header("Datos básicos")]
    public string nombre;
    public Sprite icono;
    [TextArea] public string descripcion;

    [Header("Clasificación")]
    public TipoItem tipo;   // Herramienta, Moneda, Consumible, Coleccionable, Mineral

    [Header("Atributos")]
    public int valor;       // Precio, poder o cantidad de monedas
    public int cantidadMax; // Máximo stackeable (ej: 99 minerales)
}

