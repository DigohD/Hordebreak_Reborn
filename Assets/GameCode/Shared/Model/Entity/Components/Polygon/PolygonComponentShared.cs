using FNZ.Shared.Utils.CollisionUtils;
using Lidgren.Network;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Shared.Model.Entity.Components.Polygon
{
	public class PolygonComponentShared : FNEComponent
	{
		new public PolygonComponentData m_Data
		{
			get
			{
				return (PolygonComponentData)base.m_Data;
			}
		}

		public List<float2> relativePoints = new List<float2>();
		public float2 offsetToMiddle;

		public FNEPolygon GetWorldPolygon()
		{
			return GetWorldPolygon(Vector2.zero);
		}

		public FNEPolygon GetWorldPolygon(float2 posChange, float rotationChange = 0)
		{
			FNEPolygon polygon = new FNEPolygon();

			foreach (var point in relativePoints)
			{
				polygon.Points.Add(FNECollisionUtils.RotateVector(point, ParentEntity.RotationDegrees + rotationChange) + ParentEntity.Position + offsetToMiddle + posChange);
			}

			polygon.BuildEdges();
			return polygon;
		}

		public override void Init() { }

		public override void Serialize(NetBuffer bw) { }

		public override void Deserialize(NetBuffer br) { }

		public override ushort GetSizeInBytes() { return 0; }
	}
}
