using UnityEngine;

namespace FNZ.LevelEditor
{
	public class ME_BlockingMaskButton : MonoBehaviour
	{
		public void OnClick()
		{
			ME_Chunk.ActivechunkMode = ME_Chunk.ChunkMode.BLOCKINGMASK;
			ME_Control.activeSnapMode = ME_Control.SnapMode.TILE;
			ME_Control.activePaintMode = ME_Control.PaintMode.BLOCKING_MASK;
		}
	}
}

