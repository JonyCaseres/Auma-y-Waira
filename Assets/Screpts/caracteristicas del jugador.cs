using UnityEngine;

public class caracteristicas_del_jugador : MonoBehaviour
{
    [Header("Atributos")]
    public string playerName = "Player";
    public int level = 1;
    public float currentHealth = 100f;
    public float maxHealth = 100f;

    [Header("Flags")]
    public bool talkedToGrandma = false;
    public bool missionAccepted = false;

    private void Start()
    {
        LoadFromSave();
    }

    // Cargar valores desde el SaveManager si existe
    public void LoadFromSave()
    {
        if (SaveManager.Instance == null) return;

        var data = SaveManager.Instance.Data;
        playerName = data.playerName;
        level = data.level;
        currentHealth = data.currentHealth;
        maxHealth = data.maxHealth;
        talkedToGrandma = data.talkedToGrandma;
        missionAccepted = data.missionAccepted;
    }

    // Guardar valores actuales en el SaveManager (y en disco)
    public void SaveToDisk()
    {
        if (SaveManager.Instance == null) return;

        SaveManager.Instance.SetPlayerName(playerName);
        SaveManager.Instance.SetLevel(level);
        SaveManager.Instance.SetHealth(currentHealth, maxHealth);
        SaveManager.Instance.SetTalkedToGrandma(talkedToGrandma);
        SaveManager.Instance.SetMissionAccepted(missionAccepted);
    }

    // Métodos de ayuda para actualizar un atributo y salvar inmediatamente
    public void MarkTalkedToGrandma()
    {
        talkedToGrandma = true;
        if (SaveManager.Instance != null) SaveManager.Instance.SetTalkedToGrandma(true);
    }

    public void MarkMissionAccepted()
    {
        missionAccepted = true;
        if (SaveManager.Instance != null) SaveManager.Instance.SetMissionAccepted(true);
    }

    public void ChangeHealth(float delta)
    {
        currentHealth = Mathf.Clamp(currentHealth + delta, 0f, maxHealth);
        if (SaveManager.Instance != null) SaveManager.Instance.SetHealth(currentHealth, maxHealth);
    }

    public void SetLevel(int newLevel)
    {
        level = Mathf.Max(1, newLevel);
        if (SaveManager.Instance != null) SaveManager.Instance.SetLevel(level);
    }
}