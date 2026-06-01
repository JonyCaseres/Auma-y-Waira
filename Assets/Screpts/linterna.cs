using UnityEngine;

public class linterna : MonoBehaviour
{
    public enum FollowMode { InputKeys, TargetVelocity }

    [Header("Referencia a la luz (Transform del GameObject Light2D)")]
    [Tooltip("Arrastra aqu� el Transform del Spot Light 2D (el GameObject que rota).")]
    [SerializeField] private Transform lightTransform;

    [Header("Modo de funcionamiento")]
    [SerializeField] private FollowMode mode = FollowMode.TargetVelocity;

    [Header("Si usas TargetVelocity")]
    [Tooltip("Transform del objetivo (normalmente el jugador).")]
    [SerializeField] private Transform target;
    [Tooltip("Opcional: Rigidbody2D del objetivo para leer la velocidad directamente.")]
    [SerializeField] private Rigidbody2D targetRb;
    [Tooltip("Velocidad m�nima para considerar que el objetivo se est� moviendo.")]
    [SerializeField] private float minVelocityThreshold = 0.1f;

    [Header("Ajustes de rotaci�n")]
    [Tooltip("Si est� activo, la linterna rotar� suavemente hacia la direcci�n objetivo.")]
    [SerializeField] private bool smooth = true;
    [Tooltip("Velocidad de rotaci�n (grados por segundo) usada cuando smooth = true.")]
    [SerializeField, Range(10f, 2000f)] private float smoothSpeed = 720f;

    [Header("Controles (solo InputKeys)")]
    [Tooltip("Habilitar tambi�n flechas adem�s de W/A/S/D.")]
    [SerializeField] private bool allowArrows = true;

    // �ngulo objetivo en grados (Z)
    private float targetAngleZ = 0f;

    // Para calcular velocidad por delta cuando no hay Rigidbody2D
    private Vector2 lastTargetPosition;

    private void Reset()
    {
        if (lightTransform == null) lightTransform = transform;
    }

    private void Start()
    {
        if (lightTransform == null) lightTransform = transform;
        if (target != null) lastTargetPosition = target.position;
    }

    private void Update()
    {
        if (mode == FollowMode.InputKeys)
        {
            ReadInputAndSetTarget();
        }
        else
        {
            ReadTargetVelocityAndSetTarget();
        }

        if (lightTransform == null) return;

        if (smooth)
        {
            float current = lightTransform.eulerAngles.z;
            float next = Mathf.MoveTowardsAngle(current, targetAngleZ, smoothSpeed * Time.deltaTime);
            lightTransform.rotation = Quaternion.Euler(0f, 0f, next);
        }
        else
        {
            lightTransform.rotation = Quaternion.Euler(0f, 0f, targetAngleZ);
        }
    }

    private void ReadInputAndSetTarget()
    {
        bool up = Input.GetKey(KeyCode.W) || (allowArrows && Input.GetKey(KeyCode.UpArrow));
        bool down = Input.GetKey(KeyCode.S) || (allowArrows && Input.GetKey(KeyCode.DownArrow));
        bool left = Input.GetKey(KeyCode.A) || (allowArrows && Input.GetKey(KeyCode.LeftArrow));
        bool right = Input.GetKey(KeyCode.D) || (allowArrows && Input.GetKey(KeyCode.RightArrow));

        if (up && !down)
            SetDirectionUp();
        else if (down && !up)
            SetDirectionDown();
        else if (left && !right)
            SetDirectionLeft();
        else if (right && !left)
            SetDirectionRight();
        // si no hay input, mantiene la �ltima direcci�n
    }

    private void ReadTargetVelocityAndSetTarget()
    {
        if (target == null) return;

        Vector2 vel = Vector2.zero;

        if (targetRb != null)
        {
            vel = targetRb.linearVelocity;
        }
        else
        {
            // calcular velocidad por delta posici�n
            Vector2 currentPos = target.position;
            vel = (currentPos - lastTargetPosition) / Time.deltaTime;
            lastTargetPosition = currentPos;
        }

        if (vel.magnitude < minVelocityThreshold)
            return; // no cambiar si casi no se mueve

        // decidir direcci�n dominante
        if (Mathf.Abs(vel.x) > Mathf.Abs(vel.y))
        {
            if (vel.x > 0f) SetDirectionRight(); else SetDirectionLeft();
        }
        else
        {
            if (vel.y > 0f) SetDirectionUp(); else SetDirectionDown();
        }
    }

    // Mapeo de �ngulos: derecha = 0, arriba = 90, izquierda = 180, abajo = 270
    public void SetDirectionUp()    => targetAngleZ = 90f;
    public void SetDirectionDown()  => targetAngleZ = 270f;
    public void SetDirectionLeft()  => targetAngleZ = 180f;
    public void SetDirectionRight() => targetAngleZ = 0f;
}
