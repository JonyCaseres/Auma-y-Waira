using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Question", menuName = "NPC Question")]
public class NPCCuestion : ScriptableObject
{
    [Header("Opciones")]
    public string opcion1;
    public string opcion2;
    public string opcion3;

    [Header("Respuestas según el diálogo")]
    [TextArea(2, 5)] public string[] respuesta1;
    [TextArea(2, 5)] public string[] respuesta2;
    [TextArea(2, 5)] public string[] respuesta3;
}
