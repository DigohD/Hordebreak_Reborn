using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Health
{
	public class StatComponentShared : FNEComponent
	{
		public DefenseTypeData DefenseTypeData;

		public StatComponentData Data
		{
			get
			{
				return (StatComponentData)base.m_Data;
			}
		}

		public string defenseTypeRefOverride;

		public float MaxHealth;
		public float CurrentHealth;

		public float Armor;

		public float MaxShields;
		public float CurrentShields;
		public float ShieldsRegeneration;

		public override void Init()
		{
			defenseTypeRefOverride = string.Empty;

			MaxHealth = Data.startHealth;
			CurrentHealth = MaxHealth;

			Armor = Data.startArmor;

			MaxShields = Data.startShields;
			CurrentShields = MaxShields;

			ShieldsRegeneration = Data.startShieldsRegeneration;
			if (ShieldsRegeneration <= 5)
			{
				ShieldsRegeneration = 5;
			}

			DefenseTypeData = DataBank.Instance.GetData<DefenseTypeData>(Data.defenseTypeRef);
		}

		public float GetHealthPercentage()
		{
			return CurrentHealth / Data.startHealth;
		}

		public bool SetDefenseTypeOverride(string defenseTypeRef)
		{
			if (DataBank.Instance.IsIdOfType<DefenseTypeData>(defenseTypeRef))
			{
				defenseTypeRefOverride = defenseTypeRef;

				return true;
			}

			return false;
		}

		public DefenseTypeData GetDefenseTypeOverride()
		{
			return DataBank.Instance.GetData<DefenseTypeData>(defenseTypeRefOverride);
		}

		public override void Serialize(NetBuffer bw)
		{
			bw.Write(MaxHealth);
			bw.Write(CurrentHealth);

			bw.Write(Armor);

			bw.Write(MaxShields);
			bw.Write(CurrentShields);
		}

		public override void Deserialize(NetBuffer br)
		{
			MaxHealth = br.ReadFloat();
			CurrentHealth = br.ReadFloat();

			Armor = br.ReadFloat();

			MaxShields = br.ReadFloat();
			CurrentShields = br.ReadFloat();
		}

		public override ushort GetSizeInBytes()
		{
			return 20;
		}
	}
}
