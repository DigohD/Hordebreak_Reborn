using FNZ.Shared.Model.Interfaces;

namespace FNZ.Shared.Net
{

	public interface IComponentNetEventData : ISerializeable
	{
		int GetSizeInBytes();
	}
}