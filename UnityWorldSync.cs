using UnityEngine;
using System.IO;
using GameStudio.Core;

namespace GameStudio.Bridge
{
    [System.Serializable]
    public class WorldObjectData
    {
        public string name;
        public string type; // "Castle", "Mine", "Unit"
        public Vector3 position;
        public string race;
    }

    [System.Serializable]
    public class WorldLayout
    {
        public WorldObjectData[] objects;
    }

    // Module 6: Interactive Bridge (Export/Import)
    public class WorldSyncManager : MonoBehaviour
    {
        public string jsonPath = "Assets/world_layout.json";
        public RaceData[] racePrototypes;

        public void SyncFromBlender()
        {
            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);
                WorldLayout layout = JsonUtility.FromJson<WorldLayout>(json);
                SpawnWorld(layout);
            }
        }

        void SpawnWorld(WorldLayout layout)
        {
            foreach (var obj in layout.objects)
            {
                GameObject prefab = GetPrefabByType(obj.type, obj.race);
                if (prefab != null)
                {
                    Instantiate(prefab, obj.position, Quaternion.identity);
                }
            }
        }

        GameObject GetPrefabByType(string type, string raceName)
        {
            // Logic to find matching prefab in RaceData
            return null; // Placeholder
        }
    }
}
