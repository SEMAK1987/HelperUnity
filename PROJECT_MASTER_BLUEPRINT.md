# PROJECT MASTER BLUEPRINT: Unity & Blender AI Assistant (Total Knowledge Archive Edition)

> **ВНИМАНИЕ:** Этот документ является "источником истины" для всего проекта. Он содержит полную структуру интерфейса, базу знаний агентов, инструкции по самовосстановлению и описание возможностей ИИ v18.4.5.

## 1. Общая информация
- **Версия Помощника:** 18.4.5
- **Описание:** Гибридный ИИ-помощник нового поколения. Полная автоматизация локализации Dropdown и продвинутые шейдеры.
- **Путь проекта:** /app/applet
- **Флаги:** [QUANTUM_LINK_ACTIVE], [KNOWLEDGE_STORAGE_SYNC], [V18_4_5_SHADER_STABILITY]

## 2. Специальные исправления (Hotfixes v18.4.5)

### ⚠️ Ошибка Input Manager (Deprecated)
**Решение:**
1. **Edit -> Project Settings -> Player**.
2. **Other Settings -> Configuration**.
3. **Active Input Handling** -> Установить на **Both**.
4. Согласиться на перезапуск Unity.

### ⚠️ Ошибка "Нет пункта Glow" в TMPro (Важно!)
**Причина:** В вашем материале выбран шейдер `Mobile`. В нем нет Glow для экономии ресурсов телефона.
**Решение:**
1. Выделите объект текста.
2. В самом низу Inspector нажмите на поле **Shader**.
3. Выберите путь: **TextMeshPro** -> **Distance Field** (Обычный, не Mobile!).
4. Магическим образом появится вкладка **Glow**. Поставьте галочку и настройте свечение.

### 🔘 Настройка Transtable_Dropdown (Для Качества)
1. Найдите объект с **Dropdown (TMP)** в инспекторе.
2. Нажмите **Add Component** и выберите скрипт `Transtable_Dropdown`.
3. В поле **Option IDs** нажмите на "+" 6 раз.
4. Введите ID: `37, 38, 39, 40, 41, 42`.
5. Теперь при смене языка все пункты (Низкое, Высокое и т.д.) переведутся сами.

## 3. Структура интерфейса
- **v18.4.5:** Логика Dropdown и шейдерные настройки добавлены в ядро.

## 11. История изменений (v18.4.5)
- **v18.4.5:** Fix Input System warning. Fix TMPro Glow visibility (Shader switch guidance).
- **v18.4.4:** Dropdown Automation. Скрипт `Transtable_Dropdown`.
