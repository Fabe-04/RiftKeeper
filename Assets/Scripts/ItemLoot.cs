using UnityEngine;

public class ItemLoot : MonoBehaviour
{
    public enum TipoDeItem { Moneda, Pocion }

    [Header("Configuración")]
    public TipoDeItem tipo;
    public int cantidad = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (tipo == TipoDeItem.Moneda)
            {
                // Ahora llamamos a LootManager, que redirige a GameManager
                if (LootManager.Instance != null)
                {
                    LootManager.Instance.SumarMoneda(cantidad);
                }
                Destroy(gameObject);
            }
            else if (tipo == TipoDeItem.Pocion)
            {
                Player scriptJugador = collision.GetComponent<Player>();
                if (scriptJugador != null)
                {
                    scriptJugador.Curar(20);
                    Destroy(gameObject);
                }
            }
        }
    }
}