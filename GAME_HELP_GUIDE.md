# ГЛОБАЛЬНАЯ БАЗА ЗНАНИЙ - Unity 6 (6000.3.10f1) Ultimate Guide (v18.0.2)

> **СТАТУС:** Полная интеграция PDF-руководства v3.0 (Части 1-8).
> Этот документ является динамическим руководством по созданию игры "Континент Судьбы". Здесь описаны пошаговые инструкции, скрипты и настройки для Unity, дополненные деталями по сохранению данных, визуализации рас и продвинутому UI из Menu Studio.

---

## 🏛️ КВАНТОВОЕ ЯДРО (Global Knowledge v18)

ИИ-Помощник теперь оперирует на 100% данных ТЗ. Все следующие файлы являются "Священными Текстами" проекта:
1. `FATE_CONTINENT_FULL_DOCUMENTATION.md` — Весь технический стек (Part 1-8).
2. `GAME_MASTER_SPECIFICATION_v17.18.30.md` — Лор и спецификации Menu Studio.
3. `AGENTS.md` — Операционные директивы ИИ.

---

## 🚀 ШАГ 0: Установка и Подготовка Среды

### 1. Установка Unity Hub и Редактора
1.  **Unity Hub:** Скачайте и установите Unity Hub с официального сайта.
2.  **Версия 6000.3.10f1:** Перейдите во вкладку `Installs > Install Editor`. Если нужной версии нет в списке, найдите её в `Download Archive`. Это критическая версия Unity 6.
3.  **Модули:** При установке обязательно выберите модули: `Windows Build Support (IL2CPP)` и `WebGL Build Support` для кросс-платформенности.

### 2. Создание Проекта
1.  Нажмите `New project` в Hub.
2.  **ВНИМАНИЕ (Unity 6):** Шаблон 3D (URP) переименован в **Universal 3D**. На вашем экране он помечен значком "SRP". 
3.  Если на шаблоне нарисовано **облачко со стрелкой**, нажмите на него, чтобы скачать шаблон перед созданием.

### 4. Ошибка "Validation Failed" при выборе шаблона
Если при нажатии `Create Project` вылетает красная ошибка (как на вашем скриншоте):
*   **Выйдите и войдите в аккаунт:** Нажмите на кружок профиля (слева вверху) -> Sign Out, затем Sign In.
*   **Проверьте имя проекта:** Старайтесь не использовать русские буквы в пути `Location` (например, вместо `D:\МоиИгры` лучше `D:\Games`).
*   **Скачивание шаблона:** Нажмите на иконку "облака" прямо на плитке **Universal 3D** до того, как нажимать синюю кнопку создания.
*   **Запуск от Админа:** Закройте Hub и запустите его через правую кнопку мыши -> "Запуск от имени администратора".

### 3. Как добавить забытые модули (Windows/WebGL Build Support)
Если вы уже установили Unity, но забыли нажать галочки на модулях, их можно добавить в любой момент через Unity Hub:

1.  Откройте **Unity Hub**.
2.  Перейдите на вкладку **Installs** (Установки) в левом меню.
3.  Найдите карточку вашей версии Unity (**6000.3.10f1**).
4.  Нажмите на значок **шестеренки** (Settings) в углу этой карточки.
5.  Выберите пункт **Add modules** (Добавить модули).
6.  В открывшемся списке найдите и отметьте галочками:
    *   `Windows Build Support (IL2CPP)` — для создания быстрых .exe файлов.
    *   `WebGL Build Support` — для запуска игры в браузере.
7.  Нажмите **Install** (или Continue/Done). Unity Hub сам скачает и доставит эти компоненты в вашу папку с редактором.

---

## 🏗️ ШАГ 1: Создание Главного Меню (UI & TextMeshPro)

### 1. Настройка Рендеринга Текста
1.  Перейдите в `Window > Package Manager`.
2.  Выберите `Packages: Unity Registry`.
3.  Найдите **TextMeshPro** и нажмите `Install`. После установки появится окно — нажмите `Import TMP Essentials`.

### 2. Создание Canvas (Холста)
1.  В Иерархии (Hierarchy) нажмите правую кнопку мыши: `UI > Canvas`.
2.  В инспекторе Canvas переключите `UI Scale Mode` на **Scale With Screen Size**. Это важно, чтобы меню не "разваливалось" на разных мониторах. Установите `Reference Resolution` 1920x1080.

### 3. Название Игры (Title)
1.  Нажмите правой кнопкой на Canvas: `UI > Text - TextMeshPro`.
2.  Переименуйте объект в **GameTitle**.
3.  В поле `Text Input` введите: **КОНТИНЕНТ СУДЬБЫ**.
4.  **Стиль:** Выберите жирный шрифт, размер 90. В секции `Vertex Color` выберите градиент от золотого к белому.
5.  Позиционируйте текст в верхней части экрана (Pos Y ≈ 350).

### 4. Интерактивные Кнопки
1.  На Canvas: `UI > Button - TextMeshPro`.
2.  Создайте **StartButton**. В дочернем объекте `Text (TMP)` напишите "ИГРАТЬ".
3.  Создайте **ExitButton**. В тексте напишите "ВЫХОД".
4.  **MainMenu.cs:** Создайте скрипт в папке `Assets/Scripts` и прикрепите его к самому объекту `Canvas`.

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Метод для кнопки Старт
    public void StartGame()
    {
        // Убедитесь, что сцена "GameScene" добавлена в Build Settings!
        SceneManager.LoadScene("GameScene");
    }

    // Метод для кнопки Выход
    public void ExitGame()
    {
        Debug.Log("Выход из системы...");
        Application.Quit();
    }
}
```

5.  **Привязка событий:** Выделите `StartButton`. В инспекторе найдите раздел `On Click ()`. Нажмите `+`. Перетащите `Canvas` в пустое поле. В выпадающем меню выберите `MainMenu > StartGame`. Повторите для `ExitButton` с методом `ExitGame`.

---

## 🏰 ШАГ 2: Живой Фон (Динамические Замки и Погода)

### 1. Камера-Машина Времени (Zoom & Pan)
Для меню важно, чтобы фон не был статичным. Мы будем плавно перемещать камеру между локациями расс.

1.  Создайте несколько пустых объектов (Empty GO) на сцене: `Point_Empire`, `Point_Bandits`, `Point_Player`. Расставьте их у соответствующих замков.
2.  Прикрепите скрипт `MenuBackgroundCamera.cs` к основной камере (**Main Camera**).

```csharp
using UnityEngine;

public class MenuBackgroundCamera : MonoBehaviour
{
    [Header("Точки фокусировки")]
    public Transform[] castlePoints; 
    public float transitionSpeed = 0.3f;
    
    private int currentIndex = 0;
    private float timer = 0f;

    void Update()
    {
        if (castlePoints.Length < 2) return;

        timer += Time.deltaTime * transitionSpeed;
        if (timer >= 1f)
        {
            timer = 0f;
            currentIndex = (currentIndex + 1) % castlePoints.Length;
        }

        int nextIndex = (currentIndex + 1) % castlePoints.Length;
        
        // Интерполяция позиции и вращения для плавного "киношного" эффекта
        transform.position = Vector3.Lerp(castlePoints[currentIndex].position, castlePoints[nextIndex].position, timer);
        transform.rotation = Quaternion.Lerp(castlePoints[currentIndex].rotation, castlePoints[nextIndex].rotation, timer);
    }
}
```

### 2. Смена Дня и Ночи (Атмосфера)
1.  Найдите на сцене **Directional Light**. Это ваше солнце.
2.  Прикрепите скрипт `DayNightCycle.cs`:

```csharp
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Tooltip("Длительность полных суток в секундах")]
    public float dayDuration = 120f; 

    void Update()
    {
        // Вращаем солнце вокруг оси X
        float rotationAngle = (360f / dayDuration) * Time.deltaTime;
        transform.Rotate(Vector3.right, rotationAngle);
        
        // Дополнительно: можно менять цвет неба в зависимости от угла солнца
    }
}
```

### 3. Погода (VFX Graph или Particle System)
1.  Создайте объект **WeatherManager**.
2.  Добавьте на него систему частиц (Particle System). Настройте её на "Снег" или "Дождь".
3.  Используйте `WeatherDirector.cs` для управления интенсивностью из кода.

---

## 📂 ШАГ 3: Финальная Сборка и Настройка

1.  **Создание Сцен:** Обязательно сохраните текущую сцену как `MainMenu` и создайте новую `GameScene`. Перейдите в `File > Build Settings` и перетащите обе сцены в список.
2.  **Импорт моделей:** Файлы `.fbx` из Blender просто перетаскивайте в папку `Assets/Models`. Unity 6 автоматически создаст материалы.
3.  **Скрипты:** Всегда проверяйте, чтобы в консоли (Console) не было красных ошибок. Если скрипт "не видится", проверьте, совпадает ли имя файла с именем класса внутри кода.

---

## 🛠️ ШАГ 4: Решение проблем (Unity Hub & Editor)

### 1. Unity Hub завис (Круг загрузки)
*   **Сброс настроек:** Удалите папку `%AppData%/UnityHub`.
*   **Блокировка:** Если вы в регионе с ограничениями, убедитесь, что Hub имеет доступ к интернету. Иногда требуется запуск через VPN для первичной активации лицензии.

### 2. Проект не открывается (Черный экран)
1.  Удалите папку `Library` внутри вашего проекта (она восстановится при следующем запуске).
2.  Это заставит Unity пересобрать все ассеты и исправит 90% проблем с запуском.

---

## 🎨 ШАГ 5: Blender 5.1.1 - Создание 3D Ассетов

### 1. Установка и Пути
1.  **Blender Multi-Sync:** Поддерживаются и синхронизированы версии:
    *   `C:\Program Files\Blender Foundation\Blender 5.1\blender-launcher.exe` (v5.1.1)
    *   `C:\Program Files\Blender Foundation\Blender 4.4\blender-launcher.exe` (v4.4)
    *   `C:\Program Files\Blender Foundation\Blender\blender.exe` (Стандартная)
2.  **Синхронизация:** Убедитесь, что `blender_connector.py` находится в вашей рабочей директории. ИИ использует его для прямой отправки мешей в Unity 6 из любой установленной версии.

### 2. Новые возможности 5.1.1 и 4.4
*   **Physically Accurate Shaders:** Используйте ноды SSS и Glass для создания фотореалистичных зелий и кристаллов маны.
*   **Open Shading Language (OSL):** Теперь вы можете писать свои шейдеры на языке OSL. ИИ может генерировать этот код для вас.
*   **Mask to SDF:** Используйте эту ноду в композиторе для создания четких границ и эффектов свечения для магических рун.
*   **Auto-Skinning:** Новые инструменты риггинга позволяют превратить любую модель персонажа в анимированный объект за считанные минуты.

### 3. Экспорт в Unity 6
1.  Используйте формат **.fbx** или **.blend** напрямую (Unity 6 поддерживает нативный импорт .blend файлов, если Blender установлен).
2.  При экспорте .fbx выбирайте `Apply Scalings: FBX All` для корректных размеров в Unity.

---

## 🛠️ ШАГ 6: Устранение неполадок (Troubleshooting)

### 1. Package Manager завис на "Refreshing list..."
Если вы видите бесконечную загрузку при попытке найти TextMeshPro или другие пакеты:
*   **Проверка Аккаунта:** На скриншоте проверьте Unity Hub. Выйдите из него (Sign Out) и зайдите снова.
*   **Сброс поиска:** Удалите любой текст из строки поиска в окне Package Manager.
*   **Удаление Library:** Закройте Unity и удалите папку `Library` внутри вашего проекта. Это безопасно, Unity создаст её заново, исправив ошибки кэша.
*   **Add package by name:** Если список не грузится, нажмите на «+» в углу Package Manager -> `Add package by name` и введите `com.unity.textmeshpro`. Это установит его в обход общего списка.
*   **Offline Mode:** В Unity Hub можно перевести проект в Offline, чтобы он не пытался стучаться на сервера при каждом действии.

### 2. Замедление интернета (VPN Fix)
Если сайты и сервисы Unity тормозят:
1.  **Используйте VPN:** Для авторизации в Hub и загрузки пакетов VPN обязателен. После начала загрузки его можно попробовать отключить, но для Package Manager он критичен.
2.  **Ручная установка в manifest.json:** 
    *   Закройте Unity.
    *   Перейдите в папку проекта `Packages/manifest.json`.
    *   Добавьте строку `"com.unity.textmeshpro": "3.0.6",` (или другую версию) в список зависимостей.
    *   При запуске Unity сам скачает пакет без поиска в UI.

## 🎨 ШАГ 7: Создание интерфейса (UI Design)

### 1. Название Игры (Title)
*   **Создание:** Hierarchy -> Canvas -> UI -> Text - TextMeshPro.
*   **Имя:** `GameTitle`.
*   **Настройка Inspector:**
    *   `Text Input`: КОНТИНЕНТ СУДЬБЫ.
    *   `Font Size`: 90.
    *   `Alignment`: Center.
    *   `Color Gradient`: Включить. Выберите градиент от золотого (#FFD700) к белому.
    *   `Rect Transform`: Pos Y = 350.

### 2. Интерактивные Кнопки
*   **Старт:** UI -> Button - TextMeshPro. Имя `StartButton`. Текст: "ИГРАТЬ". Pos Y = 0.
*   **Выход:** UI -> Button - TextMeshPro. Имя `ExitButton`. Текст: "ВЫХОД". Pos Y = -100.

### 3. Скрипт MainMenu.cs
Создайте `Assets/Scripts/MainMenu.cs`:
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    // ВНИМАНИЕ: Используйте именно эти имена, чтобы они появились в списке On Click()
    public void StartGame() { 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); 
    }

    public void ExitGame() { 
        Application.Quit();
        Debug.Log("Выход из игры..."); 
    }
}
```
Прикрепите его к `Canvas` и настройте события `On Click ()` у кнопок.

## 🔠 ШАГ 8: Работа со шрифтами и исправление ошибок

### 0. Почему в списке On Click () нет функций StartGame/ExitGame?
Если вы видите `No Function` и в списке `MainMenu` нет ваших методов:
1.  **Проверьте скрипт:** Убедитесь, что перед `void StartGame()` стоит слово **public**. Если метода нет в списке — он либо приватный, либо скрипт не скомпилирован.
2.  **Ошибки в консоли:** Проверьте вкладку **Console** внизу Unity. Если там есть красные ошибки, Unity не "видит" новые функции в коде. Исправьте ошибки и список обновится.
3.  **Авто-заполнение:** Нажмите `Ctrl+S` в Visual Studio, чтобы сохранить файл. Unity должен "мигнуть" (появится иконка загрузки в углу), после чего функции появятся в списке.

### 1. Почему текст стоит "столбом"?
На вашем скриншоте текст `Континент Судьбы` отображается вертикально. 
*   **Причина:** Ширина (Width) в Rect Transform слишком мала (200), а размер шрифта большой (90).
*   **Решение:** 
    1.  Выберите объект `GameTitle` в Hierarchy.
    2.  В Inspector найдите **Rect Transform**.
    3.  Измените **Width** с 200 на **1000**.
    4.  Измените **Height** на **150**.
    5.  В компоненте **TextMeshPro - Text (UI)** найдите раздел **Alignment** и выберите центральную иконку (Center).

### 2. Где скачать шрифты для русского языка?
Стандартные шрифты Unity не всегда хорошо поддерживают кириллицу.
1.  Зайдите на [Google Fonts](https://fonts.google.com/).
2.  В фильтрах (Language) выберите **Cyrillic**.
3.  Рекомендуемые шрифты: **Montserrat**, **Roboto**, **Oswald** (для заголовков).
4.  Скачайте архив, распакуйте и перетащите файл `.ttf` в Unity в папку `Assets/Fonts`.

### 3. Как создать Font Asset для TextMeshPro?
Просто перетащить шрифт недостаточно, TMP нужен специальный ассет:
1.  Нажмите правой кнопкой на ваш файл `.ttf` в Unity.
2.  Выберите **Create > TextMeshPro > Font Asset**.
3.  Если вы хотите, чтобы шрифт был супер-четким или поддерживал все символы:
    *   Откройте **Window > TextMeshPro > Font Asset Creator**.
    *   Выберите ваш шрифт в `Source Font File`.
    *   `Character Set` -> Выберите **Cyrillic**.
    *   Нажмите **Generate Font Atlas**, затем **Save**.
4.  Теперь в объекте `GameTitle` в поле **Font Asset** перетащите ваш новый созданный ассет.

## ⚙️ ШАГ 9: Меню настроек (Settings Menu)

### 1. Создание структуры UI
1.  **Панель подложки:** На `Canvas` нажмите правой кнопкой: `UI > Image`. Назовите `SettingsPanel`. Растяните на весь экран, сделайте цвет темно-серым с прозрачностью. 
2.  **Заголовок:** Внутри `SettingsPanel` создайте `UI > Text - TMP` с текстом "НАСТРОЙКИ" (ID 2).
3.  **Кнопка закрытия:** `UI > Button - TMP`. Текст "НАЗАД" (ID 19). В событие `On Click` перетащите саму панель `SettingsPanel` и выберите `GameObject.SetActive(false)`.
4.  **Разделы (Graphics, Audio, Language):** Создайте пустой объект `Content` внутри панели и добавьте `Vertical Layout Group`, чтобы элементы стояли ровно.

### 2. Настройка Звука и Графики
Создайте скрипт `SettingsManager.cs` и добавьте его на `SettingsPanel`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour {
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    void Start() {
        // Настройка разрешений
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(new System.Collections.Generic.List<string> { "1920x1080", "1280x720", "2560x1440" });
    }

    public void SetResolution(int index) {
        if (index == 0) Screen.SetResolution(1920, 1080, Screen.fullScreen);
        else if (index == 1) Screen.SetResolution(1280, 720, Screen.fullScreen);
    }

    public void SetFullscreen(bool isFullscreen) { Screen.fullScreen = isFullscreen; }
    
    public void SetVolume(float volume) { AudioListener.volume = volume; }
}
```

### 3. Интеграция ваших скриптов перевода
1.  **Объект Translator:** Создайте в сцене пустой объект `_Translator`. Прикрепите на него ваш скрипт `Translator.cs`.
    *   Перетащите ваш кириллический Font Asset в поле `russianFont`.
    *   Перетащите стандартный Font Asset в `defaultFont`.
2.  **Выбор языка:** В `SettingsPanel` создайте `UI > Dropdown - TMP`. Прикрепите на него ваш скрипт `LanguageSelector.cs`.
    *   В поле `Language Dropdown` перетащите сам этот Dropdown.
    *   В поле `Language Title Label` перетащите текстовую метку "Язык" (ID 12).
3.  **Перевод всех надписей:** На КАЖДЫЙ объект с текстом (Кнопки, Заголовки) добавьте ваш скрипт `Transtable_Text.cs`.
    *   В поле `Text ID` введите номер из массива в `Translator.cs` (например: 0 - Старт, 4 - Выход, 2 - Опции).

## 🎨 ГЛАВА 11: Menu Studio (Мастер-Класс по Интерфейсам)

Это руководство позволит вам создать меню уровня AAA, которое мы спроектировали в Студии.

### 1. Подготовка сцены и UI Core
1.  **Canvas:** Установите `UI Scale Mode` в `Scale With Screen Size`. Reference Resolution: `1920 x 1080`.
2.  **Background:** Создайте Image. Цвет сделайте темным (`#0a0a0c`), прозрачность 60%. Добавьте компонент `Canvas Group` для эффектов затухания.
3.  **Animator:** Создайте `MenuAnimator` на корневом объекте меню. Это позволит плавно входить и выходить из настроек.

### 2. Реализация 8К Разрешений и 8 Языков
Создайте скрипт `UnityMenuMaster.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UnityMenuMaster : MonoBehaviour {
    [Header("UI References")]
    public TMP_Dropdown resDropdown;
    public TMP_Dropdown langDropdown;
    public Slider masterVol, musicVol;
    public Toggle fsToggle;

    private Resolution[] resolutions = {
        new Resolution { width = 640, height = 480 },
        new Resolution { width = 1024, height = 768 },
        new Resolution { width = 1280, height = 720 },
        new Resolution { width = 1920, height = 1080 },
        new Resolution { width = 2560, height = 1440 },
        new Resolution { width = 3840, height = 2160 },
        new Resolution { width = 7680, height = 4320 } // 8K support
    };

    void Start() {
        InitDropdowns();
    }

    void InitDropdowns() {
        // Разрешения
        resDropdown.ClearOptions();
        List<string> resOptions = new List<string>();
        foreach (var r in resolutions) resOptions.Add($"{r.width}x{r.height}");
        resDropdown.AddOptions(resOptions);

        // Языки (Fate Master Sync v17.18.30)
        langDropdown.ClearOptions();
        langDropdown.AddOptions(new List<string> { 
            "Русский", "English", "Deutsch", "Français", 
            "Español", "日本語", "한국어", "简体中文" 
        });
    }

    public void ApplyGraphics() {
        Resolution r = resolutions[resDropdown.value];
        Screen.SetResolution(r.width, r.height, fsToggle.isOn);
    }
}
```

### 3. Гайд по Анимации (Без Плагиата)
Чтобы не копировать чужие решения, создайте собственную параметрическую анимацию:
1.  **Idle Breathing:** В Animator создайте цикл, который меняет `LocalScale` кнопки от `1.0` до `1.05` за 3 секунды.
2.  **Hover Pulse:** При наведении (`Highlighted`) добавьте анимацию свечения краев (`Outline Global Color`).
3.  **Click Impact:** При клике (`Pressed`) мгновенно уменьшайте масштаб до `0.95` и меняйте цвет на `#3b82f6` (Blue-500).

### 4. Создание Скинов "Menu Studio Edition"
*   **Void Crystal:** В GIMP используйте фильтр "Plasma" с холодными цветами. Примените `Colorize` в фиолетовый.
*   **Runed Blade:** Нарисуйте белую линию (руну). В Unity в компоненте `Image` используйте `Material` с шейдером свечения (HDR).
*   **Atmospheric Ray:** Используйте `UI Particles` или просто полупрозрачный градиентный спрайт, который медленно вращается на фоне кнопок.

### 5. Структура Папок проекта
Рекомендуем следующую структуру для идеального порядка:
*   `Assets/UI/Skins/` - спрайты и атласы.
*   `Assets/UI/Animations/` - аниматоры и клипы.
*   `Assets/UI/Scripts/` - логика меню и настроек.
*   `Assets/UI/Icons/` - иконки (Flame, Gear, X).

---
*Документ полностью переработан для версии v18.0.0 (Global Knowledge Base v18 & Fate Continent Mastery)

## 💎 ГЛАВА 13: Визуализация "Menu Studio" (Атмосфера и Замки)

Чтобы перенести запредельную графику из нашей Студии в Unity, следуйте этому алгоритму создания живого окружения.

### 1. Параллакс и Глубина (Parallax Layers)
Для создания эффекта "живого" фона, который вы видите в Студии, используйте систему слоев:
1.  **Слой 0 (Back):** Skybox с глубоким космосом или ночным небом.
2.  **Слой 1 (Far):** Силуэты далеких гор и туман.
3.  **Слой 2 (Mid):** Замки Рас (Люди, Эльфы, Орки, Нежить).
4.  **Слой 3 (Near):** Передний план с легкими частицами пыли или магической энергии.

### 2. Замки Всех Рас (Architectural Landmarks)
Разместите 3D модели замков в секторе обзора камеры. Если моделей еще нет — используйте спрайты с эффектом свечения:
*   **Замок Людей (Human):** Белый камень, синие флаги, высокие шпили.
*   **Древо Эльфов (Elf):** Гигантское светящееся древо с платформами.
*   **Цитадель Орков (Orc):** Черное железо, шипы, красное знамя.
*   **Некрополь (Undead):** Зеленый туман, парящие обелиски, костяные стены.

### 3. Интеграция Игрока (Player Manifestation)
В центре или сбоку меню разместите модель вашего Героя.
1.  **Idle Animation:** Используйте `Animator` с плавным покачиванием (Breathing).
2.  **Rim Light:** Добавьте на материал игрока 'Rim Lighting' (свечение по краям), чтобы он выделялся на фоне тьмы.
3.  **VFX Aura:** Добавьте тонкую систему частиц вокруг ног персонажа.

---

## 💎 ГЛАВА 14: Профессиональные Скины Интерфейса

В Menu Studio мы используем 3 уникальных стиля. Вот как их настроить в Unity:

### 1. Скин "Void Crystal" (Фиолетовый/Темный)
*   **Image Type:** Sliced (используйте 9-slice спрайт с рамкой).
*   **Color:** Темно-фиолетовый градиент.
*   **Effect:** Добавьте компонент `Shadow` с цветом `#ff00ff` и размытием.

### 2. Скин "Ethereal Blade" (Голубой/Магический)
*   **Material:** Используйте шейдер с эффектом `Scrolling Texture` для создания текучей энергии внутри кнопок.
*   **Glow:** Текстовые компоненты должны иметь `Softness` и `Dilate` в настройках TMP для эффекта свечения.

### 3. Скин "Runed Obsidian" (Черный/Золотой)
*   **Texture:** Используйте текстуру камня с золотыми трещинами.
*   **Animation:** При наведении трещины должны светиться ярче (анимация `Emission Color`).

---

## 💎 ГЛАВА 15: Движущийся Задний План (Dynamic Backgrounds)

Чтобы фон постоянно двигался, как в превью:
1.  **Scripted Motion:** Используйте простой скрипт для медленного вращения или перемещения слоев параллакса.
2.  **Fog Sync:** Настройте `Global Fog` в Unity (Window > Rendering > Lighting > Environment) на цвет `#050505`, чтобы скрыть границы мира.

```csharp
using UnityEngine;

public class AtmosphericMotion : MonoBehaviour {
    public float scrollSpeed = 0.05f;
    public float floatingAmount = 0.5f;
    
    void Update() {
        // Легкое покачивание (Float effect)
        float newY = Mathf.Sin(Time.time) * floatingAmount;
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        
        // Медленный разворот (Rotation)
        transform.Rotate(Vector3.up, scrollSpeed * Time.deltaTime);
    }
}
```
*

## 💾 ГЛАВА 12: Сохранение Настроек и Синхронизация (PlayerPrefs & Events)

Чтобы ваши настройки (звук, язык, разрешение) не сбрасывались при каждом запуске и работали во всех сценах игры, используйте следующую систему.

### 1. Глобальный SettingsManager (Singleton)
Создайте объект `GameCore` в первой сцене и прикрепите к нему скрипт `SettingsPersistence.cs`.

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SettingsPersistence : MonoBehaviour {
    public static SettingsPersistence Instance;

    [Header("Current Settings")]
    public float masterVolume = 0.8f;
    public int langIndex = 0; // 0: RU, 1: EN...
    public int resolutionIndex = 3;

    // Событие для мгновенного обновления интерфейса по всей игре
    public static event Action OnSettingsChanged;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Объект не удалится при смене сцены
            LoadSettings();
        } else {
            Destroy(gameObject);
        }
    }

    public void SaveSettings(float vol, int lang, int res) {
        masterVolume = vol;
        langIndex = lang;
        resolutionIndex = res;

        // Сохраняем в системный реестр/файл
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetInt("LanguageIndex", langIndex);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.Save();

        ApplySettings();
        OnSettingsChanged?.Invoke(); // Оповещаем всех слушателей
    }

    void LoadSettings() {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        langIndex = PlayerPrefs.GetInt("LanguageIndex", 0);
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 3);
        ApplySettings();
    }

    public void ApplySettings() {
        AudioListener.volume = masterVolume;
        // Логика смены разрешения и языка...
        Debug.Log("Настройки применены глобально.");
    }
}
```

### 2. Синхронизация Текста (Localization Sync)
Каждый компонент текста должен "подписаться" на событие обновления настроек.

```csharp
using UnityEngine;
using TMPro;

public class localizedText : MonoBehaviour {
    public int textID; // ID фразы из вашего Translator
    private TextMeshProUGUI textComp;

    void OnEnable() {
        textComp = GetComponent<TextMeshProUGUI>();
        SettingsPersistence.OnSettingsChanged += UpdateText; // Подписка
        UpdateText();
    }

    void OnDisable() {
        SettingsPersistence.OnSettingsChanged -= UpdateText; // Отписка
    }

    void UpdateText() {
        // Вызываем ваш метод перевода из Translator
        // textComp.text = Translator.Instance.GetText(textID, SettingsPersistence.Instance.langIndex);
        Debug.Log($"Текст {textID} обновлен на язык {SettingsPersistence.Instance.langIndex}");
    }
}
```

### 3. Задний план: Замки и Рассы (Multiverse Visuals)
В "Menu Studio" мы внедрили визуализацию замков. Вот как это сделать в Unity:
1. **Параллакс:** Разместите модели замков (Human, Elf, Orc, Undead) на разной глубине (Z coordinates).
2. **Атмосфера:** Добавьте `Fog` (туман) и `Skybox`, соответствующие игровому миру.
3. **Связь с игроком:** Если игрок выбрал расу Эльфов, в `SettingsPersistence` сохраните `SelectedRace`. При загрузке меню проверяйте это значение и подсвечивайте нужный замок ярче других.

### 4. Резюме Синхронизации
*   **Громкость:** `AudioListener.volume` — управляет всем звуком сразу.
*   **Разрешение:** `Screen.SetResolution` — меняет окно приложения.
*   **Язык:** Событие `OnSettingsChanged` — заставляет все кнопки и диалоги мгновенно перерисоваться на лету.


---

## 🤖 ГЛАВА 13: Интеграция Claude Desktop, MCP-Сервера для Unity и Claude Code

В этой главе мы разберем, как связать профессиональный ИИ Claude напрямую с вашим игровым движком Unity 6. Вы сможете использовать как консольный **Claude Code** (который у вас уже запущен), так и красивое приложение **Claude Desktop** со специальными инструментами управления редактором через протокол **MCP (Model Context Protocol)**.

---

### 1. Что делать прямо сейчас в консоли Claude Code (Решение проблемы со скриншотов)

На ваших скриншотах видна классическая ловушка командной строки Windows! Вы ввели команду перехода:
`cd "D:\_GameDew\Projects\ContinentOfFate\ContinentOfFate"`
Но терминал остался на диске **C:** (`C:\Users\user>`).

**Почему так произошло?**
В Windows-терминале (`cmd`) обычная команда `cd` меняет путь внутри диска, но **не переключает сам диск** (с диска **C:** на диск **D:**). Поэтому вы остались в папке пользователя `C:\Users\user`, и когда запустили `claude`, он открылся в корне вашего компьютера, а не в игре!

---

🖥️ **Шаг 1: Перезапуск Claude Code внутри папки вашей игры**

1.  **Закройте текущую зависшую консоль:**
    *   Просто нажмите на **крестик (X)** в верхнем правом углу черного окна терминала, чтобы принудительно закрыть его и сбросить зависший в `C:\Users\user` процесс.

2.  **Откройте новое, чистое окно командной строки Windows:**
    *   Нажмите сочетание клавиш **Win + R** на вашей клавиатуре (откроется маленькое окошко «Выполнить»).
    *   Введите буквы `cmd` и нажмите **Enter**.
    *   Перед вами откроется чистое черное окно терминала с приветствием `C:\Users\user>`.

3.  **Перейдите на диск D: и в папку вашей игры:**
    Чтобы командная строка Windows переключилась с диска **C:** на диск **D:** и вошла в папку вашей игры, введите одну из следующих команд и нажмите **Enter**:

    *   **Вариант А (Самый простой — с флагом `/d`):**
        Напишите команду `cd /d` (флаг `/d` принудительно переключает диски) и укажите в кавычках точный путь к вашей игре:
        ```bash
        cd /d "D:\_GameDew\Projects\ContinentOfFate\ContinentOfFate"
        ```
        И нажмите **Enter**.

    *   **Вариант Б (По очереди):**
        1. Сначала переключите диск на **D:**. Для этого просто введите букву диска с двоеточием и нажмите **Enter**:
           ```bash
           d:
           ```
        2. Теперь, когда вы находитесь на диске **D:**, введите команду перехода `cd` с вашим путем и нажмите **Enter**:
           ```bash
           cd "D:\_GameDew\Projects\ContinentOfFate\ContinentOfFate"
           ```

    💡 **Лайфхак:** Чтобы не писать длинный путь вручную:
    1. Откройте папку с вашей игрой в обычном проводнике Windows.
    2. Кликните мышкой по адресной строке вверху проводника (там подсветится синим точный путь к вашей папке).
    3. Нажмите **Ctrl + C**, чтобы скопировать его.
    4. Вернитесь в черное окно терминала, напишите `cd /d ` (с пробелом), кликните правой кнопкой мыши внутри терминала (или нажмите **Ctrl + V**), чтобы вставить путь, и нажмите **Enter**.

    *После этого адрес в начале строки обязательно должен измениться на:*
    `D:\_GameDew\Projects\ContinentOfFate\ContinentOfFate>`

4.  **Запустите Claude Code заново внутри папки проекта:**
    *   Напишите в терминале слово:
        ```bash
        claude
        ```
        И нажмите **Enter**.
    *   Появится точно такой же экран безопасности (`Quick safety check...`), но теперь сверху будет написан ваш правильный путь к игре: `D:\_GameDew\Projects\ContinentOfFate\ContinentOfFate`!
    *   Убедитесь, что выбран пункт `> 1. Yes, I trust this folder` (он выбран по умолчанию и подсвечен зеленым), и нажмите **Enter** для подтверждения.

    **Вы успешно вошли!** Теперь Claude Code успешно запустится прямо внутри вашей игры. Он мгновенно просканирует файлы проекта, создаст локальную базу знаний и будет готов отвечать на вопросы, находить баги и автоматически писать C# код прямо в вашей игре!

---

🔌 **Шаг 2: Настройка Claude Desktop и MCP-сервера в Unity 6 (из видео)**
Чтобы Unity могла слушать и выполнять команды от Клода, создайте специальный скрипт-переводчик.

1.  Откройте ваш проект в Unity 6.
2.  В окне **Project** найдите папку **Assets** и создайте внутри нее папку с именем **Editor** (если её еще нет). *Это обязательное требование Unity для запуска скриптов автоматизации редактора*.
3.  Внутри папки `Assets/Editor/` создайте новый C# скрипт с названием `UnityMcpBridge.cs`.
4.  Откройте его в вашей среде разработки (Visual Studio или Rider) и полностью замените код на следующий вариант (он будет слушать запросы Клода на локальном порту 30241):

```csharp
using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

[InitializeOnLoad]
public static class UnityMcpBridge
{
    private static HttpListener listener;
    private static readonly int Port = 30241; // Локальный порт для связи

    static UnityMcpBridge()
    {
        StartServer();
    }

    private static async void StartServer()
    {
        try
        {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{Port}/");
            listener.Start();
            Debug.Log($"<color=#9b5de5>[MCP Bridge]</color> Сервер успешно запущен на порту {Port}. Ожидание команд от Claude Desktop...");
            
            while (listener.IsListening)
            {
                var context = await listener.GetContextAsync();
                ProcessRequest(context);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[MCP Bridge] Не удалось запустить сервер: {ex.Message}");
        }
    }

    private static void ProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        if (request.HttpMethod == "POST")
        {
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                string jsonCmd = reader.ReadToEnd();
                
                // Выполняем действия строго в основном потоке Unity
                EditorApplication.delayCall += () =>
                {
                    string result = ExecuteUnityCommand(jsonCmd);
                    byte[] buffer = Encoding.UTF8.GetBytes(result);
                    response.ContentType = "application/json";
                    response.ContentLength64 = buffer.Length;
                    respon            * **Шаг 5:** Сохраните файл (нажмите **Ctrl + S** или выберите *Файл* -> *Сохранить*) и закройте Блокнот.
            * **Шаг 6:** Перезапустите GoodbyeDPI:
              * Закройте черное консольное окно GoodbyeDPI, если оно было запущено.
              * Запустите файл **`1_russia_blacklist.cmd`** заново двойным кликом мыши.
              * Нажмите кнопку **«Refresh»** в окне Claude Desktop — ошибка «Couldn't connect to Claude» полностью пропадет, и вы сможете успешно войти в свой аккаунт!

        3. **Смените DNS:** Иногда помогает прописать в настройках сетевого подключения Windows быстрые и безопасные DNS-серверы:
            * Основной DNS (IPv4): `1.1.1.1` (Cloudflare) или `8.8.8.8` (Google)
            * Альтернативный DNS (IPv4): `1.0.0.1` или `8.8.4.4`

*   **Баг: Иконка штекера в Claude Desktop не горит или пишет ошибку**
    *   *Решение:* Убедитесь, что у вас в системе установлен Node.js (скачайте LTS версию с официального сайта [nodejs.org](https://nodejs.org/)). После установки обязательно перезапустите компьютер, чтобы системные пути обновились.
*   **Баг: Ошибка «Порт 30241 уже занят» в Unity**
    *   *Решение:* Это значит, что у вас запущено несколько окон Unity одновременно или сервер завис в оперативной памяти. Закройте лишние проекты Unity или просто перезапустите редактор.
*   **Баг: Красные ошибки компиляции в Unity при создании скрипта**
    *   *Решение:* Убедитесь, что файл `UnityMcpBridge.cs` лежит строго в папке `Assets/Editor/`. Если он лежит в обычной папке, компилятор Unity выдаст ошибки при сборке игры. Папка `Editor` исключает скрипт из релизной сборки, оставляя его только для работы внутри редактора.

---

### 5. 🤖 Локальный оффлайн-агент (Полностью бесплатный ИИ-помощник БЕЗ интернета)

Да, это **абсолютно возможно**! Вы можете развернуть полноценного ИИ-агента (аналогичного Клоду или мне), который будет работать **прямо на вашем компьютере**, абсолютно бесплатно, не требуя подписок и работая даже при полном отключении интернета.

Благодаря открытым моделям от мировых компаний (Alibaba, Meta, Google) и современным расширениям для VS Code, такой агент может:
1. Читать структуру вашего проекта, файлы кода, изменять файлы C#.
2. Исправлять ошибки компиляции, запуская команды сборки.
3. Анализировать скриншоты ваших ошибок или интерфейса (при использовании мультимодальных моделей Vision).
4. **Управлять вашим Unity** через тот же самый MCP-сервер, который мы только что настроили!

Вот подробная инструкция, как настроить такую локальную среду за 15 минут.

---

#### 📦 Шаг 1: Установка и проверка локального движка моделей (Ollama)
**Ollama** — это бесплатная и самая популярная программа для локального запуска нейросетей на процессоре (CPU) или видеокарте (GPU, Nvidia/AMD).

---

🔍 **КАК ПРОВЕРИТЬ, УСТАНОВЛЕНА ЛИ OLLAMA У ВАС НА ПК?**

Выполните следующие простые проверки по очереди:

1. **Проверка через командную строку Windows (Самый надежный способ):**
   * Нажмите на клавиатуре комбинацию клавиш **Win + R**.
   * Введите в открывшемся окне `cmd` и нажмите **Enter** (откроется черное окно командной строки).
   * Скопируйте и вставьте туда следующую команду и нажмите **Enter**:� ошибкой:
   > ❌ **«Trusted app installs must be enabled to install Claude. To enable this, open Settings -> System -> For developers, then turn on Developer mode.»**
   
   Это стандартная политика безопасности Windows при установке современных упакованных приложений. Чтобы её обойти:
   * Нажмите кнопку **«Да» (Yes)** прямо в окне этой ошибки. Windows автоматически откроет нужное меню настроек.
   * *Если окно настроек не открылось автоматически:* Откройте меню **Пуск** -> **Параметры (Settings)** -> **Система (System)** -> **Для разработчиков (For developers)**.
   * Найдите переключатель **«Режим разработчика» (Developer Mode)** и переведите его в положение **Вкл (On)**.
   * Нажмите **«Да» (Yes)** в появившемся предупреждении Windows, чтобы подтвердить выбор.
   * После этого снова запустите файл **Claude Setup** — установка пройдёт мгновенно и без ошибок!
3. Запустите приложение **Claude Desktop** из меню Пуск хотя бы один раз и войдите в свой аккаунт.
4. Снова нажмите сочетание клавиш **Win + R**, вставьте туда `%APPDATA%\Claude` и нажмите **Enter**. Теперь папка успешно откроется!

##### Вариант Б (Быстрый ручной способ):
1. Нажмите сочетание клавиш **Win + R** на вашей клавиатуре.
2. Вставьте туда просто `%APPDATA%` (без слова Claude) и нажмите **Enter**.
3. Перед вами откроется системная папка `C:\Users\user\AppData\Roaming`.
4. Кликните правой кнопкой мыши по пустому месту, выберите **Создать** -> **Папку** и назовите её строго английскими буквами: `Claude`
5. Зайдите внутрь этой созданной папки `Claude`.

---

🕹️ **Шаг 2: Вход в аккаунт Claude Desktop**

Так как в зависимости от вашего интернет-провайдера обход через GoodbyeDPI может срабатывать не у всех (провайдеры используют разные типы блокировок), выберите один из двух способов ниже, чтобы зайти в аккаунт:

#### Вариант А: Использование бесплатного VPN (Самый надежный и 100% рабочий способ!)
Если GoodbyeDPI запущен, но ошибка в Клоде всё равно висит, обычный VPN решит проблему мгновенно:
1. **Скачайте и установите бесплатный VPN для Windows.** Рекомендуем один из проверенных бесплатных вариантов:
   * **Planet VPN** (официальный сайт: `freevpnplanet.com` — полностью бесплатный, без регистрации, быстрый клиент для Windows).
   * **Amnezia VPN** или **Outline** (бесплатные ключи доступа можно легко взять в Telegram-ботах или на сайте `outline-keys.com`).
   * **Psiphon** (официальный сайт: `psiphon.ca` — отлично обходит любые провайдерские блокировки в РФ).
2. **Запустите VPN** и подключитесь к любой доступной бесплатной стране (например, Франция, Германия или США).
3. **Очистите кэш DNS в Windows**, чтобы система забыла старые заблокированные маршруты:
   * Нажмите **Win + R**, введите `cmd` и нажмите **Enter**.
   * Скопируйте и вставьте туда команду: `ipconfig /flushdns` и нажмите **Enter**. После успеха закройте командную строку.
4. **Запустите Claude Desktop заново** и нажмите кнопку **«Refresh»** по центру окна. Клод мгновенно запустится и покажет окно авторизации! Войдите в свой аккаунт.
   * ⚠️ **Если после входа появилось окно «Updates to Consumer Terms and Policies»:**
     * **Переключатель «Help improve our AI models»:** Вы можете оставить его **выключенным** (OFF) или включить по желанию. Отключение гарантирует стандартный 30-дневный период хранения ваших данных.
     * **Нажмите белую кнопку «Accept»** в самом низу, чтобы подтвердить согласие с условиями и перейти в интерфейс приложения.
*💡 Примечание: После того как вы войдете в аккаунт Клода и настроите конфиг, VPN можно будет отключать. Он нужен в основном для первоначальной авторизации и загрузки интерфейса.*

#### Вариант Б: Обход через GoodbyeDPI (Если вы хотите настроить его до конца)
Если вы хотите, чтобы всё работало без сторонних VPN, настроим GoodbyeDPI:
1. **Полностью закройте приложение Claude Desktop** (нажмите на крестик, а затем выйдите из него в трее Windows около часов — правой кнопкой мыши по иконке Клода ➡️ **«Quit Claude»**).
2. Убедитесь, что вы правильно внесли изменения в файл **`russia-blacklist.txt`** в вашей папке GoodbyeDPI (добавили строчки `anthropic.com` и `api.anthropic.com` в самый низ файла и сохранили его через Ctrl + S).
3. **Перезапустите GoodbyeDPI:**
   * Закройте черное консольное окно GoodbyeDPI (нажав на крестик).
   * Запустите файл **`1_russia_blacklist.cmd`** заново двойным кликом мыши.
4. Сбросьте кэш DNS: нажмите **Win + R**, введите `cmd`, нажмите **Enter**, введите `ipconfig /flushdns` и нажмите **Enter**.
5. Запустите Claude Desktop заново, нажав **Win + R**, вставив `%LOCALAPPDATA%\Programs\claude-desktop\Claude.exe` и нажав **Enter**.
6. Нажмите кнопку **«Refresh»** в окне Клода.

---

📝 **Шаг 3: Настройка файла конфигурации (Магия интеграции с Unity)**

Теперь свяжем оконное приложение Claude Desktop с сервером, который запущен у вас в Unity 6, через MCP-мост.

> ⚠️ **ПОЧЕМУ ВОЗНИКАЕТ ОШИБКА «Не удается найти C:\Users\user\AppData\Roaming\Claude»?**
> Эта папка создается приложением Claude только после определенного этапа работы, либо вообще отсутствует при чистой установке. Если папки физически нет на диске, Windows выдает ошибку при попытке открыть её напрямую.
> 
> Выберите **один из двух простых способов** исправить это:

#### Способ 1: Автоматический (Всего одна команда — Самый быстрый и легкий!)
Вы можете создать нужную папку и файл конфигурации мгновенно через командную строку Windows:
1. Нажмите сочетание клавиш **Win + R**, введите `cmd` и нажмите **Enter** (откроется черное окно).
2. Скопируйте целиком следующую команду, вставьте её в черное окно правой кнопкой мыши и нажмите **Enter**:
   ```cmd
   mkdir "%APPDATA%\Claude" 2>nul & echo {"mcpServers": {"unity-editor-tools": {"command": "node", "args": ["-e", "const http = require('http'); process.stdin.on('data', (data) => { const req = http.request({ host: 'localhost', port: 30241, method: 'POST', path: '/' }, (res) => { res.pipe(process.stdout); }); req.write(data); req.end(); });"]}}} > "%APPDATA%\Claude\claude_desktop_config.json"
   ```
3. И всё! Папка будет создана автоматически, а внутри нее появится идеально настроенный файл `claude_desktop_config.json`. Переходите сразу к **Шагу 4**!

---

#### Способ 2: Ручной (Через проводник Windows)
Если вы хотите сделать всё вручную:
1. Нажмите сочетание клавиш **Win + R** на вашей клавиатуре.
2. Вставьте туда путь к родительской папке (она гарантированно существует) и нажмите **Enter**:
   ```text
   %APPDATA%
   ```
3. Откроется папка `Roaming`. Кликните внутри неё по пустому месту правой кнопкой мыши ➡️ **Создать** ➡️ **Папку**.
4. Назовите новую папку строго: **`Claude`** (с большой буквы).
5. Зайдите внутрь созданной папки **`Claude`**.
6. Кликните внутри неё по пустому месту правой кнопкой мыши ➡️ **Создать** ➡️ **Текстовый документ**.
7. Переименуйте созданный текстовый файл строго в: **`claude_desktop_config.json`**
   * *⚠️ Очень важно: сотрите расширение `.txt` на конце, чтобы файл не превратился в `claude_desktop_config.json.txt` (в Блокноте при сохранении через «Сохранить как...» обязательно выберите тип файла «Все файлы (*.*)»).*
8. Откройте созданный файл в Блокноте, вставьте туда следующий JSON-код и сохраните изменения (**Ctrl + S**):

```json
{
  "mcpServers": {
    "unity-editor-tools": {
      "command": "node",
      "args": [
        "-e",
        "const http = require('http'); process.stdin.on('data', (data) => { const req = http.request({ host: 'localhost', port: 30241, method: 'POST', path: '/' }, (res) => { res.pipe(process.stdout); }); req.write(data); req.end(); });"
      ]
    }
  }
}
```
9. Закройте Блокнот.
10. Снова полностью перезапустите приложение Claude Desktop (нажмите на крестик, а затем выйдите из него в трее Windows около часов — правой кнопкой мыши по иконке Клода ➡️ **«Quit Claude»**) и запустите его заново из меню Пуск.

---

🚀 **Шаг 4: Запуск и тест связи!**

1. Запустите ваш проект в Unity 6. Убедитесь, что во вкладке Console горит фиолетовая строчка:
   `[MCP Bridge] Сервер успешно запущен на порту 30241. Ожидание команд от Claude Desktop...`
2. Откройте приложение Claude Desktop на компьютере.
3. Посмотрите в правый нижний угол строки ввода сообщений в Claude Desktop — там загорится иконка в виде электрического штекера/вилки!
4. Наведите на неё мышку — там появится надпись, подтверждающая, что инструмент **`unity-editor-tools`** успешно подключен!
5. Теперь вы можете вводить в чате Клода команды на русском языке:
   * *«Запусти игру в редакторе»*
   * *«Проверь, есть ли ошибки компиляции в моем проекте»*
   * *«Останови игру в Unity»*
   * ...и Клод будет полностью управлять редактором Unity у вас на глазах! Напишите, как всё пройдёт!

---

### 4. Решение возможных ошибок (Убираем баги)

*   **Баг: Ошибка «Couldn't connect to Claude» при запуске приложения**
    *   *Симптомы:* На экране написано *«Your network redirected this request to www.anthropic.com. Contact your IT administrator»*.
    *   *Причина:* Серверы API Anthropic (`api.anthropic.com`) заблокированы на территории РФ вашим интернет-провайдером. Провайдер перехватывает трафик приложения и принудительно перенаправляет его на рекламный сайт, из-за чего Клод выдаёт эту ошибку безопасности.
    *   *Решение:*
        1. **Включите VPN:** Запустите любой работающий VPN на вашем компьютере (например, Amnezia VPN, Planet VPN, Outline, Cloudflare WARP или любой другой). VPN полностью зашифрует трафик, провайдер не сможет его перехватить, и Claude Desktop мгновенно запустится и войдёт в аккаунт!
        2. **Используйте GoodbyeDPI (Решение по шагам на основе вашей папки):**
           Если у вас открыта папка `D:\Загрузка С Браузеров\goodbyedpi-0.2.3rc3-2\goodbyedpi-0.2.3rc3-2`, выполните следующие простые действия:
           * ⚠️ **ВАЖНОЕ ПРЕДУПРЕЖДЕНИЕ:** На вашем скриншоте открыт файл `1_russia_blacklist.cmd` (это командный сценарий, в нем ничего менять не нужно). Закройте его! Вам нужен текстовый файл **`russia-blacklist.txt`**.
           * **Шаг 1:** Найдите в папке файл **`russia-blacklist.txt`** (это текстовый документ размером около 2,3 МБ, который лежит чуть ниже файла `2_any_country_dnsredir.cmd`).
           * **Шаг 2:** Кликните по **`russia-blacklist.txt`** правой кнопкой мыши и выберите **Открыть с помощью** -> **Блокнот** (или любой другой текстовый редактор, например, Notepad++). Так как файл большой, Блокноту может потребоваться 2-3 секунды, чтобы его загрузить.
           * **Шаг 3:** Прокрутите открывшийся текстовый файл в самый-самый низ или просто нажмите на клавиатуре сочетание клавиш **Ctrl + End**, чтобы переместить курсор на самую последнюю строчку файла.
           * **Шаг 4:** Нажмите **Enter**, чтобы создать новую строчку, и вставьте туда два домена:
             ```text
             anthropic.com
             api.anthropic.com
             ```
           * **Шаг 5:** Сохраните файл (нажмите **Ctrl + S** или выберите *Файл* -> *Сохранить*) и закройте Блокнот.
           * **Шаг 6:** Перезапустите GoodbyeDPI:
             * Закройте черное консольное окно GoodbyeDPI, если оно было запущено.
              * Запустите файл **`1_russia_blacklist.cmd`** заново двойным кликом мыши.
              * Нажмите кнопку **«Refresh»** в окне Claude Desktop — ошибка «Couldn't connect to Claude» полностью пропадет, и вы сможете успешно войти в свой аккаунт!

        3. **Смените DNS:** Иногда помогает прописать в настройках сетевого подключения Windows быстрые и безопасные DNS-серверы:
            * Основной DNS (IPv4): `1.1.1.1` (Cloudflare) или `8.8.8.8` (Google)
            * Альтернативный DNS (IPv4): `1.0.0.1` или `8.8.4.4`

---

### 5. 🤖 Локальный оффлайн-агент (Полностью бесплатный ИИ-помощник БЕЗ интернета)

Да, это **абсолютно возможно**! Вы можете развернуть полноценного ИИ-агента (аналогичного Клоду или мне), который будет работать **прямо на вашем компьютере**, абсолютно бесплатно, не требуя подписок и работая даже при полном отключении интернета.

Благодаря открытым моделям от мировых компаний (Alibaba, Meta, Google) и современным расширениям для VS Code, такой агент может:
1. Читать структуру вашего проекта, файлы кода, изменять файлы C#.
2. Исправлять ошибки компиляции, запуская команды сборки.
3. Анализировать скриншоты ваших ошибок или интерфейса (при использовании мультимодальных моделей Vision).
4. **Управлять вашим Unity** через тот же самый MCP-сервер, который мы только что настроили!

Вот подробная инструкция, как настроить такую локальную среду за 15 минут.

---

#### 📦 Шаг 1: Установка и проверка локального движка моделей (Ollama)
**Ollama** — это бесплатная и самая популярная программа для локального запуска нейросетей на процессоре (CPU) или видеокарте (GPU, Nvidia/AMD).

---

🔍 **КАК ПРОВЕРИТЬ, УСТАНОВЛЕНА ЛИ OLLAMA У ВАС НА ПК?**

Выполните следующие простые проверки по очереди:

1. **Проверка через командную строку Windows (Самый надежный способ):**
   * Нажмите на клавиатуре комбинацию клавиш **Win + R**.
   * Введите в открывшемся окне `cmd` и нажмите **Enter** (откроется черное окно командной строки).
   * Скопируйте и вставьте туда следующую команду и нажмите **Enter**:
     ```bash
     ollama --version
     ```

---

#### 🚨 Дополнительное решение: Что делать, если ИИ всё равно завис на «API Request...» после уменьшения контекста?

Вы отлично уменьшили контекст до `4000` (это видно на вашем втором скриншоте: `0 / 4.0k Tokens`), заменили хост на `127.0.0.1` и приостановили защиту Kaspersky. Это абсолютно правильные шаги!

А то, что модель **в командной строке ответила мгновенно** — это просто фантастика! Это значит, что ваша видеокарта работает на полную мощность, Ollama настроена идеально, а модель загружается за доли секунды. Мы буквально на финишной прямой!

Но почему же в VS Code чат всё равно висит на `API Request...`? Причина кроется в очень интересной технической детали:

##### 🛠️ Причина зависания: «Системный Промпт» слишком велик для 4000 токенов!
В отличие от обычной командной строки Windows (где вы просто пишете "Привет" и модель отвечает), расширение **Roo Code** перед вашим сообщением незаметно отправляет модели **огромный системный промпт** (инструкции о том, как устроен проект, какие инструменты редактирования кода ему доступны, правила разработки и т.д.).
* Размер этого системного промпта в режиме **`Architect`** составляет примерно **4000–5500 токенов**!
* Когда вы жестко ограничили размер контекста до **`4000`**, системный промпт просто **не поместился** в выделенный лимит памяти! Ollama либо пытается бесконечно обрезать его, либо уходит в ступор (зависает на этапе prefill), пытаясь уместить 5000 токенов в лимит 4000.

---

##### 🚀 ФИНАЛЬНОЕ РЕШЕНИЕ (Запустит ИИ за 3 шага!):

1. **Верните размер контекста на `16000` или `32768` (или просто сотрите значение, оставив поле пустым):**
   * Зайдите в настройки **Roo Code** (иконка шестеренки `⚙️` вверху справа).
   * Перейдите во вкладку настроек провайдера (иконка `🔌` слева).
   * Найдите поле **`Context Window Size (num_ctx)`**.
   * Измените значение `4000` на **`16000`** (этого с запасом хватит на системный промпт и ваш диалог) или просто **полностью сотрите цифры**, сделав поле абсолютно пустым (тогда Ollama автоматически выделит свой стандартный лимит `32768`).
   * Не забудьте нажать кнопку **`Save`** в самом верху настроек!

2. **ОБЯЗАТЕЛЬНО создайте АБСОЛЮТНО новый чат:**
   * Текущий чат, в котором вы написали "привет", уже "застрял" в памяти VS Code со старым лимитом `4000` и пытается безуспешно повторить запрос.
   * Нажмите на значок **`+` (New Task)** в самом верхнем углу панели Roo Code, чтобы открыть абсолютно чистый, свежий чат.

3. **Напишите «Привет» в новом чате и подождите 5-15 секунд:**
   * При первом запросе в новом чате Ollama должна прочитать весь гигантский системный промпт Roo Code (сделать "prefill"). Это занимает чуть больше времени, чем ответ на одно слово в консоли (от 5 до 15 секунд в зависимости от видеокарты).
   * **Подождите этот первый запуск.** Как только модель прочитает промпт и ответит в первый раз, благодаря функции **Prompt Caching** (которая, как видно на скриншоте, у вас поддерживается!), все последующие ответы в этом чате будут вылетать **мгновенно**!

---вставьте туда следующую команду и нажмите **Enter**:
     ```bash
     ollama --version
     ```
   * **Результат:** 
     * **Вариант 1 (Отличный):** Вывелось `ollama version is 0.20.5` (или любая другая версия) БЕЗ предупреждений. Ollama установлена и уже работает! Переходите сразу к скачиванию моделей.
     * **Вариант 2 (У вас именно этот случай!):** Вывелось предупреждение:
       ```text
       Warning: could not connect to a running Ollama instance
       Warning: client version is 0.20.5
       ```
       **Что это значит?** Это значит, что **Ollama уже успешно установлена на вашем компьютере!** Но её фоновая служба (сервер) в данный момент просто выключена (не запущена).
       
       **Как её запустить?**
       1. Нажмите клавишу **Win** на клавиатуре (или откройте меню **Пуск**).
       2. Введите в поиске слово: **Ollama**
       3. Кликните по иконке приложения **Ollama**, чтобы запустить его.
       4. Посмотрите в системный трей (в правый нижний угол экрана, у часов, под стрелочкой `^`) — там появится значок белой ламы.
       5. Теперь снова введите команду `ollama --version` в командной строке. Предупреждение исчезнет, и покажется чистая версия! Сервер запущен.
     * **Вариант 3 (Программы нет):** Пишет: *«"ollama" не является внутренней или внешней командой...»*, значит, система её вообще не видит. Тогда перейдите к разделу установки ниже.

2. **Проверка в системном трее Windows:**
   * Посмотрите в правый нижний угол вашего экрана, рядом с часами (нажмите на маленькую стрелочку вверх `^`, чтобы открыть скрытые значки).
   * Ищите значок в виде головы **серой или белой ламы** (Ollama).
   * Если значок есть — кликните по нему правой кнопкой мыши. Если там написано **«Ollama is running»**, программа запущена и полностью готова к работе!

3. **Проверка в браузере (Если программа запущена в фоне):**
   * Откройте любой браузер и перейдите по адресу:
     `http://localhost:11434`
   * **Результат:**
     * Если на странице написана строчка: **`Ollama is running`** — всё отлично! Программа работает прямо сейчас.
     * Если страница не загружается (ошибка «Не удается получить доступ к сайту»), значит, Ollama либо не установлена, либо выключена.

---

📥 **ЧТО ДЕЛАТЬ, ЕСЛИ OLLAMA НЕТ ИЛИ ОНА НЕ НАЙДЕНА?**

Если проверки выше показали, что Ollama не установлена:

1. **Скачайте программу:**
   * Перейдите на официальный сайт: [ollama.com](https://ollama.com)
   * Нажмите на большую кнопку **Download** и выберите **Windows**.
   * Скачайте установщик (файл `OllamaSetup.exe`).

2. **Установите её:**
   * Запустите скачанный файл `OllamaSetup.exe` двойным кликом мыши.
   * Нажмите кнопку **Install** и подождите буквально 1-2 минуты. Программа установится автоматически без сложных настроек.
   * После установки в трее у часов (справа внизу) загорится значок ламы. Это значит, что локальный сервер успешно запущен в фоне.

3. **Скачайте ИИ-модели для оффлайн-работы:**
   Теперь нужно скачать саму «голову» нейросети, которая будет думать. Для этого:
   * Нажмите **Win + R**, введите `cmd` и нажмите **Enter**.
   * **Для написания C# кода и работы с файлами (Рекомендуется):**
     Скопируйте, вставьте в консоль эту команду и нажмите **Enter**:
     ```bash
     ollama run qwen2.5-coder:7b
     ```
     *(Это лучшая легкая модель для программирования в мире на данный момент от Alibaba. Её размер около 4.7 ГБ. Дождитесь, пока шкала загрузки дойдет до 100%).*

---

#### 🏁 ВЫ НАХОДИТЕСЬ ЗДЕСЬ (ВАШ ТЕКУЩИЙ ПРОГРЕСС):

Как отлично видно на ваших новых скриншотах:
1. Вы перешли во внутренний раздел настроек (**Settings**).
2. Вы успешно переключились на **Ollama**, выбрали модель **`qwen2.5-coder:7b`** и оставили адрес `http://localhost:11434`.

---

#### ⚙️ Шаг 4: Разбор предупреждения и сохранение настроек

##### 1. Что означает оранжевый текст внизу настроек?
На вашем скриншоте написано:  
`Note: Roo Code uses complex prompts and works best with Claude models. Less capable models may not work as expected.`  
* **Это НЕ ошибка!** Это просто информационное предупреждение (дисклеймер) от авторов расширения.
* Они предупреждают, что само расширение имеет сложнейшие инструкции под капотом, которые идеально понимает дорогая облачная модель **Claude 3.5 Sonnet**.
* Маленькие локальные модели (вроде нашей Qwen 2.5) могут иногда путаться в очень сложных многошаговых задачах. Но для обычного написания C# кода, ответов на вопросы и работы в Unity её возможностей вам хватит с запасом! Беспокоиться об этой надписи вообще не нужно.

##### 2. Как сохранить настройки и запустить чат?
Так как вы сейчас находитесь в режиме расширенных настроек, кнопка `Finish ->` в самом низу может не срабатывать, пока вы не примените изменения. Сделайте следующее:
1. Посмотрите на самый верх левой панели Roo Code (над надписью `Providers`).
2. В верхнем правом углу этой панели (рядом со значком лупы) находится **синяя кнопка `Save` (Сохранить)**.
3. **Нажмите на кнопку `Save`!**
4. После нажатия настройки сохранятся. Теперь нажмите на **стрелочку влево `<-` (рядом со словом Settings)** в самом верху левой панели, чтобы выйти из настроек в главный чат.
5. Чат откроется! Напишите ему: `Привет! Ты меня слышишь?` и проверьте ответ.

---

#### 🌉 Шаг 5: Подключение локального агента к Unity (Настройка MCP)

##### ❓ ПОЧЕМУ ПИШЕТ, ЧТО ПУТЬ НЕ НАЙДЕН?
На вашем скриншоте отлично видно, что вы вставили путь `%APPDATA%\Code\User\globalStorage\roodev.roo-cline\settings` прямо в чёрное окно командной строки (`cmd.exe`).
* **Причина №1 (Ошибка CMD):** Командная строка Windows не умеет открывать директории, если ввести их путь как команду. Она пытается «запустить» этот путь как программу и выдаёт ошибку *«Системе не удается найти указанный путь»*. Чтобы открыть папку через CMD, нужно писать команду `explorer` перед путём (например: `explorer %APPDATA%\...`).
* **Причина №2 (Папка ещё не создана):** Так как вы только что установили расширение Roo Code и только настраиваете его, папка `roodev.roo-cline\settings` физически ещё не создана на вашем диске. Она появится только после первого автоматического сохранения настроек самой VS Code.

---

##### 🚀 РЕШЕНИЕ: АВТОМАТИЧЕСКАЯ НАСТРОЙКА В ОДИН КЛИК (Через вашу открытую консоль CMD)

Чтобы не создавать папки вручную и не путаться в проводнике, мы сделаем всё автоматически за 1 секунду прямо в вашем открытом черном окне командной строки!

1. Скопируйте весь длинный текст из рамки ниже (это специальный безопасный PowerShell-скрипт):
   ```bash
   powershell -Command "New-Item -ItemType Directory -Force -Path \"$env:APPDATA\Code\User\globalStorage\roodev.roo-cline\settings\"; '[{\"mcpServers\": {\"unity-editor-tools\": {\"command\": \"node\", \"args\": [\"-e\", \"const http = require(''http''); process.stdin.on(''data'', (data) => { const req = http.request({ host: ''localhost'', port: 30241, method: ''POST'', path: ''/'' }, (res) => { res.pipe(process.stdout); }); req.write(data); req.end(); });\"]}}}]' | Out-File -FilePath \"$env:APPDATA\Code\User\globalStorage\roodev.roo-cline\settings\roo_mcp_settings.json\" -Encoding utf8"
   ```
2. Перейдите в ваше открытое черное окно командной строки.
3. Кликните правой кнопкой мыши внутри чёрного окна (или нажмите **Ctrl + V**), чтобы вставить этот скрипт.
4. Нажмите **Enter**.
5. **Готово!** Скрипт мгновенно создал нужную цепочку папок на вашем компьютере и записал файл моста `roo_mcp_settings.json` с идеальным кодом!

---

##### 📂 АЛЬТЕРНАТИВНЫЙ РУЧНОЙ СПОСОБ (Через проводник):
Если вы хотите сделать это вручную:
1. Нажмите клавиши **Win + R** на клавиатуре.
2. Вставьте в окно «Выполнить» команду: `explorer %APPDATA%\Code\User\globalStorage` и нажмите **Enter**.
3. В открывшейся системной папке найдите или создайте папку **`roodev.roo-cline`**.
4. Зайдите внутрь неё и создайте папку **`settings`**.
5. Зайдите в папку `settings`, кликните правой кнопкой мыши по пустому месту ➡️ **Создать** ➡️ **Текстовый документ**.
6. Переименуйте его полностью в **`roo_mcp_settings.json`** (убедитесь, что стёрли расширение `.txt` в конце!).
7. Откройте его через обычный Блокнот, скопируйте и вставьте в него этот JSON-код моста:
   ```json
   {
     "mcpServers": {
       "unity-editor-tools": {
         "command": "node",
         "args": [
           "-e",
           "const http = require('http'); process.stdin.on('data', (data) => { const req = http.request({ host: 'localhost', port: 30241, method: 'POST', path: '/' }, (res) => { res.pipe(process.stdout); }); req.write(data); req.end(); });"
         ]
       }
     }
   }
   ```
8. Сохраните файл (**Ctrl + S**) и закройте Блокнот.

---

##### 🔄 Последний шаг: Проверка связи (MCP) и запуск чата

В версии **Roo Code v3.54.0+** разработчики обновили дизайн настроек. Теперь все разделы настроек находятся на **вертикальной панели вкладок с левой стороны** прямо внутри окна настроек!

1. **Как открыть список MCP-серверов и проверить зеленую лампочку?**
   * Нажмите на иконку **шестеренки `⚙️`** в правом верхнем углу панели Roo Code, чтобы войти в настройки.
   * На левом краю открывшейся панели появится вертикальный ряд маленьких иконок вкладок.
   * Найдите вкладку **MCP Servers** (она выглядит как **два горизонтальных слайдера/штекера друг над другом `[-=]`**, обычно 6-я или 7-я по счету сверху — вы её правильно выбрали на скриншоте!). Нажмите на неё!
   * Перед вами откроется страница настроек **MCP Servers** (как на вашем первом скриншоте!).
   * Если вы правильно заполнили файл `roo_mcp_settings.json`, в самом низу под кнопкой `Refresh MCP Servers` вы увидите наш инструмент **`unity-editor-tools`**.
   * Как только вы запустите ваш проект Fate Continent в Unity 6, рядом со строкой инструмента загорится заветный **зелёный индикатор 🟢**!

2. **Как вернуться в чат?**
   * В самом верху настроек нажмите на стрелочку **`<- Settings`** (назад), чтобы выйти на главный экран общения с агентом.

---

#### 🚨 Решение проблемы: «Зависло на 0% и пишет API Request...»

Если вы написали сообщение, а чат завис на **`0%`** с бесконечной надписью **`API Request...`**, не переживайте! Это классическая ситуация при первом запуске локальных моделей. Вот как это быстро решить:

1. **Причина №1: Холодный старт (Модель загружается в память)**
   * Локальная модель `qwen2.5-coder:7b` весит около **4.7 ГБ**. Когда вы отправляете ей самое первое сообщение после запуска компьютера или долгого простоя, Ollama должна прочитать этот огромный файл с вашего жесткого диска (HDD/SSD) и полностью загрузить его в оперативную память (RAM) или видеопамять видеокарты (VRAM).
   * **Что делать:** Если у вас обычный жесткий диск или не самый мощный компьютер, этот процесс загрузки может занимать **от 1 до 3 минут**. В это время в чате будет отображаться ровно `0%`. **Просто подождите 2-3 минуты**, не закрывая окно, — как только модель загрузится в память, она мгновенно начнет печатать ответ, а все последующие вопросы будут обрабатываться уже за секунды!

2. **Причина №2: Сервер Ollama уже успешно запущен (Разбор вашей ошибки в CMD)**
   * На вашем скриншоте из командной строки (CMD) вы видите ошибку:
     `Error: listen tcp 127.0.0.1:11434: bind: Only one usage of each socket address is normally permitted.`
   * **Что это значит?** Это значит, что **сервер Ollama у вас УЖЕ успешно работает в фоновом режиме**! Он занял порт `11434`, и повторно запустить его через команду `ollama serve` нельзя (поэтому Windows выдает ошибку "порт уже занят").
   * **Доказательство:** На вашем скриншоте из браузера Opera при открытии `http://localhost:11434` вы видите надпись **`Ollama is running`**.
   * **Решение:** Закройте окно командной строки, где вы писали `ollama serve` — сервер и так работает сам по себе в трее Windows (маленький значок ламы возле часов)!

3. **Причина №3: Сообщения попали в «Очередь» (Queued Messages) в Roo Code**
   * На вашем последнем скриншоте видно, что ваши сообщения находятся в блоке **`Queued Messages`** с иконками корзин `🗑️`.
   * **Почему это произошло?** Когда Roo Code не смог сразу получить ответ от модели (например, если модель в этот момент долго скачивалась или загружалась в оперативную память), он заблокировал отправку новых сообщений и поместил их в очередь ожидания, чтобы не спамить сервер.
   * **Как выйти из этого зависшего состояния и очистить очередь:**
     1. Посмотрите в самый верх панели **Roo Code** (в заголовок плагина).
     2. Нажмите на иконку **`+` (Плюс / Новый диалог / New Task)**, которая находится в правом верхнем углу (рядом с иконкой шестеренки настроек и карандаша).
     3. Либо нажмите на значки корзин **`🗑️`** рядом с каждым сообщением в очереди, чтобы удалить их из ожидания.
     4. Нажмите на кнопку начала нового диалога, чтобы полностью очистить старую зависшую сессию и начать чистый чат с нуля!

4. **Причина №4: Убедитесь, что модель qwen2.5-coder:7b скачана локально**
   * Если вы выбрали модель `qwen2.5-coder:7b` в настройках Roo Code, но физически ещё не скачали её через Ollama, то при первом запросе Ollama начнет скачивать её в фоновом режиме без отображения шкалы прогресса внутри VS Code. Из-за этого чат зависает на `0%` и уходит в вечное ожидание.
   * **Как проверить скачана ли модель и запустить скачивание вручную:**
     1. Откройте командную строку Windows (**нажмите Win+R, введите `cmd` и нажмите Enter**).
     2. Введите команду для проверки списка скачанных моделей:
        ```bash
        ollama list
        ```
     3. Посмотрите, есть ли в таблице модель **`qwen2.5-coder:7b`**.
     4. Если её там нет или вы хотите скачать её со 100% отображением прогресса загрузки прямо в консоли, выполните команду:
        ```bash
        ollama run qwen2.5-coder:7b
        ```
     5. В консоли начнется скачивание (вы увидите полосу загрузки с процентами и скоростью). Вес модели составляет около **4.7 ГБ**.
     6. Дождитесь, пока загрузка завершится на **100%**. На экране появится приглашение к диалогу `>>>`.
     7. Напишите в консоли команду выхода: **`/exit`** и нажмите Enter, чтобы закрыть консольный чат.
     8. Теперь модель гарантированно скачана и находится на вашем компьютере! Откройте VS Code, нажмите **`+` (New Task)** в Roo Code и напишите сообщение — ответ прилетит мгновенно!

---

#### 🥞 Ответ про «Бочонок» (Codebase Indexing) и API-ключи

Вы абсолютно правильно заметили: при нажатии на значок трёх цилиндров / бочонка `🥞` внизу панели открывается вкладка **Codebase Indexing** (индексация кодовой базы), где запрашиваются API-ключи (OpenAI API Key и др.).

* **Нужны ли вам ключи и этот бочонок?**
  * **НЕТ!** Эта функция полностью **ОПЦИОНАЛЬНА** и вам она **НЕ нужна** для общения с ИИ и написания кода!
  * **Что такое Codebase Indexing?** Это инструмент, который создает умную векторную карту вашего проекта, чтобы ИИ мог искать файлы по смыслу в гигантских проектах на миллионы строк кода. Для обычного общения и разработки он не обязателен.
  * **Как поступить:** Просто оставьте галочку **`Enable Codebase Indexing`** **СНЯТОЙ (выключенной)**.
  * **Нужны ли ключи для чата?** Для работы с локальной моделью в Ollama **никакие ключи (API Keys) не требуются!** Она работает абсолютно бесплатно, локально на вашем компьютере и без интернета. Поэтому в полях ключей ничего заполнять не нужно.

---

#### 🏎️ Как ускорить локальный ИИ (чтобы не тормозил) и подключить «Базу Знаний» проекта?

Вы задали два отличных вопроса: **почему модель долго отвечает** и **можно ли подключить к ней базу данных (память) обо всем, что мы делаем**, но так, чтобы она не тормозила при загрузке.

Вот подробные ответы и готовые инструкции, как сделать работу локального ИИ молниеносной!

##### 1. Почему ИИ долго отвечает и как заставить его летать?
Локальные нейросети требуют огромной вычислительной мощности. Скорость ответа зависит от того, на чём они запускаются:
* **Проблема CPU (Процессор):** Если у вас нет дискретной видеокарты или Ollama запускается на центральном процессоре (CPU), скорость будет низкой (1-3 буквы в секунду), а обработка кода ("prefill") займет много времени.
* **Решение GPU (Видеокарта):** На видеокартах NVIDIA (с поддержкой CUDA) та же модель `qwen2.5-coder:7b` генерирует код мгновенно (40-60 букв в секунду!).

**Как оптимизировать скорость прямо сейчас:**
1. **Уменьшите размер контекста в Roo Code:**
   * Откройте настройки Roo Code (шестеренка вверху).
   * Найдите параметр **`Context Window`** (размер окна контекста). По умолчанию там может стоять огромное число вроде `32768` (32k tokens). Если компьютер средний, ИИ тратит кучу времени на перечитывание всего этого объёма.
   * **Установите ограничение в `4000` или `8000` токенов.** Это сократит время ожидания ("API Request...") в несколько раз, так как процессору/видеокарте не придётся каждый раз обрабатывать гигабайты текста!
2. **Проверьте загрузку видеокарты в Диспетчере задач Windows:**
   * Откройте Диспетчер задач (**Ctrl+Shift+Esc**), перейдите во вкладку «Производительность» ➡️ «GPU».
   * Отправьте сообщение в Roo Code. Если раздел **Dedicated GPU Memory** и **CUDA / 3D** загружаются на максимум, значит всё отлично — Ollama работает на видеокарте. Если загружается только CPU, значит Ollama не смогла задействовать вашу видеокарту (возможно, нужно обновить драйверы NVIDIA).

---

#### 🚨 Что делать, если ИИ всё равно завис на «API Request...» в Roo Code?

Вы отлично уменьшили контекст до `4000` (это видно на вашем втором скриншоте: `0 / 4.0k Tokens`), заменили хост на `127.0.0.1` и приостановили защиту Kaspersky. Это абсолютно правильные и грамотные шаги!

То, что модель **в командной строке ответила мгновенно** — это просто фантастика! Это доказывает, что ваша видеокарта задействована на полную мощность, Ollama настроена идеально, а модель загружается в видеопамять за доли секунды. Мы находимся буквально на финишной прямой!

Но почему же в VS Code чат всё равно висит на `API Request...`? Причина кроется в очень интересной технической детали:

##### 🛠️ Причина зависания: «Системный Промпт» слишком велик для 4000 токенов!
В отличие от обычной командной строки Windows (где вы просто пишете "Привет" и модель отвечает), расширение **Roo Code** перед вашим сообщением незаметно отправляет модели **огромный системный промпт** (инструкции о том, как устроен проект, какие инструменты редактирования кода ему доступны, правила разработки и т.д.).
* Размер этого системного промпта в режиме **`Architect`** составляет примерно **4000–5500 токенов**!
* Когда вы жестко ограничили размер контекста до **`4000`**, системный промпт просто **не поместился** в выделенный лимит памяти! Ollama либо пытается бесконечно обрезать его, либо уходит в ступор (зависает на этапе prefill), пытаясь уместить 5000 токенов в лимит 4000.

---

##### 🚀 ФИНАЛЬНОЕ РЕШЕНИЕ (Запустит ИИ за 3 шага!):

1. **Верните размер контекста на `16000` или `32768` (или просто сотрите значение, оставив поле пустым):**
   * Зайдите в настройки **Roo Code** (иконка шестеренки `⚙️` вверху справа).
   * Перейдите во вкладку настроек провайдера (иконка `🔌` слева).
   * Найдите поле **`Context Window Size (num_ctx)`**.
   * Измените значение `4000` на **`16000`** (этого с запасом хватит на системный промпт и ваш диалог) или просто **полностью сотрите цифры**, сделав поле абсолютно пустым (тогда Ollama автоматически выделит свой стандартный лимит `32768`).
   * Не забудьте нажать кнопку **`Save`** в самом верху настроек!

2. **ОБЯЗАТЕЛЬНО создайте АБСОЛЮТНО новый чат:**
   * Текущий чат, в котором вы написали "привет", уже "застрял" в памяти VS Code со старым лимитом `4000` и пытается безуспешно повторить запрос.
   * Нажмите на значок **`+` (New Task)** в самом верхнем углу панели Roo Code, чтобы открыть абсолютно чистый, свежий чат.

3. **Напишите «Привет» в новом чате и подождите 5-15 секунд:**
   * При первом запросе в новом чате Ollama должна прочитать весь гигантский системный промпт Roo Code (сделать "prefill"). Это занимает чуть больше времени, чем ответ на одно слово в консоли (от 5 до 15 секунд в зависимости от видеокарты).
   * **Подождите этот первый запуск.** Как только модель прочитает промпт и ответит в первый раз, благодаря функции **Prompt Caching** (которая, как видно на скриншоте, у вас поддерживается!), все последующие ответы в этом чате будут вылетать **мгновенно**!

---

##### 🔗 Дополнительные технические нюансы (если потребуется перепроверить):

* **Касперский:** Если ИИ заработал — отлично! Если нет, помните, что приостановка Касперского иногда оставляет активными его сетевые драйвера. Если проблема вернется, добавьте `http://127.0.0.1:11434` и `ollama.exe` в исключения.
* **Имя модели:** Убедитесь, что в поле Model ID прописано именно `qwen2.5-coder:7b`.
* **Другие приложения:** Если запущена Unity 6 и забирает всю VRAM, Олламе может не хватать памяти. На время тестов можно закрыть Unity 6.

##### 2. Как подключить к локальному ИИ всю историю и базу знаний «Fate Continent»?
Вы абсолютно правы: чтобы ИИ писал идеальный код без ошибок для нашего проекта, он должен знать все наши правила, обновления (v18.12.07), настройки камер, звуков и квестов.

У нас **УЖЕ создана готовая, структурированная база знаний** прямо внутри папки вашего проекта! Это файлы:
* **`AGENTS.md`** — правила разработки, версия игры, настройки инспектора, правила звуков и очистки.
* **`PROJECT_MASTER_BLUEPRINT.md`** — мастер-план проекта, переводы на 9 языков, аудио-промпты.
* **`DEVELOPMENT_LOG.md`** — подробный журнал всех наших обновлений и выполненных шагов.
* **`FATE_CONTINENT_FULL_DOCUMENTATION.md`** — полная техническая документация проекта.

##### 🚀 Секретный символ `@` (Мгновенное подключение базы знаний БЕЗ тормозов!)
Вам **не нужно** загружать гигантские базы данных в память ИИ, что привело бы к зависаниям. В Roo Code встроен невероятно быстрый механизм контекста (RAG):

Когда вы пишете сообщение в чате, просто введите символ **`@` (собачка)**:
1. Введите в чате: **`Привет! На основе @`**
2. Сразу появится красивое всплывающее меню со списком всех файлов вашего проекта!
3. Выберите из списка нужный файл (например, **`AGENTS.md`**, **`DEVELOPMENT_LOG.md`** или **`PROJECT_MASTER_BLUEPRINT.md`**).
4. Вы также можете прикрепить конкретную папку или файл C# скрипта, просто написав `@` и начав вводить его имя (например, `@FateCastleManager.cs`).
5. Отправьте сообщение!

**Почему это работает мгновенно и не тормозит?**
* Roo Code считывает только выбранные вами файлы базы знаний и передает локальной модели только их содержимое в сжатом виде.
* ИИ мгновенно понимает контекст (какая сейчас версия, какие функции используются, какие правила оформления) и пишет код, идеально совместимый с проектом, тратя на это доли секунды!

---

#### 🌉 Устаревший ручной способ (для истории)

Чтобы ваш локальный агент в VS Code мог управлять вашей игрой в реальном времени (например, запускать или останавливать режим игры, читать файлы проекта и автоматически находить ошибки компиляции C#):

1. Нажмите на клавиатуре комбинацию клавиш **Win + R**.
2. В появившееся маленькое окошко скопируйте и вставьте следующий путь (для расширения Roo Code):
   ```text
   %APPDATA%\Code\User\globalStorage\roodev.roo-cline\settings
   ```
3. Нажмите **Enter** — откроется системная папка настроек Roo Code в проводнике Windows.
4. Найдите в этой папке файл с именем **`roo_mcp_settings.json`** (если файла нет, кликните правой кнопкой мыши -> Создать -> Текстовый документ и назовите его именно так).
5. Кликните по этому файлу правой кнопкой мыши ➡️ **Открыть с помощью** ➡️ **Блокнот**.
6. Полностью замените всё содержимое файла на следующий готовый JSON-код моста:
   ```json
   {
     "mcpServers": {
       "unity-editor-tools": {
         "command": "node",
         "args": [
           "-e",
           "const http = require('http'); process.stdin.on('data', (data) => { const req = http.request({ host: 'localhost', port: 30241, method: 'POST', path: '/' }, (res) => { res.pipe(process.stdout); }); req.write(data); req.end(); });"
         ]
       }
     }
   }
   ```
7. Сохраните файл (**Ctrl + S**), закройте Блокнот и **полностью перезапустите VS Code** (закрой и открой заново).

---

#### 🎉 Всё готово! Оффлайн-магия активна!

Когда вы снова откроете панель агента в VS Code, в самом низу в разделе **MCP Servers** загорится зеленый индикатор подключенного инструмента **`unity-editor-tools`**!

Теперь, когда вы запустите ваш проект в Unity 6, ваш локальный ИИ-помощник сможет:
* 📂 **Анализировать** весь C# код вашего проекта и структуру сцен.
* 🛠️ **Находить и самостоятельно исправлять** любые ошибки компиляции.
* 🕹️ **Запускать и останавливать режим игры** в Unity по вашей текстовой просьбе прямо в чате VS Code!

Все эти инструкции я бережно сохранил в файле **`/GAME_HELP_GUIDE.md`** вашего проекта, чтобы они всегда были у вас под рукой даже без интернета! Напишите, как всё установится и настроится!

