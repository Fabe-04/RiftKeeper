using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] string nombreDelJefe = "EL GUARDIÁN"; // Ahora sí lo usaremos

    private Enemy enemyScript;

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.OnHealthChanged += ActualizarBarra;
            enemyScript.OnDeath += Victoria;
        }

        // OPCIONAL: Podrías añadir un método en UIManager para setear el nombre
        // Por ahora, solo lo dejamos aquí para futuras expansiones o lo quitamos si molesta.
        // Para limpiar el warning sin borrar la variable, simplemente la leemos en un Debug:
        Debug.Log($"[BossController] {nombreDelJefe} ha despertado.");
    }

    void ActualizarBarra(int current, int max)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ActualizarVidaJefe(current, max);
    }

    void Victoria()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ActivarModoJefe(false);

        if (GameManager.instance != null)
            GameManager.instance.Victory();
    }

    private void OnDestroy()
    {
        if (enemyScript != null)
        {
            enemyScript.OnHealthChanged -= ActualizarBarra;
            enemyScript.OnDeath -= Victoria;
        }
    }
}