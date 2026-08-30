using SDE.Core;
using System;
using System.Collections.Generic;

namespace SDE.Databases.Pets.Features {
	public class Pet : ICloneable {
		public string AegisName;
		public string DisplayName;
		public string TameItem;
		public string EggItem;
		public string EquipItem;
		public string FoodItem;
		public string Fullness;
		public string HungryDelay = "60";
		public string HungerIncrease = "20";
		public string IntimacyStart = "250";
		public string IntimacyFed = "50";
		public string IntimacyOverfed = "-100";
		public string IntimacyHungry = "-5";
		public string IntimacyOwnerDie = "-20";
		public string CaptureRate;
		public string Speed;
		public bool SpecialPerformance = true;
		public bool DisablePetTalk = false;
		public string AttackRate;
		public string RetaliateRate;
		public string ChangeTargetRate;
		public bool AllowAutoFeed;
		public string Script;
		public string SupportScript;
		public List<Evolution> Evolutions = new List<Evolution>();

		public object Clone() {
			Pet pet = (Pet)MemberwiseClone();

			pet.Evolutions = new List<Evolution>();

			foreach (var evolution in Evolutions)
				pet.Evolutions.Add((Evolution)evolution.Clone());

			return pet;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			return StructuralComparer<Pet>.Equals(this, (Pet)obj);
		}

		public override int GetHashCode() {
			return StructuralComparer<Pet>.GetHashCode(this);
		}
	}
}
