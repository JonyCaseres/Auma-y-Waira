using UnityEngine;

public class CameraCantroler : MonoBehaviour
{
    [SerializeField] public Transform player;
    [SerializeField] public float velocidadCamera = 10f;
    [SerializeField] public Vector3 Movinput;

    private void LateUpdate()
    {
        Vector3 posicion = player.position + Movinput;
        Vector3 posicionSuavisada = Vector3.Lerp(transform.position, posicion, velocidadCamera);
        transform.position = posicionSuavisada;
    }
}
