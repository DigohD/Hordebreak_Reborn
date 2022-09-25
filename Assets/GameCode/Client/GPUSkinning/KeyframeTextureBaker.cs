using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FNZ.Client.GPUSkinning 
{
	public class BakedData
	{
		public AnimationTextures AnimationTextures;
		public Mesh NewMesh;
		public LodData Lods;
		public float FrameRate;

		public List<AnimationClipData> Animations = new List<AnimationClipData>();
		public Dictionary<string, AnimationClipData> AnimationsDictionary;
	}

	public class AnimationClipData
	{
		public AnimationClip Clip;
		public int PixelStart;
		public int PixelEnd;
	}

	public static class KeyframeTextureBaker
	{
		public static BakedData BakeClips(GameObject animationRoot, AnimationClip[] animationClips, float frameRate, LodData lods)
		{
			var wasActive = animationRoot.activeSelf;
			animationRoot.SetActive(false);
			var instance = Object.Instantiate(animationRoot, Vector3.zero, Quaternion.identity);
			animationRoot.SetActive(wasActive);

			instance.transform.localScale = Vector3.one;
			var skinRenderer = instance.GetComponentInChildren<SkinnedMeshRenderer>();

			var bakedData = new BakedData
			{
				NewMesh = CreateMesh(skinRenderer)
			};

			var lod1Mesh = CreateMesh(skinRenderer, lods.Lod1Mesh);
			var lod2Mesh = CreateMesh(skinRenderer, lods.Lod2Mesh);
			var lod3Mesh = CreateMesh(skinRenderer, lods.Lod3Mesh);

			bakedData.Lods = new LodData(lod1Mesh, lod2Mesh, lod3Mesh, lods.Lod1Distance, lods.Lod2Distance, lods.Lod3Distance);
			bakedData.FrameRate = frameRate;

			var sampledBoneMatrices = new List<Matrix4x4[,]>(animationClips.Length);

			var numberOfKeyFrames = 0;

			for (var i = 0; i < animationClips.Length; i++)
			{
				var clip = animationClips[i];
				if (clip == null)
				{
					sampledBoneMatrices.Add(null);
					continue;
				}
				var sampledMatrix = SampleAnimationClip(instance, clip, skinRenderer, bakedData.FrameRate);
				sampledBoneMatrices.Add(sampledMatrix);
				numberOfKeyFrames += sampledMatrix.GetLength(0);
			}

			var sbm = sampledBoneMatrices.Find(x => x != null);
			var numberOfBones = sbm.GetLength(1);

			var tex0 = bakedData.AnimationTextures.Animation0 = new Texture2D(numberOfKeyFrames, numberOfBones, TextureFormat.RGBAFloat, false);
			tex0.wrapMode = TextureWrapMode.Clamp;
			tex0.filterMode = FilterMode.Point;
			tex0.anisoLevel = 0;

			var tex1 = bakedData.AnimationTextures.Animation1 = new Texture2D(numberOfKeyFrames, numberOfBones, TextureFormat.RGBAFloat, false);
			tex1.wrapMode = TextureWrapMode.Clamp;
			tex1.filterMode = FilterMode.Point;
			tex1.anisoLevel = 0;

			var tex2 = bakedData.AnimationTextures.Animation2 = new Texture2D(numberOfKeyFrames, numberOfBones, TextureFormat.RGBAFloat, false);
			tex2.wrapMode = TextureWrapMode.Clamp;
			tex2.filterMode = FilterMode.Point;
			tex2.anisoLevel = 0;

			var texture0Color = new Color[tex0.width * tex0.height];
			var texture1Color = new Color[tex0.width * tex0.height];
			var texture2Color = new Color[tex0.width * tex0.height];

			var runningTotalNumberOfKeyframes = 0;

			for (var i = 0; i < sampledBoneMatrices.Count; i++)
			{
				if (animationClips[i] == null)
				{
					bakedData.Animations.Add(null);
					continue;
				}
				
				for (var boneIndex = 0; boneIndex < sampledBoneMatrices[i].GetLength(1); boneIndex++)
				{
					for (var keyframeIndex = 0; keyframeIndex < sampledBoneMatrices[i].GetLength(0); keyframeIndex++)
					{
						var index = Get1DCoord(runningTotalNumberOfKeyframes + keyframeIndex, boneIndex, tex0.width);

						texture0Color[index] = sampledBoneMatrices[i][keyframeIndex, boneIndex].GetRow(0);
						texture1Color[index] = sampledBoneMatrices[i][keyframeIndex, boneIndex].GetRow(1);
						texture2Color[index] = sampledBoneMatrices[i][keyframeIndex, boneIndex].GetRow(2);
					}
				}

				var clipData = new AnimationClipData
				{
					Clip = animationClips[i],
					PixelStart = runningTotalNumberOfKeyframes + 1,
					PixelEnd = runningTotalNumberOfKeyframes + sampledBoneMatrices[i].GetLength(0) - 1
				};

				if (clipData.Clip.wrapMode == WrapMode.Default) clipData.PixelEnd -= 1;

				bakedData.Animations.Add(clipData);

				runningTotalNumberOfKeyframes += sampledBoneMatrices[i].GetLength(0);
			}

			tex0.SetPixels(texture0Color);
			tex1.SetPixels(texture1Color);
			tex2.SetPixels(texture2Color);

			runningTotalNumberOfKeyframes = 0;

			for (var i = 0; i < sampledBoneMatrices.Count; i++)
			{
				if (animationClips[i] == null) continue;
				
				for (var boneIndex = 0; boneIndex < sampledBoneMatrices[i].GetLength(1); boneIndex++)
				{
					for (var keyframeIndex = 0; keyframeIndex < sampledBoneMatrices[i].GetLength(0); keyframeIndex++)
					{
						var pixel0 = tex0.GetPixel(runningTotalNumberOfKeyframes + keyframeIndex, boneIndex);
						var pixel1 = tex1.GetPixel(runningTotalNumberOfKeyframes + keyframeIndex, boneIndex);
						var pixel2 = tex2.GetPixel(runningTotalNumberOfKeyframes + keyframeIndex, boneIndex);

						if ((Vector4)pixel0 != sampledBoneMatrices[i][keyframeIndex, boneIndex].GetRow(0))
						{
							Debug.LogError("Error at (" + (runningTotalNumberOfKeyframes + keyframeIndex) + ", " + boneIndex + ") expected " + Format(sampledBoneMatrices[i][keyframeIndex, boneIndex].GetRow(0)) + " but got " + Format(pixel0));
						}
						if ((Vector4)pixel1 != sampledBoneMatrices[i][keyframeIndex, boneIndex].GetRow(1))
						{
							Debug.LogError("Error at (" + (runningTotalNumberOfKeyframes + keyframeIndex) + ", " + boneIndex + ") expected " + Format(sampledBoneMatrices[i][keyframeIndex, boneIndex].GetRow(1)) + " but got " + Format(pixel1));
						}
						if ((Vector4)pixel2 != sampledBoneMatrices[i][keyframeIndex, boneIndex].GetRow(2))
						{
							Debug.LogError("Error at (" + (runningTotalNumberOfKeyframes + keyframeIndex) + ", " + boneIndex + ") expected " + Format(sampledBoneMatrices[i][keyframeIndex, boneIndex].GetRow(2)) + " but got " + Format(pixel2));
						}
					}
				}

				runningTotalNumberOfKeyframes += sampledBoneMatrices[i].GetLength(0);
			}

			tex0.Apply(false, true);
			tex1.Apply(false, true);
			tex2.Apply(false, true);

			Object.DestroyImmediate(instance);

			return bakedData;
		}

		private static string Format(Vector4 v)
		{
			return "(" + v.x + ", " + v.y + ", " + v.z + ", " + v.w + ")";
		}

		private static string Format(Color v)
		{
			return "(" + v.r + ", " + v.g + ", " + v.b + ", " + v.a + ")";
		}

		private static Mesh CreateMesh(SkinnedMeshRenderer originalRenderer, Mesh mesh = null)
		{
			var newMesh = new Mesh();
			var originalMesh = mesh == null ? originalRenderer.sharedMesh : mesh;
			var boneWeights = originalMesh.boneWeights;

			originalMesh.CopyMeshData(newMesh);

			var vertices = originalMesh.vertices;
			var boneIds = new Vector2[originalMesh.vertexCount];
			var boneInfluences = new Vector2[originalMesh.vertexCount];

			int[] boneRemapping = null;

			if (mesh != null)
			{
				var originalBindPoseMatrices = originalRenderer.sharedMesh.bindposes;
				var newBindPoseMatrices = mesh.bindposes;

				if (newBindPoseMatrices.Length != originalBindPoseMatrices.Length)
				{
					Debug.LogError(mesh.name + " - Invalid bind poses, got " + newBindPoseMatrices.Length + ", but expected "
									+ originalBindPoseMatrices.Length);
				}
				else
				{
					boneRemapping = new int[originalBindPoseMatrices.Length];
					for (var i = 0; i < boneRemapping.Length; i++)
					{
						boneRemapping[i] = Array.FindIndex(originalBindPoseMatrices, x => x == newBindPoseMatrices[i]);
					}
				}
			}

			var bones = originalRenderer.bones;

			for (var i = 0; i < originalMesh.vertexCount; i++)
			{
				var boneIndex0 = boneWeights[i].boneIndex0;
				var boneIndex1 = boneWeights[i].boneIndex1;

				if (boneRemapping != null)
				{
					boneIndex0 = boneRemapping[boneIndex0];
					boneIndex1 = boneRemapping[boneIndex1];
				}

				boneIds[i] = new Vector2((boneIndex0 + 0.5f) / bones.Length, (boneIndex1 + 0.5f) / bones.Length);

				var mostInfluentialBonesWeight = boneWeights[i].weight0 + boneWeights[i].weight1;

				boneInfluences[i] = new Vector2(boneWeights[i].weight0 / mostInfluentialBonesWeight, boneWeights[i].weight1 / mostInfluentialBonesWeight);
			}

			newMesh.vertices = vertices;
			newMesh.uv2 = boneIds;
			newMesh.uv3 = boneInfluences;

			return newMesh;
		}

		private static Matrix4x4[,] SampleAnimationClip(GameObject root, AnimationClip clip, SkinnedMeshRenderer renderer, float framerate)
		{
			var bindPoses = renderer.sharedMesh.bindposes;
			var bones = renderer.bones;
			var boneMatrices = new Matrix4x4[Mathf.CeilToInt(framerate * clip.length) + 3, bones.Length];

			for (var i = 1; i < boneMatrices.GetLength(0) - 1; i++)
			{
				var t = (float)(i - 1) / (boneMatrices.GetLength(0) - 3);

				var oldWrapMode = clip.wrapMode;
				clip.wrapMode = WrapMode.Clamp;
				clip.SampleAnimation(root, t * clip.length);
				clip.wrapMode = oldWrapMode;

				for (var j = 0; j < bones.Length; j++)
					boneMatrices[i, j] = bones[j].localToWorldMatrix * bindPoses[j];
			}

			for (var j = 0; j < bones.Length; j++)
			{
				boneMatrices[0, j] = boneMatrices[boneMatrices.GetLength(0) - 2, j];
				boneMatrices[boneMatrices.GetLength(0) - 1, j] = boneMatrices[1, j];
			}

			return boneMatrices;
		}

		private static void CopyMeshData(this Mesh originalMesh, Mesh newMesh)
		{
			var vertices = originalMesh.vertices;
			newMesh.vertices = vertices;
			newMesh.triangles = originalMesh.triangles;
			newMesh.normals = originalMesh.normals;
			newMesh.uv = originalMesh.uv;
			newMesh.tangents = originalMesh.tangents;
			newMesh.name = originalMesh.name;
		}

		private static int Get1DCoord(int x, int y, int width)
		{
			return y * width + x;
		}
	}
}