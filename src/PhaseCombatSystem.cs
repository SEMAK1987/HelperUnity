using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FateContinent
{
    public class PhaseCombatSystem : MonoBehaviour
    {
        public static PhaseCombatSystem Instance { get; private set; }
        
        public enum CombatPhase { Planning, Action, Reaction, End }
        public CombatPhase CurrentPhase { get; private set; }

        [Header("Настройки фаз")]
        [SerializeField] private float planningTime = 30f; // секунд
        [SerializeField] private int maxRounds = 10;

        [Header("Ссылки на ресурсы")]
        [SerializeField] private BalanceConfig balance; // ScriptableObject с формулами/порогами

        private List<CombatCommand> commandQueue = new();
        private List<UnitBase> aliveUnits = new();
        private int currentRound = 0;
        private float planningTimer = 0f;

        void Awake() 
        { 
            Instance = this; 
        }

        public void StartCombat(List<UnitBase> participants)
        {
            aliveUnits = participants.Where(u => u.CurrentHP > 0).ToList();
            currentRound = 0;
            NextPhase(CombatPhase.Planning);
            EventHub.InvokeCombatStart(aliveUnits);
        }

        void Update()
        {
            if (CurrentPhase != CombatPhase.Planning) return;

            planningTimer += Time.deltaTime;
            EventHub.InvokePlanningTime(planningTime - planningTimer);

            if (planningTimer >= planningTime || Input.GetKeyDown(KeyCode.Space))
            {
                StartActionPhase();
            }
        }

        // === ФАЗА 1: ПЛАНИРОВАНИЕ ===
        public void AddCommand(CombatCommand cmd)
        {
            if (CurrentPhase != CombatPhase.Planning) return;
            commandQueue.Add(cmd);
            EventHub.InvokeCommandAdd(cmd);
        }

        // === ФАЗА 2: ИСПОЛНЕНИЕ ===
        private void StartActionPhase()
        {
            CurrentPhase = CombatPhase.Action;
            EventHub.InvokePhaseChange(CombatPhase.Action);

            // Сортировка по инициативе (SPD * 1.2 + LCK * 0.1)
            commandQueue.Sort((a, b) =>
            {
                float initA = a.Unit.SPD * 1.2f + a.Unit.LCK * 0.1f;
                float initB = b.Unit.SPD * 1.2f + b.Unit.LCK * 0.1f;
                return initB.CompareTo(initA); // Сортируем от большего к меньшему
            });

            StartCoroutine(ExecuteCommands());
        }

        private System.Collections.IEnumerator ExecuteCommands()
        {
            for (int i = 0; i < commandQueue.Count; i++)
            {
                var cmd = commandQueue[i];
                if (cmd.Unit.CurrentHP <= 0) continue;

                EventHub.InvokeCommandExec(cmd);

                // Проверка встречного боя (clash)
                var clash = commandQueue.FirstOrDefault(c =>
                    c.Target == cmd.Unit && c.Type == CommandType.Attack && !c.Executed);

                if (clash != null && !clash.Executed)
                {
                    ResolveClash(cmd, clash);
                }
                else if (cmd.Type == CommandType.Attack)
                {
                    ResolveAttack(cmd);
                }
                else if (cmd.Type == CommandType.Skill)
                {
                    ResolveSkill(cmd);
                }
                else if (cmd.Type == CommandType.Defend)
                {
                    ResolveDefense(cmd);
                }

                cmd.Executed = true;
                yield return new WaitForSeconds(0.25f); // Визуальная задержка

                if (CheckBattleEnd()) yield break;
            }

            NextPhase(CombatPhase.Reaction);
        }

        // === ФАЗА 3: РЕАКЦИЯ ===
        private void StartReactionPhase()
        {
            CurrentPhase = CombatPhase.Reaction;
            EventHub.InvokePhaseChange(CombatPhase.Reaction);

            // Юниты, которые не атаковали, могут восстановиться / защититься
            var reactors = aliveUnits.Where(u => !commandQueue.Any(c => c.Unit == u && c.Type == CommandType.Attack)).ToList();
            foreach (var unit in reactors)
            {
                if (unit.CurrentHP <= 0) continue;
                // Простая реакция: регенерация HP (10% от показателя защиты)
                unit.CurrentHP = Mathf.Clamp(unit.CurrentHP + (unit.Def * 0.1f), 0, unit.MaxHP);
                EventHub.InvokeReaction(unit);
            }

            EndRound();
        }

        // === РАЗРЕШЕНИЕ КОНФЛИКТОВ ===
        private void ResolveClash(CombatCommand a, CombatCommand b)
        {
            float dmgA = CalculateDamage(a.Unit, b.Unit, true);
            float dmgB = CalculateDamage(b.Unit, a.Unit, true);

            ApplyDamage(b.Unit, dmgA);
            ApplyDamage(a.Unit, dmgB);

            EventHub.InvokeClash(a.Unit, b.Unit, dmgA, dmgB);
        }

        private void ResolveAttack(CombatCommand cmd)
        {
            float dmg = CalculateDamage(cmd.Unit, cmd.Target, false);
            ApplyDamage(cmd.Target, dmg);
            EventHub.InvokeAttack(cmd.Unit, cmd.Target, dmg);
        }

        private void ResolveSkill(CombatCommand cmd)
        {
            // Базовый урон навыка + модификатор силы героя
            float skillDmg = cmd.SkillData.Power * (1f + cmd.Unit.Level * 0.15f);
            ApplyDamage(cmd.Target, skillDmg);
            cmd.Unit.CurrentMP -= cmd.SkillData.ManaCost;
            EventHub.InvokeSkill(cmd.Unit, cmd.SkillData);
        }

        private void ResolveDefense(CombatCommand cmd)
        {
            cmd.Unit.Def += 5; // Временный бонус защиты
            EventHub.InvokeDefend(cmd.Unit);
        }

        // === ФОРМУЛЫ УРОНА ===
        private float CalculateDamage(UnitBase attacker, UnitBase target, bool isClash)
        {
            float baseDmg = attacker.BaseATK * (isClash ? 0.85f : 1.0f);
            float critChance = Mathf.Clamp01(attacker.LCK * 0.005f);
            bool isCrit = Random.value < critChance;
            
            float critMult = isCrit ? 1.8f * (1f + attacker.Level * 0.02f) : 1.0f;
            float reduction = target.Def / (target.Def + 50f + (target.Level * 2f));
            float final = baseDmg * critMult * (1f - reduction);

            return Mathf.Max(1f, Mathf.Floor(final));
        }

        private void ApplyDamage(UnitBase target, float dmg)
        {
            target.CurrentHP -= dmg;
            EventHub.InvokeDamage(target, dmg);
            if (target.CurrentHP <= 0)
            {
                target.CurrentHP = 0;
                EventHub.InvokeDeath(target);
            }
        }

        // === УПРАВЛЕНИЕ РАУНДОМ ===
        private void EndRound()
        {
            currentRound++;
            commandQueue.Clear();

            // Сброс временных модов защиты
            foreach (var u in aliveUnits)
            {
                u.Def = u.BaseDefStatic;
            }

            EventHub.InvokeRoundEnd(currentRound);

            if (currentRound >= maxRounds || CheckBattleEnd())
            {
                CurrentPhase = CombatPhase.End;
                EventHub.InvokeCombatEnd(GetWinner());
                return;
            }

            planningTimer = 0f;
            NextPhase(CombatPhase.Planning);
        }

        private bool CheckBattleEnd()
        {
            bool sideAAlive = aliveUnits.Any(u => u.Faction == 0 && u.CurrentHP > 0);
            bool sideBAlive = aliveUnits.Any(u => u.Faction == 1 && u.CurrentHP > 0);
            return !sideAAlive || !sideBAlive;
        }

        private int GetWinner()
        {
            bool sideA = aliveUnits.Any(u => u.Faction == 0 && u.CurrentHP > 0);
            bool sideB = aliveUnits.Any(u => u.Faction == 1 && u.CurrentHP > 0);
            if (sideA && !sideB) return 0; // Победитель - Игрок
            if (!sideA && sideB) return 1; // Победитель - ИИ
            return -1; // Ничья
        }

        private void NextPhase(CombatPhase phase)
        {
            CurrentPhase = phase;
            EventHub.InvokePhaseChange(phase);
            if (phase == CombatPhase.Reaction)
            {
                StartReactionPhase();
            }
        }
    }

    // === Вспомогательные классы ===

    [System.Serializable]
    public class CombatCommand
    {
        public UnitBase Unit;
        public UnitBase Target;
        public CommandType Type;
        public SkillData SkillData;
        public bool Executed;
    }

    public enum CommandType 
    { 
        Attack, 
        Skill, 
        Defend, 
        Retreat 
    }

    [System.Serializable]
    public class SkillData 
    { 
        public float Power; 
        public int ManaCost; 
        public string ID; 
    }
}
