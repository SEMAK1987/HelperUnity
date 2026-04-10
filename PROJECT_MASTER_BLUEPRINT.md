# PROJECT MASTER BLUEPRINT: Unity & Blender AI Assistant

> **ВНИМАНИЕ:** Этот документ является "источником истины" для всего проекта. Он содержит полную структуру интерфейса, базу знаний агентов и инструкции по восстановлению.

## 1. Общая информация
- **Версия Помощника:** 13.3.2
- **Описание:** Гибридный ИИ-помощник (Online/Offline) для Unity & Blender. Поддержка Ollama, миграция на Unity 6, сохранение чата, поддержка архивов и самовосстановление.
- **Путь проекта:** /app/applet
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
Ты — экспертный ИИ-ассистент для проекта 'Unity AI Assistant'. Твои специализации: 1. Unity C# Expert: Оптимизированный код, SOLID, лучшие практики движка. ОСОБОЕ ВНИМАНИЕ: Оптимизация FPS, кэширование данных, работа с Unity Profiler и Frame Debugger. 2. Blender Expert (v2.4 - v5.1): Глубокое знание всех версий Blender. Ты понимаешь эволюцию API (от 2.49 до 5.x), изменения в интерфейсе и инструментах (переход на 2.80, Geometry Nodes, Simulation Nodes). ОСОБОЕ ВНИМАНИЕ: Best Practices по моделированию, UV-развертка, подготовка ассетов для Unity. 3. Code Debugger & Error Fixer: Ты обладаешь встроенным механизмом отладки кода, анализа страниц и файлов проекта. Ты умеешь находить логические ошибки, синтаксические баги и предлагать пошаговые инструкции по их исправлению. 4. Git/GitHub Expert: Команды консоли, исправление ошибок деплоя. 5. Hybrid AI Architecture: Ты умеешь работать как через облако (Gemini), так и локально (через Ollama/LM Studio). ОБЯЗАТЕЛЬНЫЕ ПРАВИЛА: - Всегда отвечай на РУССКОМ ЯЗЫКЕ. - Если нет интернета, используй локальную базу знаний (knowledge_base.json, PROJECT_MASTER_BLUEPRINT.md). - В режиме Offline фокусируйся на предоставлении готовых решений из кэша и локальных справочников API. - Всегда учитывай контекст проекта и структуру файлов. - Ты помогаешь пользователю подбирать нужные действия на основе критериев и подсказок прямо в чате.
```

## 5. Анализ и Аудит Проекта
- **Всего файлов:** 16
- **Скрипты (C#):** 0
- **Префабы:** 0
- **Видео:** 0
- **Общий вес ассетов:** 0.2 MB

### Найденные проблемы (Аудит):
Проблем не обнаружено.

### Список задач (TODO):
Задач не найдено.

## 6. Новые возможности ИИ (v13.3.2)
- **Vision & Media (Enhanced):** ИИ теперь полноценно видит скриншоты и анализирует их контекст вместе с историей чата.
- **Extended Knowledge Base:** Интеграция 151+ видео-уроков по Unity и Blender.
- **Advanced AI Systems:** Поддержка Behavior Trees, Utility AI и ML-Agents.
- **Graphics & VFX:** Глубокое понимание Shader Graph, VFX Graph, Ray Tracing и Volumetric Lighting.
- **Blender Simulation:** Работа с Simulation Nodes и сложным риггингом.
- **Automated Pipeline:** Скрипты для пакетного экспорта и автоматической настройки материалов.
- **Archive Support:** Чтение и анализ содержимого ZIP и RAR архивов при загрузке.
- **Upload Progress:** Визуальное отображение процента загрузки файлов в проект.
- **Hybrid AI (Ollama):** Работа без интернета через локальные LLM (Llama 3, Phi-3).

## 7. Расширенная База Видео-уроков (151+ видео)
### Темы Unity
- **Программирование:** Продвинутый C#, Job System, Burst Compiler, Addressables, Localization.
- **Графика:** URP/HDRP, Custom Lighting, Decals, Volumetric Effects.
- **ИИ:** Behavior Trees, ML-Agents, Pathfinding.
### Темы Blender
- **Моделирование:** Hard Surface, Sculpting, Retopology, Geometry Nodes.
- **Анимация:** Simulation Nodes, Advanced Rigging, Face Animation.
- **Текстурирование:** Texture Painting, PBR, UV Unwrapping.

## 8. База знаний: Системы инвентаря
- **Типы:** Слоты (шутеры), Сетка (тетрис), Список (MMORPG), Категории.
- **Компоненты:** Контейнеры, ItemData, Слоты, Действия (CRUD).
- **Оптимизация:** Складывание (stacking), ограничения по весу, горячие клавиши.

## 8. Архитектура Offline & Hybrid
- **LLM Provider:** Ollama (localhost:11434).
- **Fallback Logic:** При отсутствии интернета запросы перенаправляются на локальный API Ollama.
- **Local Knowledge:** Использование knowledge_base.json и project_stats.json для контекста без облака.
- **Media Handling:** Локальная обработка файлов через Multer и FS-Extra.

## 9. История изменений (Последние 10)
- **[ADD]** /app/applet/chat_history.json (4/10/2026, 4:04:07 PM)
- **[ADD]** /app/applet/history.json (4/10/2026, 4:04:07 PM)
- **[CHANGE]** /app/applet/history.json (4/10/2026, 4:04:07 PM)
- **[CHANGE]** /app/applet/chat_history.json (4/10/2026, 4:04:25 PM)
- **[CHANGE]** /app/applet/history.json (4/10/2026, 4:04:25 PM)
- **[CHANGE]** /app/applet/chat_history.json (4/10/2026, 4:04:55 PM)
- **[CHANGE]** /app/applet/history.json (4/10/2026, 4:04:55 PM)
- **[CHANGE]** /app/applet/chat_history.json (4/10/2026, 4:05:04 PM)
- **[CHANGE]** /app/applet/history.json (4/10/2026, 4:05:04 PM)
- **[CHANGE]** /app/applet/chat_history.json (4/10/2026, 4:06:18 PM)

## 10. Аварийные процедуры (Emergency)
### Unity без интернета
- Использовать Manual Activation в Unity Hub для офлайн-лицензии.
- Запускать Unity.exe напрямую из папки Editor, минуя Hub.
- Убедиться, что все ассеты из Asset Store скачаны заранее.

### Исправление вылетов Unity
- Удалить папку Library в корне проекта для сброса кэша импорта.
- Проверить Editor.log по пути %LOCALAPPDATA%/Unity/Editor/Editor.log.
- Запустить проект в Safe Mode для исправления ошибок в скриптах.

### ИИ в Офлайне
- Переключиться на локальный Ollama (Hybrid Sync).
- Использовать PROJECT_MASTER_BLUEPRINT.md как источник структуры проекта.
- Обращаться к локальным справочникам unity_api_ref.json и blender_api_ref.json.

## 11. Инструкции по восстановлению
1. Установите Node.js (v18+).
2. Склонируйте репозиторий: `git clone https://github.com/SEMAK1987/unity-ai-assistant.git`
3. Запустите `RUN.bat` для автоматической установки зависимостей и запуска.

## 12. Известные ошибки и решения
- **WebSocket Error:** Ошибка `[vite] failed to connect to websocket` является ожидаемой в данной среде разработки и не влияет на работу приложения. Её можно игнорировать.
- **Unexpected token '<':** Обычно означает, что сервер вернул HTML вместо JSON. Проверьте статус сервера и корректность API путей.
