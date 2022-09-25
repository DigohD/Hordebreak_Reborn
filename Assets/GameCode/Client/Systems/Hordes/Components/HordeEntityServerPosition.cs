using Unity.Entities;
using Unity.Mathematics;

namespace FNZ.Client.Systems.Hordes.Components 
{
    public struct NetworkIdComponent : IComponentData
    {
        public int NetId;
    }

    public struct HordeEntityServerPosition : IComponentData
    {
        public float2 TargetPosition;
    }
    
    public struct HordeEntityAttackTargetPosition : IComponentData
    {
        public float2 TargetPosition;
    }
    
    public struct HordeEntityStatsData : IComponentData
    {
        public float AttackRange;
    }
}