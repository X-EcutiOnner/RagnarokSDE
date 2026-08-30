namespace SDE.Editor.Database.Commands {
	public interface IGenericDbCommand {
		string CommandDescription { get; }
		void Execute();
		void Undo();
	}
}