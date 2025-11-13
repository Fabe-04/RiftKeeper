using System.Collections.Generic;
using UnityEngine;

public class GunManager : MonoBehaviour
{
    // --- NUEVO: Convertir a Singleton ---
    public static GunManager Instance;
    // --- FIN NUEVO ---

    [SerializeField] GameObject gunPrefab;
    
    // --- NUEVO: Lista pública de armas ---
    [HideInInspector]
    public List<Gun> activeGuns = new List<Gun>();
    // --- FIN NUEVO ---

    Transform player;
    List<Vector2> gunPositions = new List<Vector2>();

    int spawnedGuns = 0;

    // --- NUEVO: Awake para Singleton ---
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    // --- FIN NUEVO ---

    private void Start()
    {
        player = GameObject.Find("Player").transform;

        gunPositions.Add(new Vector2(-0.7f, -0.15f));
        gunPositions.Add(new Vector2(0.7f, -0.15f));

        AddGun();
    }


    void AddGun()
    {
        var pos = gunPositions[spawnedGuns];

        // Lo creamos en (0,0) y lo hacemos hijo del Player
        var newGun = Instantiate(gunPrefab, player.position, Quaternion.identity, player);
        // Luego ajustamos su posición local RELATIVA al player
        newGun.transform.localPosition = pos;

        // --- NUEVO: Añadir el script del arma a la lista ---
        Gun newGunScript = newGun.GetComponent<Gun>();
        newGunScript.SetOffset(pos);
        activeGuns.Add(newGunScript);
        // --- FIN NUEVO ---

        spawnedGuns++;
    }
}