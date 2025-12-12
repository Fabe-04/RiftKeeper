using UnityEngine;

public class ArrowPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Solo el jugador puede recogerla
        if (collision.CompareTag("Player")) // Asegúrate que tu Player tenga el Tag "Player"
        {
            // Buscamos el script del jugador para sumar flechas
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.RecogerFlecha(1); // Suma 1 flecha
                Destroy(gameObject);     // Desaparece del suelo
            }
        }
    }
}