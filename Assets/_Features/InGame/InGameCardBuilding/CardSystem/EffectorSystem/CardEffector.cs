public class CardEffect {

	//======================================================================| Properties

	public bool IsPoison { get; protected set; } = false;
    public int FireLevel { get; protected set; } = 0;
	public int WaterLevel { get; protected set; } = 0;
	public int ElectricLevel { get; protected set;  } = 0;
	public int HealLevel { get; protected set; } = 0;

    public float SizeMultiplier { get; protected set; } = 0f;
	public float SpeedMutliplier { get; protected set; } = 0f;

	public float AdditionalMultiplier { get; protected set; } = 0f;

	//======================================================================| Operators

	public static CardEffect operator+(CardEffect left, CardEffect right) {
		return new CardEffect() {

			IsPoison = left.IsPoison || right.IsPoison,
            FireLevel = left.FireLevel + right.FireLevel,
			WaterLevel = left.WaterLevel + right.WaterLevel,
			ElectricLevel = left.ElectricLevel + right.ElectricLevel,
			HealLevel = left.HealLevel + right.HealLevel,

            SizeMultiplier = left.SizeMultiplier * right.SizeMultiplier,
			SpeedMutliplier = left.SpeedMutliplier * right.SpeedMutliplier,

			AdditionalMultiplier = left.AdditionalMultiplier + right.AdditionalMultiplier

		};
	}

}