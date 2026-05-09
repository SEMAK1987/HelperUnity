# PROJECT MASTER BLUEPRINT: Unity & Blender AI Assistant (Total Knowledge Archive Edition)

> **ВНИМАНИЕ:** Этот документ является "источником истины" для всего проекта. Он содержит полную структуру интерфейса, базу знаний агентов, инструкции по самовосстановлению и описание возможностей ИИ v18.4.7.

## 1. Общая информация
- **Версия Помощника:** 18.4.7
- **Описание:** Гибридный ИИ-помощник v18.7. Полная синхронизация Dropdown, HDR Glow Setup и Input System Master.
- **Путь проекта:** /app/applet
- **Флаги:** [QUANTUM_LINK_ACTIVE], [KNOWLEDGE_STORAGE_SYNC], [V18_4_7_HDR_STABILITY]

## 2. Специальные исправления (Hotfixes v18.4.7)

### ⚠️ Исправление ошибок компиляции (AddDropdown)
Если вы получили ошибку `CS0117`, значит скрипт `Translator.cs` устарел. 
**Решение:** Помощник уже обновил `Translator.cs`. Убедитесь, что в нем есть методы `AddDropdown` и `DeleteDropdown`. Теперь `Transtable_Dropdown.cs` работает идеально.

### 🎨 ИДЕАЛЬНОЕ СВЕЧЕНИЕ (HDR Glow Setup)
**Пошагово (по вашему скриншоту):**
1. **Shader:** Убедитесь, что выбран `TextMeshPro -> Distance Field`.
2. **Glow:** Поставьте галочку **Enable**.
3. **Color (HDR):** Кликните на цвет. В окне **HDR Color** введите:
   - **R:** 0
   - **G:** 255
   - **B:** 255 (Яркий Циан / Неоново-голубой)
   - **Intensity:** Поднимите ползунок до `+1` или `+1.5` для эффекта "свечения в темноте".
4. **Parameters:**
   - **Offset:** 0
   - **Inner:** 0.3
   - **Outer:** 0.4
   - **Power:** 1.0

### 🔘 Настройка Dropdown (Качество)
1. Объект: **Quality Dropdown**.
2. Компонент: `Transtable_Dropdown`.
3. **Option IDs:** `37, 38, 39, 40, 41, 42`.
4. Результат: Мгновенный перевод всех пунктов при переключении языка.

### ⚠️ Проблема Input System
Используйте **только** `Input System Package (New)`. Это устраняет конфликты в Unity 6.

## 3. Структура интерфейса
- **v18.4.7:** Все системы (Translator, Dropdown, HDR) полностью синхронизированы.

## 11. История изменений (v18.4.7)
- **v18.4.7:** Final HDR Color values added. Script sync fix. Input System normalization.
- **v18.4.6:** Master stability update.
