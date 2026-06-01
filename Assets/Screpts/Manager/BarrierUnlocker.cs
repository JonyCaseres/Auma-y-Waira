using UnityEngine;

public class BarrierUnlocker : MonoBehaviour
{
    public enum RequiredCondition
    {
        TalkedToGrandma,
        MissionAccepted
    }

    [Header("Condición para desbloquear")]
    public RequiredCondition required = RequiredCondition.MissionAccepted;

    [Header("Comportamiento al desbloquear")]
    [Tooltip("Si true, desactiva todo el GameObject (visual + colisión). Si false, sólo desactiva el Collider2D.")]
    public bool disableWholeGameObject = true;

    [Tooltip("Si true, la barrera se desbloquea sólo una vez.")]
    public bool singleUse = true;

    [Header("Opcional: mensaje si no cumple la condición")]
    public string lockedMessage = "No puedes pasar aún.";

    private bool unlocked = false;
    private Collider2D cachedCollider;

    private void Awake()
    {
        cachedCollider = GetComponent<Collider2D>();
        // Si la barrera no tiene collider, no pasa nada; Show/Hide seguirá funcionando.
    }

    private void Start()
    {
        // Comprobar inmediatamente (por si la condición ya estaba guardada)
        CheckAndUnlockIfNeeded();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (unlocked) return;
        if (!other.CompareTag("Player")) return;

        // Al entrar el jugador, comprobar el save y desbloquear si corresponde
        if (CheckAndUnlockIfNeeded())
            return;

        // Si no cumple, opcional: mostrar un mensaje (puedes enlazar tu UI en otra parte)
        if (!string.IsNullOrEmpty(lockedMessage))
            Debug.Log(lockedMessage);
    }

    // Comprueba SaveManager y desbloquea si la condición se cumple. Devuelve true si desbloqueó.
    public bool CheckAndUnlockIfNeeded()
    {
        if (unlocked) return true;
        if (SaveManager.Instance == null) return false;

        bool cond = false;
        switch (required)
        {
            case RequiredCondition.TalkedToGrandma:
                cond = SaveManager.Instance.GetTalkedToGrandma();
                break;
            case RequiredCondition.MissionAccepted:
                cond = SaveManager.Instance.GetMissionAccepted();
                break;
        }

        if (cond)
        {
            Unlock();
            return true;
        }

        return false;
    }

    // Método público que puedes invocar desde NPCEspecial justo después de seleccionar la opción "sí".
    public void TryUnlockImmediate()
    {
        // Forzar lectura y desbloqueo (por ejemplo NPCEspecial ya habrá guardado mediante SaveManager)
        CheckAndUnlockIfNeeded();
    }

    private void Unlock()
    {
        if (unlocked) return;
        unlocked = true;

        if (disableWholeGameObject)
        {
            gameObject.SetActive(false);
        }
        else
        {
            if (cachedCollider != null) cachedCollider.enabled = false;
            // Si deseas, también puedes ocultar el sprite u otros componentes aquí.
            var sprite = GetComponent<SpriteRenderer>();
            if (sprite != null) sprite.enabled = false;
        }

        if (singleUse)
        {
            // mantener unlocked = true para no reactivar
        }

        Debug.Log($"BarrierUnlocker: barrera desbloqueada ({name}).");
    }
}