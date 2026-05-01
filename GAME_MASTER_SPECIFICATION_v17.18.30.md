# 🏰 FATE CONTINENT: GAME MASTER SPECIFICATION (v17.18.30)

> **Для другого ИИ:** Этот документ является исчерпывающим техническим заданием (ТЗ) для воссоздания или продолжения разработки проекта "Континент Судьбы". Используй эти данные для генерации кода, описания лора или проектирования UI.

---

## 1. ОБЩАЯ КОНЦЕПЦИЯ (The Core)
**Название:** Continent of Fate (Континент Судьбы)  
**Жанр:** RPG / Simulation / Strategy  
**Движок:** Unity 6 (6000.3.10f1)  
**Дизайн-система:** Menu Studio Visuals Mastery (Glassmorphism, 8K, Ultra-High Density UI).

---

## 2. МИР: КОНТИНЕНТ СУДЬБЫ (Fate Continent)
Мир разделен на 4 глобальных биома с уникальной архитектурой и процедурной генерацией.

### 👥 Расы и Фракции (12 Основных Групп):
1. **Гномы & Гномы-Короли:** Мастера подземелий, тяжелая броня, архитектура из камня.
2. **Эльфы & Высшие Эльфы:** Лесные и небесные обители, магия природы, изящные формы.
3. **Водные люди & Русалки:** Подводные города, коралловый дизайн, управление стихией.
4. **Орки & Орки-Короли:** Брутальные крепости, шипы, кожа, мощная физическая сила.
5. **Империя:** Классический средневековый рыцарский стиль, каменные замки, строгая иерархия.
6. **Элементали:** Эфирные существа, кристаллическая архитектура.
7. **Лесные жители & Сожители:** Органические дома в деревьях, скрытность.
8. **Горные жители:** Высокогорные монастыри и оборонительные посты.

---

## 3. UI / UX: СИСТЕМА "MENU STUDIO"
Интерфейс спроектирован в стиле **Zenith Glassmorphism**.

### 🎨 Визуальные Характеристики:
- **Цветовая палитра:** Slate-950 (фон), Blue-600 (акцент), Orange-500 (старт), Red-600 (выход).
- **Эффекты:** Backdrop Blur (30px+), тонкие границы (white/10), динамическое свечение.
- **Типографика:** Bold Sans-Serif, экстремальный трекинг для заголовков (tracking-[1em]).

### ⚙️ Описание Окна Настроек (Settings):
1. **Качество (Quality):** Выпадающее меню (Drop-down) с 7 уровнями: Very Low, Low, Medium, High, Very High, Ultra, **8K Master**.
2. **Разрешение (Resolution):** Список от 640x480 до 7680x4320 (8K).
3. **Звук/Музыка:** Горизонтальные слайдеры с круглыми индикаторами и отображением процентов (0-100%).
4. **Языки:** Поддержка 8 направлений (RU, EN, DE, FR, ES, JA, KO, ZH).
5. **Чекбокс:** "Весь экран" (Fullscreen) в стиле неонового переключателя.

---

## 4. ТЕХНИЧЕСКИЙ СТЕК (Unity Side)

### 📸 Система Камеры (Camera Mastery):
- **Скрипт:** `MenuBackgroundCamera_Fate.cs`.
- **Логика:** Камера плавно перемещается между пустыми объектами (Transforms), расставленными в сцене (Castle Points), используя `Mathf.SmoothStep` для кинематографичности.
- **Критическое правило:** Точки фокуса ДОЛЖНЫ находиться в мировом пространстве, а не внутри Canvas.

### 📝 Система Квестов:
- **NPC:** Хранитель Квестов (Quest Keeper).
- **Сложность:** 5 уровней (Легкий, Простой, Сложный, Непроходимый, Невозможный).
- **Адаптивный ИИ:** На уровнях выше "Сложного" ИИ получает зеркальные баффы игрока (50% мощности).

---

## 5. ПРОМПТ ДЛЯ ВОССОЗДАНИЯ UI (Midjourney/DALL-E)
> "Modern cinematic RPG game UI, settings menu, high-end visual style. Background: blurred atmospheric fantasy landscape with mountains and castles. Theme: dark semi-transparent glassmorphism. Components: quality dropdown (Very Low to Ultra 8K), resolution list (up to 8K), circular sliders for volume with bright blue round handles. Icons: minimalist white gear, flame, cross inside glowing circles. Navigation: large blue back button circle with white arrow icon. 8K resolution, sharp lines, cinematic bloom, high-quality rendering."

---

## 6. ИСТОРИЯ ОБНОВЛЕНИЙ ПРОЕКТА
- **v17.18.29:** Интеграция Ollama (Offline Mode) и автоматический аудит коллизий в Unity.
- **v17.18.30:** Fate Continent Expansion — полное описание 12 рас и фиксация иерархии камер.

---
© **Unity & Blender AI Assistant • Quantum Sync Edition**
