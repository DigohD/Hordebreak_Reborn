using FNZ.Shared.Model.Entity;
using Unity.Mathematics;

public struct FNERayCastHitStruct
{
	public bool IsHit;
	public float2 HitLocation;
	public FNEEntity HitEntity;

	public FNERayCastHitStruct(bool hit, float2 hitLocation, FNEEntity hitEntity)
	{
		IsHit = hit;
		HitLocation = hitLocation;
		HitEntity = hitEntity;
	}
}
