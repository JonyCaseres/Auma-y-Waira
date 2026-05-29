using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Dialogue", menuName = "NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    [Header("Datos del NPC")]
    public string npcName;
    public Sprite npcPortrait;

    [Header("Diálogo principal")]
    [TextArea(2, 5)]
    public string[] dialogueLines;

    [Header("Preguntas (opciones de elección)")]
    [TextArea(2, 5)]
    public string[] questions; // aquí puedes poner las preguntas que el NPC hará

    [Header("Diálogo adicional (después de preguntas)")]
    [TextArea(2, 5)]
    public string[] extraDialogueLines; // líneas que se muestran después de responder

    [Header("Configuración de escritura")]
    public float typingSpeed = 0.05f;

    [Header("Audio")]
    public AudioClip voiceSound;
    public float voicePitch = 1f;

    [Header("Auto progreso")]
    public bool[] autoProgressLines;   // debe tener mismo tamaño que dialogueLines
    public float autoProgressDelay = 1.5f;
}
