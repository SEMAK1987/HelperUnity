using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Разработчик: Алхимический Кот (Alchemist Cat Core)
/// Скрипт глобальной системы сохранения и загрузки прогресса Кота-Алхимика.
/// </summary>
public static class SaveGameSystem
{
    public static SaveData CurrentData = new SaveData();
    public static bool IsStartedFromMenu = false;

    [Serializable]
    public class SaveData
    {
        [Header("Основное")]
        public string saveName = "Кот-Алхимик";
        public int currentSceneIndex = 1;
        public string saveDateTime = "";
        
        [Header("Экономика и Валюта")]
        public int gold = 100;
        public int crystals = 0;
        public int vipXP = 0;
        public int daysActive = 0;

        [Header("Развитие Кота")]
        public int catLevel = 1;
        public int currentXP = 0;
        public int cauldronLevel = 1;
        public int potionsBrewed = 0;

        [Header("Миниигры и Разблокировки")]
        public bool unlockedDarts = false;
        public bool unlockedMouseCatch = false;
    }

    /// <summary>
    /// Полное сохранение игры в определенный слот.
    /// </summary>
    public static void Save(int slotIndex)
    {
        CurrentData.currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        CurrentData.saveDateTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

        // Синхронизируем с текущим состоянием GameManager, если он запущен
        if (GameManager.Instance != null)
        {
            CurrentData.gold = GameManager.Instance.gold;
            CurrentData.crystals = GameManager.Instance.crystals;
            CurrentData.vipXP = GameManager.Instance.vipXP;
            CurrentData.daysActive = GameManager.Instance.daysActive;
            CurrentData.catLevel = GameManager.Instance.catLevel;
            CurrentData.currentXP = GameManager.Instance.currentXP;
            CurrentData.cauldronLevel = GameManager.Instance.cauldronLevel;
            CurrentData.potionsBrewed = GameManager.Instance.potionsBrewed;
            CurrentData.unlockedDarts = GameManager.Instance.unlockedDarts;
            CurrentData.unlockedMouseCatch = GameManager.Instance.unlockedMouseCatch;
        }

        string json = JsonUtility.ToJson(CurrentData, true);

        // Краткое описание для отображения в UI слота
        string langPrefix = GetLanguageInfoPrefix();
        string infoText = $"{CurrentData.saveDateTime} | {langPrefix} {CurrentData.catLevel} | {SceneManager.GetActiveScene().name}";

        PlayerPrefs.SetInt("Alchemist_Slot_Used_" + slotIndex, 1);
        PlayerPrefs.SetInt("Alchemist_Slot_Scene_" + slotIndex, CurrentData.currentSceneIndex);
        PlayerPrefs.SetString("Alchemist_Slot_Info_" + slotIndex, infoText);
        PlayerPrefs.SetString("Alchemist_Slot_Data_" + slotIndex, json);
        PlayerPrefs.Save();

        Debug.Log($"[ALCHEMIST SAVE] Игра успешно СОХРАНЕНА в Слот {slotIndex}. Данные: {infoText}");
    }

    /// <summary>
    /// Загрузка игры из слота.
    /// </summary>
    public static bool Load(int slotIndex, bool loadScene = true)
    {
        if (!PlayerPrefs.HasKey("Alchemist_Slot_Used_" + slotIndex))
        {
            Debug.LogWarning($"[ALCHEMIST LOAD] Попытка загрузки пустого слота {slotIndex}");
            return false;
        }

        string json = PlayerPrefs.GetString("Alchemist_Slot_Data_" + slotIndex);
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError($"[ALCHEMIST LOAD] Пустые данные сохранения в слоте {slotIndex}");
            return false;
        }

        try
        {
            CurrentData = JsonUtility.FromJson<SaveData>(json);
            
            // Записываем данные в GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.gold = CurrentData.gold;
                GameManager.Instance.crystals = CurrentData.crystals;
                GameManager.Instance.vipXP = CurrentData.vipXP;
                GameManager.Instance.daysActive = CurrentData.daysActive;
                GameManager.Instance.catLevel = CurrentData.catLevel;
                GameManager.Instance.currentXP = CurrentData.currentXP;
                GameManager.Instance.cauldronLevel = CurrentData.cauldronLevel;
                GameManager.Instance.potionsBrewed = CurrentData.potionsBrewed;
                GameManager.Instance.unlockedDarts = CurrentData.unlockedDarts;
                GameManager.Instance.unlockedMouseCatch = CurrentData.unlockedMouseCatch;
                GameManager.Instance.SyncUI();
            }

            IsStartedFromMenu = true;
            Debug.Log($"[ALCHEMIST LOAD] Успешная загрузка слота {slotIndex}. Уровень Кота: {CurrentData.catLevel}");

            if (loadScene)
            {
                SceneManager.LoadScene(CurrentData.currentSceneIndex);
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ALCHEMIST LOAD] Критическая ошибка при загрузке слота {slotIndex}: {ex}");
            return false;
        }
    }

    /// <summary>
    /// Очистка слота сохранения.
    /// </summary>
    public static void DeleteSave(int slotIndex)
    {
        PlayerPrefs.DeleteKey("Alchemist_Slot_Used_" + slotIndex);
        PlayerPrefs.DeleteKey("Alchemist_Slot_Scene_" + slotIndex);
        PlayerPrefs.DeleteKey("Alchemist_Slot_Info_" + slotIndex);
        PlayerPrefs.DeleteKey("Alchemist_Slot_Data_" + slotIndex);
        PlayerPrefs.Save();
        Debug.Log($"[ALCHEMIST SAVE] Слот сохранения {slotIndex} успешно очищен.");
    }

    private static string GetLanguageInfoPrefix()
    {
        int lang = PlayerPrefs.GetInt("Alchemist_Language", 0);
        switch (lang)
        {
            case 0: return "Ур.";
            case 7: return "레벨";
            case 6: return "レベル";
            case 8: return "等级";
            default: return "Lvl";
        }
    }
}
