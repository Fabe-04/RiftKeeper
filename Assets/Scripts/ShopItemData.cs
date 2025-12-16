using UnityEngine;

[System.Serializable]
public class ShopItemData
{
	public string nombreItem;
	public int precio;
	public Sprite icono;

	[Header("Ajustes Visuales del Icono")]
	public Vector3 escalaIcono = Vector3.one; // Por defecto (1, 1, 1)
	public Vector2 posicionIcono = Vector2.zero; // Por defecto (0, 0) al centro

	[Header("Lógica")]
	public TipoEfecto tipo;
	public int valorEfecto;

	public enum TipoEfecto
	{
		Curacion,
		Flechas,
		UpgradeVida,
		UpgradeDaño,
		ComprarArco
	}
}