using FNZ.Client.Model.Entity.Components.EquipmentSystem;
using FNZ.Client.Model.Entity.Components.Excavator;
using FNZ.Client.Utils;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.Enemy;
using FNZ.Shared.Model.Entity.Components.EnemyMesh;
using FNZ.Shared.Model.Entity.Components.EquipmentSystem;
using FNZ.Shared.Model.Entity.Components.Excavator;
using FNZ.Shared.Model.Entity.Components.PlayerMesh;
using FNZ.Shared.Model.Entity.Components.PlayerViewSetup;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using Siccity.GLTFUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FNZ.Client.View.EnemySwapMesh
{
	public class EnemySwapMesh : MonoBehaviour
	{

		private FNEEntity m_EnemyEntity;

		[SerializeField] private SkinnedMeshRenderer mainRenderer;

		[SerializeField] private Transform m_AccessoryAnchor;
		[SerializeField] private MeshRenderer accessoryRenderer;

		void Start()
		{
			// If enemy entity is null, we are probably in start menu screen
			if (m_EnemyEntity == null)
				return;
		}

		public void Init(FNEEntity enemyEntity)
		{
			m_EnemyEntity = enemyEntity;
		}

		// public void SetDefaultMeshes(EnemyComponentData enemyData)
		// {
		// 	if (enemyData != null)
		// 	{
		// 		var meshData = DataBank.Instance.GetData<FNEEntityMeshData>(enemyData.enemyMeshRef);
		// 		var enemyRenderer = GetSkinnedMeshRendererFromPath(meshData.MeshPath);
		// 		enemyRenderer.sharedMaterial = MaterialUtils.CreateMaterialFromTextureData(meshData, false);
		// 		SetNewSkinnedMesh(enemyRenderer);
		//
		// 		var newAccessoryData = DataBank.Instance.GetData<FNEEntityMeshData>(enemyData.accessoryMeshRef);
		// 		var newAccessoryRenderer = GetMeshRendererFromPath(newAccessoryData.MeshPath);
		// 		newAccessoryRenderer.sharedMaterial = MaterialUtils.CreateMaterialFromMeshData(newAccessoryData, false);
		// 		SetNewStaticMesh(newAccessoryRenderer);
		// 	}
		// 	else
		// 	{
		// 		Debug.LogError("Could not load 'enemyData'");
		// 	}
		// }


		private SkinnedMeshRenderer GetSkinnedMeshRendererFromPath(string path)
		{
			//Don't really NEED to do this but it keeps things nice and clean in the Hierarchy
			GameObject temp = Importer.LoadFromFile($"{Application.streamingAssetsPath}/{path}");
			if (temp == null)
				return null;

			var renderer = temp.GetComponentInChildren<SkinnedMeshRenderer>();

			Destroy(temp);
			return renderer;
		}

		private MeshRenderer GetMeshRendererFromPath(string path)
		{
			//Don't really NEED to do this but it keeps things nice and clean in the Hierarchy
			GameObject temp = Importer.LoadFromFile($"{Application.streamingAssetsPath}/{path}");
			if (temp == null)
				return null;

			var renderer = temp.GetComponentInChildren<MeshRenderer>();
			//var mesh = temp.GetComponentInChildren<MeshFilter>().mesh;
			renderer.GetComponent<MeshFilter>().sharedMesh = temp.GetComponentInChildren<MeshFilter>().mesh;

			Destroy(temp);
			//return new Tuple<MeshRenderer, Mesh>(renderer, mesh);
			return renderer;
		}


		private void SetNewSkinnedMesh(SkinnedMeshRenderer newRenderer)
		{
			if (newRenderer == null)
				return;

			ProcessRenderer(mainRenderer, newRenderer);
			mainRenderer.sharedMesh = newRenderer.sharedMesh;
			mainRenderer.sharedMaterial = newRenderer.sharedMaterial;
		}

		private void SetNewStaticMesh(MeshRenderer newRenderer)
		{
			if (newRenderer == null)
				return;

			accessoryRenderer.GetComponent<MeshFilter>().sharedMesh = newRenderer.GetComponent<MeshFilter>().sharedMesh;
			accessoryRenderer.sharedMaterial = newRenderer.sharedMaterial;
		}


		/// <summary>
		/// Process NewRenderer to sort the order of the bones according to the ReferenceRenderer and adjust boneweight and bindposes.
		/// </summary>
		/// <param name="referenceRenderer">Renderer that contains the correct order of bones</param>
		/// <param name="newRenderer">Renderer that contains the new Mesh</param>
		private void ProcessRenderer(SkinnedMeshRenderer referenceRenderer, SkinnedMeshRenderer newRenderer)
		{
			List<Transform> tListRenderer = newRenderer.bones.ToList();

			Dictionary<int, int> refMapping = new Dictionary<int, int>();

			for (int i = 0; i < referenceRenderer.bones.Length; i++)
			{
				Transform bone = Array.Find(newRenderer.bones, transform => transform.name == referenceRenderer.bones[i].name);
				refMapping[i] = tListRenderer.IndexOf(bone);
			}

			Transform[] newBoneOrder = new Transform[newRenderer.bones.Length];
			for (int i = 0; i < newRenderer.bones.Length; i++)
			{
				newBoneOrder[i] = newRenderer.bones[refMapping[i]];
			}
			List<Transform> newBoneOrderList = newBoneOrder.ToList();

			BoneWeight[] boneWeights = newRenderer.sharedMesh.boneWeights;
			for (int i = 0; i < boneWeights.Length; i++)
			{
				boneWeights[i].boneIndex0 = newBoneOrderList.IndexOf(newRenderer.bones[boneWeights[i].boneIndex0]);
				boneWeights[i].boneIndex1 = newBoneOrderList.IndexOf(newRenderer.bones[boneWeights[i].boneIndex1]);
				boneWeights[i].boneIndex2 = newBoneOrderList.IndexOf(newRenderer.bones[boneWeights[i].boneIndex2]);
				boneWeights[i].boneIndex3 = newBoneOrderList.IndexOf(newRenderer.bones[boneWeights[i].boneIndex3]);
			}

			Matrix4x4[] bindPoses = newRenderer.sharedMesh.bindposes;
			for (int i = 0; i < bindPoses.Length; i++)
			{
				bindPoses[i] = newRenderer.sharedMesh.bindposes[refMapping[i]];
			}

			newRenderer.bones = newBoneOrder;
			newRenderer.sharedMesh.boneWeights = boneWeights;
			newRenderer.sharedMesh.bindposes = bindPoses;
		}
	}
}