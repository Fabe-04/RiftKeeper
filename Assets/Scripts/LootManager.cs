using UnityEngine;
using TMPro; // <--- NECESARIO PARA EL TEXTO

public class LootManager : MonoBehaviour
{
    public static LootManager Instance;

    [Header("Objetos que van a caer")]
    public GameObject monedaPrefab;
    public GameObject pocionPrefab;

    [Header("Interfaz (UI)")]
    public TextMeshProUGUI textoMonedas; // <--- ARRASTRA AQUÍ TU TEXTO NUEVO
    public int monedasTotales = 0;

    public enum EnemyType { Basico, Charger }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Al empezar, ponemos el contador a 0 en la pantalla
        ActualizarTexto();
    }

    public void SpawnLoot(Vector3 posicionMuerte, EnemyType tipoEnemigo)
    {
        // Forzamos Z=0 para intentar arreglar lo visual
        Vector3 posicionVisible = new Vector3(posicionMuerte.x, posicionMuerte.y, 0f);

        if (tipoEnemigo == EnemyType.Basico)
        {
            float probabilidad = Random.Range(0f, 100f);
            if (probabilidad <= 70f)
            {
                Instantiate(monedaPrefab, posicionVisible, Quaternion.identity);
            }
        }
        else if (tipoEnemigo == EnemyType.Charger)
        {
            Instantiate(monedaPrefab, posicionVisible, Quaternion.identity);

            float probabilidadPocion = Random.Range(0f, 100f);
            // Puedes dejar esto en 25f o subirlo para pruebas
            if (probabilidadPocion <= 25f)
            {
                Instantiate(pocionPrefab, posicionVisible, Quaternion.identity);
            }
        }
    }

    public void SumarMoneda(int cantidad)
    {
        monedasTotales += cantidad;
        ActualizarTexto(); // Actualizamos la pantalla
        Debug.Log("💰 MONEDAS: " + monedasTotales);
    }

    // Función auxiliar para escribir en pantalla
    void ActualizarTexto()
    {
        if (textoMonedas != null)
        {
            textoMonedas.text = "Monedas: " + monedasTotales.ToString();
        }
    }
}