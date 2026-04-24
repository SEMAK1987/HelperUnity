using UnityEngine;

namespace GameStudio.Core
{
    // Module 5: Smart Camera & Boundaries
    public class SmartCameraController : MonoBehaviour
    {
        public Transform target;
        public Vector2 minBounds = new Vector2(-100, -100);
        public Vector2 maxBounds = new Vector2(100, 100);
        public float smoothSpeed = 0.125f;

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position;
            
            // Boundary Lock Module
            float clampedX = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            float clampedZ = Mathf.Clamp(desiredPosition.z, minBounds.y, maxBounds.y);
            
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, new Vector3(clampedX, transform.position.y, clampedZ), smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
    
    // Module 2: Castle Evolution logic
    public class CastleController : MonoBehaviour
    {
        public int currentLevel = 1;
        public GameObject[] levelMeshes; // 1-5 levels
        public ParticleSystem upgradeEffect;

        public void Upgrade()
        {
            if (currentLevel < 5)
            {
                currentLevel++;
                UpdateVisuals();
                if (upgradeEffect != null) upgradeEffect.Play();
            }
        }

        void UpdateVisuals()
        {
            for (int i = 0; i < levelMeshes.Length; i++)
            {
                levelMeshes[i].SetActive(i == currentLevel - 1);
            }
        }
    }

    // Module 3: Battle Zone Generator
    public class BattleZoneGenerator : MonoBehaviour
    {
        public int width = 10;
        public int height = 10;
        public GameObject cellPrefab;
        public float cellSize = 1.0f;

        public void GenerateZone(string terrainType)
        {
            // Clear existing
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }

            for (int x = 0; x < width; x++) {
                for (int z = 0; z < height; z++) {
                    GameObject cell = Instantiate(cellPrefab, new Vector3(x * cellSize, 0, z * cellSize), Quaternion.identity, transform);
                    cell.name = $"Cell_{x}_{z}_{terrainType}";
                    // Apply terrain specific visuals
                }
            }
        }
    }

    // Module 7: Magic & Alchemy Visuals
    public class MagicController : MonoBehaviour
    {
        public ParticleSystem healEffect;
        public ParticleSystem manaEffect;
        public ParticleSystem explosionEffect;

        public void PlayEffect(string type, Vector3 position)
        {
            ParticleSystem ps = null;
            if (type == "Health") ps = healEffect;
            else if (type == "Mana") ps = manaEffect;
            else if (type == "Explosion") ps = explosionEffect;

            if (ps != null) {
                GameObject instance = Instantiate(ps.gameObject, position, Quaternion.identity);
                Destroy(instance, 2f);
            }
        }
    }

    // Module: Dynamic Weather System
    public class WeatherSystem : MonoBehaviour
    {
        public ParticleSystem rainEffect;
        public ParticleSystem fogEffect;
        
        public void SetWeather(string type)
        {
            if (rainEffect) rainEffect.gameObject.SetActive(type == "Rain");
            if (fogEffect) fogEffect.gameObject.SetActive(type == "Fog");
            
            // Apply debuffs logic (example)
            if (type == "Rain") Debug.Log("Apply Movement Speed Debuff");
        }
    }

    // Module: AI Director
    public class AIDirector : MonoBehaviour
    {
        public bool isAggressive = false;
        public float spawnRate = 5f;

        public void SetAggression(float level)
        {
            isAggressive = level > 0.5f;
            Debug.Log($"AI Aggression set to: {level}");
        }
    }

    // Module: Quest System (Cultivation)
    [System.Serializable]
    public class QuestNode
    {
        public string title;
        public string description;
        public QuestNode nextNode;
    }

    public class QuestManager : MonoBehaviour
    {
        public QuestNode currentQuest;
        
        public void CompleteCurrent()
        {
            if (currentQuest.nextNode != null)
            {
                currentQuest = currentQuest.nextNode;
                Debug.Log($"Next Quest: {currentQuest.title}");
            }
        }
    }
}
