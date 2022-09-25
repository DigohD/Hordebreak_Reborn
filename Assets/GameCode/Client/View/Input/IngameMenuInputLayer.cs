using FNZ.Client.View.UI.Manager;
using UnityEngine;

namespace FNZ.Client.View.Input
{
	public class IngameMenuInputLayer : InputLayer
	{
		public IngameMenuInputLayer() : base(false)
		{
			IsUIBlockingMouse = true;
		}

		protected override void AddActionMappings()
		{
			AddActionMapping("CloseAll", KeyCode.Escape);
		}

		protected override void BindActions()
		{
			BindAction("CloseAll", InputActionType.PRESS, OnCloseAll);
		}

		private void OnCloseAll()
		{
			UIManager.Instance.CloseAllActiveUI();
		}

		public override void OnActivated()
		{
			base.OnActivated();
		}

		public override void OnDeactivated()
		{
			base.OnDeactivated();
		}
	}
}