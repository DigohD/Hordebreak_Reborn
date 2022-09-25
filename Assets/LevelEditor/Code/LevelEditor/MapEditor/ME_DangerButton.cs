using UnityEngine;
namespace FNZ.LevelEditor
{
	public class ME_DangerButton : MonoBehaviour
	{

		public short dangerLevel;

		public void OnClick()
		{
			ME_Chunk.ActivechunkMode = ME_Chunk.ChunkMode.DANGERMAP;
			ME_Control.activeSnapMode = ME_Control.SnapMode.TILE;
			ME_Control.activePaintMode = ME_Control.PaintMode.DANGER_LEVEL;
			ME_Control.SelectedDangerLevel = (byte)dangerLevel;
		}
	}
}