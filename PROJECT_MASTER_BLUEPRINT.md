# PROJECT MASTER BLUEPRINT: Unity & Blender AI Assistant (Total Knowledge Archive Edition)

> **ВНИМАНИЕ:** Этот документ является "источником истины" для всего проекта. Он содержит полную структуру интерфейса, базу знаний агентов, инструкции по самовосстановлению и описание возможностей ИИ v18.4.6.

## 1. Общая информация
- **Версия Помощника:** 18.4.6
- **Описание:** Гибридный ИИ-помощник v18. Полная автоматизация локализации Dropdown, исправленный Glow и поддержка новой Input System.
- **Путь проекта:** /app/applet
- **Флаги:** [QUANTUM_LINK_ACTIVE], [KNOWLEDGE_STORAGE_SYNC], [V18_4_6_FINAL_STABILITY]

## 2. Специальные исправления (Hotfixes v18.4.6)

### ⚠️ Ошибка Input System Package (New)
**Решение:** Если при выборе `Both` возникают ошибки, используйте только **Input System Package (New)**. Это современный стандарт для Unity 6. Убедитесь, что у вас установлен пакет через Window -> Package Manager -> Unity Registry -> Input System.

### ⚠️ Включение эффекта Glow (Пошагово)
На вашем скриншоте выбран шейдер `TextMeshPro/Mobile/Distance Field`. В нем **НЕТ** Glow.
**Чтобы включить:**
1. Выделите объект названия игры (GameTitle).
2. В самом низу Inspector нажмите на поле **Shader**.
3. Выберите: **TextMeshPro -> Distance Field** (Обычный, без слова Mobile).
4. Теперь в Inspector (под разделом Face) появится вкладка **Glow**.
5. Поставьте галочку **Enable Glow**.
6. **Inner / Outer:** Установите `0.3` или `0.4` для красивого сияния.

### 🔘 Настройка авто-перевода Dropdown
**Если вы видите ошибки AddDropdown/DeleteDropdown:**
1. Полностью замените код в `Translator.cs` на новый (из помощника).
2. На объекте с выпадающим списком нажмите **Add Component** -> `Transtable_Dropdown`.
3. В **Option IDs** впишите: `37, 38, 39, 40, 41, 42`.
4. Список будет переводиться автоматически при смене языка.

## 3. Структура интерфейса
- **v18.4.6:** Финализация всех логических цепочек локализации и визуализации.

## 11. История изменений (v18.4.6)
- **v18.4.6:** Master stability update. Glow shader guidance confirmed. Input system logic fixed.
- **v18.4.5:** Shader switch guidance.
