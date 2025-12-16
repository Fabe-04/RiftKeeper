using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] int cantidadCuracion = 30;
    [SerializeField] bool destruirAlUsar = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Solo el jugador puede recogerla
        if (collision.CompareTag("Player"))
        {
            Player playerScript = collision.GetComponent<Player>();

            if (playerScript != null)
            {
                // Solo curamos si no tiene la vida llena (Opcional, quita el if si quieres que siempre se use)
                if (playerScript.currentHealth < playerScript.maxHealth)
                {
                    playerScript.Curar(cantidadCuracion);
                    Debug.Log($"<color=green>Poción recogida: +{cantidadCuracion} HP</color>");

                    if (destruirAlUsar) Destroy(gameObject);
                }
            }
        }
    }
}