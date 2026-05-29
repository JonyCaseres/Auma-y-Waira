using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Dictionary<string, string> decisiones = new Dictionary<string, string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- Evento narrativo del Yatiri ---
    public void IniciarEventoYatiri()
    {
        // Diálogo inicial
        DialogueController.Instance.IniciarDialogo(
            new List<string> { "Has encontrado al Yatiri... ¿qué deseas hacer?" }
        );

        // Elecciones
        DialogueController.Instance.MostrarElecciones("yatiri",
            new List<string> { "Pedir consejo", "Ignorar", "Provocar" },
            (decision) => {
                RegistrarDecision("yatiri", decision);
                LanzarDialogoSalida(decision);
            }
        );
    }

    private void RegistrarDecision(string npcId, string decision)
    {
        decisiones[npcId] = decision;
        Debug.Log($"Decisión tomada con {npcId}: {decision}");
    }

    private void LanzarDialogoSalida(string decision)
    {
        if (decision == "Pedir consejo")
            DialogueController.Instance.IniciarDialogo(new List<string> { "El Yatiri te bendice y te guía en tu camino..." });
        else if (decision == "Ignorar")
            DialogueController.Instance.IniciarDialogo(new List<string> { "El Yatiri se aleja en silencio, decepcionado..." });
        else if (decision == "Provocar")
            DialogueController.Instance.IniciarDialogo(new List<string> { "El Yatiri se enfurece y te maldice..." });
    }
}

