using UnityEngine;
using System;
using System.Collections.Generic;

namespace FateContinent
{
    public static class EventHub
    {
        // === ЖИЗНЕННЫЙ ЦИКЛ БОЯ ===
        public static event Action<List<UnitBase>> OnCombatStart;
        public static event Action<int> OnRoundStart;
        public static event Action<int> OnRoundEnd;
        public static event Action<int> OnCombatEnd; // Победитель (0 - Игрок, 1 - ИИ, -1 - ничья)

        // === ФАЗЫ ===
        public static event Action<PhaseCombatSystem.CombatPhase> OnPhaseChange;
        public static event Action<float> OnPlanningTimeUpdate;

        // === КОМАНДЫ ===
        public static event Action<CombatCommand> OnCommandAdded;
        public static event Action<CombatCommand> OnCommandExecute;
        public static event Action<UnitBase, UnitBase, float, float> OnClash; // Атакующий A, Защищающийся B, урон A, урон B
        public static event Action<UnitBase, UnitBase, float> OnAttackHit;
        public static event Action<UnitBase, SkillData> OnSkillUsed;
        public static event Action<UnitBase> OnDefend;
        public static event Action<UnitBase> OnReaction; // Реакция (лечение/защита)

        // === СОСТОЯНИЕ ЮНИТОВ ===
        public static event Action<UnitBase, float> OnDamageTaken;
        public static event Action<UnitBase> OnUnitDeath;
        public static event Action<UnitBase, int> OnLevelUp;
        public static event Action<UnitBase> OnDissonanceTrigger;

        // === UI/СИСТЕМНЫЕ СОБЫТИЯ ===
        public static event Action<string> OnLogMessage;
        public static event Action<float, Color> OnResonanceVisualUpdate; // интенсивность резонанса, цвет фракции

        // === БЕЗОПАСНЫЕ ВЫЗОВЫ (ЗАЩИТА ОТ NULL REFERENCE EXCEPTION) ===
        public static void InvokeCombatStart(List<UnitBase> units) => OnCombatStart?.Invoke(units);
        public static void InvokeRoundStart(int r) => OnRoundStart?.Invoke(r);
        public static void InvokeRoundEnd(int r) => OnRoundEnd?.Invoke(r);
        public static void InvokeCombatEnd(int winnerF) => OnCombatEnd?.Invoke(winnerF);
        public static void InvokePhaseChange(PhaseCombatSystem.CombatPhase p) => OnPhaseChange?.Invoke(p);
        public static void InvokePlanningTime(float t) => OnPlanningTimeUpdate?.Invoke(t);
        public static void InvokeCommandAdd(CombatCommand c) => OnCommandAdded?.Invoke(c);
        public static void InvokeCommandExec(CombatCommand c) => OnCommandExecute?.Invoke(c);
        public static void InvokeClash(UnitBase a, UnitBase b, float da, float db) => OnClash?.Invoke(a, b, da, db);
        public static void InvokeAttack(UnitBase a, UnitBase b, float d) => OnAttackHit?.Invoke(a, b, d);
        public static void InvokeSkill(UnitBase u, SkillData s) => OnSkillUsed?.Invoke(u, s);
        public static void InvokeDefend(UnitBase u) => OnDefend?.Invoke(u);
        public static void InvokeReaction(UnitBase u) => OnReaction?.Invoke(u);
        public static void InvokeDamage(UnitBase u, float d) => OnDamageTaken?.Invoke(u, d);
        public static void InvokeDeath(UnitBase u) => OnUnitDeath?.Invoke(u);
        public static void InvokeLevelUp(UnitBase u, int lvl) => OnLevelUp?.Invoke(u, lvl);
        public static void InvokeDissonance(UnitBase u) => OnDissonanceTrigger?.Invoke(u);
        public static void InvokeLog(string msg) => OnLogMessage?.Invoke(msg);
        public static void InvokeResonance(float i, Color c) => OnResonanceVisualUpdate?.Invoke(i, c);

        // === ОЧИСТКА ВСЕХ СОБЫТИЙ (ОБЯЗАТЕЛЬНО ПРИ СМЕНЕ СЦЕНЫ ИЛИ ВЫХОДЕ В МЕНЮ) ===
        public static void ClearAll()
        {
            OnCombatStart = null;
            OnRoundStart = null;
            OnRoundEnd = null;
            OnCombatEnd = null;
            OnPhaseChange = null;
            OnPlanningTimeUpdate = null;
            OnCommandAdded = null;
            OnCommandExecute = null;
            OnClash = null;
            OnAttackHit = null;
            OnSkillUsed = null;
            OnDefend = null;
            OnReaction = null;
            OnDamageTaken = null;
            OnUnitDeath = null;
            OnLevelUp = null;
            OnDissonanceTrigger = null;
            OnLogMessage = null;
            OnResonanceVisualUpdate = null;
        }
    }
}
