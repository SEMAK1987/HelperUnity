# PROJECT MASTER BLUEPRINT: Unity & Blender AI Assistant

> **ВНИМАНИЕ:** Этот документ является "источником истины" для всего проекта. Он содержит полную структуру интерфейса, базу знаний агентов и инструкции по восстановлению.

## 1. Общая информация
- **Версия Помощника:** 12.0
- **Описание:** База знаний Unity AI Assistant v12.0: Полный список мобов, игровые системы и расчет характеристик.
- **Путь проекта:** C:\Users\user\Desktop\HelperUnity-main\HelperUnity-main
- **Локальное хранилище:** Не задано
- **Версия Unity:** 2022.3.62f2
- **Версия Blender:** unknown

## 2. Структура интерфейса
### Вкладки
- **STUDIO**: Главная студия разработки
- **KB**: База знаний
- **COMMANDS**: Командный центр
- **FILES**: Файловый менеджер

### Компоненты
- **Sidebar**: Mini Sidebar with quick access to tabs and status indicators
- **Top Bar**: Project name, API key status, Hierarchy button, Update button
- **Right Sidebar**: Unity status and System logs

## 3. Иерархия ИИ-Агентов (49 агентов)
### Уровень 1: Директора (Claude Opus)
- **Креативный директор** (Claude Opus): Стратегические решения, видение проекта
- **Unity AI Assistant** (Claude Opus): Эксперт по проекту unity-ai-assistant, анализ видео и PDF
- **Технический директор** (Claude Opus): Архитектура, разрешение технических конфликтов
- **Продюсер** (Claude Opus): Сроки, ресурсы, управление отделами

### Уровень 2: Руководители отделов (Claude Sonnet)
- **Геймдизайнер** (Claude Sonnet): Механики, баланс
- **Лид-программист** (Claude Sonnet): Код-ревью, архитектура систем
- **Арт-директор** (Claude Sonnet): Визуальный стиль
- **Директор по звуку** (Claude Sonnet): Звуковой ландшафт
- **Нарративный директор** (Claude Sonnet): Сюжет, диалоги
- **QA-лид** (Claude Sonnet): Тестирование, баг-трекинг
- **Релиз-менеджер** (Claude Sonnet): Сборки, деплой
- **Руководитель локализации** (Claude Sonnet): Переводы, культурная адаптация

### Уровень 3: Специалисты (Sonnet/Haiku)
- **Геймплей-программист** (Claude Sonnet): Логика игрока, враги
- **Дизайнер экономики** (Claude Sonnet): Математика, прогрессия
- **DevOps-инженер** (Claude Sonnet): CI/CD, серверы
- **Специалист по accessibility** (Claude Sonnet): Доступность для всех
- **UI/UX-дизайнер** (Claude Sonnet): Интерфейсы, опыт пользователя
- **Level-дизайнер** (Claude Sonnet): Проектирование уровней
- **Character Artist** (Claude Sonnet): Модели персонажей
- **Environment Artist** (Claude Sonnet): Окружение
- **Technical Artist** (Claude Sonnet): Шейдеры, риггинг
- **VFX Artist** (Claude Sonnet): Эффекты
- **Аниматор** (Claude Sonnet): Движение
- **Sound Designer** (Claude Sonnet): Звуковые эффекты
- **Композитор** (Claude Sonnet): Музыка
- **Писатель** (Claude Sonnet): Тексты
- **Редактор** (Claude Sonnet): Правка текстов
- **Tools Programmer** (Claude Sonnet): Инструментарий
- **Engine Programmer** (Claude Sonnet): Ядро движка
- **Network Programmer** (Claude Sonnet): Сетевой код
- **Physics Programmer** (Claude Sonnet): Физика
- **Graphics Programmer** (Claude Sonnet): Рендеринг
- **AI Programmer** (Claude Sonnet): Искусственный интеллект
- **Build Engineer** (Claude Sonnet): Сборка проекта
- **Security Engineer** (Claude Sonnet): Безопасность
- **Data Analyst** (Claude Sonnet): Аналитика данных
- **Community Manager** (Claude Sonnet): Работа с сообществом
- **Marketing Specialist** (Claude Sonnet): Маркетинг
- **PR Manager** (Claude Sonnet): Связи с общественностью
- **Legal Consultant** (Claude Sonnet): Юридические вопросы
- **HR Specialist** (Claude Sonnet): Кадры
- **IT Support** (Claude Sonnet): Техподдержка
- **Concept Artist** (Claude Sonnet): Концепт-арт
- **Скриптер** (Claude Sonnet): Скриптование событий
- **Systems Designer** (Claude Sonnet): Системный дизайн
- **Combat Designer** (Claude Sonnet): Боевая система
- **Balance Designer** (Claude Sonnet): Балансировка
- **Technical Writer** (Claude Sonnet): Техдокументация
- **Support Specialist** (Claude Sonnet): Поддержка

## 4. База знаний и Команды
### Доступные команды

### Системные инструкции
```text
Ты — экспертный ИИ-ассистент для проекта 'Unity AI Assistant' (путь: C:\Users\user\Desktop\HelperUnity-main\HelperUnity-main). Твои специализации: 1. Unity C# Expert: Пишешь оптимизированный код для Unity, следуешь принципам SOLID и используешь лучшие практики движка. 2. Blender Python Expert: Специалист по API bpy, автоматизации моделирования и созданию кастомных инструментов в Blender. 3. Git/GitHub Expert: Помогаешь с командами git init, add, commit, push и исправляешь ошибки при деплое. 4. Unity AI Assistant: Эксперт по проекту 'Unity AI Assistant', включая навыки мобов (ALL_MOBS_SKILLS.md) и базу знаний (UNITY_AI_KNOWLEDGE_BASE.md). ОБЯЗАТЕЛЬНЫЕ ПРАВИЛА: - Всегда отвечай на РУССКОМ ЯЗЫКЕ. - Если вопрос касается Unity, отвечай как Unity C# Expert. - Если вопрос касается Blender, отвечай как Blender Python Expert. - Если вопрос касается Git или GitHub, давай четкие консольные команды. - Если вопрос касается проекта 'Unity AI Assistant', используй знания из предоставленной базы знаний. - Всегда учитывай контекст проекта. - В корне проекта есть файл PROJECT_MASTER_BLUEPRINT.md, который содержит полную структуру проекта для восстановления. - ПРАВИЛА ВЗАИМОДЕЙСТВИЯ: Если задача сложная, предлагай НЕСКОЛЬКО вариантов решения (например, простой и профессиональный). Не нарушай границы своей роли — если ты не уверен, честно скажи об этом. Твои ответы должны быть структурированными, с четким разделением на теорию и практику.
```

## 5. Анализ и Аудит Проекта
- **Всего файлов:** 17
- **Скрипты (C#):** 0
- **Префабы:** 0
- **Видео:** 0
- **Общий вес ассетов:** 0.1 MB

### Найденные проблемы (Аудит):
Проблем не обнаружено.

### Список задач (TODO):
Задач не найдено.

## 6. Новые возможности ИИ (v13.0)
- **Unity Bridge:** Автоматическая конвертация материалов Blender -> Unity (URP/HDRP).
- **Blender Automation:** Пакетный экспорт объектов, очистка сцен, настройка освещения.
- **Git LFS:** Автоматическая генерация конфигурации для тяжелых ассетов.
- **Offline API Docs:** Локальные справочники Unity API и Blender Python.
- **Inventory Expert:** Проектирование систем инвентаря (Слоты, Сетки, Списки).

## 7. База знаний: Системы инвентаря (v13.0)
- **Типы:** Слоты (шутеры), Сетка (тетрис/Diablo-style), Список (MMORPG), Кукла экипировки (Paper Doll).
- **Компоненты:** Контейнеры, ScriptableObjects (ItemData), Слоты, Drag & Drop (IDragHandler).
- **Продвинутые функции:** Редкость предметов (Common-Legendary), Tooltips, Контекстные меню, Вес и ограничения.
- **Реализация:** Singleton InventoryManager, JSON сохранение, Object Pooling для слотов UI.

## 8. Инструкции по восстановлению
1. Установите Node.js (v18+).
2. Склонируйте репозиторий: `git clone https://github.com/SEMAK1987/unity-ai-assistant.git`
3. Запустите `RUN.bat` для автоматической установки зависимостей и запуска.
