using System;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public string playerName = "Player";
    public int level = 1;
    public float currentHealth = 100f;
    public float maxHealth = 100f;

    public bool talkedToGrandma = false;
    public bool missionAccepted = false;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string FILE_NAME = "playerdata.json";
    private string SavePath => Path.Combine(Application.persistentDataPath, FILE_NAME);

    public PlayerSaveData Data { get; private set; } = new PlayerSaveData();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(Data, prettyPrint: true);
            File.WriteAllText(SavePath, json);
#if UNITY_EDITOR
            Debug.Log($"SaveManager: Guardado en {SavePath}");
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"SaveManager: Error guardando: {ex.Message}");
        }
    }

    public void Load()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                Data = JsonUtility.FromJson<PlayerSaveData>(json) ?? new PlayerSaveData();
#if UNITY_EDITOR
                Debug.Log("SaveManager: Datos cargados.");
#endif
            }
            else
            {
                Data = new PlayerSaveData();
#if UNITY_EDITOR
                Debug.Log("SaveManager: No hay archivo, usando datos por defecto.");
#endif
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"SaveManager: Error cargando: {ex.Message}");
            Data = new PlayerSaveData();
        }
    }

    // Helpers para modificar datos y salvar inmediatamente
    public void SetTalkedToGrandma(bool value)
    {
        Data.talkedToGrandma = value;
        Save();
    }

    public void SetMissionAccepted(bool value)
    {
        Data.missionAccepted = value;
        Save();
    }

    public void SetPlayerName(string name)
    {
        Data.playerName = name;
        Save();
    }

    public void SetLevel(int level)
    {
        Data.level = Mathf.Max(1, level);
        Save();
    }

    public void SetHealth(float current, float max)
    {
        Data.currentHealth = Mathf.Clamp(current, 0f, max);
        Data.maxHealth = Mathf.Max(1f, max);
        Save();
    }
}