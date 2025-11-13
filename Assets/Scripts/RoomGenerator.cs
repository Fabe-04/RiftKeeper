using UnityEngine;
using UnityEngine.Tilemaps; // ¡Importante! Para poder hablar con los Tilemaps

public class RoomGenerator : MonoBehaviour
{
    // Singleton para que otros scripts (como WaveManager) puedan llamarlo fácilmente
    public static RoomGenerator Instance;

    [Header("Tilemaps de la Escena")]
    // La "ranura" para el Tilemap de Suelo que está en tu Hierarchy
    [SerializeField] private Tilemap tilemapSuelo;
    // La "ranura" para el Tilemap de Muros que está en tu Hierarchy
    [SerializeField] private Tilemap tilemapMuros;

    [Header("Assets de Tiles (Arrastra desde Assets/Tiles)")]
    // La "ranura" para el asset del Tile de Suelo (.asset)
    [SerializeField] private TileBase tileSuelo;
    // La "ranura" para el asset del Tile de Muro (.asset)
    [SerializeField] private TileBase tileMuro;

    [Header("Configuración de Sala")]
    [SerializeField] private int minAncho = 10; // Ancho mínimo de la sala
    [SerializeField] private int maxAncho = 20; // Ancho máximo
    [SerializeField] private int minAlto = 10;  // Alto mínimo
    [SerializeField] private int maxAlto = 15;  // Alto máximo

    // Propiedad pública para que el EnemyManager sepa los límites de la sala
    public BoundsInt SalaGeneradaBounds { get; private set; }

    private void Awake()
    {
        // Configuración del Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Evita duplicados si accidentalmente pones dos
        }
    }

    // Función principal que el WaveManager llamará
    public void GenerateRoom()
    {
        // 1. Limpiamos cualquier tile de la sala anterior
        tilemapSuelo.ClearAllTiles();
        tilemapMuros.ClearAllTiles();

        // 2. Decidimos el tamaño aleatorio de la nueva sala
        int ancho = Random.Range(minAncho, maxAncho);
        int alto = Random.Range(minAlto, maxAlto);

        // 3. Calculamos las coordenadas para centrar la sala en (0,0)
        int xMin = -ancho / 2;
        int xMax = ancho / 2;
        int yMin = -alto / 2;
        int yMax = alto / 2;

        // Guardamos los límites calculados para que el EnemyManager los use
        SalaGeneradaBounds = new BoundsInt(xMin, yMin, 0, ancho, alto, 1);

        // 4. Bucle para "pintar" la sala con código
        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                // Posición actual en la cuadrícula del Tilemap
                Vector3Int pos = new Vector3Int(x, y, 0);

                // Comprobamos si estamos en el borde de la sala
                if (x == xMin || x == xMax || y == yMin || y == yMax)
                {
                    // Si es borde, pintamos un "Muro" en el Tilemap de Muros
                    tilemapMuros.SetTile(pos, tileMuro);
                }
                else // Si no es borde, es interior
                {
                    // Pintamos "Suelo" en el Tilemap de Suelo
                    tilemapSuelo.SetTile(pos, tileSuelo);
                }
            }
        }
        Debug.Log($"Sala generada: Ancho={ancho}, Alto={alto}"); // Mensaje para consola
    }
}