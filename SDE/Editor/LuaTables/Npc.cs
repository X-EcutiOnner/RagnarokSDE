namespace SDE.Editor.LuaTables {
	public struct Npc {
		public string IngameSprite;
		public string NpcName;

		public Npc(Npc npc) {
			NpcName = npc.NpcName;
			IngameSprite = npc.IngameSprite;
		}
	}
}