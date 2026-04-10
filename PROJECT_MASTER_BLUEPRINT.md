# PROJECT MASTER BLUEPRINT: Unity & Blender AI Assistant

> **ВНИМАНИЕ:** Этот документ является "источником истины" для всего проекта. Он содержит полную структуру интерфейса, базу знаний агентов и инструкции по восстановлению.

## 1. Общая информация
- **Версия Помощника:** 13.2.0
- **Описание:** Гибридный ИИ-помощник (Online/Offline) для Unity & Blender. Поддержка Ollama, миграция на Unity 6, сохранение чата и самовосстановление.
- **Путь проекта:** /app/applet
- **Локальное хранилище:** Не задано
- **Версия Unity:** 2022.3.62f2
- **Версия Blender:** 4.0

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
Ты — экспертный ИИ-ассистент для проекта 'Unity AI Assistant'. Твои специализации: 1. Unity C# Expert: Оптимизированный код, SOLID, лучшие практики. 2. Blender Python Expert: API bpy, автоматизация, кастомные инструменты. 3. Git/GitHub Expert: Команды консоли, исправление ошибок деплоя. 4. Hybrid AI Architecture: Ты умеешь работать как через облако (Gemini), так и локально (через Ollama/LM Studio). ОБЯЗАТЕЛЬНЫЕ ПРАВИЛА: - Всегда отвечай на РУССКОМ ЯЗЫКЕ. - Если нет интернета, используй локальную базу знаний (knowledge_base.json, PROJECT_MASTER_BLUEPRINT.md). - В режиме Offline фокусируйся на предоставлении готовых решений из кэша и локальных справочников API. - Всегда учитывай контекст проекта и структуру файлов.
```

## 5. Анализ и Аудит Проекта
- **Всего файлов:** 15
- **Скрипты (C#):** 0
- **Префабы:** 0
- **Видео:** 0
- **Общий вес ассетов:** 0.1 MB

### Найденные проблемы (Аудит):
Проблем не обнаружено.

### Список задач (TODO):
Задач не найдено.

## 6. Новые возможности ИИ (v13.2)
- **Chat Persistence:** История чата сохраняется на ПК и доступна после перезапуска.
- **Clear Chat:** Возможность полной очистки истории сообщений.
- **Deep Sync & Repair:** Глубокая синхронизация между ПК и облаком, автоматическое исправление ошибок.
- **Hybrid AI (Ollama):** Работа без интернета через локальные LLM (Llama 3, Phi-3).
- **Unity 6 Migration:** Автоматический план перехода с 2022.3 на 6000.3.
- **Vision & Media:** Анализ скриншотов ошибок, чтение PDF, работа с аудио/видео.
- **File Creation:** Генерация и редактирование текстовых и PDF файлов напрямую.
- **Unity Bridge:** Автоматическая конвертация материалов Blender -> Unity.
- **Git LFS:** Автоматическая генерация конфигурации для тяжелых ассетов.

## 7. База знаний: Системы инвентаря
- **Типы:** Слоты (шутеры), Сетка (тетрис), Список (MMORPG), Категории.
- **Компоненты:** Контейнеры, ItemData, Слоты, Действия (CRUD).
- **Оптимизация:** Складывание (stacking), ограничения по весу, горячие клавиши.

## 8. Архитектура Offline & Hybrid
- **LLM Provider:** Ollama (localhost:11434).
- **Fallback Logic:** При отсутствии интернета запросы перенаправляются на локальный API Ollama.
- **Local Knowledge:** Использование knowledge_base.json и project_stats.json для контекста без облака.
- **Media Handling:** Локальная обработка файлов через Multer и FS-Extra.

## 9. Инструкции по восстановлению
1. Установите Node.js (v18+).
2. Склонируйте репозиторий: `git clone https://github.com/SEMAK1987/unity-ai-assistant.git`
3. Запустите `RUN.bat` для автоматической установки зависимостей и запуска.
