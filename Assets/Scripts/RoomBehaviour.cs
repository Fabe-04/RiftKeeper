using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class RoomBehaviour : MonoBehaviour
{
    [Header("Estado")]
    public DungeonGenerator.RoomType tipoSala;
    public bool salaCompletada = true;
    private bool eventoActivo = false;

    [Header("Referencias Generales")]
    [SerializeField] private GameObject muroPrefab;
    [SerializeField] private float margenTrigger = 1.0f;
    [SerializeField] private int offsetMuros = 1;

    [Header("Configuración TIENDA y JEFE")]
    [SerializeField] private GameObject vendedorPrefab;
    [SerializeField] private GameObject puertaJefePrefab;

    [Header("Minimapa (Referencias Internas)")]
    [Tooltip("Arrastra el objeto hijo 'Icono_Mapa_Tienda' aquí")]
    [SerializeField] private GameObject iconoTiendaRef;
    [Tooltip("Arrastra el objeto hijo 'Icono_Mapa_Boss' aquí")]
    [SerializeField] private GameObject iconoBossRef;

    // Variable interna que recordará cuál de los dos usar en esta instancia
    private GameObject iconoMapaSeleccionado;

    [Header("Rotaciones Visuales")]
    [SerializeField] private float rotArriba = 0f;
    [SerializeField] private float rotAbajo = 0f;
    [SerializeField] private float rotDerecha = 90f;
    [SerializeField] private float rotIzquierda = 90f;

    private List<GameObject> murosGenerados = new List<GameObject>();
    private List<Enemy> enemigosEnSala = new List<Enemy>();
    private BoxCollider2D miTrigger;
    private int anchoLogico;
    private int altoLogico;
    private int ultimosEnemigosVivos = -1;

    private void Awake()
    {
        miTrigger = GetComponent<BoxCollider2D>();
        miTrigger.isTrigger = true;

        // 1. Asegurar que los iconos empiecen apagados al nacer
        if (iconoTiendaRef != null) iconoTiendaRef.SetActive(false);
        if (iconoBossRef != null) iconoBossRef.SetActive(false);
    }

    private void Update()
    {
        if (eventoActivo)
        {
            enemigosEnSala.RemoveAll(e => e == null || e.gameObject == null);
            int vivosActuales = enemigosEnSala.Count;

            if (vivosActuales != ultimosEnemigosVivos)
            {
                ultimosEnemigosVivos = vivosActuales;
                if (UIManager.Instance != null)
                    UIManager.Instance.ActualizarEnemigosRestantes(vivosActuales);
            }

            if (vivosActuales == 0) SalaLimpiada();
        }
    }

    public void ConfigurarSalaInteligente(DungeonGenerator.RoomType tipo, int w, int h, int anchoPasillo, Tilemap mapa)
    {
        tipoSala = tipo;
        anchoLogico = w;
        altoLogico = h;
        salaCompletada = (tipoSala != DungeonGenerator.RoomType.Normal);

        Vector3 cellSize = mapa.layoutGrid.cellSize;
        float sizeX = (w * cellSize.x) - margenTrigger;
        float sizeY = (h * cellSize.y) - margenTrigger;
        miTrigger.size = new Vector2(sizeX, sizeY);
        miTrigger.offset = Vector2.zero;

        // Reiniciar selección
        iconoMapaSeleccionado = null;

        switch (tipoSala)
        {
            case DungeonGenerator.RoomType.Shop:
                GenerarTienda();
                iconoMapaSeleccionado = iconoTiendaRef; // Asignamos Tienda
                break;

            case DungeonGenerator.RoomType.Boss:
                GenerarPuertaJefe();
                iconoMapaSeleccionado = iconoBossRef; // Asignamos Boss
                break;

            case DungeonGenerator.RoomType.Normal:
                if (!salaCompletada) GenerarMurosRobustos(w, h, anchoPasillo, mapa);
                break;

                // Start no necesita icono especial según tu petición
        }
    }

    // Métodos de Generación
    private void GenerarTienda() { if (vendedorPrefab != null) Instantiate(vendedorPrefab, transform.position, Quaternion.identity, transform); }
    private void GenerarPuertaJefe() { if (puertaJefePrefab != null) Instantiate(puertaJefePrefab, transform.position, Quaternion.identity, transform); }

    private void GenerarMurosRobustos(int w, int h, int anchoPasillo, Tilemap mapa)
    {
        foreach (var m in murosGenerados) if (m != null) Destroy(m);
        murosGenerados.Clear();

        Vector3Int centro = mapa.WorldToCell(transform.position);
        int dx = (w / 2) + offsetMuros;
        int dy = (h / 2) + offsetMuros;

        var direcciones = new (Vector3Int offset, float rot, Vector3 esc, bool lateral)[]
        {
            (new Vector3Int(0, dy, 0), rotArriba, new Vector3(anchoPasillo, 1, 1), false),
            (new Vector3Int(0, -dy, 0), rotAbajo, new Vector3(anchoPasillo, 1, 1), false),
            (new Vector3Int(dx, 0, 0), rotDerecha, new Vector3(anchoPasillo, 1, 1), true),
            (new Vector3Int(-dx, 0, 0), rotIzquierda, new Vector3(anchoPasillo, 1, 1), true)
        };

        foreach (var (offset, rot, esc, lateral) in direcciones)
        {
            Vector3Int targetCell = centro + offset;
            if (CheckPasillo(mapa, targetCell, lateral))
            {
                Vector3 pos = mapa.GetCellCenterWorld(targetCell);
                CrearMuro(pos, Quaternion.Euler(0, 0, rot), esc);
            }
        }
    }

    private bool CheckPasillo(Tilemap mapa, Vector3Int centro, bool lateral)
    {
        if (mapa.HasTile(centro)) return true;
        Vector3Int v1 = lateral ? Vector3Int.up : Vector3Int.left;
        Vector3Int v2 = lateral ? Vector3Int.down : Vector3Int.right;
        return mapa.HasTile(centro + v1) || mapa.HasTile(centro + v2);
    }

    void CrearMuro(Vector3 pos, Quaternion rot, Vector3 esc)
    {
        if (muroPrefab == null) return;
        GameObject muro = Instantiate(muroPrefab, transform);
        muro.transform.position = pos;
        muro.transform.rotation = rot;
        muro.transform.localScale = esc;
        muro.SetActive(false);
        murosGenerados.Add(muro);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. REVELAR ICONO DEL MAPA (Si existe uno asignado para este tipo de sala)
            if (iconoMapaSeleccionado != null && !iconoMapaSeleccionado.activeSelf)
            {
                iconoMapaSeleccionado.SetActive(true);
            }

            // 2. LOGICA DE COMBATE
            if (!salaCompletada && !eventoActivo && tipoSala != DungeonGenerator.RoomType.Start)
            {
                EmpezarCombate();
            }
        }
    }

    void EmpezarCombate()
    {
        eventoActivo = true;
        ToggleMuros(true);
        if (EnemyManager.Instance != null)
            enemigosEnSala = EnemyManager.Instance.SpawnEnemiesInRoom(transform.position, anchoLogico, altoLogico);

        ultimosEnemigosVivos = enemigosEnSala.Count;
        if (UIManager.Instance != null) UIManager.Instance.ActivarCombateUI(ultimosEnemigosVivos);
    }

    void SalaLimpiada()
    {
        salaCompletada = true;
        eventoActivo = false;
        ToggleMuros(false);
        if (UIManager.Instance != null) UIManager.Instance.DesactivarCombateUI();
    }

    void ToggleMuros(bool estado)
    {
        foreach (var m in murosGenerados) if (m != null) m.SetActive(estado);
    }
}