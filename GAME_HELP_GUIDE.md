# Помощь По Игре - Unity 6 (6000.3.10f1) Ultimate Guide (v17.18.1)

Этот документ является динамическим руководством по созданию игры "Континент Судьбы". Здесь описаны пошаговые инструкции, скрипты и настройки для Unity, дополненные деталями по сохранению данных и синхронизации.

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

        // Языки (Master Sync v17.18.1)
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
*Документ полностью переработан для версии v17.18.1 (Settings Persistence & Master Sync)*

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



