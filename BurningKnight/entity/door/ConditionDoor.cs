using System;
using BurningKnight.entity.creature.npc;
using BurningKnight.save;
using ImGuiNET;
using Lens;
using Lens.util.file;

namespace BurningKnight.entity.door {
	public class ConditionDoor : LockableDoor {
		private static string[] conditions = {
			"Played Once",
			"Saved Hat Trader",
			"Saved Weapon Trader",
			"Saved Artifact Trader",
			"Saved Active Trader",
			"Save Boss Rush Guy",
			"Completed 10 Challenges",
			"Completed 20 Challenges",
			"Completed 30 Challenges"
		};

		private bool shouldLock;
		private bool cached;
		private int condition;

		private bool DecideState() {
			switch (condition) {
				case 0: return GlobalSave.IsTrue("played_once");
				case 1: return GlobalSave.IsTrue(ShopNpc.HatTrader);
				case 2: return GlobalSave.IsTrue(ShopNpc.WeaponTrader);
				case 3: return GlobalSave.IsTrue(ShopNpc.AccessoryTrader);
				case 4: return GlobalSave.IsTrue(ShopNpc.ActiveTrader);
				case 5: return GlobalSave.IsTrue(ShopNpc.Mike);
				case 6: return GlobalSave.GetInt("challenges_completed") >= 10;
				case 7: return GlobalSave.GetInt("challenges_completed") >= 20;
				case 8: return GlobalSave.GetInt("challenges_completed") >= 30;
			}

			return false;
		}
		
		public bool ShouldLock() {
			if (!cached) {
				shouldLock = !DecideState();
				cached = true;
			}

			return shouldLock;
		}

		public override void PostInit() {
			SkipLock = false;
			Replaced = false;
			base.PostInit();

			if (!Vertical) {
				CenterX = (float) (Math.Round(CenterX / 16) * 16) + 8;
			}
		}
		
		protected override Lock CreateLock() {
			//Replaced = false;
			return /*Engine.EditingLevel ? null : */new ConditionLock();
		}

		public override void RenderImDebug() {
			base.RenderImDebug();
			ImGui.Combo("Condition", ref condition, conditions, conditions.Length);
		}

		public override void Load(FileReader stream) {
			base.Load(stream);
			condition = stream.ReadByte();
		}

		public override void Save(FileWriter stream) {
			base.Save(stream);
			stream.WriteByte((byte) condition);
		}
	}
}