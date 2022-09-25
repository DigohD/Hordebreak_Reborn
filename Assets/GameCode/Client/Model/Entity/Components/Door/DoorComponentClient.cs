using FNZ.Client.Model.Entity.Components.EdgeObject;
using FNZ.Client.Model.Entity.Components.Polygon;
using FNZ.Client.View.Audio;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Model.Entity.Components.Door;
using FNZ.Shared.Model.Entity.EntityViewData;
using Lidgren.Network;
using Siccity.GLTFUtility;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FNZ.Client.Model.Entity.Components.Door
{
	public class DoorComponentClient : DoorComponentShared, IInteractableComponent
	{
		private EdgeObjectComponentClient m_EdgeObjectComponent;
		private PolygonComponentClient m_PolygonComponent;
		private Animation animLegacy;
		private GameObject parentGO;
		private AnimatorOverrideController aoc;
		private AnimationClipOverrides clipOverrides;

		public override void Deserialize(NetBuffer br)
		{
            var wasOpen = IsOpen;

            base.Deserialize(br);

            if (parentGO == null)
				return;
			else if (m_EdgeObjectComponent == null || m_PolygonComponent == null || animLegacy == null)
				GetComponentReferences();			

            m_EdgeObjectComponent.IsSeethrough = IsOpen;
			m_EdgeObjectComponent.IsHittable = !IsOpen;
			m_EdgeObjectComponent.IsPassable = IsOpen;

			m_PolygonComponent.Enabled = !IsOpen;

			if (animLegacy != null && wasOpen != IsOpen)
			{
				if (IsOpen)
                {
                    PlayOpenEffects();
                }
                else
                {
                    PlayCloseEffects();
                }
			}
		}

		private void GetComponentReferences()
		{
			m_EdgeObjectComponent = ParentEntity.GetComponent<EdgeObjectComponentClient>();
			m_PolygonComponent = ParentEntity.GetComponent<PolygonComponentClient>();
        }

		public void OnInteract()
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(this, (byte)DoorNetEvent.DOOR_INTERACT);
		}

		public void OnPlayerExitRange()
		{ }

		public void OnPlayerInRange()
		{ }

        public void OnViewInit()
        {
            parentGO = GameClient.ViewConnector.GetGameObject(ParentEntity.NetId);

            InitAnimations();

            if (IsOpen)
                PlayOpenEffects();
            else
                PlayCloseEffects();
        }

        private void InitAnimations()
        {
            if (!parentGO.GetComponent<Animation>())
                animLegacy = parentGO.AddComponent<Animation>();
            else if (animLegacy == null)
                animLegacy = parentGO.GetComponent<Animation>();

            var rac = Resources.Load("Prefab/Entity/EdgeObject/Door/DoorController") as RuntimeAnimatorController;
            aoc = new AnimatorOverrideController(rac);
            clipOverrides = new AnimationClipOverrides(aoc.overridesCount);
            aoc.GetOverrides(clipOverrides);

            //Import animations from file
            var viewRef = DataBank.Instance.GetData<FNEEntityViewData>(ParentEntity.Data.entityViewVariations[0]);
            var animClips = new List<AnimationClip>();

            foreach (var animData in viewRef.animations)
            {
                GLTFAnimation.ImportResult[] importedAnimations;
                importedAnimations = Importer.LoadAnimationsFromFile($"{Application.streamingAssetsPath}/{animData.animPath}");

                if (importedAnimations == null)
                {
                    Debug.LogError($"Error: {Path.GetFileName(animData.animPath)} \n" + $"No animations found in object at {animData.animPath}.");
                    break;
                }

                foreach (var animation in importedAnimations)
                {
                    animClips.Add(animation.clip);
                }
            }

            //Add imported animations to override.
            foreach (var animation in animClips)
            {
                //These "ClipOverrides[-]" are the names of dummy animator animations.
                //Don't confuse them with the XML entries.
                if (animation.name.Equals(Data.closeAnimationName))
                    clipOverrides["DoorClose"] = animation;

                if (animation.name.Equals(Data.openAnimationName))
                    clipOverrides["DoorOpen"] = animation;
            }

            animLegacy.AddClip(clipOverrides["DoorOpen"], clipOverrides["DoorOpen"].name);
            animLegacy.AddClip(clipOverrides["DoorClose"], clipOverrides["DoorClose"].name);

            //Error handling
            foreach (var ovr in clipOverrides)
            {
                if (ovr.Value == null)
                {
                    var meshRef = DataBank.Instance.GetData<FNEEntityMeshData>(viewRef.entityMeshData);
                    switch (ovr.Key.name)
                    {
                        case "DoorOpen":
                            Debug.LogError(
                                $"Error: {Path.GetFileName($"{Application.streamingAssetsPath}/{meshRef.MeshPath}")} \n" + 
                                $"openAnimationName in DoorComponentData for {ParentEntity.Data.Id} did not return a valid animation. Make sure the name is correct and that the animation exists.");
                            break;

                        case "DoorClose":
	                        Debug.LogError(
                                $"Error: {Path.GetFileName($"{Application.streamingAssetsPath}/{meshRef.MeshPath}")} \n" +
                                $"closeAnimationName in DoorComponentData for {ParentEntity.Data.Id} did not return a valid animation.Make sure the name is correct and that the animation exists.");
                            break;

                        default:
                            Debug.LogError("This should never happen!");
                            break;
                    }
                }
            }
            PlayCloseEffects();
        }

        private void PlayOpenEffects()
        {
            animLegacy.Play(clipOverrides["DoorOpen"].name);
            if (!string.IsNullOrEmpty(Data.openSFXRef))
                AudioManager.Instance.PlaySfx3dClip(Data.openSFXRef, ParentEntity.Position + new Unity.Mathematics.float2(0.5f, 0.5f));
        }

        private void PlayCloseEffects()
        {
            animLegacy.Play(clipOverrides["DoorClose"].name);
            if (!string.IsNullOrEmpty(Data.closeSFXRef))
                AudioManager.Instance.PlaySfx3dClip(Data.closeSFXRef, ParentEntity.Position + new Unity.Mathematics.float2(0.5f, 0.5f));
        }

        public string InteractionPromptMessageRef()
		{
			return "door_component_interact";
		}

		public bool IsInteractable()
		{
			return true;
		}

	}

	public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
	{
		public AnimationClipOverrides(int capacity) : base(capacity) { }

		public AnimationClip this[string name]
		{
			get { return this.Find(x => x.Key.name.Equals(name)).Value; }
			set
			{
				int index = this.FindIndex(x => x.Key.name.Equals(name));
				if (index != -1)
					this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
			}
		}
	}
}
