using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseSceneManager : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Nombre de la escena de pausa (debe estar en Build Settings). Se cargará en modo Additive.")]
    public string pauseSceneName = "Pause";
    [Tooltip("Etiqueta del objeto jugador (se intentará encontrar por tag si no se asigna).")]
    public string playerTag = "Player";
    [Tooltip("Player GameObject (si se asigna, se usará en vez de buscar por tag).")]
    public GameObject playerOverride;
    [Tooltip("Si true, al pausar se pondrá Time.timeScale = 0 para congelar el juego.")]
    public bool freezeTime = true;

    // Nombres de campos/propiedades que intentaremos guardar automáticamente si existen en componentes del Player
    private readonly string[] candidateStatNames = new string[]
    {
        "currentHealth", "health", "vida", "hp",
        "maxHealth", "maxHp",
        "stamina", "energy",
        "monedas", "coins", "gold",
        "level", "xp", "experience"
    };

    private const string KEY_PREFIX = "player_";
    private const string KEY_SCENE = "player_last_scene";
    private const string KEY_POSX = "player_pos_x";
    private const string KEY_POSY = "player_pos_y";
    private const string KEY_POSZ = "player_pos_z";

    private bool isPaused = false;

    // Public API: pausar y reanudar
    public void Pause()
    {
        if (isPaused) return;

        SavePlayerState();
        LoadPauseScene();
        if (freezeTime) Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        if (!isPaused) return;

        UnloadPauseScene();
        if (freezeTime) Time.timeScale = 1f;
        RestorePlayerState();
        isPaused = false;
    }

    // Guarda posición + stats detectadas en PlayerPrefs
    public void SavePlayerState()
    {
        GameObject player = ResolvePlayer();
        if (player == null)
        {
            Debug.LogWarning("PauseSceneManager: jugador no encontrado al guardar estado.");
            return;
        }

        Vector3 pos = player.transform.position;
        PlayerPrefs.SetFloat(KEY_POSX, pos.x);
        PlayerPrefs.SetFloat(KEY_POSY, pos.y);
        PlayerPrefs.SetFloat(KEY_POSZ, pos.z);

        // Guardar escena activa
        PlayerPrefs.SetString(KEY_SCENE, SceneManager.GetActiveScene().name);

        // Guardar stats detectadas en componentes del jugador
        var components = player.GetComponents<MonoBehaviour>();
        var savedKeys = new HashSet<string>();
        foreach (var comp in components)
        {
            if (comp == null) continue;
            Type t = comp.GetType();

            foreach (var name in candidateStatNames)
            {
                if (savedKeys.Contains(name)) continue;

                // Buscar campo
                var fi = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fi != null)
                {
                    object val = fi.GetValue(comp);
                    SavePrimitiveValue(name, val);
                    savedKeys.Add(name);
                    continue;
                }

                // Buscar propiedad
                var pi = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (pi != null && pi.CanRead)
                {
                    object val = null;
                    try { val = pi.GetValue(comp); } catch { val = null; }
                    SavePrimitiveValue(name, val);
                    savedKeys.Add(name);
                }
            }
        }

        PlayerPrefs.Save();
        Debug.Log("PauseSceneManager: estado del jugador guardado (position + stats).");
    }

    // Restaura valores guardados (intenta reasignarlos a campos/propiedades del Player)
    public void RestorePlayerState()
    {
        GameObject player = ResolvePlayer();
        if (player == null)
        {
            Debug.LogWarning("PauseSceneManager: jugador no encontrado al restaurar estado.");
            return;
        }

        // Restaurar posición
        if (PlayerPrefs.HasKey(KEY_POSX) && PlayerPrefs.HasKey(KEY_POSY) && PlayerPrefs.HasKey(KEY_POSZ))
        {
            float x = PlayerPrefs.GetFloat(KEY_POSX);
            float y = PlayerPrefs.GetFloat(KEY_POSY);
            float z = PlayerPrefs.GetFloat(KEY_POSZ);
            player.transform.position = new Vector3(x, y, z);
        }

        // Restaurar stats
        var components = player.GetComponents<MonoBehaviour>();
        foreach (var comp in components)
        {
            if (comp == null) continue;
            Type t = comp.GetType();

            foreach (var name in candidateStatNames)
            {
                string key = KEY_PREFIX + name;
                if (!PlayerPrefs.HasKey(key)) continue;

                // Intentar asignar a campo
                var fi = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fi != null)
                {
                    object parsed = GetParsedValueForField(fi.FieldType, PlayerPrefs, key);
                    if (parsed != null)
                    {
                        fi.SetValue(comp, parsed);
                    }
                    continue;
                }

                // Intentar asignar a propiedad
                var pi = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (pi != null && pi.CanWrite)
                {
                    object parsed = GetParsedValueForField(pi.PropertyType, PlayerPrefs, key);
                    if (parsed != null)
                    {
                        try { pi.SetValue(comp, parsed); } catch { }
                    }
                }
            }
        }

        Debug.Log("PauseSceneManager: estado del jugador restaurado (posición + stats).");
    }

    // Helper: guarda valor primitivo a PlayerPrefs con prefijo
    private void SavePrimitiveValue(string name, object val)
    {
        string key = KEY_PREFIX + name;
        if (val == null) return;

        if (val is int i)
        {
            PlayerPrefs.SetInt(key, i);
        }
        else if (val is float f)
        {
            PlayerPrefs.SetFloat(key, f);
        }
        else if (val is double d)
        {
            PlayerPrefs.SetFloat(key, (float)d);
        }
        else if (val is long l)
        {
            PlayerPrefs.SetString(key, l.ToString());
        }
        else if (val is string s)
        {
            PlayerPrefs.SetString(key, s);
        }
        else if (val is bool b)
        {
            PlayerPrefs.SetInt(key, b ? 1 : 0);
        }
        else
        {
            // Si es otro tipo, intentar convertir a string
            PlayerPrefs.SetString(key, val.ToString());
        }
    }

    // Helper: parsea y devuelve objeto adecuado para asignar a campo/propiedad
    private object GetParsedValueForField(Type fieldType, PlayerPrefs prefs, string key)
    {
        try
        {
            if (fieldType == typeof(int))
            {
                return prefs.GetInt(key);
            }
            if (fieldType == typeof(float))
            {
                return prefs.GetFloat(key);
            }
            if (fieldType == typeof(double))
            {
                return (double)prefs.GetFloat(key);
            }
            if (fieldType == typeof(bool))
            {
                return prefs.GetInt(key) != 0;
            }
            if (fieldType == typeof(string))
            {
                return prefs.GetString(key);
            }
            if (fieldType == typeof(long))
            {
                var s = prefs.GetString(key);
                if (long.TryParse(s, out long lng)) return lng;
            }
        }
        catch { }
        return null;
    }

    // Carga la escena de pausa en modo additive (no cambia la escena principal)
    private void LoadPauseScene()
    {
        if (string.IsNullOrEmpty(pauseSceneName))
        {
            Debug.LogWarning("PauseSceneManager: pauseSceneName vacío.");
            return;
        }

        // Si ya está cargada, no volver a cargar
        if (IsSceneLoaded(pauseSceneName))
            return;

        SceneManager.LoadSceneAsync(pauseSceneName, LoadSceneMode.Additive);
    }

    // Descarga la escena de pausa
    private void UnloadPauseScene()
    {
        if (string.IsNullOrEmpty(pauseSceneName)) return;
        if (!IsSceneLoaded(pauseSceneName)) return;

        SceneManager.UnloadSceneAsync(pauseSceneName);
    }

    // Comprueba si una escena por nombre está cargada
    private bool IsSceneLoaded(string name)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.name == name) return true;
        }
        return false;
    }

    // Intenta resolver el GameObject del jugador según override, tag o buscando la clase Player
    private GameObject ResolvePlayer()
    {
        if (playerOverride != null) return playerOverride;

        if (!string.IsNullOrEmpty(playerTag))
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go != null) return go;
        }

        // Intentar buscar componente Player por nombre de tipo
        var playerType = FindPlayerType();
        if (playerType != null)
        {
            var comp = GameObject.FindObjectOfType(playerType);
            if (comp is Component c && c.gameObject != null) return c.gameObject;
        }

        // fallback: buscar cualquier objeto llamado "Player"
        var named = GameObject.Find("Player");
        if (named != null) return named;

        return null;
    }

    private static Type _cachedPlayerType = null;

    // Busca un tipo MonoBehaviour llamado "Player" (cachéa resultado)
    private Type FindPlayerType()
    {
        if (_cachedPlayerType != null) return _cachedPlayerType;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies)
        {
            try
            {
                var types = asm.GetTypes();
                foreach (var t in types)
                {
                    if (!t.IsSubclassOf(typeof(MonoBehaviour))) continue;
                    if (t.Name.Equals("Player", StringComparison.OrdinalIgnoreCase)) { _cachedPlayerType = t; return t; }
                }
            }
            catch { }
        }
        return null;
    }
}