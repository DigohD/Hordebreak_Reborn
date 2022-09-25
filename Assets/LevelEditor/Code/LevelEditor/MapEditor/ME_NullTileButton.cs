using FNZ.LevelEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.LevelEditor.Code.LevelEditor.MapEditor 
{

	public class ME_NullTileButton : MonoBehaviour
	{
		public void OnClick()
		{
			ME_Chunk.ActivechunkMode = ME_Chunk.ChunkMode.FLOOR;
			ME_Control.activeSnapMode = ME_Control.SnapMode.TILE;
			ME_Control.activePaintMode = ME_Control.PaintMode.NULL_TILE;
		}
	}
}