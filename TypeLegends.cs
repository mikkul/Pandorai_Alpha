using System;
using System.Collections.Generic;
using Pandorai.Conditions;
using Pandorai.Creatures.Behaviours;
using Pandorai.Effects;
using Pandorai.Structures.Behaviours;

namespace Pandorai
{
    public static class TypeLegends
    {
        public static Dictionary<string, Type> Effects = new Dictionary<string, Type>
		{
			{ "ModifyHP", typeof(ModifyHP) },
			{ "CastFireball", typeof(CastFireball) },
			{ "CastWave", typeof(CastWave) },
			{ "SpawnSpikes", typeof(SpawnSpikes) },
			{ "ModifySpeed", typeof(ModifySpeed) },
			{ "StealthEquipableItem", typeof(StealthEquipableItem) },
			{ "TransformSpell", typeof(TransformSpell) },
			{ "ModifyFireResistance", typeof(ModifyFireResistance) },
			{ "ModifyMaxHP", typeof(ModifyMaxHP) },
			{ "ModifyMaxMana", typeof(ModifyMaxMana) },
			{ "ModifySkillPoints", typeof(ModifySkillPoints) },
			{ "CastIceProjectile", typeof(CastIceProjectile) },
			{ "ModifyStrength", typeof(ModifyStrength) },
			{ "ModifyMana", typeof(ModifyMana) },
			{ "Blink", typeof(Blink) },
			{ "Slingshot", typeof(Slingshot) },
			{ "SummonCreature", typeof(SummonCreature) },
		};

        
		public static Dictionary<string, Type> StructureBehaviours = new Dictionary<string, Type>
		{
			{ "Container", typeof(Container) },
			{ "Armor", typeof(Armor) },
			{ "Destructible", typeof(Destructible) },
			{ "PandoraiPedestal", typeof(PandoraiPedestal) },
			{ "LightEmitter", typeof(LightEmitter) },
			{ "Door", typeof(Door) },
			{ "Dialogue", typeof(Dialogue) },
			{ "Teleporter", typeof(Teleporter) },
			{ "BookStand", typeof(BookStand) },
		};

        public static Dictionary<string, Type> CreatureBehaviours = new Dictionary<string, Type>
		{
			{ "AggroOnDistance", typeof(AggroOnDistance) },
			{ "ChaseTarget", typeof(ChaseTarget) },
			{ "ParalelPosition", typeof(ParalelPosition) },
			{ "ProjectileSpellCaster", typeof(ProjectileSpellCaster) },
			{ "NormalHitResponse", typeof(NormalHitResponse) },
			{ "Talkative", typeof(Talkative) },
			{ "NormalVision", typeof(NormalVision) },
			{ "AllSeeingVision", typeof(AllSeeingVision) },
			{ "AggroOnVision", typeof(AggroOnVision) },
			{ "Awakening", typeof(Awakening) },
			{ "RandomWalk", typeof(RandomWalk) },
			{ "PlaceWebs", typeof(PlaceWebs) },
			{ "SummonSpellCaster", typeof(SummonSpellCaster) },
		};

        public static Dictionary<string, Type> Conditions = new Dictionary<string, Type>
		{
			{ "RequiredSkillPoints", typeof(RequiredSkillPointsCondition) },
		};		
    }
}