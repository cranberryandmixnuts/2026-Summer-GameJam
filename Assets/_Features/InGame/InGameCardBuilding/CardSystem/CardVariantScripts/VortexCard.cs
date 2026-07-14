using System.Collections.Generic;
using UnityEngine;

public class VortexCard : Card {

	//======================================================================| Fields

	[SerializeField]
	private float[] _additionalDamageOverSlots;

	[SerializeField]
	private float[] _additionalMultiplierOverSlots;

	[Header("Animation")]
	[SerializeField]
	private float _slotRadius;

	[SerializeField]
	private float _spiningSpeed;

	private readonly List<SpecialCardSlot> _slots = new();

	//======================================================================| Properties

	private int SlotCount => Mathf.Min(_additionalDamageOverSlots.Length, _additionalMultiplierOverSlots.Length);
	public override IReadOnlyList<SpecialCardSlot> SpecialCardSlots => _slots;

	//======================================================================| Unity Methods

	private void Start() {

		for (int i = 0; i < SlotCount; i++) {
			_slots.Add(SpecialCardSlot.NewSlot(this, i));
		}

	}

	protected override void Update() {

		base.Update();

		for (int i = 0; i < SlotCount; i++) {

			var factor = Time.time * _spiningSpeed;
			factor += 2 * Mathf.PI * i / SlotCount;

			var angle = new Vector2(
				Mathf.Cos(factor),
				Mathf.Sin(factor)
			);

			_slots[i].transform.localPosition = _slotRadius * angle;
			_slots[i].transform.localEulerAngles = _slots[i].transform.localEulerAngles.WithZ(
				Vector2.SignedAngle(Vector2.up, angle)
			);

		}

	}

	//======================================================================| Methods

	public override float CalculateDamage() {
		return base.CalculateDamage() + _additionalDamageOverSlots[_cardOnSpecialSlot.Count];
	}

	public override float CalculateAdditionalMultiplier() {
		return base.CalculateAdditionalMultiplier() + _additionalMultiplierOverSlots[_cardOnSpecialSlot.Count];
	}

}
