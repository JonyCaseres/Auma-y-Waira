using System.Collections;
using UnityEngine;

public class Camara : MonoBehaviour
{
    [Header("Referencia de cámara")]
    public Camera mainCamera; // si no se asigna, se usará Camera.main
    [Tooltip("Plano Z donde medir el tamaño del frustum (solo para cámaras en perspectiva).")]
    public float referenceZ = 0f;

    [Header("Tiempos y curva")]
    public float segmentDuration = 1.2f;
    public float pauseBetweenSegments = 0.2f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Opciones")]
    public bool playOnStart = true;
    public bool loop = false;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        Debug.Log("[Camara] Start: mainCamera = " + (mainCamera ? mainCamera.name : "null") + ", playOnStart=" + playOnStart);
        if (playOnStart) StartCoroutine(CinematicSequence());
    }

    [ContextMenu("Play Cinematic")]
    public void PlayCinematic()
    {
        StopAllCoroutines();
        StartCoroutine(CinematicSequence());
    }

    IEnumerator CinematicSequence()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Camara: No hay Camera asignada ni Camera.main disponible.");
            yield break;
        }

        // Calcular tamaño del cuadro en unidades mundo en el plano referenceZ
        float viewHeight, viewWidth;
        GetCameraWorldSizeAtZ(referenceZ, out viewHeight, out viewWidth);

        Vector3 startPos = transform.position;
        Vector3 up1 = startPos + Vector3.up * viewHeight;                 // primer movimiento arriba
        Vector3 up2 = up1 + Vector3.up * viewHeight;                      // segundo movimiento arriba
        Vector3 right = up2 + Vector3.right * viewWidth;                 // movimiento a la derecha (ancho de la cam)
        Vector3 down1 = right + Vector3.down * viewHeight;               // primer movimiento abajo
        Vector3 down2 = down1 + Vector3.down * viewHeight;               // segundo movimiento abajo (añadido)

        Debug.Log($"[Camara] start={startPos}, up1={up1}, up2={up2}, right={right}, down1={down1}, down2={down2}");

        do
        {
            yield return MoveTo(up1, segmentDuration);
            yield return new WaitForSeconds(pauseBetweenSegments);

            yield return MoveTo(up2, segmentDuration);
            yield return new WaitForSeconds(pauseBetweenSegments);

            yield return MoveTo(right, segmentDuration);
            yield return new WaitForSeconds(pauseBetweenSegments);

            yield return MoveTo(down1, segmentDuration);
            yield return new WaitForSeconds(pauseBetweenSegments);

            yield return MoveTo(down2, segmentDuration);
            yield return new WaitForSeconds(pauseBetweenSegments);
        }
        while (loop);

        Debug.Log("[Camara] CinematicSequence finished (loop=" + loop + ")");
    }

    IEnumerator MoveTo(Vector3 target, float duration)
    {
        Vector3 initial = transform.position;
        Debug.Log($"[Camara] MoveTo from {initial} to {target} duration={duration}");
        if (duration <= 0f)
        {
            transform.position = target;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duration);
            float eased = ease.Evaluate(normalized);
            transform.position = Vector3.LerpUnclamped(initial, target, eased);
            yield return null;
        }
        transform.position = target;
    }

    void GetCameraWorldSizeAtZ(float z, out float height, out float width)
    {
        if (mainCamera.orthographic)
        {
            height = mainCamera.orthographicSize * 2f;
            width = height * mainCamera.aspect;
            return;
        }

        float camZ = mainCamera.transform.position.z;
        float distance = Mathf.Abs(z - camZ);
        if (distance < 0.0001f) distance = 0.0001f;
        float frustumHeight = 2f * distance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        height = frustumHeight;
        width = frustumHeight * mainCamera.aspect;
    }
}
