using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator Instance;
    public enum RoomType { Start, Normal, Shop, Boss }

    [System.Serializable]
    public class RoomData
    {
        public Vector2Int centro;
        public RoomType tipo;
        public int ancho;
        public int alto;
        public RectInt areaOcupada;
    }

    [Header("Configuración Boss")]
    public Vector2Int coordenadasSalaJefe = new Vector2Int(1000, 1000); // Zona lejana
    public int tamanoSalaJefe = 15;

    [Header("Referencias")]
    [SerializeField] private Tilemap tilemapSuelo;
    [SerializeField] private Tilemap tilemapMuros;
    [SerializeField] private TileBase tileSuelo;
    [SerializeField] private TileBase tileMuro;
    [SerializeField] private GameObject roomLogicPrefab;

    [Header("Elementos Especiales")]
    [SerializeField] private GameObject cofreTutorialPrefab;

    [Header("Configuración Estricta")]
    [SerializeField] private int numeroDeSalas = 8;
    [SerializeField] private int anchoSalaMin = 10;
    [SerializeField] private int anchoSalaMax = 16;
    [SerializeField] private int altoSalaMin = 8;
    [SerializeField] private int altoSalaMax = 12;

    [Tooltip("Distancia mínima de pasillo entre salas")]
    [SerializeField] private int longitudPasillo = 5;
    [SerializeField] private int anchoPasillo = 3;

    public List<RoomData> roomList = new List<RoomData>();
    private HashSet<Vector2Int> sueloPositions = new HashSet<Vector2Int>();

    private readonly Vector2Int[] direcciones = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GenerarMazmorra();
        MoverJugadorASalaSegura();
    }

    public void GenerarMazmorra()
    {
        Limpiar(); // 1. Borra todo
        CrearSalaExitosa(Vector2Int.zero); // 2. Crear Start y Cofre

        int intentosFallidos = 0;
        int salasCreadas = 1;
        int maxIntentos = 100;

        while (salasCreadas < numeroDeSalas && intentosFallidos < maxIntentos)
        {
            RoomData salaOrigen = roomList[Random.Range(0, roomList.Count)];

            if (IntentarCrearSalaAdyacente(salaOrigen))
            {
                salasCreadas++;
                intentosFallidos = 0;
            }
            else
            {
                intentosFallidos++;
            }
        }

        PintarTiles();      // 3. Pinta la mazmorra normal

        // 4. Generar SOLO el terreno de la sala del Jefe (Sin spawnear al enemigo aún)
        GenerarSalaJefeAislada();

        AsignarRoles();     // 5. Asigna lógica a las salas normales
        InstalarLogica();
    }

    // --- CORRECCIÓN CLAVE: FUNCIÓN PÚBLICA PARA LA PUERTA ---
    public Vector3 ObtenerCentroJefeWorld()
    {
        // Esto le permite a la Puerta saber exactamente dónde aterrizar
        if (tilemapSuelo != null)
        {
            return tilemapSuelo.GetCellCenterWorld((Vector3Int)coordenadasSalaJefe);
        }
        return new Vector3(coordenadasSalaJefe.x, coordenadasSalaJefe.y, 0);
    }
    // --------------------------------------------------------

    private bool IntentarCrearSalaAdyacente(RoomData origen)
    {
        Vector2Int dir = direcciones[Random.Range(0, direcciones.Length)];
        int w = Random.Range(anchoSalaMin, anchoSalaMax);
        int h = Random.Range(altoSalaMin, altoSalaMax);
        if (w % 2 != 0) w++;
        if (h % 2 != 0) h++;

        int distanciaX = (origen.ancho / 2) + (w / 2) + longitudPasillo;
        int distanciaY = (origen.alto / 2) + (h / 2) + longitudPasillo;
        Vector2Int desplazamiento = new Vector2Int(dir.x * distanciaX, dir.y * distanciaY);
        Vector2Int nuevoCentro = origen.centro + desplazamiento;
        RectInt nuevaArea = new RectInt(nuevoCentro.x - w / 2 - 2, nuevoCentro.y - h / 2 - 2, w + 4, h + 4);

        foreach (var sala in roomList) if (sala.areaOcupada.Overlaps(nuevaArea)) return false;

        RoomData nuevaSala = new RoomData
        {
            centro = nuevoCentro,
            ancho = w,
            alto = h,
            areaOcupada = new RectInt(nuevoCentro.x - w / 2, nuevoCentro.y - h / 2, w, h),
            tipo = RoomType.Normal
        };

        for (int x = -w / 2; x <= w / 2; x++)
            for (int y = -h / 2; y <= h / 2; y++)
                sueloPositions.Add(nuevoCentro + new Vector2Int(x, y));

        GenerarPasilloEstricto(origen.centro, nuevoCentro, anchoPasillo);
        roomList.Add(nuevaSala);
        return true;
    }

    private void CrearSalaExitosa(Vector2Int centro)
    {
        int w = Random.Range(anchoSalaMin, anchoSalaMax);
        int h = Random.Range(altoSalaMin, altoSalaMax);
        if (w % 2 != 0) w++;
        if (h % 2 != 0) h++;

        RoomData sala = new RoomData
        {
            centro = centro,
            ancho = w,
            alto = h,
            areaOcupada = new RectInt(centro.x - w / 2, centro.y - h / 2, w, h),
            tipo = RoomType.Start
        };

        for (int x = -w / 2; x <= w / 2; x++)
            for (int y = -h / 2; y <= h / 2; y++)
                sueloPositions.Add(centro + new Vector2Int(x, y));

        roomList.Add(sala);

        // --- INSTANCIAR COFRE TUTORIAL ---
        if (cofreTutorialPrefab != null && tilemapSuelo != null)
        {
            Vector3 posicionCentroMundo = tilemapSuelo.GetCellCenterWorld((Vector3Int)centro);
            // CORREGIDO A -1.5f: -4.5f es muy arriesgado en salas pequeñas, podría quedar dentro del muro sur.
            Vector3 posicionCofre = posicionCentroMundo + new Vector3(0, -7.5f, 0);

            GameObject cofre = Instantiate(cofreTutorialPrefab, posicionCofre, Quaternion.identity, transform);
            cofre.name = "Cofre_Tutorial";
        }
    }

    public void GenerarSalaJefeAislada()
    {
        // 1. Definir el área
        int w = tamanoSalaJefe;
        int h = tamanoSalaJefe;
        Vector2Int centro = coordenadasSalaJefe;

        // 2. Llenar suelo y muros (SOLO PINTAR, NO SPAWNEAR)
        for (int x = -w / 2 - 2; x <= w / 2 + 2; x++)
        {
            for (int y = -h / 2 - 2; y <= h / 2 + 2; y++)
            {
                Vector2Int pos = centro + new Vector2Int(x, y);

                bool esBorde = x <= -w / 2 || x >= w / 2 || y <= -h / 2 || y >= h / 2;

                if (esBorde) tilemapMuros.SetTile((Vector3Int)pos, tileMuro);
                else tilemapSuelo.SetTile((Vector3Int)pos, tileSuelo);
            }
        }

        // ELIMINADO: EnemyManager.Instance.SpawnBoss(centro);
        // Ahora el BossDoor se encargará de esto cuando entres.
    }

    private void GenerarPasilloEstricto(Vector2Int inicio, Vector2Int fin, int ancho)
    {
        Vector2Int actual = inicio;
        bool primeroX = Random.value > 0.5f;
        if (primeroX)
        {
            while (actual.x != fin.x) { actual.x += (int)Mathf.Sign(fin.x - actual.x); PintarBloquePasillo(actual, ancho); }
            while (actual.y != fin.y) { actual.y += (int)Mathf.Sign(fin.y - actual.y); PintarBloquePasillo(actual, ancho); }
        }
        else
        {
            while (actual.y != fin.y) { actual.y += (int)Mathf.Sign(fin.y - actual.y); PintarBloquePasillo(actual, ancho); }
            while (actual.x != fin.x) { actual.x += (int)Mathf.Sign(fin.x - actual.x); PintarBloquePasillo(actual, ancho); }
        }
    }

    private void PintarBloquePasillo(Vector2Int p, int grosor)
    {
        int radio = grosor / 2;
        for (int x = -radio; x <= radio; x++) for (int y = -radio; y <= radio; y++) sueloPositions.Add(p + new Vector2Int(x, y));
    }

    private void PintarTiles()
    {
        tilemapSuelo.ClearAllTiles(); tilemapMuros.ClearAllTiles();
        foreach (var pos in sueloPositions) tilemapSuelo.SetTile((Vector3Int)pos, tileSuelo);
        foreach (var pos in sueloPositions)
        {
            foreach (var dir in direcciones) if (!sueloPositions.Contains(pos + dir)) tilemapMuros.SetTile((Vector3Int)(pos + dir), tileMuro);
        }
    }

    private void AsignarRoles()
    {
        if (roomList.Count == 0) return;
        foreach (var r in roomList) r.tipo = RoomType.Normal;
        roomList[0].tipo = RoomType.Start;
        roomList[roomList.Count - 1].tipo = RoomType.Boss;
        if (roomList.Count > 2)
        {
            int intento = Random.Range(1, roomList.Count - 1);
            roomList[intento].tipo = RoomType.Shop;
        }
    }

    private void InstalarLogica()
    {
        List<GameObject> aBorrar = new List<GameObject>();
        foreach (Transform child in transform)
        {
            if (!child.name.Contains("Cofre")) aBorrar.Add(child.gameObject);
        }
        foreach (var obj in aBorrar) Destroy(obj);

        foreach (var sala in roomList)
        {
            Vector3 worldPos = tilemapSuelo.GetCellCenterWorld((Vector3Int)sala.centro);
            GameObject obj = Instantiate(roomLogicPrefab, worldPos, Quaternion.identity, transform);
            obj.name = $"Sala_{sala.tipo}";
            RoomBehaviour logic = obj.GetComponent<RoomBehaviour>();
            if (logic != null) logic.ConfigurarSalaInteligente(sala.tipo, sala.ancho, sala.alto, anchoPasillo, tilemapSuelo);
        }
    }

    private void Limpiar()
    {
        roomList.Clear(); sueloPositions.Clear();
        if (tilemapSuelo != null) tilemapSuelo.ClearAllTiles();
        if (tilemapMuros != null) tilemapMuros.ClearAllTiles();
        foreach (Transform child in transform) Destroy(child.gameObject);
    }

    private void MoverJugadorASalaSegura()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && roomList.Count > 0)
        {
            Vector3 startPos = tilemapSuelo.GetCellCenterWorld((Vector3Int)roomList[0].centro);
            player.transform.position = new Vector3(startPos.x, startPos.y, 0);
        }
    }

    public List<Vector2Int> CentrosDeSalas { get { List<Vector2Int> c = new List<Vector2Int>(); foreach (var r in roomList) c.Add(r.centro); return c; } }
}