using UnityEngine;

public class ItemLoot : MonoBehaviour
{
    // Definimos los tipos de objetos que pueden caer
    public enum TipoDeItem { Moneda, Pocion }

    [Header("Configuración")]
    public TipoDeItem tipo; // Aquí elegiremos qué es desde el Inspector
    public int cantidad = 1; // Cuánto suma o cura

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificamos si chocamos con el Player
        if (collision.CompareTag("Player"))
        {
            // --- CASO 1: ES UNA MONEDA ---
            if (tipo == TipoDeItem.Moneda)
            {
                if (LootManager.Instance != null)
                {
                    LootManager.Instance.SumarMoneda(cantidad);
                    Destroy(gameObject); // Desaparece
                }
            }
            // --- CASO 2: ES UNA POCIÓN ---
            else if (tipo == TipoDeItem.Pocion)
            {
                // Buscamos el script 'Player' en el objeto con el que chocamos
                Player scriptJugador = collision.GetComponent<Player>();

                if (scriptJugador != null)
                {
                    scriptJugador.Curar(20); // Cura 20 de vida (puedes cambiar este número)
                    Destroy(gameObject); // Desaparece la poción
                }
            }
        }
    }
}