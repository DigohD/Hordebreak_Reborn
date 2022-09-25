using FNZ.Shared.Model.Entity.Components.Health;
using Lidgren.Network;

namespace FNZ.Client.Model.Entity.Components.Health
{

	public delegate void OnHealthChange(float newHealth, float previousHealth, float maxHealth);
	public delegate void OnArmorChange(float newArmor);
	public delegate void OnShieldsChange(float newShields, float previousShields, float maxShields);

	public class StatComponentClient : StatComponentShared
	{
		public OnHealthChange d_HealthChange;
		public OnArmorChange d_ArmorChange;
		public OnShieldsChange d_ShieldsChange;

		public override void Deserialize(NetBuffer br)
		{
			float previousHealth = CurrentHealth;
			float previousShields = CurrentShields;

			base.Deserialize(br);

			d_HealthChange?.Invoke(CurrentHealth, previousHealth, MaxHealth);
			d_ShieldsChange?.Invoke(CurrentShields, previousShields, MaxShields);
			d_ArmorChange?.Invoke(Armor);
		}
	}
}