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

        float viewHeight, viewWidth;
        GetCameraWorldSizeAtZ(referenceZ, out viewHeight, out viewWidth);
        Debug.Log($"[Camara] viewHeight={viewHeight:F3}, viewWidth={viewWidth:F3}, referenceZ={referenceZ}");

        Vector3 startPos = transform.position;
        Vector3 upTarget = startPos + Vector3.up * viewHeight;
        Vector3 rightTarget = upTarget + Vector3.right * viewWidth;
        Vector3 downTarget = rightTarget + Vector3.down * viewHeight * 2.5f;

        Debug.Log($"[Camara] start={startPos}, upTarget={upTarget}, rightTarget={rightTarget}, downTarget={downTarget}");

        do
        {
            yield return MoveTo(upTarget, segmentDuration);
            yield return new WaitForSeconds(pauseBetweenSegments);

            yield return MoveTo(rightTarget, segmentDuration);
            yield return new WaitForSeconds(pauseBetweenSegments);

            yield return MoveTo(downTarget, segmentDuration);
            yield return new WaitForSeconds(pauseBetweenSegments);

            yield return MoveTo(startPos, segmentDuration);
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
