using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    private Enemy enemyStats;

    private void Start()
    {
        // Buscamos el script 'Enemy' en el padre para saber cuánto daño hacer
        enemyStats = GetComponentInParent<Enemy>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Si el objeto que entró en el Trigger es el Player
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null && enemyStats != null)
            {
                // Usamos la variable damageToPlayer que ya tienes en Enemy.cs
                // Nota: Asegúrate de que 'damageToPlayer' sea pública en Enemy.cs o usa un Getter

                // Opción A: Si damageToPlayer es público (recomendado cambiar a public en Enemy.cs)
                player.TakeDamage(enemyStats.damageToPlayer);
            }
        }
    }
}