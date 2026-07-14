using UnityEngine;

[CreateAssetMenu(
	fileName = nameof(EffectVisualSettings),
	menuName = "Cards/" + nameof(EffectVisualSettings)
)]

public class EffectVisualSettings : ScriptableObject {

	[Header("Fire")]
	public Material FireMaterial;

}