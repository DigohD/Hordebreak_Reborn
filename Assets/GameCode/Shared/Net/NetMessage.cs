using Lidgren.Network;

namespace FNZ.Shared.Net
{
	public struct NetMessage
	{
		public NetOutgoingMessage Buffer;
		public NetMessageType Type;
		public NetDeliveryMethod DeliveryMethod;
		public SequenceChannel Channel;
	}
}

