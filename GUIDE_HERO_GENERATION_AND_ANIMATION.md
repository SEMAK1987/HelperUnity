# ⚔️ Fate Continent • Полное пошаговое руководство по генерации фигурок Героев и настройке высокопроизводительной анимации для BattleScene (v18.12.06)

Это исчерпывающее профессиональное руководство разработано специально для оптимизации процесса создания графики, 3D-моделирования и программирования высокопроизводительной анимационной системы для персонажей **Стрелка (Archer)**, **Мага (Mage)**, **Воина (Warrior)** и обычных воинов-отрядов на сцене боя **BattleScene**.

Все шаги переработаны под ваш подход с учетом предоставленных настроек интерфейсов **Leonardo.ai**, **Tencent Hunyuan 3D v3.1** и требований к экстремальной экономии оперативной памяти (RAM), видеопамяти (VRAM) и ресурсов процессора (CPU/GPU) на минимальных настройках.

---

## ЭТАП 1: 🎨 Генерация заготовок фигурок в Leonardo.ai

Для создания качественной 3D-модели или плоского 2D-спрайта заготовка должна быть максимально изолированной, симметричной и чистой.

### ⚙️ Точные настройки генератора (левая панель Leonardo.ai):
1. **Инструмент:** Выберите вкладку **Image Generation** (как на вашем Скриншоте 1).
2. **Модель (Model):** Выберите пресет `Auto` или `Lucid Origin` / `Leonardo Vision XL` (для получения сочного пластилинового, полуигрушечного 3D-стиля фигурок).
3. **Стиль (Style):** Выберите `Dynamic` или `None` (для четкого следования текстовому промпту без лишней отсебятины).
4. **Размер (Aspect Ratio):** Выберите `1:1 (Square)` (разрешение **1024×1024** пикселей для максимальной четкости мелких деталей брони и оружия).
5. **Количество генераций (Number of generations):** `1` или `2` (для экономии токенов).
6. **Негативный промпт (Negative Prompt):** Включите тумблер **Negative Prompt** справа или снизу и вставьте данный список исключений (он гарантирует отсутствие грязного пола, двойных ракурсов и кривых мечей):
   ```text
   black background, dark background, grey background, floor shadow, ambient shadow, color gradient, vignette, pedestal, circular base, float pedestal, duplicate items, floating weapons, weapons on side, multiple angles, background leaks, splatters, dirt, cropped image, bad anatomy, flat 2D graphic, volumetric dust.
   ```

---

### 🛡️ Точные промпты для генерации Основных Героев:

#### 1. Воин (Warrior / Paladin)
> Меч прочно зажат в правой руке, щит — на левом предплечье, Т-поза, чистейший изолированный белый фон.
```text
Symmetrical full-body 3D game model of a heroic warrior knight in heavy steel armor with gold accents, standing symmetrically in a clear front-facing T-pose. He is firmly holding a simple iron broadsword in his right hand. Stylized toy figurine aesthetic, high-detail clay render, bright lighting. Isolated on a solid flat pure white background (#ffffff), no floor shadows, no ambient occlusion, no pedestal, ready for rigging.
```

#### 2. Стрелок (Archer / Scout)
> Лук зажат в руке, стрелы в колчане за спиной, А-поза, симметрия.
```text
Symmetrical full-body 3D fantasy model of an elven archer in light leather forest armor, standing symmetrically in a straight front-facing A-pose. He is firmly holding a beautiful wooden recurve bow in his left hand, with a small quiver of arrows secured strictly on his back. Soft toy plastic shader, game-ready asset. Isolated on a solid flat pure white background (#ffffff), zero shadows on floor, no pedestal base, clear background.
```

#### 3. Маг (Mage / Wizard)
> Волшебный посох прочно зажат в правой руке, симметрия, Т-поза/А-поза (как на Скриншоте 1).
```text
Symmetrical front-facing studio photography of a single tabletop miniature figurine of a cosmic archmage in a long violet magic robe, standing in a straight front-facing T-pose, tightly holding a simple wooden magic staff with a small glowing blue crystal in his right hand. Cute claymation render style, stylized look, clean textures. Isolated on a solid flat pure white background (#ffffff), no ambient gradients, no pedestal, no shadows.
```

> **🔥 ВАЖНОЕ ДЕЙСТВИЕ:** После генерации наведите курсор на получившееся изображение в Leonardo.ai и нажмите на кнопку **«Remove Background»** (Вырезать фон). Скачайте полученный файл в формате **PNG с прозрачным фоном**.

---

## ЭТАП 2: 🚀 3D-реконструкция в Tencent Hunyuan 3D (Вэньшэнь 3D)

Сервис от Tencent является передовым решением для мгновенного превращения 2D-рисунков в готовые 3D-модели (Скриншот 2).

### ⚙️ Точные настройки генерации на сайте Hunyuan 3D:
1. Выберите режим **«Вэньшэнь 3D» (или «Изображение/Вэньшэнь 3D»)** на левой панели.
2. Вкладка сверху: **«Одиночное изображение»** (Single image).
3. Нажмите кнопку **«Загрузить изображения»** (Upload image) и загрузите ваш PNG-файл с прозрачным фоном, полученный из Leonardo.
4. **Выберите модель:** Выберите новейшую стабильную версию **«Поколение 3D - V3.1» (Generation 3.1)**.
5. **Номер модели лиц (Target Face / Vertex Count) — КРИТИЧЕСКИЙ ШАГ ДЛЯ ОПТИМИЗАЦИИ:**
   * На скриншоте представлены варианты: `1,5 M`, `1 M`, `500k`, `50k`.
   * **Выбор для минимальных настроек (Ultra-Low Mobile/PC):** Выбирайте строго **`50k`** (50 000 полигонов) или **`500k`** (500 000 полигонов). 
   * *Почему?* Модели класса `1.5 M` (1.5 миллиона полигонов) мгновенно перегрузят видеопамять (VRAM) мобильного устройства или слабого ПК, когда на тактическом поле боя `BattleScene` появится более 10-15 воинов одновременно. Вариант `50k` идеален — он выглядит превосходно на расстоянии камеры боя и потребляет в 30 раз меньше ресурсов!
6. Включите переключатель **«Земляная сетка»** (Ground Grid) в правом верхнем углу для контроля горизонтали.
7. Нажмите кнопку **«Генерируйте немедленно»** (Generate Now). Спустя 1-2 минуты скачайте 3D-модель в формате `.fbx` или `.gltf`/`.glb`.

---

## ЭТАП 3: 🦴 Создание скелета и привязка костей (Rigging)

Если Hunyuan 3D выдал статичную сетку (Mesh) без встроенного скелета (или кости привязаны некорректно), мы используем индустриальный стандарт автоматического риггинга — **Adobe Mixamo**.

1. Зайдите на бесплатный сайт [Mixamo](https://www.mixamo.com/).
2. Нажмите кнопку **Upload Character** справа и перетащите туда скачанный `.fbx` или `.obj` файл вашей модели.
3. В окне **Auto-Rigger** перетащите цветные маркеры на соответствующие суставы персонажа:
   * **Chin** (Подбородок) — на нижнюю челюсть.
   * **Wrists** (Запястья) — на кисти рук.
   * **Elbows** (Локти) — на локтевые сгибы.
   * **Knees** (Колени) — на коленные чашечки.
   * **Groin** (Пах) — в область промежности.
4. Нажмите **Next**. Mixamo сгенерирует полноценный скелет, совместимый со стандартом **Unity Humanoid**.
5. Теперь ваш персонаж готов принимать любые анимации!

---

## ЭТАП 4: 🎭 Унифицированная система анимаций (Один Контроллер на Всех!)

Чтобы игра работала без лагов даже на слабых устройствах и не ела гигабайты оперативной памяти, мы применим **профессиональную архитектуру общего Humanoid-скелета**.

### В чем секрет оптимизации?
В Unity все гуманоидные персонажи (как главные герои, так и обычные солдаты) могут использовать **один и тот же файл анимации в оперативной памяти** и управляться **одним общим Animator Controller**. Вам не нужно создавать 20 разных контроллеров! Вы настраиваете один легкий контроллер, а Unity автоматически проецирует его кости на любого гуманоида через систему **Avatar**.

### Скачивание анимаций из Mixamo:
Используйте строку поиска в Mixamo, чтобы скачать следующие движения (все они будут похожи и унифицированы):

1. **Анимация Покоя (Idle):**
   * Найдите анимацию `Idle`. Для каждого класса скачайте легкую вариацию:
     * *Воин:* Более тяжелая, устойчивая стойка с мечом.
     * *Стрелок:* Легкая, собранная стойка, лук опущен.
     * *Маг:* Мистическое покачивание, посох удерживается вертикально.
   * **Параметры скачивания:** `Format: FBX for Unity`, `Skin: With Skin` (скачиваем только ОДИН базовый файл персонажа с кожей, все остальные анимации скачиваем `Without Skin` для экономии места!).

2. **Анимация Перехода по Клеткам (Movement):**
   * Вместо сложных шагов во все стороны, скачайте ОДНУ качественную анимацию ходьбы/бега на месте `Run` или `Walk`.
   * **ОБЯЗАТЕЛЬНО** поставьте галочку **`In Place`** (Бег на месте) перед скачиванием! Персонаж будет бежать на месте, а физически перемещать его по тактической сетке мы будем сверхлегким C#-кодом. Это исключит баги с коллизиями и сэкономит CPU.

3. **Анимация Нападения (Attack):**
   * Скачайте универсальную анимацию удара перед собой (`Sword Slash` или `Melee Punch`). Она будет настроена так, что при подходе к цели фигурка воспроизведет этот удар.

4. **Анимация Супер Атаки (Ultimate Skill):**
   * Скачайте выразительную анимацию триумфа или каста заклинания обеими руками вверх (`Spell Casting` или `Victory Celebration`). Мы подстроим её под всех героев с помощью запуска различных визуальных эффектов (частиц) из кода.

---

## ЭТАП 5: 💻 Сверхлегкий C# Скрипт оптимизации `TacticalUnitAnimator.cs`

Этот скрипт устанавливается на каждую фигурку героя или воина на сцене `BattleScene`. Он сводит нагрузку на процессор и видеокарту к абсолютному минимуму за счет:
1. **Кэширования хэшей параметров** (исключает медленные строковые операции в каждом кадре).
2. **Использования событий culling-а** (анимации не вычисляются, когда объект находится вне зоны видимости камеры).
3. **Плавного математического Lerp-вращения** вместо тяжелой физики Unity.
4. **GPU Instancing** для материалов фигурок (все одинаковые воины рендерятся видеокартой за 1 проход!).

### Создайте файл `TacticalUnitAnimator.cs` и добавьте в проект:

```csharp
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TacticalUnitAnimator : MonoBehaviour
{
    [Header("Настройки Оптимизации")]
    [Tooltip("Тип анимации покоя: 0 = Воин, 1 = Стрелок, 2 = Маг")]
    public int idleType = 0;
    
    [Tooltip("Скорость плавного разворота фигурки")]
    public float rotationSpeed = 10f;

    private Animator animator;
    private Transform cachedTransform;
    
    // Кэшируем хэши параметров для экстремальной производительности CPU
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IdleTypeHash = Animator.StringToHash("IdleType");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
    private static readonly int SuperAttackTriggerHash = Animator.StringToHash("SuperAttack");

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private float moveSpeedValue = 0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        cachedTransform = transform;
        targetPosition = cachedTransform.position;
        targetRotation = cachedTransform.rotation;

        // Включаем важнейшую оптимизацию: отключаем обновление костей, если персонажа не видно на экране
        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        
        // Передаем уникальный тип стойки Idle в контроллер
        animator.SetInteger(IdleTypeHash, idleType);
    }

    private void Update()
    {
        // Плавное перемещение и разворот силами C# (работает быстрее физического движка)
        if (isMoving)
        {
            // Перемещаем фигурку к целевой клетке сетки
            cachedTransform.position = Vector3.MoveTowards(cachedTransform.position, targetPosition, Time.deltaTime * 5f);
            
            // Плавно разворачиваем в сторону движения
            if (targetRotation != cachedTransform.rotation)
            {
                cachedTransform.rotation = Quaternion.Slerp(cachedTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            // Если достигли цели, выключаем анимацию бега
            if (Vector3.Distance(cachedTransform.position, targetPosition) < 0.01f)
            {
                cachedTransform.position = targetPosition;
                isMoving = false;
                moveSpeedValue = 0f;
                animator.SetFloat(SpeedHash, 0f);
            }
        }
    }

    /// <summary>
    /// Приказ фигурке совершить пошаговый переход на новую тактическую клетку сетки
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
        moveSpeedValue = 1f;
        
        // Запускаем общую анимацию перехода (бег/шаг)
        animator.SetFloat(SpeedHash, moveSpeedValue);
    }

    /// <summary>
    /// Вызов стандартной атаки (подстраивается под всех воинов)
    /// </summary>
    public void PlayStandardAttack(Vector3 lookAtTarget)
    {
        LookAtTargetInstant(lookAtTarget);
        animator.SetTrigger(AttackTriggerHash);
    }

    /// <summary>
    /// Вызов разрушительной супер-атаки (выглядит как торжественный каст)
    /// </summary>
    public void PlaySuperAttack(Vector3 lookAtTarget)
    {
        LookAtTargetInstant(lookAtTarget);
        animator.SetTrigger(SuperAttackTriggerHash);
    }

    /// <summary>
    /// Моментальный разворот к противнику перед нанесением удара
    /// </summary>
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

## ЭТАП 6: 🗜️ Финальные настройки Unity 6 для экстремальной производительности

Чтобы игра летала со стабильными **60 FPS** даже на старых смартфонах или офисных ноутбуках, примените следующие настройки импорта к вашим 3D-моделям:

1. **Ограничение разрешения текстур (Texture Max Size):**
   * Выделите текстуру вашей фигурки в Unity.
   * В окне Inspector найдите пункт **Max Size** и установите его в значение **`512`** или **`1024`** вместо дефолтных `2048`. Разница в качестве на экране боя незаметна, но потребление VRAM снижается в **4 раза**!
   * Установите формат сжатия в **ASTC** (для мобильных) или **DXT5** (для PC).

2. **Включение GPU Instancing на материалах:**
   * Откройте материал (Material), используемый вашими фигурками.
   * Поставьте галочку **«Enable GPU Instancing»** (Включить инстансинг GPU) в самом низу параметров материала.
   * *Почему это важно?* Теперь, если на поле боя стоят 10 одинаковых пехотинцев, видеокарта нарисует их все за 1 операцию отрисовки (Draw Call), разгрузив шину данных процессора до нуля.

3. **Оптимизация Rig-скелета:**
   * Выберите импортированный FBX-файл персонажа, откройте вкладку **Rig** и убедитесь, что включена галочка **Optimize Game Objects**. Это скроет ненужные кости из иерархии объектов сцены (Hierarchy), освобождая Unity от необходимости пересчитывать тысячи пустых Transform-координат каждую секунду.

Следуя этой подробной пошаговой инструкции, вы создадите идеально оптимизированную, красивую и плавную боевую систему для **Fate Continent**!
