using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

public class MainMenuController : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Escribe aquí el nombre EXACTO de tu escena de juego")]
    [SerializeField] string nombreEscenaJuego = "GameScene"; // ¡Cámbialo por el tuyo!

    // Esta función será llamada por el botón
    public void EmpezarJuego()
    {
        Debug.Log("Iniciando partida...");
        // Carga la escena por su nombre. Asegúrate que está en Build Settings.
        SceneManager.LoadScene(nombreEscenaJuego);
    }
}