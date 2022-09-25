namespace FNZ.Shared.Model.Entity.Components
{

	public interface IInteractableComponent
	{
		void OnPlayerInRange();
		void OnPlayerExitRange();
		void OnInteract();

		string InteractionPromptMessageRef();
		bool IsInteractable();
	}
}