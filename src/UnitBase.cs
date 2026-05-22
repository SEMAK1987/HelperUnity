using UnityEngine;

namespace FateContinent
{
    public class UnitBase : MonoBehaviour
    {
        [Header("Основная информация")]
        public string UnitName;
        public int Faction; // 0 - Игрок, 1 - ИИ

        [Header("Базовые характеристики (Ур. 1)")]
        public int BaseHP;
        public int BaseMP;
        public int BaseATK;
        public int BaseDEF;
        public int BaseSPD;
        public int BaseLCK;

        [Header("Текущий уровень и прогресс")]
        public int Level = 1;
        public int XP;

        [Header("Текущие динамические показатели в бою")]
        public float CurrentHP;
        public float CurrentMP;
        
        [HideInInspector] public float MaxHP;
        [HideInInspector] public int Def; // Динамическая защита (с баффами)
        [HideInInspector] public int BaseDefStatic; // Статическая защита для сброса модов

        public void Awake()
        {
            InitializeStats();
        }

        public void InitializeStats()
        {
            // Базовая инициализация. Значения могут рассчитываться по формулам из BalanceConfig
            MaxHP = BaseHP;
            CurrentHP = MaxHP;
            CurrentMP = BaseMP;
            Def = BaseDEF;
            BaseDefStatic = BaseDEF;
        }

        // Свойство SPD для инициативы
        public float SPD => BaseSPD;
        // Свойство LCK для шанса крита
        public float LCK => BaseLCK;
    }
}
