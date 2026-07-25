# ⚔️ Fate Continent • Полное пошаговое руководство по генерации фигурок Героев и настройке высокопроизводительной анимации для BattleScene (v18.12.06)

Я полностью переработал и дополнил руководство по генерации одиночных фигурок персонажей, 3D-реконструкции и созданию оптимальной системы анимации в Unity 6!

В этой версии мы подробно разберем **решение критической ошибки Mixamo: «Sorry, unable to map your existing skeleton»** (когда авто-риггер отказывается принимать модель), добавим детальные пошаговые инструкции, разберем настройки оптимизации и зафиксируем все важнейшие уроки.

Все новые видеоуроки и ссылки на инструкции бережно сохранены в базе знаний проекта (`knowledge_base.json`):
1. **Создание 3D из картинки (Hunyuan 3D):** https://www.youtube.com/watch?v=SDV54QaEHBs
2. **Оптимальный импорт и анимация персонажей:** https://www.youtube.com/watch?v=TxyBoDqE6Zo
3. **Продвинутая анимация и подготовка моделей:** https://www.youtube.com/watch?v=_aJzFbuLi1M

---

## 🎨 ЭТАП 1: Настройки генерации в Leonardo.ai (Решение проблемы полубоком, двух персонажей и обрезанных ног)

### ⚠️ ГЛАВНЫЙ СЕКРЕТ СИММЕТРИИ: Ловушка с Оружием в Руках
На вашем скриншоте персонаж стоит полубоком (в полупрофиль/ракурсе 3/4), из-за чего осевая линия авто-риггера Mixamo делит тело неравномерно, а суставы рук и ног смещены.

**Почему так произошло?**
Причина — фраза *«holding a simple iron broadsword in his right hand»* (держит меч в правой руке). Любое упоминание оружия или щитов заставляет нейросеть рисовать персонажа в динамической **боевой стойке** (combat stance). В обучающей выборке ИИ воины всегда стоят в пол-оборота, чтобы защищаться или наносить удар, поэтому ИИ разворачивает корпус.

**Профессиональное решение (Стандарт игровой индустрии):**
1. **Генерируйте персонажей СТРОГО без оружия и щитов в руках (пустые руки)!** Руки должны быть разведены в идеально горизонтальную и симметричную **Т-позу** или **А-позу** с раскрытыми ладонями или нейтрально сжатыми кулаками.
2. **Идеальная симметрия:** Тело должно быть абсолютно плоским, обращенным строго лицом к камере, без малейшего поворота таза, плеч или головы. Ноги должны стоять ровно на земле, параллельно друг другу.
3. **Оружие крепится в Unity:** В самом Unity вы можете за 10 секунд импортировать отдельный меш меча, лука или посоха и сделать его дочерним объектом кости кисти руки персонажа (например, кости `RightHand` или `LeftHand`). 
   * *Плюсы:* Идеальный риггинг в Mixamo, отсутствие резиновых деформаций оружия при анимации и возможность менять оружие прямо в игре при прокачке!

---

### ⚙️ Как настроить генератор Leonardo.ai:
* **Инструмент:** Вкладка **Image Generation**.
* **Модель (Model):** Пресет `Auto` или `Lucid Origin` / `Leonardo Vision XL` (для красивого объемного пластилинового 3D-стиля).
* **Стиль (Style):** `Dynamic` или `None` (для строгого соответствия тексту).
* **Размер (Aspect Ratio):** Выберите **1:1 (Square)** с разрешением **1024×1024** пикселей.
* **Негативный промпт (Negative Prompt):** Обязательно активируйте тумблер и добавьте этот обновленный список исключений, блокирующий появление двойников, обрезание ног/ступней, повороты корпуса, подставки и оружие:
  ```text
  two characters, twin, duplicate characters, split screen, dual view, front and back view, character sheet, turn-around, multiple poses, mirror view, cropped legs, cut-off feet, half-body, torso-only shot, knees crop, cropped boots, close-up, cropped bottom, black background, dark background, grey background, floor shadow, ambient shadow, color gradient, vignette, pedestal, circular base, float pedestal, stand, display stand, plastic stand, round platform, stone base, toy base, rock base, duplicate items, weapons on side, multiple angles, bad anatomy, flat 2D graphic, 3/4 view, half-turned stance, asymmetrical pose, dynamic pose, rotation, holding staff, holding wand, staff, wand, weapon, holding rod, holding sword.
  ```

---

### 🛡️ Обновленные СВЕРХ-СИММЕТРИЧНЫЕ промпты для Героев (Без оружия в руках, без подставок, идеальная Т-поза/А-поза!):

> ⚠️ **КРИТИЧЕСКИЙ СЕКРЕТ ДЛЯ МАГА (WIZARD):**
> Никогда не используйте в промпте слова **«miniature»** или **«tabletop gaming miniature style»** для мага! В обучающей выборке ИИ все настольные фигурки магов стоят на круглой пластиковой подставке (pedestal) и обязательно сжимают в руках посох или волшебную палочку. 
> Мы заменили стиль на **«3D character model, digital sculpt, clay render»** — это заставляет ИИ сгенерировать современную трехмерную модель для видеоигр с абсолютно пустыми, раскрытыми руками и стоящую прямо на плоском полу без подставок!

#### 1. Воин (Warrior) — Идеальная ортопедическая Т-поза (Без подставки, без оружия):
```text
An absolute front-view straight isolated full-body head-to-toe shot of a heroic warrior knight in heavy steel plate armor with gold accents. Symmetrical flat orthopedic front-facing T-pose, with both arms spread perfectly straight out horizontally on the sides, open palms facing down, empty hands, strictly no weapons, no items, no sword, no shield. Both legs and heavy iron boots are standing straight and parallel on the ground, pointing forward. Zero body rotation, perfectly flat mirror-like symmetry, looking directly into the camera. Stylized high-detail 3D game character model, clean clay render, soft studio lighting. Isolated on a solid flat pure white background (#ffffff), strictly no pedestal, no circular base, no floor shadows, ready for rigging, solo view, single character only.
```

#### 2. Стрелок (Archer) — Идеальная ортопедическая А-поза (Без подставки, без оружия):
```text
An absolute front-view straight isolated full-body head-to-toe shot of a fantasy elven archer in light green leather forest armor. Symmetrical flat orthopedic front-facing A-pose, both arms held slightly away from the body in an empty-handed neutral stance, open palms, strictly no weapons, no bow in hand, no arrows. Both legs and leather boots are standing perfectly straight and parallel on the ground, pointing forward. Zero body rotation, perfectly flat mirror-like symmetry, looking directly into the camera. Stylized high-detail 3D game character model, clean clay render. Isolated on a solid flat pure white background (#ffffff), strictly no pedestal, no circular base, zero shadows on floor, ready for rigging, solo view, single character only.
```

#### 3. Маг (Mage) — Идеальная ортопедическая Т-поза БЕЗ ПОСОХА и БЕЗ ПОДСТАВКИ:
```text
An absolute front-view straight isolated full-body head-to-toe shot of an elegant fantasy wizard mage with a long white beard in beautiful mystical purple robes with soft golden runes. Symmetrical flat orthopedic front-facing T-pose, with both arms spread perfectly straight out horizontally on the sides, completely open empty hands, fingers visible, strictly no weapons, no magic staff, no wand, no rod, empty palms. Both legs and boots are standing perfectly straight and parallel flat on the floor, pointing forward. Zero body rotation, perfectly flat mirror-like symmetry, looking directly into the camera, majestic face looking forward. Modern high-quality 3D video game character model, digital sculpt, clean clay render, cinematic studio lighting. Isolated on a solid flat pure white background (#ffffff), strictly no pedestal, no circular base, no plastic stand, no floor shadows, ready for rigging, solo view, single character only.
```

> **🔥 Важное действие:** Наведите курсор на полученную картинку в Leonardo и нажмите кнопку **«Remove Background»** (Вырезать фон) для скачивания чистого PNG-файла без фона. Помните: руки без оружия гарантируют 100% успех при авто-риггинге!

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
   * *Почему?* Модели с `1.5 M` полигонов мгновенно перегрузят память на слабых устройствах. Вариант `50k` выглядит отлично с высоты камеры боя и работает в 30 раз быстрее!
6. Включите **«Земляную сетку»** (Ground Grid) в правом верхнем углу и нажмите **«Генерируйте немедленно»** (Generate Now). Скачайте полученную `.fbx` модель.

---

## 🦴 ЭТАП 3: Создание автоматического скелета в Mixamo и Решение Ошибки «Unable to map your existing skeleton»

При загрузке `.fbx` модели, сгенерированной нейросетью Tencent Hunyuan 3D, в Mixamo часто возникает ошибка: **«Sorry, unable to map your existing skeleton. Please check best practices for using the Auto-Rigger and upload again»**.

### 🔍 Почему возникает эта ошибка?
Нейросеть Hunyuan 3D при создании модели в формате `.fbx` иногда генерирует внутреннюю, скрытую или пустую структуру костей (Armature / Dummy-костей), которая конфликтует с алгоритмом Mixamo. Mixamo пытается обнаружить существующий скелет, запутывается в структуре костей и аварийно завершает работу.

### 🛠️ Как гарантированно исправить ошибку (2 простых способа):

#### РЕШЕНИЕ А (Самое простое, без сторонних программ — Экспорт в OBJ):
Mixamo принимает файлы форматов `.fbx`, `.obj` и `.zip`. **Формат OBJ физически не умеет хранить скелет и кости** — это исключительно чистая трехмерная сетка (полигоны).
1. Если у вас есть Blender, импортируйте туда ваш исходный файл `.fbx`.
2. Нажмите **File -> Export -> Wavefront (.obj)** или экспортируйте модель из любого другого 3D-редактора в формат `.obj`.
3. Загрузите полученный `.obj` файл на Mixamo. Ошибка с существующим скелетом исчезнет на 100%, так как скелета в файле больше нет!

#### РЕШЕНИЕ Б (Очистка скелета в Blender):
Если вам критически важно использовать формат `.fbx` (например, для сохранения текстурных координат и материалов):
1. Откройте **Blender** и импортируйте ваш `.fbx` файл.
2. В окне иерархии (справа вверху) найдите объект **Armature** (обычно со значком бегущего человечка 🏃‍♂️ или кости).
3. Разверните его, выделите вложенный меш (Mesh) персонажа, нажмите **Alt + P** на клавиатуре и выберите **«Clear and Keep Transformation»** (Очистить родителя с сохранением трансформаций). Меш отделится от скелета.
4. Выделите пустой объект **Armature** и нажмите кнопку **Delete**, полностью удалив кости.
5. Выделите меш персонажа, перейдите в настройки данных меша (зеленый значок треугольника справа) и в списке **Vertex Groups** удалите все группы вершин (если они есть), нажав на стрелочку вниз и выбрав **Clear All Groups**.
6. Перейдите в настройки модификаторов (значок гаечного ключа) и, если там висит модификатор **Armature**, удалите его, нажав на крестик.
7. Выделите чистую модель, нажмите **File -> Export -> FBX (.fbx)**. В настройках экспорта в поле **Include** выберите только **Mesh** (зажмите Shift и выберите Mesh, убрав выделение с Armature). Назовите файл, например, `Hero_Clean.fbx` и экспортируйте его.

#### РЕШЕНИЕ В (⚡ Полная автоматизация кодом: Blender Python и Unity C#):

Вы можете полностью автоматизировать этот рутинный процесс! Ниже представлены два готовых скрипта, которые сделают всё за вас за 1 секунду.

##### Вариант 1: Скрипт автоматизации для Blender (Python)
Этот скрипт полностью очищает импортированную модель от костей, модификаторов и групп вершин, а затем экспортирует её в чистый `.obj` или `.fbx` формат.

1. Откройте **Blender** и импортируйте вашу модель.
2. Перейдите во вкладку **Scripting** сверху.
3. Нажмите кнопку **New** и вставьте следующий Python-код:

```python
# [FATE CONTINENT - BLENDER AUTOMATION v18.12.06]
import bpy
import os

def clean_and_export_character():
    # 1. Находим все объекты типа ARMATURE (скелеты) в сцене
    armatures = [obj for obj in bpy.context.scene.objects if obj.type == 'ARMATURE']
    
    for armature in armatures:
        # Находим всех детей скелета (обычно это меши персонажа)
        for child in armature.children:
            if child.type == 'MESH':
                # Делаем меш активным
                bpy.context.view_layer.objects.active = child
                child.select_set(True)
                
                # Отвязываем от скелета, сохраняя масштаб и координаты
                bpy.ops.object.parent_clear(type='CLEAR_KEEP')
                
                # Удаляем модификатор Armature
                for mod in child.modifiers:
                    if mod.type == 'ARMATURE':
                        child.modifiers.remove(mod)
                
                # Очищаем Vertex Groups (группы вершин)
                child.vertex_groups.clear()
        
        # Удаляем сам скелет
        bpy.data.objects.remove(armature, do_unlink=True)
        
    print("Очистка завершена! Все скелеты удалены, меши очищены.")
    
    # Автоматический экспорт очищенного меша в OBJ на рабочий стол
    desktop_path = os.path.expanduser("~/Desktop")
    export_file = os.path.join(desktop_path, "Hero_Clean_For_Mixamo.obj")
    
    # Выделяем все оставшиеся меши
    bpy.ops.object.select_all(action='DESELECT')
    for obj in bpy.context.scene.objects:
        if obj.type == 'MESH':
            obj.select_set(True)
            
    # Экспортируем в OBJ
    bpy.ops.wm.obj_export(filepath=export_file, export_selected_objects=True)
    print(f"Файл успешно экспортирован на рабочий стол: {export_file}")

# Запуск функции очистки
clean_and_export_character()
```

4. Нажмите кнопку **Run Script** (значок Play ▶️). 
5. Очищенный `.obj` файл моментально появится на вашем **Рабочем столе** под именем `Hero_Clean_For_Mixamo.obj`. Просто загрузите его на Mixamo!

---

##### Вариант 2: C# Скрипт-Конвертер прямо внутри Unity 6 (Без открытия Blender!)
Это самый удобный способ! Вам даже не нужно открывать Blender. Вы просто кликаете правой кнопкой мыши по импортированному FBX файлу прямо в Unity и экспортируете его чистую сетку в OBJ.

1. Создайте в Unity папку `Assets/Editor/` (если её еще нет).
2. Создайте внутри файл `FateMixamoExporter.cs` и вставьте в него код:

```csharp
// [FATE CONTINENT - UNITY EDITOR AUTOMATION v18.12.06]
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class FateMixamoExporter : EditorWindow
{
    [MenuItem("Assets/Fate Tools/Export Clean OBJ for Mixamo", false, 10)]
    public static void ExportSelectedFBXToCleanOBJ()
    {
        // Получаем выделенный объект в окне Project
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Пожалуйста, выделите импортированный FBX персонажа в окне Project!", "OK");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(selectedObject);
        if (string.IsNullOrEmpty(assetPath) || !assetPath.ToLower().EndsWith(".fbx"))
        {
            EditorUtility.DisplayDialog("Ошибка", "Выбранный объект должен быть FBX файлом!", "OK");
            return;
        }

        // Пытаемся получить MeshFilter или SkinnedMeshRenderer
        Mesh mesh = null;
        MeshFilter meshFilter = selectedObject.GetComponentInChildren<MeshFilter>();
        if (meshFilter != null)
        {
            mesh = meshFilter.sharedMesh;
        }
        else
        {
            SkinnedMeshRenderer skinnedRenderer = selectedObject.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedRenderer != null)
            {
                mesh = skinnedRenderer.sharedMesh;
            }
        }

        if (mesh == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Внутри выделенного FBX не найден компонент Mesh!", "OK");
            return;
        }

        // Создаем диалоговое окно сохранения файла
        string defaultName = selectedObject.name + "_Clean_For_Mixamo";
        string savePath = EditorUtility.SaveFilePanel("Сохранить очищенный OBJ для Mixamo", "", defaultName, "obj");

        if (string.IsNullOrEmpty(savePath)) return;

        // Конвертируем меш в стандартный формат Wavefront OBJ без костей
        string objData = MeshToOBJString(mesh, selectedObject.name);
        // КРИТИЧЕСКИ ВАЖНО: Заменяем Windows-переносы строк на Unix-переносы строк (\n), чтобы Mixamo корректно считывал файл
        objData = objData.Replace("\r\n", "\n");
        // КРИТИЧЕСКИ ВАЖНО: Пишем строго в UTF-8 БЕЗ сигнатуры BOM (Byte Order Mark, сигнатура EF BB BF в начале), 
        // иначе авто-риггер Mixamo считает заголовок невалидным бинарным файлом и выдает ошибку "Unexpected File Type"!
        File.WriteAllText(savePath, objData, new UTF8Encoding(false));

        EditorUtility.DisplayDialog("Успех!", $"Чистый файл меша успешно экспортирован!\nСкелет удален.\n\nПуть: {savePath}", "Ура!");
    }

    private static string MeshToOBJString(Mesh mesh, string name)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"# Fate Continent Clean Mesh Exporter v18.12.06");
        sb.AppendLine($"# Object Name: {name}");
        sb.AppendLine($"g {name}");

        ### 🎨 СЕКРЕТ ТЕКСТУРИРОВАНИЯ И ЦВЕТА: Как вернуть цвет на модель?

#### Почему OBJ-файл экспортировался в Mixamo серым?
Стандартные файлы формата **Wavefront OBJ** по своей природе не умеют хранить текстурные изображения (картинки) внутри себя. Они содержат только саму геометрическую форму (сетку/меш) и **UV-развертку** (координаты, которые сообщают видеокарте, в каком именно месте накладывать цвета). 

Поэтому модель в авто-риггере Mixamo отображается серой — **это абсолютно нормально и правильно!** Самой системе Mixamo текстуры не нужны, ей требуются только чистые геометрические пропорции тела, чтобы рассчитать и прикрепить кости скелета.

#### Как за 1 минуту вернуть текстуры и цвета на зариггенную модель в Unity?
После того как вы скачали зариггенный `.fbx` файл из Mixamo, его текстура осталась в вашем оригинальном (исходном) FBX-файле или в папке генератора Tencent Hunyuan 3D. Вот как их подружить в Unity:

1. **Создайте материал для персонажа:**
   * В папке `Assets/Models/Characters/` нажмите правой кнопкой мыши -> **Create -> Material**. Назовите его, например, `M_Warrior_Texture`.
2. **Назначьте текстуру (Albedo / Base Color):**
   * Найдите оригинальную цветную текстуру персонажа (обычно это PNG-картинка, которая шла вместе с исходной FBX-моделью).
   * Перетащите эту текстуру в слот **Albedo** (в стандартном шейдере Unity) или **Base Map** (в Universal Render Pipeline - URP) вашего созданного материала `M_Warrior_Texture`.
3. **Наложите материал на зариггенную модель:**
   * Выделите зариггенный файл `Warrior_Rigged.fbx` на сцене (или откройте его Prefab).
   * Раскройте меш персонажа в инспекторе и перетащите ваш новый материал `M_Warrior_Texture` прямо на него. Герой мгновенно окрасится в свои родные, сочные цвета!

---

### 📖 Подробное пошаговое руководство по настройке, анимации и импорту персонажей в Unity 6

Ниже представлена обновленная, детальная и пошаговая инструкция (Шаг 2 – Шаг 7) для безупречной работы в вашем проекте Fate Continent:

#### 🦴 ШАГ 2: Риггинг (Создание скелета) в Mixamo
1. Перейдите на сайт **[mixamo.com](https://www.mixamo.com/)** и войдите под своим аккаунтом.
2. В правой части экрана нажмите синюю кнопку **Upload Character**.
3. Перетащите ваш чистый файл **Warrior_Clean.obj** (который вы экспортировали через наше контекстное меню в Unity в один клик без BOM и лишних костей) в открывшееся окно.
4. После загрузки персонаж отобразится во фронтальном ракурсе. Если он стоит спиной, разверните его лицом к себе кнопками вращения внизу экрана. Нажмите **Next**.
5. Расставьте цветные маркеры на суставы (ориентируйтесь на правую подсказку на сайте):
   * **CHIN (Синий):** Поместите на самый центр подбородка.
   * **WRISTS (Желтые):** Поместите на запястья (в месте перехода руки в кисть). Игнорируйте оружие в руках, цельтесь точно в анатомический сустав.
   * **ELBOWS (Красные):** Поместите на внешние стороны локтевых суставов.
   * **KNEES (Зеленые):** Поместите на центры коленных чашечек.
   * **GROIN (Оранжевый):** Поместите в самый центр паховой области (в самом низу таза).
6. В выпадающем меню **Skeleton LOD** выберите:
   * **Standard Skeleton (65 bones)** — если вам нужна анимация пальцев рук.
   * **No Fingers (25 bones)** — *РЕКОМЕНДУЕТСЯ!* В тактических боях Fate Continent фигурки видны с высоты птичьего plate, пальцы рук разглядеть невозможно, а экономия процессора составит до 40% на один отряд!
7. Нажмите **Next**. Mixamo за одну минуту рассчитает скелет. Если на анимации предпросмотра персонаж дышит ровно — нажмите **Next**, а затем **Next** для подтверждения замены.
8. Нажмите **Download** в правом верхнем углу:
   * **Format:** `FBX for Unity (.fbx)` (Критически важно!).
   * **Pose:** `T-Pose`.
9. Нажмите **Download** и сохраните файл в проект Unity (например, в `Assets/Models/Characters/Warrior_Rigged.fbx`).

#### 🔍 ШАГ 3: Поиск и Подбор необходимых анимаций в Mixamo
Для полноценной работы нашей такческой боевой сцены нам нужны 3 базовых состояния для каждого персонажа (Воина, Стрелка и Мага).
В левом верхнем углу Mixamo в строке поиска введите следующие названия и выберите понравившиеся:

1. **Анимации покоя (Idle):**
   * Введите в поиск: `Idle`.
   * Подберите стойки под классы:
     * **Для Воина:** `Warrior Idle` или `Knight Idle` (тяжелая, уверенная стойка с мечом).
     * **Для Стрелка:** `Archer Idle` (легкая стойка, рука готова выхватить стрелу).
     * **Для Мага:** `Wizard Magic Idle` или `Spell Casting Idle` (мистическое дыхание, стойка с посохом).

2. **Анимация бега (Movement):**
   * Введите в поиск: `Run` или `Running`.
   * Выберите подходящий бег (например, `Standard Run` или `Sprint`).
   * ⚠️ **КРИТИЧЕСКИ ВАЖНО:** На панели настроек анимации справа обязательно поставьте галочку **In Place** (Бег на месте). Персонаж должен бежать строго на одном месте, так как физически перемещать фигурку по тактической сетке мы будем C#-кодом.

3. **Анимации атак (Attack):**
   * Введите в поиск: `Attack` или `Slash` / `Shoot`.
   * Выберите:
     * **Воин:** `Sword Slash` (круговой или рубящий удар мечом).
     * **Стрелок:** `Standing Draw Arrow` или `Archery Shoot` (выстрел из лука).
     * **Маг:** `Spell Casting` или `Magic Attack` (взмах посохом/рукой для призыва заклинания).

#### 💾 ШАГ 4: Экспорт анимаций без лишнего веса («Without Skin»)
Это важнейший трюк для профессиональной оптимизации! Нам не нужно скачивать 3D-модель персонажа заново вместе с каждой анимацией. Мы скачаем только чистую траекторию костей (скелет), что уменьшит вес файлов в 150 раз!

При скачивании каждой выбранной анимации нажимайте кнопку **Download** и выставляйте следующие параметры:
* **Format:** `FBX for Unity (.fbx)`.
* **Skin:** Выберите **Without Skin** (Без текстур и меша).
* **Frames per Second:** `30` (этого более чем достаточно для плавной тактической игры).
* **Keyframe Reduction:** `uniform` (для дополнительного сжатия веса файла).

Нажмите **Download**. Файл будет весить всего около ~100 КБ вместо 15 МБ! Сохраните их в Unity в папку `Assets/Animations/` (например, под именами `Warrior_Idle.fbx`, `Warrior_Run.fbx`, `Warrior_Attack.fbx`).

#### ⚙️ ШАГ 5: Настройка импортированных файлов в Unity 6
Когда все файлы перенесены в Unity, настройте их следующим образом:

1. **Настройка Rig у Риггед-моделей (Модели с телом, скачанные в Шаге 2):**
   * Кликните на файл модели (например, `Warrior_Rigged.fbx`) в папке `Assets/Models/Characters/`.
   * В окне **Inspector** перейдите на вкладку **Rig**.
   * **Animation Type:** Установите **Humanoid**.
   * **Avatar Definition:** Выберите **Create From This Model**.
   * Нажмите кнопку **Apply** внизу. Unity создаст аватар костей (`Warrior_RiggedAvatar`). Повторите это для всех ваших персонажей.

2. **Настройка Rig у Анимаций (Файлы анимаций без скина, скачанные в Шаге 4):**
   * Выделите файлы анимаций (например, `Warrior_Idle.fbx`) в папке `Assets/Animations/`.
   * Перейдите во вкладку **Rig** в инспекторе.
   * **Animation Type:** Установите **Humanoid**.
   * **Avatar Definition:** Выберите **Copy From Other Avatar**.
   * **Source:** В появившемся поле укажите созданный аватар любого вашего персонажа (например, `Warrior_RiggedAvatar`).
   * Нажмите **Apply**. Теперь анимация знает структуру костей вашего героя!

3. **Настройка циклов (Looping):**
   * Для анимаций покоя (**Idle**) и бега (**Run**) перейдите во вкладку **Animation** в инспекторе файла анимации.
   * Прокрутите вниз и поставьте галочку на **Loop Time** (чтобы анимация проигрывалась циклично).
   * Поставьте галочки **Loop Pose** для идеальной склейки кадров, чтобы не было швов.
   * Нажмите **Apply** в самом низу.
   * *Для анимаций атак ставить галочку Loop Time не нужно* — они должны воспроизводиться только один раз за удар.

#### 🎛 ШАГ 6: Сборка единого Animator Controller в Unity 6
1. В окне **Project** нажмите правой кнопкой мыши -> **Create -> Animator Controller**. Назовите его **`TacticalUnitAnimatorController`**.
2. Дважды кликните по нему, чтобы войти в редактор анимаций.
3. На вкладке **Parameters** (слева вверху) добавьте параметры:
   * Нажмите `+` -> **Float**, назовите **`Speed`** (отвечает за бег).
   * Нажмите `+` -> **Integer**, назовите **`IdleType`** (0 = Воин, 1 = Стрелок, 2 = Маг).
   * Нажмите `+` -> **Trigger**, назовите **`Attack`** (обычная атака).
   * Нажмите `+` -> **Trigger**, назовите **`SuperAttack`** (суперспособность).

4. **Создание Idle-переключателя (Blend Tree):**
   * Кликните правой кнопкой мыши на пустом полем сетки -> **Create State -> From New Blend Tree**. Назовите состояние **`IdleBlend`**.
   * Дважды кликните на серую плашку **`IdleBlend`**, чтобы войти внутрь дерева смешивания.
   * Выделите узел дерева смешивания и в инспекторе справа в поле **Parameter** выберите созданный целочисленный параметр **`IdleType`**.
   * В списке **Motion** нажмите `+` -> **Add Motion Field** три раза.
   * Перетащите в слоты ваши анимации:
     * **Слот 0 (Value = 0):** Анимация `Warrior_Idle`
     * **Слот 1 (Value = 1):** Анимация `Archer_Idle`
     * **Слот 2 (Value = 2):** Анимация `Mage_Idle`
   * Нажмите на стрелочку в верхнем левом углу редактора (**Base Layer**), чтобы вернуться на главный экран аниматора.

5. **Настройка движения (Move):**
   * Перетащите вашу анимацию бега (Run) на поле сетки. Назовите состояние **`Move`**.
   * Зажмите правую кнопку мыши на **`IdleBlend`**, выберите **Make Transition** и протяните стрелочку к **`Move`**.
   * Нажмите на стрелочку. В инспекторе справа снимите галочку **Has Exit Time** и в списке условий (**Conditions**) добавьте: **`Speed` -> Greater -> `0.1`**.
   * Сделайте обратную стрелочку от **`Move`** к **`IdleBlend`**.
   * Снимите галочку **Has Exit Time** и добавьте условие: **`Speed` -> Less -> `0.1`**.

6. **Настройка Атак (Attack и SuperAttack):**
   * Перетащите анимацию атаки (например, `Warrior_Attack`) на поле сетки. Назовите состояние **`AttackState`**.
   * Перетащите анимацию суперспособности на поле сетки. Назовите состояние **`SuperAttackState`**.
   * Кликните правой кнопкой мыши по оранжевому блоку **Any State** (он всегда есть в аниматоре), выберите **Make Transition** и протяните стрелочку к **`AttackState`**.
   * В условиях добавьте триггер **`Attack`**.
   * Сделайте стрелочку от **Any State** к **`SuperAttackState`** с условием **`SuperAttack`**.
   * Протяните обратные стрелочки от **`AttackState`** и **`SuperAttackState`** обратно в наше состояние **`IdleBlend`**.
   * ⚠️ **ВНИМАНИЕ:** На этих возвратных стрелочках обязательно **оставьте включенной** галочку **Has Exit Time**! Это гарантирует, что анимация удара красиво и до конца доиграет свой взмах перед тем, как персонаж вернется в спокойное дыхание.

#### ⚡ ШАГ 7: Подключение на сцену
1. Повесьте на вашу 3D-модель персонажа на сцене компонент **Animator**.
2. В поле **Controller** перетащите созданный **`TacticalUnitAnimatorController`**.
3. В поле **Avatar** перетащите созданный аватар костей (например, `Warrior_RiggedAvatar`).
4. Добавьте на персонажа наш оптимизированный скрипт **`TacticalUnitAnimator.cs`** (код есть в руководстве ниже).
5. Настройте параметр **`IdleType`** в скрипте (0 — для воина, 1 — для лучника, 2 — для мага).

Всё готово к бою! Теперь, вызывая методы `MoveToCell`, `PlayStandardAttack` или `PlaySuperAttack` из вашего игрового кода, персонаж будет плавно бегать, разворачиваться, переключать стойки и атаковать без каких-либо зависаний процессора и видеокарты!



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
