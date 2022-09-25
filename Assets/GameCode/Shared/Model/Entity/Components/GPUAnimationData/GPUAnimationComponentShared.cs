using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.GPUAnimationData
{
    public class GPUAnimationComponentShared : FNEComponent
    {
        public GPUAnimationComponentData Data => (GPUAnimationComponentData)base.m_Data;

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