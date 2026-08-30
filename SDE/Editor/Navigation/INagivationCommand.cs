namespace SDE.Editor.Navigation {
	public interface INagivationCommand {
		string CommandDescription { get; }
		void Execute(TabNavigation navEngine);
		void Undo(TabNavigation navEngine);
	}
}