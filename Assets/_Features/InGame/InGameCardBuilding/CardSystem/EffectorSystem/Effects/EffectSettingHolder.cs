using UnityEngine;

public class EffectSettingHolder : SingletonBehaviour<EffectSettingHolder, SceneScope> {

	[SerializeField]
	private EffectVisualSettings _setting;

	public static EffectVisualSettings Setting => Instance._setting;

}