using Lidgren.Network;
using System.Collections.Generic;
using Unity.Mathematics;

namespace FNZ.Shared.Model.Entity.Components.Enemy 
{

	public class EnemyComponentShared : FNEComponent
	{
		new public EnemyComponentData m_Data
		{
			get
			{
				return (EnemyComponentData)base.m_Data;
			}
		}

		public override void Init() { }

		public override void Serialize(NetBuffer bw)
		{
		}

		public override void Deserialize(NetBuffer br)
		{
		}

		public override ushort GetSizeInBytes()
		{
			return sizeof(int);
		}
	}
}