using UnityEngine;
using UnityEngine.SceneManagement;

public class iniciodeecena : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void IniciarJuego()
    {
        SceneManager.LoadScene("Pueblo");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
