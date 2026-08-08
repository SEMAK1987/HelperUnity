using UnityEngine;
using System;
using System.Collections.Generic;

namespace FateContinent
{
    [CreateAssetMenu(fileName = "BalanceConfig", menuName = "FateContinent/Balance Config", order = 1)]
    public class BalanceConfig : ScriptableObject
    {
        [Header("📈 Формулы роста")]
        public FormulaData HeroGrowth;
        public FormulaData CastleEconomy;
        public FormulaData CombatInitiative;

        [Header("🦸 Герои")]
        public HeroBalance[] Heroes;

        [Header("🏰 Замки")]
        public CastleBalance[] Castles;

        [Header("🤖 Сложность")]
        public DifficultyBalance[] Difficulties;

        [Header("🌊 Резонанс/Диссонанс")]
        public ResonanceRule[] ResonanceRules;

        [Header("👹 Монстры и Нейтралы")]
        public MonsterBalance[] Monsters;

        [System.Serializable]
        public struct FormulaData
        {
            [Tooltip("XP = Base * Lvl^Power")] public float XP_Base; public float XP_Power;
            [Tooltip("HP += Base + Lvl * Mult")] public float Stat_HP_Base; public float Stat_HP_Mult;
            public float Stat_MP_Base; public float Stat_MP_Mult;
            public float Stat_ATK_Base; public float Stat_ATK_Mult;
            public float Stat_DEF_Base; public float Stat_DEF_Mult;
            public float Stat_SPD_Base; public float Stat_SPD_Mult;
            public float Stat_LCK_Base; public float Stat_LCK_Mult;
        }

        [System.Serializable]
        public struct HeroBalance
        {
            public string ID; 
            public string Name; 
            public string Type; // "Premium" или "Basic"
            public int HP, MP, ATK, DEF, SPD, LCK;
            public float GrowthMult;
            public string[] Passives;
            public string SuperSkill; 
            public int SuperCooldown; 
            public float SuperPower;
        }

        [System.Serializable]
        public struct MonsterBalance
        {
            public string ID;
            public string Name;
            public string Faction; // например: "Empire", "Bandits", "Clans"
            public string Grade; // "Minion", "Elite", "Boss"
            public int HP, MP, ATK, DEF, SPD, LCK;
            public int LootXP;
            public int LootGold;
            public string CustomSkill;
            public float CustomSkillPower;
        }

        [System.Serializable]
        public struct CastleBalance
        {
            public int Level;
            public int GoldPerTurn, UpgradeCost, UpgradeTime;
            public int MaxUnitTier;
            public int[] AllowedContinents;
        }

        [System.Serializable]
        public struct DifficultyBalance
        {
            public string LevelName;
            public float Aggression, Defense, EconMod;
            public int ForecastTurns;
            public float AIGoldBonus, PlayerSkipBonus;
        }

        [System.Serializable]
        public struct ResonanceRule
        {
            public int MinUnits, MaxUnits;
            public string EffectType;
            public float StatMod, DissonanceChance, VisualIntensity;
        }
    }
}
