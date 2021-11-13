namespace GameData {
	public class UnitType : TypeObject {
		public string Icon { get; set; }

		public static UnitType Warrior = new UnitType {
			Icon = "swordsman"
		};

        public UnitType() : base("UnitType") {}
    }
}