using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.LevelEditor.Code.LevelEditor 
{
	public class LevelEditorUtils
	{
		public static readonly string EDITOR_FILE_ENDiNG = ".fnl";
		public static readonly string SITE_FILE_ENDING = ".fns";

		public static void AddTileObjectEditorHitbox(GameObject go)
		{
			var hitbox = new GameObject();
			hitbox.transform.SetParent(go.transform);
			hitbox.transform.localPosition = Vector3.zero;
			hitbox.layer = LayerMask.NameToLayer("EditorCol");
			hitbox.tag = "Editor";
			var bc = hitbox.AddComponent<BoxCollider>();
			bc.center = Vector3.back * 0.5f;
			bc.size = Vector3.one;
		}

		public static void AddEdgeObjectEditorHitbox(GameObject go)
		{
			var hitbox = new GameObject();
			hitbox.transform.SetParent(go.transform);
			hitbox.transform.localPosition = Vector3.zero;
			hitbox.layer = LayerMask.NameToLayer("EditorCol");
			hitbox.tag = "Editor";
			var bc = hitbox.AddComponent<BoxCollider>();
			bc.center = Vector3.back;
			bc.size = new Vector3(1, 0.1f, 2);
		}
	}
}