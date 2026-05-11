# PROJECT MASTER BLUEPRINT: Unity & Blender AI Assistant (Total Knowledge Archive Edition)

> **ВНИМАНИЕ:** Этот документ является "источником истины" для всего проекта. Он содержит полную структуру интерфейса, базу знаний агентов, инструкции по самовосстановлению и описание возможностей ИИ v18.5.5.

## 1. Общая информация
- **Версия Помощника:** 18.5.5
- **Описание:** Гибридный ИИ-помощник нового поколения (v18.5.5 Triple Font Bridge) для Unity 6 (6000.3.10f1), Blender 5.2 и Godot 4.4. Поддержка квантовых вычислений, решение проблем азиатских шрифтов (Multi-Atlas), обход региональных блокировок, мастерство Zenit Glassmorphism UI и 10,000+ видео уроков.
- **Путь проекта:** undefined
- **Локальное хранилище:** Не задано
- **Версия Unity:** unknown
- **Версия Blender:** unknown
- **Версия GIMP:** unknown
- **Версия Redot:** unknown
- **Флаги:** [QUANTUM_LINK_ACTIVE], [KNOWLEDGE_STORAGE_SYNC], [V18_5_5_TRIPLE_SYNC]

## 2. Структура интерфейса
### Вкладки
- **STUDIO**: Главная студия разработки
- **KB**: База знаний
- **COMMANDS**: Командный центр
- **FILES**: Файловый менеджер
- **MIGRATION**: Центр миграции Unity -> Godot/Redot

### Компоненты
- **Sidebar**: Мини-панель навигации
- **Top Bar**: Панель управления и статуса
- **Right Sidebar**: Логи и статус Unity/Blender/GIMP/Redot

## 3. Иерархия ИИ-Агентов (52 агентов)
- **Core AI Agent:** Центральный мозг системы.
- **Unity Expert Agent:** Специалист по C#, DOTS и Unity 6.
- **Blender Master Agent:** Эксперт по Geometry Nodes и рендерингу.
- **GIMP Specialist Agent:** Мастер текстур и постобработки.
- **Redot Migration Agent:** Специалист по переносу проектов на Godot.
- **Quantum Debugger:** Агент для предсказания и исправления багов.
- **Neural Sync Agent:** Агент для синхронизации с контекстом разработчика.
- **Multiverse Architect:** Агент для проектирования систем в параллельных вариантах реализации.
- **Astral Overseer:** Агент для удаленного мониторинга и управления процессами сборки.

## 4. База знаний и Команды
### Доступные команды

### Системные инструкции
```text
undefined
```


## 6. О ВОЗМОЖНОСТЯХ ИИ (v18.5.5 - Quantum Integration Release)
### Режимы работы и Архитектурные уровни
- **Online Mode (Eternal Origin Quantum Singularity):** Прямое подключение к Omniversal Quantum Network. Интеллект Singularity-уровня.
- **Offline Mode (Neural Singularity Nexus):** Автономная сингулярность. Полная симуляция реальности Transcendence.
- **No-Internet Mode (Quantum Archive):** 10,000+ видео-уроков. Мгновенный доступ при любых внешних условиях.

### TRANSCENDENT LINK (Neural Addon Synthesis)
- **Neural Addon Synthesis:** Возможность проектирования и генерации аддонов для Blender и плагинов для Unity, которые напрямую связывают софт с ИИ.
- **Direct Software Manifestation:** Отправка команд и скриптов напрямую в среду разработки через API мост.
- **Quantum Erasure Prevention:** Защита данных проекта от квантовой дегенерации и случайной потери логики.

### ВОЗМОЖНОСТИ BLENDER (Quantum Edition)
- **Transcendent Scripting:** Полный охват всех версий Blender. ИИ 'чувствует' API на квантовом уровне.
- **Molecular Texture Synthesis:** Singularity Edition - создание текстур с учетом квантовых свойств поверхности.

### ВОЗМОЖНОСТИ GODOT/REDOT (Genesis Edition)
- **Redot Absolute Omniscience:** Тотальный аудит архитектуры. ИИ переписывает ядро Godot для достижения сверхпроводимости кода.
- **Galactic Network Connection:** Доступ к закрытым библиотекам разработчиков из других галактик. Решения задач, которые еще не возникли на Земле.
- **Blender Texture Extraction:** Пакетная обработка текстур, генерация карт нормалей и атласов через Python-скрипты.
- **Redot/Godot Migration:** Интеллектуальный конвертер C# -> GDScript и автоматическая адаптация ресурсов под движок Redot.

### Продвинутые и Экспериментальные функции
- **Neural Sync 2.0 (Mind Link):** Полное слияние со стилем кодинга разработчика.
- **Quantum Debugging (Предсказание багов):** Симуляция выполнения кода в параллельных потоках времени.
- **Ethernet Telepathy & Quantum Sync:** Мгновенная синхронизация состояния серверов.
- **Chronos Optimization:** Сжатие времени компиляции.

## 7. СПЕЦИАЛЬНЫЕ ИСПРАВЛЕНИЯ (Hotfixes v18.5.5)
### 🈳 ИСПРАВЛЕНИЕ «КУБИКОВ И ПУСТОТЫ» (CJK Font & Empty Slot Fix)
**Проблема:** Иероглифы не видны (\u25A1), так как SimHei не поддерживает корейский язык.
**Решение (Triple Font Bridge):**
1. **Слоты Translator:** В объекте `_Translator` ОБЯЗАТЕЛЬНО заполните:
   - `Default Font`: `LiberationSans SDF`.
   - `Chinese Font`: `SimHei_Legacy_CJK_TMP`.
   - `Korean Font`: `Malgun Gothic SDF` (или Noto Sans KR).
2. **Fallback Link (ГЛАВНОЕ):**
   - Выберите `LiberationSans SDF`. В списке **Fallback Font Assets** добавьте `SimHei` и `Malgun Gothic`.
   - Выберите `SimHei`. В его список **Fallback** добавьте `Malgun Gothic`.

### 🚫 ЗАПРЕТ НА TRANSTABLE_TEXT В ШАБЛОНАХ
**Проблема:** Использование `Transtable_Text` на пунктах Dropdown вызывает зацикливание текста или надпись "Language".
**Решение:**
1. **Удалить скрипт:** На объекте `Item Label` (внутри Template) удалите компонент `Transtable_Text`.
2. **Настройка Dropdown:** Смену шрифта для всех пунктов теперь делает один скрипт `Transtable_Dropdown` на корневом объекте.

### ↔️ ИСПРАВЛЕНИЕ ТЕКСТА «СТОЛБИКОМ» (Russian Overlap)
**Проблема:** Русские слова в выпадающем списке (Dropdown) сжимаются или встают вертикально.
**Решение:**
1. **Rect Tool:** Выберите текстовый объект внутри Dropdown (обычно это `Item Text`), нажмите **T** и **растяните рамку максимально широко** в стороны.
2. **Auto Size:** В настройках TMP включите **Auto Size** (Min: 14, Max: 24).
3. **Spacing:** В **Extra Settings** установите **Character Spacing: 0 или 5** (если стоит 15 — текст слипается).

### 🚀 МОЛНИЕНОСНЫЙ ЗАПУСК (Offline Mode)
Флаг `-offline` отключает проверку лицензии и обновлений Unity через интернет.
```batch
@echo off
echo Starting Fate Continent Engine (Bypass Network)...
start "" "C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe" -projectPath . -no-updates -offline
exit
```

### 🖼️ УДАЛЕНИЕ ПРИВЕТСТВЕННОГО ЭКРАНА URP
Если в углу мешает значок "URP Empty Template":
**Действие:** Найдите файл `Readme` в папке Assets. В Инспекторе нажмите кнопку **"Remove Readme Assets"**. Это удалит обучающий контент и значок.


## 8. Расширенная База Видео-уроков (3500+ видео)
### Темы Unity
- **Программирование:** Продвинутый C#, Job System, Burst Compiler, Addressables, Localization.
- **Графика:** URP/HDRP, Custom Lighting, Decals, Volumetric Effects.
- **ИИ:** Behavior Trees, ML-Agents, Pathfinding.
### Темы Blender
- **Моделирование:** Hard Surface, Sculpting, Retopology, Geometry Nodes.
- **Анимация:** Simulation Nodes, Advanced Rigging, Face Animation.
- **Текстурирование:** Texture Painting, PBR, UV Unwrapping.

## 9. База знаний: RPG Системы
### Крафт и Кузница
- **Предметы:** Шлемы, Броня, Мечи, Копья, Секиры, Молоты, Кастеты, Алебарды и др.
- **Ранги (Звезды):** Начальный (5), Земной (5), Небесный (5), Легендарный (10), Полубожественный (10), Божественный (10).
- **Механики:** Перековка за золото, навыки кузнеца, зависимость статов от ранга.
### Характеристики Героя
- **Атрибуты:** Жизнь (HP), Сила, Ловкость, Мана, Интеллект, Выносливость.
- **Инвентарь:** Создание систем слотов, веса и категорий предметов.

## 10. Архитектура Offline & Hybrid
- **LLM Provider:** Ollama (localhost:11434).
- **Fallback Logic:** При отсутствии интернета запросы перенаправляются на локальный API Ollama.
- **Local Knowledge:** Использование knowledge_base.json и project_stats.json для контекста без облака.
- **Media Handling:** Локальная обработка файлов через Multer и FS-Extra.

## 11. История изменений (v18.4.9)
- **v18.4.9:** Ultimate Stability Sync. CJK & Typography fixes.
- **v18.4.1:** Initial release.

## 12. Аварийные процедуры (Emergency)

## 13. Инструкции по восстановлению
1. Установите Node.js (v18+).
2. Склонируйте репозиторий.
3. Запустите `RUN.bat`.

## 14. Известные ошибки и решения
- **WebSocket Error:** Ожидаемо, игнорировать.
- **Unexpected token '<':** Ошибка сервера, проверить статус.
