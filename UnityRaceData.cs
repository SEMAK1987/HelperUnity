using UnityEngine;
using System.Collections.Generic;

namespace GameStudio.Core
{
    // Module 4: Army & Hero Hierarchy (Data-oriented)
    [CreateAssetMenu(fileName = "RaceData", menuName = "ContinentOfFate/RaceData")]
    public class RaceData : ScriptableObject
    {
        public string raceName;
        public Color raceColor;
        public Sprite raceIcon;
        public GameObject castlePrefab;
        public GameObject warriorPrefab;
        public GameObject archerPrefab;
        public GameObject magePrefab;
        
        [Header("Stats")]
        public float baseHealth = 100f;
        public float baseAttack = 15f;
        public float baseDefense = 10f;
    }

    public class HeroStats
    {
        public string heroName;
        public bool isMainHero;
        public RaceData race;
        public int level;
        
        // Passive bonuses from support heroes
        public float GetSupportBonus(List<HeroStats> supportHeroes)
        {
            float bonus = 0;
            foreach (var hero in supportHeroes)
            {
                if (!hero.isMainHero) bonus += 2.5f; // Flat bonus example
            }
            return bonus;
        }
    }
}
