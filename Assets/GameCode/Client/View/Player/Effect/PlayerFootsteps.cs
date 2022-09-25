using FNZ.Client.View.Audio;
using FNZ.Shared.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View.Player.Effect
{

	public class PlayerFootsteps : MonoBehaviour
	{

		public Transform LeftFoot;
		public Transform RightFoot;

		public void LeftStep()
		{
			var pos = new float2(LeftFoot.transform.position.x, LeftFoot.transform.position.z);

			GameClient.ViewAPI.SpawnVFX("vfx_foot_step", pos, 0, 0);
			switch (FNERandom.GetRandomIntInRange(0, 4))
			{
				case 0:
					AudioManager.Instance.PlaySfx3dClip("sfx_step_rock_1", pos);
					break;

				case 1:
					AudioManager.Instance.PlaySfx3dClip("sfx_step_rock_2", pos);
					break;

				case 2:
					AudioManager.Instance.PlaySfx3dClip("sfx_step_rock_3", pos);
					break;

				case 3:
					AudioManager.Instance.PlaySfx3dClip("sfx_step_rock_4", pos);
					break;
			}
		}

		public void RightStep()
		{
			var pos = new float2(RightFoot.transform.position.x, RightFoot.transform.position.z);

			GameClient.ViewAPI.SpawnVFX("vfx_foot_step", pos, 0, 0);
			switch (FNERandom.GetRandomIntInRange(0, 4))
			{
				case 0:
					AudioManager.Instance.PlaySfx3dClip("sfx_step_rock_1", pos);
					break;

				case 1:
					AudioManager.Instance.PlaySfx3dClip("sfx_step_rock_2", pos);
					break;

				case 2:
					AudioManager.Instance.PlaySfx3dClip("sfx_step_rock_3", pos);
					break;

				case 3:
					AudioManager.Instance.PlaySfx3dClip("sfx_step_rock_4", pos);
					break;
			}
		}
	}
}