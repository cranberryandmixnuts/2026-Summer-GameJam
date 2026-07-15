using System;
using System.Collections;
using UnityEngine;

public class DrawTimer : SingletonBehaviour<DrawTimer, SceneScope> {

	//======================================================================| Fields

	[SerializeField]
	private float _drawCooldown;

	private Coroutine _drawTimingRoutine;

	//======================================================================| Properties

	public float DrawCooldown => _drawCooldown;
	public float CurrentDrawCooldown { get; private set; }
	public float DrawCooldownTimeScale { get; private set; } = 1f;

	public float DrawCooldownRate => CurrentDrawCooldown / _drawCooldown;

	//======================================================================| Event

	public static event Action OnDrawTiming;

	//======================================================================| Unity Methods

	public void OnEnable() => CardBuildingManager.OnRestarted += Restart;
	public void OnDisable() => CardBuildingManager.OnRestarted -= Restart;

	//======================================================================| Methods

	public void Restart() {

		CurrentDrawCooldown = _drawCooldown;

		if (_drawTimingRoutine != null) StopCoroutine(_drawTimingRoutine);
		_drawTimingRoutine = StartCoroutine(DrawTiming());

	}

	private IEnumerator DrawTiming() {
		
		while(true) {
		
			CurrentDrawCooldown -= Time.deltaTime * DrawCooldownTimeScale;
		
			if (CurrentDrawCooldown <= 0f) {
				CurrentDrawCooldown += DrawCooldown;
				OnDrawTiming?.Invoke();
			}

			yield return null;

		}

	}

}