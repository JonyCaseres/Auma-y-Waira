using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestor global de diálogos. 
/// Almacena todas las interacciones de texto que el jugador tiene con los NPC.
/// </summary>
public class NpcDialogueManager : MonoBehaviour
{
    // Singleton para acceso global
    public static NpcDialogueManager Instance { get; private set; }

    // Historial de todos los diálogos
    private List<string> historialDialogos = new List<string>();

    private void Awake()
    {
        // Asegurar que solo exista uno en la escena
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // persiste entre escenas
    }

    /// <summary>
    /// Registrar un nuevo diálogo en el historial.
    /// </summary>
    public void RegistrarDialogo(string npcName, string linea)
    {
        string registro = $"[{npcName}] {linea}";
        historialDialogos.Add(registro);
        Debug.Log("Diálogo registrado: " + registro);
    }

    /// <summary>
    /// Obtener todo el historial de diálogos.
    /// </summary>
    public List<string> ObtenerHistorial()
    {
        return new List<string>(historialDialogos);
    }

    /// <summary>
    /// Obtener el último diálogo registrado.
    /// </summary>
    public string ObtenerUltimoDialogo()
    {
        if (historialDialogos.Count == 0) return "No hay diálogos registrados.";
        return historialDialogos[historialDialogos.Count - 1];
    }

    /// <summary>
    /// Limpiar todo el historial.
    /// </summary>
    public void LimpiarHistorial()
    {
        historialDialogos.Clear();
    }
}

