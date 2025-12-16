using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("¡Nivel Completado!");
            // Reinicia la escena para generar una nueva mazmorra infinita (Roguelike Loop)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            // O ir a un menú de victoria:
            // SceneManager.LoadScene("WinScreen");
        }
    }
}