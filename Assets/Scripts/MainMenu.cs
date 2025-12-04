using UnityEngine;
using UnityEngine.SceneManagement; // <--- NECESARIO PARA CAMBIAR DE ESCENA

public class MainMenu : MonoBehaviour
{
    // Esta función la llamará el botón Jugar
    public void Jugar()
    {
        // "SampleScene" es el nombre de tu nivel. 
        // Si tu escena se llama diferente, cambia el nombre aquí.
        SceneManager.LoadScene("SampleScene");
    }

    // Esta función la llamará el botón Salir
    public void Salir()
    {
        Debug.Log("Saliendo del juego..."); // Mensaje para saber que funciona en el editor
        Application.Quit(); // Esto cierra el juego (solo funciona en el ejecutable final)
    }
}