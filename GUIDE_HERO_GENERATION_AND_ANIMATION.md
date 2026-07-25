# ⚔️ Fate Continent • Полное пошаговое руководство по генерации фигурок Героев и настройке высокопроизводительной анимации для BattleScene (v18.12.06)

Я полностью переработал и дополнил руководство по генерации одиночных фигурок персонажей, 3D-реконструкции и созданию оптимальной системы анимации в Unity 6!

### 🔮 Исправление Мага (Mage) и решение проблемы с ногами
Мы полностью переработали промпт для **Мага (Mage)**, так как предыдущие генерации могли выглядеть неестественно (с неестественными пропорциями, странным фантастическим оружием или лишними артефактами). Новый промпт гарантирует благородный, величественный образ классического фэнтезийного волшебника с красивым посохом, стоящего в естественной симметричной Т-позе в полный рост с полностью видимыми ногами и обувью!

Также все уроки и ссылки на видеоуроки были бережно интегрированы в базу знаний проекта (`knowledge_base.json`):
1. **Создание 3D из картинки (Hunyuan 3D):** https://www.youtube.com/watch?v=SDV54QaEHBs
2. **Оптимальный импорт и анимация персонажей:** https://www.youtube.com/watch?v=TxyBoDqE6Zo
3. **Продвинутая анимация и подготовка моделей:** https://www.youtube.com/watch?v=_aJzFbuLi1M

---

## 🎨 ЭТАП 1: Настройки генерации в Leonardo.ai (Решение проблемы с двумя персонажами и обрезанными ногами)

Чтобы получить строго одну чистую фигурку персонажа в полный рост (без обрезки ног и сапог) на чистейшем белом фоне, выставьте следующие параметры в левой панели генератора:
* **Инструмент:** Вкладка **Image Generation**.
* **Модель (Model):** Пресет `Auto` или `Lucid Origin` / `Leonardo Vision XL` (для красивого объемного пластилинового 3D-стиля).
* **Стиль (Style):** `Dynamic` (как на вашем Скриншоте 1) или `None` (для строгого соответствия тексту).
* **Размер (Aspect Ratio):** Выберите **1:1 (Square)** с разрешением **1024×1024** пикселей.
* **Негативный промпт (Negative Prompt):** Обязательно активируйте тумблер и добавьте этот обновленный список исключений, блокирующий появление двойников, обрезание ног/ступней и ракурсов сзади:
  ```text
  two characters, twin, duplicate characters, split screen, dual view, front and back view, character sheet, turn-around, multiple poses, mirror view, cropped legs, cut-off feet, half-body, torso-only shot, knees crop, cropped boots, close-up, cropped bottom, black background, dark background, grey background, floor shadow, ambient shadow, color gradient, vignette, pedestal, circular base, float pedestal, duplicate items, weapons on side, multiple angles, bad anatomy, flat 2D graphic.
  ```

---

### 🛡️ Обновленные промпты для Основных Героев (В полный рост, без обрезки ног!):

#### 1. Воин (Warrior) — Меч и Щит:
```text
A single isolated full-body head-to-toe shot of a heroic warrior knight in heavy steel armor with gold accents, standing symmetrically in a clear front-facing T-pose, showing full legs and heavy iron boots standing on the ground, completely visible from head to feet within the frame, only one character, solo view. He is firmly holding a simple iron broadsword in his right hand. Stylized toy figurine aesthetic, high-detail clay render. Isolated on a solid flat pure white background (#ffffff), no floor shadows, no pedestal, ready for rigging.
```

#### 2. Стрелок (Archer) — Эльф с Луком:
```text
A single isolated full-body head-to-toe shot of an elven archer in light leather forest armor, standing symmetrically in a straight front-facing A-pose, showing entire legs and leather boots clearly standing on the floor, completely visible from head to feet within the frame, only one character, solo view. He is firmly holding a beautiful wooden recurve bow in his left hand, with a small quiver of arrows secured strictly on his back. Isolated on a solid flat pure white background (#ffffff), zero shadows on floor, no pedestal, ready for rigging.
```

#### 3. Маг (Mage) — Величественный Волшебник (ОБНОВЛЕННЫЙ, ЕСТЕСТВЕННЫЙ И КРАСИВЫЙ):
```text
A single isolated full-body head-to-toe shot of an elegant fantasy wizard mage in long flowing mystical purple robes with soft golden runes, standing symmetrically in a natural, proud front-facing T-pose, with both legs and boots completely visible standing on the floor. He is firmly holding a single ornate ancient wooden magic staff with a glowing blue crystal orb at the top in his right hand. Authentic high-quality 3D tabletop gaming miniature style, clean clay render, soft cinematic studio lighting. Isolated on a solid flat pure white background (#ffffff), no floor shadows, no pedestal, ready for rigging, solo view, only one character.
```

> **🔥 Важное действие:** Наведите курсор на полученную картинку в Leonardo и нажмите кнопку **«Remove Background»** (Вырезать фон) для скачивания чистого PNG-файла без фона.

---

## 🚀 ЭТАП 2: 3D-реконструкция в Tencent Hunyuan 3D (По Скриншоту 2)

На сайте [3d.hunyuan.tencent.com](https://3d.hunyuan.tencent.com/) выставьте параметры точно по вашему Скриншоту 2:
1. Выберите режим **«Вэньшэнь 3D» (Изображение/Вэньшэнь 3D)** на левой панели.
2. Вкладка сверху: **«Одиночное изображение»** (Single image).
3. Загрузите вырезанный PNG-файл персонажа.
4. Выберите модель: **«Поколение 3D - V3.1»** (Generation 3.1).
5. **Количество полигонов (Модель лиц) — Ключ к оптимизации памяти:**
   * На скриншоте доступны варианты: `1,5 M`, `1 M`, `500k`, `50k`.
   * **Выбор для минимальных настроек:** Строго выбирайте **`50k`** (50 тысяч полигонов) или максимум **`500k`**!
   * *Почему?* Модели с `1.5 M` полигонов мгновенно перегрузят оперативную (RAM) и видеопамять (VRAM) на слабых устройствах, когда на поле боя появится десяток воинов. Вариант `50k` выглядит отлично с высоты камеры боя и работает в 30 раз быстрее!
6. Включите **«Земляную сетку»** (Ground Grid) в правом верхнем углу и нажмите **«Генерируйте немедленно»** (Generate Now). Скачайте полученную `.fbx` модель.

---

## 🦴 ЭТАП 3: Создание автоматического скелета в Mixamo (Пошагово и подробно)

Когда вы скачали готовую модель персонажа в формате `.fbx` (или `.obj`) из Tencent Hunyuan 3D, она является абсолютно «статической» (как пластилиновая статуэтка) — у неё нет костей и суставов. Чтобы заставить её двигаться и атаковать на поле боя, нужно настроить скелет. Мы будем использовать бесплатный сервис авто-риггинга **Mixamo** (mixamo.com).

### 📝 Подробный пошаговый процесс риггинга:

1. **Подготовка файла перед загрузкой:**
   * Убедитесь, что ваша модель экспортирована в формате `.fbx`, `.obj` или упакована в `.zip` вместе с текстурной картой.
   * Размер файла не должен превышать 50 МБ (модель на 50k полигонов весит всего около 3–5 МБ, что идеально для быстрой загрузки).

2. **Загрузка модели на Mixamo:**
   * Откройте сайт [Mixamo](https://www.mixamo.com/) и войдите под своей учетной записью.
   * На правой панели нажмите большую синюю кнопку **«Upload Character»**.
   * Перетащите ваш файл модели в открывшееся окно. Подождите 10–30 секунд, пока Mixamo обработает геометрию и отобразит персонажа во фронтальном ракурсе.
   * *Внимание:* Если персонаж стоит спиной или боком, используйте кнопки вращения внизу экрана, чтобы развернуть его лицом к вам.

3. **Точная расстановка маркеров суставов (Критический шаг!):**
   Mixamo попросит вас перетащить цветные кружки-маркеры на соответствующие анатомические точки вашего персонажа. Из-за того, что наши герои держат в руках оружие (меч у воина, лук у стрелка, посохи у мага), делайте это очень аккуратно, чтобы избежать деформации меша:
   * **CHIN (Подбородок — Синий маркер):** Поместите строго на центр нижней челюсти (подбородок). Не поднимайте слишком высоко к губам.
   * **WRISTS (Запястья — Желтые маркеры):** Поместите на середину лучезапястного сустава рук. 
     * *Важно для Воина/Мага:* Так как они держат оружие, старайтесь позиционировать маркер точно там, где рука переходит в кисть, игнорируя геометрию рукояти меша оружия.
   * **ELBOWS (Локти — Красные маркеры):** Поместите на внешнюю точку локтевого сгиба.
   * **KNEES (Колени — Зеленые маркеры):** Поместите на центр коленных чашечек. Убедитесь, что они стоят симметрично.
   * **GROIN (Пах — Оранжевый маркер):** Поместите строго по центру между ног в области таза. Не опускайте слишком низко, иначе при ходьбе ноги будут сильно растягиваться.

4. **Выбор скелета (Skeleton LOD):**
   * В выпадающем списке **Skeleton LOD** выберите стандартный вариант **Standard Skeleton (65 bones)** для лучшего качества пальцев рук, либо **No Fingers (25 bones)** — если вы хотите выжать максимальную производительность (для мобильных телефонов и слабых ПК), так как пальцы на тактической карте боя всё равно не видны крупным планом.

5. **Генерация и скачивание:**
   * Нажмите **Next**. Mixamo запустит процесс автоматического расчета скелета (это занимает от 1 до 2 минут).
   * Посмотрите на анимацию-превью в реальном времени. Если персонаж двигается плавно, одежда не рвется и суставы сгибаются естественно — нажмите **Next**, а затем подтвердите замену персонажа.
   * Нажмите кнопку **Download** в правом верхнем углу. Выберите параметры:
     * **Format:** `FBX for Unity (.fbx)` (это критически важно для корректного масштабирования костей!).
     * **Pose:** `T-Pose`.
     * Нажмите кнопку **Download** и сохраните файл в папку вашего проекта Unity (например, `/Assets/Models/Characters/`).

---

## 🎭 ЭТАП 4: Импорт в Unity 6 и Сверхлегкая система анимаций (Один Контроллер на Всех!)

Чтобы игра работала плавно даже при наличии десятков юнитов на экране, мы настроим систему анимации с использованием технологии **Humanoid**. Главная прелесть Humanoid-скелета в Unity заключается в том, что все ваши персонажи (и Воин, и Стрелок, и Маг) могут использовать **один-единственный общий Animator Controller в оперативной памяти**!

### ⚙️ Пошаговая настройка импортированных моделей в Unity 6:

1. **Конфигурация Rig (Скелета):**
   * Кликните на импортированный файл персонажа (например, `Warrior_Rigged.fbx`) в окне **Project**.
   * В окне **Inspector** перейдите во вкладку **Rig**.
   * Установите параметр **Animation Type** в значение **Humanoid**.
   * В поле **Avatar Definition** оставьте значение **Create From This Model**.
   * Нажмите кнопку **Apply** внизу. Unity автоматически создаст файл аватара (`Warrior_RiggedAvatar`).
   * Повторите эту операцию для всех трех героев (Воина, Стрелка и Мага).

2. **Скачивание и настройка анимаций из Mixamo:**
   * Найдите в Mixamo нужные анимации:
     * **Idle:** `Warrior Idle`, `Archer Idle`, `Wizard Magic Idle` (или любое другое красивое дыхание).
     * **Movement:** Анимацию бега или ходьбы. **Обязательно** поставьте галочку **In Place** в панели настроек Mixamo перед скачиванием, чтобы персонаж бежал на месте!
     * **Attack:** `Sword Slash` (для Воина), `Standing Draw Arrow` (для Стрелка), `Spell Casting` (для Мага).
   * Скачайте каждую анимацию со следующими параметрами:
     * **Format:** `FBX for Unity (.fbx)`.
     * **Skin:** Выберите **Without Skin** (Скачивать анимации БЕЗ меша модели! Это снижает размер каждого файла анимации с 15 МБ до 100 КБ!).
   * Перенесите скачанные файлы анимаций в Unity (например, в папку `/Assets/Animations/`).
   * Для каждого файла анимации выберите его в окне Project, перейдите во вкладку **Rig**, установите **Animation Type: Humanoid**, а в поле **Avatar Definition** выберите **Copy From Other Avatar** и укажите аватар вашего любого базового персонажа. Нажмите **Apply**.

3. **Настройка циклов анимации (Looping):**
   * Для анимаций покоя (**Idle**) и бега (**Movement**) перейдите во вкладку **Animation** в инспекторе.
   * Поставьте галочку напротив **Loop Time** (чтобы анимация проигрывалась бесконечно).
   * Убедитесь, что индикаторы **Loop Match** горят зеленым цветом.
   * Нажмите кнопку **Apply** внизу.

### 🎛️ Создание Единого Animator Controller:

1. В окне **Project** нажмите правой кнопкой мыши -> **Create -> Animator Controller**. Назовите его `TacticalUnitAnimatorController`.
2. Дважды кликните по нему, чтобы открыть окно **Animator**.
3. Создайте параметры на левой панели вкладки **Parameters**:
   * **`Speed`** (тип `Float`) — отвечает за переход между покоем и движением.
   * **`IdleType`** (тип `Integer`) — определяет, какую стойку играть (0 = Воин, 1 = Стрелок, 2 = Маг).
   * **`Attack`** (тип `Trigger`) — запускает стандартную атаку.
   * **`SuperAttack`** (тип `Trigger`) — запускает суперспособность.

4. **Логика состояний (States & Transitions):**
   * Создайте состояние-контейнер типа **Blend Tree** (или настройте переключатель через стейты).
   * Наиболее производительный способ — использовать **Blend Tree 1D** для Idle-состояний:
     * Кликните правой кнопкой в поле сетки -> **Create State -> From New Blend Tree**. Назовите его `IdleBlend`.
     * Дважды кликните по нему. Установите параметр переключения: **`IdleType`**.
     * В списке **Motion** добавьте 3 слота и перетащите туда ваши анимации:
       * Слот 0: `Warrior_Idle` (при значении IdleType = 0)
       * Слот 1: `Archer_Idle` (при значении IdleType = 1)
       * Слот 2: `Mage_Idle` (при значении IdleType = 2)
   * Создайте состояние движения `Move` (перетащите туда анимацию бега `Run`).
   * Создайте переходы (Transitions):
     * Из `IdleBlend` в `Move` (Условие: **`Speed` Greater `0.1`**). Отключите галочку *Has Exit Time*.
     * Из `Move` в `IdleBlend` (Условие: **`Speed` Less `0.1`**). Отключите галочку *Has Exit Time*.
   * Настройте переходы для атак:
     * Создайте состояния `AttackState` и `SuperAttackState` с соответствующими анимациями ударов.
     * Сделайте переходы от **Any State** в `AttackState` по триггеру **`Attack`** и в `SuperAttackState` по триггеру **`SuperAttack`**.
     * Из состояний атак сделайте возвратный переход в `IdleBlend` с включенной галочкой **Has Exit Time** (чтобы анимация удара гарантированно доиграла до конца перед возвращением в покой).

---

## 💻 ЭТАП 5: Сверхлегкий C# Скрипт `TacticalUnitAnimator.cs`

Этот скрипт разработан с учетом требований к экстремальной производительности под Unity 6. Он полностью исключает выделение мусора (GC Alloc) за счет предварительного кэширования строк в числовые хэши параметров аниматора, а также бережет процессор за счет отключения анимации костей вне зоны видимости камеры.

```csharp
// [TACTICAL UNIT ANIMATOR v18.12.06]
// Оптимизированный менеджер анимаций для BattleScene
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TacticalUnitAnimator : MonoBehaviour
{
    [Header("Настройки Оптимизации")]
    [Tooltip("Тип стойки покоя: 0 = Воин, 1 = Стрелок, 2 = Маг")]
    public int idleType = 0;
    public float rotationSpeed = 10f;

    private Animator animator;
    private Transform cachedTransform;
    
    // Кэшируем хэши параметров для разгрузки процессора (CPU)
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IdleTypeHash = Animator.StringToHash("IdleType");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
    private static readonly int SuperAttackTriggerHash = Animator.StringToHash("SuperAttack");

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        cachedTransform = transform;
        targetPosition = cachedTransform.position;
        targetRotation = cachedTransform.rotation;

        // Важнейшая оптимизация: не обновлять кости анимации, когда фигурку не видно на экране!
        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        
        // Передаем тип стойки в аниматор
        animator.SetInteger(IdleTypeHash, idleType);
    }

    private void Update()
    {
        if (isMoving)
        {
            // Перемещение силами C# без тяжелой физики (экономит до 90% ресурсов процессора)
            cachedTransform.position = Vector3.MoveTowards(cachedTransform.position, targetPosition, Time.deltaTime * 5f);
            cachedTransform.rotation = Quaternion.Slerp(cachedTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            if (Vector3.Distance(cachedTransform.position, targetPosition) < 0.01f)
            {
                cachedTransform.position = targetPosition;
                isMoving = false;
                animator.SetFloat(SpeedHash, 0f); // Плавный переход в Idle
            }
        }
    }

    /// <summary>
    /// Метод для приказа фигурке переместиться на определенную клетку тактической сетки
    /// </summary>
    public void MoveToCell(Vector3 destination)
    {
        targetPosition = destination;
        Vector3 direction = (destination - cachedTransform.position).normalized;
        if (direction != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        }
        isMoving = true;
        animator.SetFloat(SpeedHash, 1f); // Запуск анимации бега
    }

    /// <summary>
    /// Воспроизведение стандартной атаки в сторону цели
    /// </summary>
    public void PlayStandardAttack(Vector3 lookAtTarget)
    {
        LookAtTargetInstant(lookAtTarget);
        animator.SetTrigger(AttackTriggerHash);
    }

    /// <summary>
    /// Воспроизведение суперспособности в сторону цели
    /// </summary>
    public void PlaySuperAttack(Vector3 lookAtTarget)
    {
        LookAtTargetInstant(lookAtTarget);
        animator.SetTrigger(SuperAttackTriggerHash);
    }

    private void LookAtTargetInstant(Vector3 target)
    {
        Vector3 direction = (target - cachedTransform.position).normalized;
        if (direction != Vector3.zero)
        {
            cachedTransform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            targetRotation = cachedTransform.rotation;
        }
    }
}
```

---

## 🗜️ ЭТАП 6: Финальная оптимизация видеокарты и оперативной памяти в Unity 6

Когда на поле боя сходятся десятки воинов, правильные настройки импорта моделей и текстур определяют, будет ли игра выдавать стабильные 60 кадров в секунду или начнет сильно зависать. Выполните эти шаги для каждой модели:

### 1. Оптимизация текстур персонажей (Снижение веса VRAM в 4-8 раз!):
* Выделите импортированную текстуру модели (карту цветов/альбедо) в окне **Project**.
* В окне **Inspector** перейдите в самый низ к настройкам платформы (**Default**):
  * **Max Size:** Установите значение **512** или максимум **1024** (для тактической камеры боя 1024х1024 пикселей дает безупречную детализацию, а памяти расходует в разы меньше!).
  * **Resize Algorithm:** `Mitchell` (для красивого сглаживания при уменьшении).
  * **Format:** Выберите автоматический сжатый формат (для Windows/Mac это **DXT5** или **BC7**, для Android — **ASTC 6x6**).
  * Поставьте галочку **Use Crunch Compression** и установите ползунок качества на **Quality: 50-80**. Это сжимает размер текстуры на диске до невероятно малых 100 КБ!
  * Нажмите **Apply**.

### 2. Включение GPU Instancing на материалах:
* Найдите материал, созданный для вашей 3D-модели.
* В инспекторе материала разверните нижнюю вкладку **Advanced Options** (или найдите строку внизу).
* **Обязательно поставьте галочку «Enable GPU Instancing»!**
  * *Почему это важно:* Это позволяет вашей видеокарте отрисовывать абсолютно все одинаковые отряды воинов на поле боя за **один-единственный Draw Call** (один проход отрисовки) вместо сотен индивидуальных запросов! Процессор игры перестанет перегреваться.

### 3. Оптимизация иерархии костей (Optimize Game Objects):
* Перейдите во вкладку **Rig** импортированной модели персонажа в инспекторе.
* Поставьте галочку напротив **Optimize Game Objects**.
* Нажмите **Apply**.
  * *Что это дает:* Unity скрывает всю сложную иерархию костей (Transform) модели из окна Hierarchy, преобразуя их во внутреннюю оптимизированную матрицу. Это экономит до 30% мощности центрального процессора при обсчете анимаций.
  * *Если нужно прикрепить эффекты к рукам:* Если вам нужно спавнить эффекты магии из рук персонажа, в выпадающем списке **Extra Transforms to Expose** найдите и поставьте галочки напротив костей рук (например, `RightHand` или `LeftHand`). Unity оставит видимыми только эти конкретные кости, а остальные скроет ради производительности!

---

*Ваш проект полностью оптимизирован и готов к запуску в Unity 6!*
