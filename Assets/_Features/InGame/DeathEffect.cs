using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DeathEffect : BaseBehaviour {

	public Volume _volume;
	public Vignette _vignette;
	private float _daethValue = 0f;

	private void Awake() {
		if (_volume.profile.TryGet<Vignette>(out var vignette)) {

			_vignette = vignette;

		}
	}

	private void OnEnable() {
		CombatBridge.PlayerDamaged += Instance_PlayerDamaged;
	}

	private void OnDisable() {
		CombatBridge.PlayerDamaged -= Instance_PlayerDamaged;
	}

	private void Instance_PlayerDamaged(float range) => _daethValue = 1f - range;

	public void Update() {

		_vignette.color.value = Color.Lerp(Color.black, Color.red, _daethValue);
		_vignette.smoothness.value = Mathf.Lerp(0.3f, 1f, _daethValue);

	}

}