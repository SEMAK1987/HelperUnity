# Помощь По Игре - Unity 6 (6000.3.10f1) Ultimate Guide

Этот документ является динамическим руководством по созданию игры "Континент Судьбы". Здесь описаны пошаговые инструкции, скрипты и настройки для Unity, дополненные деталями по установке и настройке среды.

---

## 🚀 ШАГ 0: Установка и Подготовка Среды

### 1. Установка Unity Hub и Редактора
1.  **Unity Hub:** Скачайте и установите Unity Hub с официального сайта.
2.  **Версия 6000.3.10f1:** Перейдите во вкладку `Installs > Install Editor`. Если нужной версии нет в списке, найдите её в `Download Archive`. Это критическая версия Unity 6.
3.  **Модули:** При установке обязательно выберите модули: `Windows Build Support (IL2CPP)` и `WebGL Build Support` для кросс-платформенности.

### 2. Создание Проекта
1.  Нажмите `New project` в Hub.
2.  Выберите шаблон **3D (URP)** — Universal Render Pipeline. Это обеспечит лучшую графику для мобильных и ПК.
3.  Назовите проект `ContinentOfFate` и выберите папку на диске SSD (для быстрой работы).

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
*Документ обновлен для версии v17.17.1 (Supreme Support Edition)*

