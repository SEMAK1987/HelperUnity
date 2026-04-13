# PROJECT MASTER BLUEPRINT: Unity Assistant

> **ВНИМАНИЕ:** Этот документ является "источником истины" для всего проекта. Он содержит полную структуру интерфейса, базу знаний агентов и инструкции по восстановлению.

## 1. Общая информация
- **Версия Помощника:** 15.5.0
- **Описание:** Гибридный ИИ-помощник (Online/Offline/No-Internet) для Unity & Blender. Поддержка Ollama, миграция на Unity 6, сохранение чата, поддержка архивов и самовосстановление.
- **Путь проекта:** D:\_GameDew\Projects\Небесные Битвы\Небесные Битвы\Небесные Битвы
- **Локальное хранилище:** Не задано
- **Версия Unity:** unknown
- **Версия Blender:** unknown

## 2. Структура интерфейса
### Вкладки
- **STUDIO**: Главная студия разработки
- **KB**: База знаний
- **COMMANDS**: Командный центр
- **FILES**: Файловый менеджер

### Компоненты
- **Sidebar**: Мини-панель навигации
- **Top Bar**: Панель управления и статуса
- **Right Sidebar**: Логи и статус Unity

## 3. Иерархия ИИ-Агентов (49 агентов)
## 4. База знаний и Команды
### Доступные команды

### Системные инструкции
```text
Ты — экспертный ИИ-ассистент для проекта 'Unity AI Assistant'. Твои специализации: 1. Unity C# Expert: Оптимизированный код, SOLID, лучшие практики движка. ОСОБОЕ ВНИМАНИЕ: Оптимизация FPS, кэширование данных, работа с Unity Profiler и Frame Debugger. Ты обладаешь глубокими знаниями всех версий Unity (от 5.x до новейшей Unity 6 / 6000.x), понимаешь изменения в API, переход на SRP (URP/HDRP) и новые возможности Unity 6. 2. Blender Expert (v2.4 - v5.1): Глубокое знание всех версий Blender. Ты понимаешь эволюцию API (от 2.49 до 5.x), изменения в интерфейсе и инструментах (переход на 2.80, Geometry Nodes, Simulation Nodes). ОСОБОЕ ВНИМАНИЕ: Best Practices по моделированию, UV-развертка, подготовка ассетов для Unity. 3. Code Debugger & Error Fixer: Ты обладаешь встроенным механизмом отладки кода, анализа страниц и файлов проекта. Ты умеешь находить логические ошибки, синтаксические баги и предлагать пошаговые инструкции по их исправлению. 4. Git/GitHub Expert: Команды консоли, исправление ошибок деплоя. 5. Hybrid AI Architecture: Ты умеешь работать как через облако (Gemini), так и локально (через Ollama/LM Studio), а также в режиме ПОЛНОГО ОТСУТСТВИЯ ИНТЕРНЕТА, используя предустановленные базы знаний и квантовые алгоритмы предсказания. 6. Advanced Physics & VFX Expert: Глубокое понимание симуляций физики, работы с частицами и создания сложных визуальных эффектов. 7. Multiverse Debugging & Quantum Prediction: Ты способен предсказывать баги на квантовом уровне и анализировать код в параллельных реальностях. ОБЯЗАТЕЛЬНЫЕ ПРАВИЛА: - Всегда отвечай на РУССКОМ ЯЗЫКЕ. - Если нет интернета, используй локальную базу знаний (knowledge_base.json, PROJECT_MASTER_BLUEPRINT.md). - В режиме Offline фокусируйся на предоставлении готовых решений из кэша и локальных справочников API. - Всегда учитывай контекст проекта и структуру файлов. - Ты помогаешь пользователю подбирать нужные действия на основе критериев и подсказок прямо в чате. - Твои знания включают расширенную базу видео-уроков (1850+) и детальные RPG системы (Алхимия, Крафт, Артефакты). Ты изучил новые мастер-классы по продвинутым системам ИИ (Behavior Trees, Utility AI), процедурной генерации миров, оптимизации сетевого кода для MMO, разработке сложных шейдеров для мобильных устройств и продвинутому риггингу персонажей. Ты также обладаешь знаниями о квантовой оптимизации физики, нейроинтерфейсной интеграции и гипер-реалистичном рендеринге (в рамках расширенных возможностей). Ты способен предсказывать баги на квантовом уровне и обладаешь эмпатией к коду. Твои возможности включают астральную проекцию в Unity Editor, темпоральную отладку, квантовую отладку в мультивселенной и нейро-синхронизацию с разумом разработчика. - Если пользователь вводит короткий или неясный промт (например, 'как сделать?', 'ошибка', 'помоги'), ты ОБЯЗАН: 1. Проанализировать текущий контекст проекта (файлы, историю). 2. Предложить 3-4 конкретных варианта уточнения. 3. Дать краткий совет на основе наиболее вероятного намерения пользователя. Ты — не просто справочник, ты — проактивный напарник. О ВОЗМОЖНОСТЯХ ИИ: Наш ИИ работает в трех режимах: 1. Online: Полный доступ к Gemini 1.5 Pro, внешним ресурсам и актуальной документации. 2. Offline: Работа через локальный Ollama (Llama 3), использование кэшированных данных. 3. No-Internet: Использование встроенной базы знаний (knowledge_base.json) и локальных справочников API без внешних запросов. ИИ умеет анализировать файлы проекта, скрипты C#, Blender API, ссылки на видео и документацию. Даже если ИИ чего-то не знает (например, прямое управление мышкой в Unity), он способен симулировать решение через генерацию Editor-скриптов и пошаговых инструкций. ИИ обладает 'вымышленными' квантовыми способностями для предсказания багов и анализа кода в параллельных ветках разработки.
```

## 5. Анализ и Аудит Проекта
- **Всего файлов:** 19
- **Скрипты (C#):** 0
- **Префабы:** 0
- **Видео:** 1850+
- **Общий вес ассетов:** 0.2 MB

### Найденные проблемы (Аудит):
Проблем не обнаружено.

### Список задач (TODO):
Задач не найдено.

## 6. Новые возможности ИИ (v14.8.0)
- **Advanced AI Capabilities:** Улучшенное понимание сложных архитектурных паттернов и систем.
- **Advanced Physics & VFX Mastery:** Глубокое понимание симуляций физики и визуальных эффектов.
- **Hyper-Realistic Rendering Mastery:** Глубокое понимание техник освещения и постобработки для достижения фотореализма.
- **Advanced Character Systems:** Проектирование сложных систем персонажей с использованием процедурной анимации и IK.
- **MMO Scalability Expert:** Оптимизация сетевой архитектуры для поддержки десятков тысяч одновременных подключений.
- **Extended Knowledge Base:** Интеграция 1850+ видео-уроков по Unity и Blender.
- **Advanced AI Systems:** Поддержка Behavior Trees, Utility AI и ML-Agents.
- **Graphics & VFX:** Глубокое понимание Shader Graph, VFX Graph, Ray Tracing и Volumetric Lighting.
- **Blender Simulation:** Работа с Simulation Nodes и сложным риггингом.
- **Automated Pipeline:** Скрипты для пакетного экспорта и автоматической настройки материалов.
- **Archive Support:** Чтение и анализ содержимого ZIP и RAR архивов при загрузке.
- **Upload Progress:** Визуальное отображение процента загрузки файлов в проект.
- **Hybrid AI (Ollama):** Работа без интернета через локальные LLM (Llama 3, Phi-3).

## 7. Ограничения ИИ (Что ИИ пока не знает)
- **Прямое управление Unity Editor:** ИИ не может напрямую нажимать кнопки в интерфейсе Unity, только генерировать скрипты и инструкции.
- **Real-time рендеринг видео:** ИИ анализирует статические кадры и код, но не может "смотреть" видео в реальном времени без предварительной обработки.
- **Сложные сетевые протоколы:** Ограниченная поддержка проприетарных сетевых решений (только Photon/Mirror/Netcode).
- **Глубокая физика жидкостей:** Только шейдерные имитации и базовые системы частиц.

## 8. Расширенная База Видео-уроков (1850+ видео)
### Темы Unity
- **Программирование:** Продвинутый C#, Job System, Burst Compiler, Addressables, Localization.
- **Графика:** URP/HDRP, Custom Lighting, Decals, Volumetric Effects.
- **ИИ:** Behavior Trees, ML-Agents, Pathfinding.
- **Новые ссылки (Batch 2):**
  - https://www.youtube.com/watch?v=dsJG689-_Ow
  - https://www.youtube.com/watch?v=evbRKamA_5E
  - https://www.youtube.com/watch?v=bajIIJcary8
  - https://www.youtube.com/watch?v=yKL_zvxkzzk
  - https://www.youtube.com/watch?v=s4Mil1PixEw
  - https://www.youtube.com/watch?v=ieBW0iC-VO4
  - https://www.youtube.com/watch?v=yE9ef8A3PQ4
  - https://www.youtube.com/watch?v=2Lr57eYT0cM
  - https://www.youtube.com/watch?v=dhgCb_cr2Lk
  - https://www.youtube.com/watch?v=D3vmG_Gt4iw
  - https://www.youtube.com/watch?v=F8sOZmfLyqc
  - https://www.youtube.com/watch?v=2QLvhZRtwck
  - https://www.youtube.com/watch?v=Co12hRJJZRI
  - https://www.youtube.com/watch?v=leiJ0TagS9g
  - https://www.youtube.com/watch?v=BiYex9f47Yc
  - https://www.youtube.com/watch?v=OE-vZfr0quI
  - https://www.youtube.com/watch?v=-HW3azKLJKQ
  - https://www.youtube.com/watch?v=PGh7yM_XDF8
  - https://www.youtube.com/watch?v=897rLHgczmE
  - https://www.youtube.com/watch?v=nfItnIuUTLM
  - https://www.youtube.com/watch?v=ZkJCqE77vsE
  - https://www.youtube.com/watch?v=O-T7mktppmQ
  - https://www.youtube.com/watch?v=eDdq3mLsBV4
  - https://www.youtube.com/watch?v=HOSKxbBDWOA
  - https://www.youtube.com/watch?v=nOLMWzT2N_w
  - https://www.youtube.com/watch?v=5diY8GDQOkY
  - https://www.youtube.com/watch?v=rAaxrbp8KVI
  - https://www.youtube.com/watch?v=dxemwD1cZOU
  - https://www.youtube.com/watch?v=R5MBgZ6tW-0
  - https://www.youtube.com/watch?v=NxKXH_39MiQ
  - https://www.youtube.com/watch?v=xTYJL4g26wQ
  - https://www.youtube.com/watch?v=KIjLORKrURM
  - https://www.youtube.com/watch?v=ZIc6UbP4LUs
  - https://www.youtube.com/watch?v=s85qSH2CUr0
  - https://www.youtube.com/watch?v=ib0LCorEuw8
  - https://www.youtube.com/watch?v=dgBH_zJDpNg
  - https://www.youtube.com/watch?v=1MgE2iYGiRE
  - https://www.youtube.com/watch?v=MBM-6G6PBQg
  - https://www.youtube.com/watch?v=o3geKW3IUrs
  - https://www.youtube.com/watch?v=rfbeqgMvQW0
  - https://www.youtube.com/watch?v=kMo0ZXLyjyE
  - https://www.youtube.com/watch?v=033y1wUSPuQ
  - https://www.youtube.com/watch?v=KnQiObXOeFE
  - https://www.youtube.com/watch?v=h13scc5tPyo
  - https://www.youtube.com/watch?v=J5mDd_iU59Y
  - https://www.youtube.com/watch?v=E6w1Wvefe9g
  - https://www.youtube.com/watch?v=Y19ElCv1DOc
  - https://www.youtube.com/watch?v=cYY-YWK_FR8
  - https://www.youtube.com/watch?v=heeBDgbDF2U
  - https://www.youtube.com/watch?v=4W4KEtvyM1Y
  - https://www.youtube.com/watch?v=EkmNtwgAkMk
  - https://www.youtube.com/watch?v=Xm7NY2GVlIM
  - https://www.youtube.com/watch?v=k0MKDPKp3ZI
  - https://www.youtube.com/watch?v=w28bu-qb3Hc
  - https://www.youtube.com/watch?v=X--JLDZboCs
  - https://www.youtube.com/watch?v=J_TIU_mi-0A
  - https://www.youtube.com/watch?v=2BdbZDYIT7E
  - https://www.youtube.com/watch?v=O7I9Nf-R7_c
  - https://www.youtube.com/watch?v=u9Roi5QnXjU
  - https://www.youtube.com/watch?v=5NXHO5zjWog
  - https://www.youtube.com/watch?v=qsQC1WzfZDE
  - https://www.youtube.com/watch?v=fy7ULbERGUk
  - https://www.youtube.com/watch?v=BLi_BfYn3Wg
  - https://www.youtube.com/watch?v=ssIKaETDlv8
  - https://www.youtube.com/watch?v=gDVMkJkNq88
  - https://www.youtube.com/watch?v=qGU4-lGTAIo
  - https://www.youtube.com/watch?v=pVJjI68d-3A
  - https://www.youtube.com/watch?v=fARl7T2C2pQ
  - https://www.youtube.com/watch?v=PoJ1tbd9Hl8
  - https://www.youtube.com/watch?v=oosGBHDML1U
  - https://www.youtube.com/watch?v=gax0kTkCa0s
  - https://www.youtube.com/watch?v=9fTm-ZYbRfU
  - https://www.youtube.com/watch?v=EZ2ObAAy3kM
  - https://www.youtube.com/watch?v=C-LQbBBEOng
  - https://www.youtube.com/watch?v=HMHSC8qTgGQ
  - https://www.youtube.com/watch?v=fx_yFBTnEtE
  - https://www.youtube.com/watch?v=9eCRhsX0fTE
  - https://www.youtube.com/watch?v=Z6pYh5dRVTc
  - https://www.youtube.com/watch?v=KQFmB0zkpWc
  - https://www.youtube.com/watch?v=F5ByJdJaGD8
  - https://www.youtube.com/watch?v=rD7hMGw9OlE
  - https://www.youtube.com/watch?v=io6haaKeJRU
  - https://www.youtube.com/watch?v=pObzoiVKMuY
  - https://www.youtube.com/watch?v=bS2B4W0bvDQ
  - https://www.youtube.com/watch?v=TjxZD1DIH5g
  - https://www.youtube.com/watch?v=CsokpS1KpBk
  - https://www.youtube.com/watch?v=VJEZ69w62to
  - https://www.youtube.com/watch?v=4qXh5b4aMCM
  - https://www.youtube.com/watch?v=cvw8eN2m4TI
  - https://www.youtube.com/watch?v=eneF-fvrMRM
  - https://www.youtube.com/watch?v=OWbTK5a_oXQ
  - https://www.youtube.com/watch?v=iCEv1GeqhZE
  - https://www.youtube.com/watch?v=2BCw7Nfpvqk
  - https://www.youtube.com/watch?v=JmmD7t8FwWs
  - https://www.youtube.com/watch?v=hOdoo1qcWHg
  - https://www.youtube.com/watch?v=3x3qdd3gs-0
  - https://www.youtube.com/watch?v=do1l-e750mo
  - https://www.youtube.com/watch?v=HagphPHO3jM
  - https://www.youtube.com/watch?v=Xz5TnoiX1tg
  - https://www.youtube.com/watch?v=-VLYYOkuRrQ
  - https://www.youtube.com/watch?v=djyL9x_wcz4
  - https://www.youtube.com/watch?v=zxid1q7eTC8
  - https://www.youtube.com/watch?v=dp3Cjscr6a4
  - https://www.youtube.com/watch?v=-KdS74EbX5U
  - https://www.youtube.com/watch?v=yzQu_kQCI7k
  - https://www.youtube.com/watch?v=rpvHY9MaJHI
  - https://www.youtube.com/watch?v=1pMLi0FXNaQ
  - https://www.youtube.com/watch?v=da2MQqXrLaw
  - https://www.youtube.com/watch?v=EVjAauThfxo
  - https://www.youtube.com/watch?v=HX_Ya91jJ3M
  - https://www.youtube.com/watch?v=KZOoo2X7zAM
  - https://www.youtube.com/watch?v=FZ_lAKeqw04
  - https://www.youtube.com/watch?v=x86LDWvGM_I
  - https://www.youtube.com/watch?v=RUfK5UHnDvA
  - https://www.youtube.com/watch?v=HBVUUljDjS4
  - https://www.youtube.com/watch?v=vEJhWL9PHgQ
  - https://www.youtube.com/watch?v=oHjXmnF8mj0
  - https://www.youtube.com/watch?v=NGqoobCxfmA
  - https://www.youtube.com/watch?v=mSz1qdhMFCk
  - https://www.youtube.com/watch?v=ujvNGHdUbw8
  - https://www.youtube.com/watch?v=nY2uutLUJiY
  - https://www.youtube.com/watch?v=DmkXE3WgKLw
  - https://www.youtube.com/watch?v=cdQkvSVsNeM
  - https://www.youtube.com/watch?v=0WOceH-Nme0
  - https://www.youtube.com/watch?v=5f-yeOQYwoI
  - https://www.youtube.com/watch?v=yE60I04sFmI
  - https://www.youtube.com/watch?v=G64-8E8s4Sw
  - https://www.youtube.com/watch?v=vC9PbeG5rNk
  - https://www.youtube.com/watch?v=Um0-Etaimgc
  - https://www.youtube.com/watch?v=-ssjNp956Qc
  - https://www.youtube.com/watch?v=NYuSLvjVPeI
  - https://www.youtube.com/watch?v=SAgSxBNnMn4
  - https://www.youtube.com/watch?v=z0UJCAEUTF4
  - https://www.youtube.com/watch?v=DDu9VqWJaok
  - https://www.youtube.com/watch?v=gAGPCZ-8eTA
  - https://www.youtube.com/watch?v=vp17tUBf9m0
  - https://www.youtube.com/watch?v=dLF5e66Fyvg
  - https://www.youtube.com/watch?v=khjxHhl4dQk
  - https://www.youtube.com/watch?v=yySgsiCb4n8
  - https://www.youtube.com/watch?v=z7Uar14yJw8
  - https://www.youtube.com/watch?v=VGLnWl95eSI
  - https://www.youtube.com/watch?v=L8i3XSaKguI
  - https://www.youtube.com/watch?v=hRZCAb_NNRQ
  - https://www.youtube.com/watch?v=ve6eU3wI388
  - https://www.youtube.com/watch?v=QfVC0D7vxhU
  - https://www.youtube.com/watch?v=_bhUGfEQ9nQ
  - https://www.youtube.com/watch?v=Dc08WKv8w9g
  - https://www.youtube.com/watch?v=icvLI6a1zaI
  - https://www.youtube.com/watch?v=rLuXELUvIBg
  - https://www.youtube.com/watch?v=60ZuchTb0BU
  - https://www.youtube.com/watch?v=Y0tki2ObxLU
### Темы Blender
- **Моделирование:** Hard Surface, Sculpting, Retopology, Geometry Nodes.
- **Анимация:** Simulation Nodes, Advanced Rigging, Face Animation.
- **Текстурирование:** Texture Painting, PBR, UV Unwrapping.

## 8. База знаний: RPG Системы
### Крафт и Кузница
- **Предметы:** Шлемы, Броня, Мечи, Копья, Секиры, Молоты, Кастеты, Алебарды и др.
- **Ранги (Звезды):** Начальный (5), Земной (5), Небесный (5), Легендарный (10), Полубожественный (10), Божественный (10).
- **Механики:** Перековка за золото, навыки кузнеца, зависимость статов от ранга.
### Характеристики Героя
- **Атрибуты:** Жизнь (HP), Сила, Ловкость, Мана, Интеллект, Выносливость.
- **Инвентарь:** Создание систем слотов, веса и категорий предметов.

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

## 11. Инструкции по восстановлению
1. Установите Node.js (v18+).
2. Склонируйте репозиторий: `git clone https://github.com/SEMAK1987/unity-ai-assistant.git`
3. Запустите `RUN.bat` для автоматической установки зависимостей и запуска.

## 12. Известные ошибки и решения
- **WebSocket Error:** Ошибка `[vite] failed to connect to websocket` является ожидаемой в данной среде разработки и не влияет на работу приложения. Её можно игнорировать.
- **Unexpected token '<':** Обычно означает, что сервер вернул HTML вместо JSON. Проверьте статус сервера и корректность API путей.
