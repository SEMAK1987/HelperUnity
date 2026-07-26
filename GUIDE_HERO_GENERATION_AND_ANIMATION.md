# ⚔️ Fate Continent • Полное пошаговое руководство по генерации фигурок Героев и настройке высокопроизводительной анимации для BattleScene (v18.12.06)

Я полностью переработал и дополнил руководство по генерации одиночных фигурок персонажей, 3D-реконструкции и созданию оптимальной системы анимации в Unity 6!

В этой версии мы подробно разберем **решение критической ошибки Mixamo: «Sorry, unable to map your existing skeleton»** (когда авто-риггер отказывается принимать модель), добавим детальные пошаговые инструкции, разберем настройки оптимизации и зафиксируем все важнейшие уроки.

Все новые видеоуроки и ссылки на инструкции бережно сохранены в базе знаний проекта (`knowledge_base.json`):
1. **Создание 3D из картинки (Hunyuan 3D):** https://www.youtube.com/watch?v=SDV54QaEHBs
2. **Оптимальный импорт и анимация персонажей:** https://www.youtube.com/watch?v=TxyBoDqE6Zo
3. **Продвинутая анимация и подготовка моделей:** https://www.youtube.com/watch?v=_aJzFbuLi1M

---

## 🎨 ЭТАП 1: Настройки генерации в Leonardo.ai (Решение проблемы полубоком, двух персонажей и обрезанных ног)

### ⚠️ ТРИ ГЛАВНЫХ СЕКРЕТА ДЛЯ СОВЕРШЕННОЙ Т-ПОЗЫ БЕЗ ОРУЖИЯ:

На ваших скриншотах возникли 3 классические проблемы ИИ-генерации персонажей для 3D:
1. **У воина срезало тело и получились только ноги:** Это происходит, когда модель пытается сделать макро-кадр текстуры брони. Решается жестким требованием "full-body head-to-toe shot" и указанием центрирования персонажа с пустым пространством вокруг («with generous empty space at the top, bottom, and sides so no part is cut off»).
2. **У стрелка сгенерировались женщины и несколько ракурсов (сетка):** Название класса "ranger archer" без указания пола по умолчанию часто вызывает женских эльфов. А слова типа "character sheet" или "concept art" в позитивном промпте провоцируют ИИ на создание листов с ракурсами (сбоку/сзади). Решается явным добавлением слова **«male»** (мужчина) в позитивный промпт, удалением любых слов "sheet/poses/views" из позитивного промпта и строгим переносом их в Negative Prompt!
3. **Маг встал в А-позу (Л-позу) и держит посох:**
   * **Золотое правило ИИ:** *Никогда не пишите слова-отрицания типа "no weapon, no staff, no sword" в ПОЗИТИВНОМ промпте!* Нейросети (особенно Leonardo Diffusion / Phoenix) не понимают частицу "not/no". Они видят слово "staff/weapon" и тут же рисуют его в руке! Оружие и посохи должны быть прописаны **СТРОГО И ТОЛЬКО в Negative Prompt**!
   * Чтобы руки поднялись из А-позы в идеальную горизонтальную Т-позу под углом 90 градусов, нужно математически точно описать положение рук в позитивном промпте («both arms are perfectly raised and outstretched horizontally straight to the sides at a strict 90-degree angle... forming a rigid T-shape»).

---

### 🚫 КРИТИЧЕСКИ ВАЖНО: Настройка полей в Leonardo.ai
Чтобы получить 100% результат, вам необходимо правильно заполнить **оба** текстовых поля в генераторе Leonardo:

1. **Активируйте тумблер «Add Negative Prompt» (Добавить негативный промпт)** в настройках Leonardo (он находится прямо под полем основного промпта или в левой панели).
2. В основное (верхнее) поле вставьте один из наших **Позитивных промптов** ниже.
3. В появившееся нижнее поле вставьте наш **Универсальный Негативный промпт**. Он гарантирует, что у мага не будет посоха, у стрелка не будет лука, никто не будет стоять спиной или боком, и все персонажи будут строго мужчинами во весь рост!

---

### 🛑 Универсальный Негативный Промпт для Т-позы (Скопируйте в поле Negative Prompt):
```text
two characters, three characters, multiple characters, twin, duplicate characters, split screen, dual view, front and back view, character sheet, turn-around, multiple poses, mirror view, cropped legs, cut-off feet, half-body, torso-only shot, knees crop, cropped boots, close-up, cropped bottom, black background, dark background, grey background, floor shadow, ambient shadow, color gradient, vignette, pedestal, circular base, float pedestal, stand, display stand, plastic stand, round platform, stone base, toy base, rock base, duplicate items, weapons on side, multiple angles, bad anatomy, flat 2D graphic, 3/4 view, half-turned stance, asymmetrical pose, dynamic pose, rotation, holding staff, holding wand, staff, wand, weapon, holding rod, holding sword, holding bow, holding dagger, dagger, sword, bow, shield, quiver, forest background, trees, woods, outdoor background, leaves, nature background, grass, landscape, plants, scenic background, female, woman, girl, lady, feminine features
```
*(**Примечание:** теги `female, woman, girl, lady` в конце этого негативного списка физически блокируют появление женских персонажей, гарантируя получение именно мужественных Героев. Если вам когда-нибудь понадобится женский персонаж, просто сотрите эти 4 слова из негативного промпта).*

---

### 🛡️ Обновленные СВЕРХ-СИММЕТРИЧНЫЕ промпты для Героев (Сверхточная ортопедическая Т-поза без подставок и оружия):

> ⚠️ **КРИТИЧЕСКИЕ СЕКРЕТЫ ДЛЯ ПРЕДОТВРАЩЕНИЯ СМАЗЫВАНИЯ И СЛИЯНИЯ КОСТЕЙ В MIXAMO:**
> 1. **Проблема слияния подмышек (Smudged Weights):** Если у персонажа (особенно у Мага в мантии или Стрелка в плаще) широкие рукава или свисающая ткань, 3D-нейросеть объединяет руки с телом. В Mixamo при попытке пошевелить рукой будет тянуться кожа с боков персонажа («смазанная геометрия»).
> 2. **Решение:** В новые промпты мы добавили строгие требования: **«wide negative space under the armpits»** (широкое пустое пространство под подмышками), **«completely separate arms from torso»** (полностью отделенные от тела руки), **«tight-fitting sleeves»** (облегающие рукава) и **«no hanging cloth, no cape, no draped fabric»** (никакой свисающей ткани, плащей или складок, соединяющих руки с туловищем). Это заставит ИИ нарисовать идеальную Т-позу с четкими просветами!

#### 1. Воин (Warrior) — Идеальная ортопедическая Т-поза (Без подставки, без оружия, руки строго горизонтально):
```text
An absolute front-view straight isolated full-body head-to-toe shot of a powerful male human knight in heavy steel plate armor with gold accents. Symmetrical flat orthopedic front-facing T-pose, both arms are perfectly raised and outstretched horizontally straight to the sides at a strict 90-degree angle relative to the spine, parallel to the ground, forming a perfect straight line across the shoulders (rigid T-shape). Empty hands, open flat palms facing the floor, fingers separated. Wide open negative space under the armpits, ensuring the arms are completely detached from the torso, tight-fitting armor plates. Both legs and heavy iron boots are standing straight and parallel flat on the ground, pointing forward. Zero body rotation, perfectly flat mirror-like symmetry, looking directly into the camera. The entire character, from the top of the helmet to the bottom of the boots, is fully contained and centered inside the frame, with generous empty space at the top, bottom, and sides so no part of the body is cut off or cropped. Stylized high-detail 3D game character model, clean clay render, soft studio lighting. Isolated on a solid flat pure white background (#ffffff), strictly no floor shadows, ready for rigging, solo view, single character only.
```

#### 2. Стрелок (Archer) — Идеальная ортопедическая Т-поза (Без подставки, без оружия, без плаща, руки строго в стороны):
```text
An absolute front-view straight isolated full-body head-to-toe shot of a handsome male elven hunter ranger in tight leather armor with green trim. Symmetrical flat orthopedic front-facing T-pose, both arms are perfectly raised and outstretched horizontally straight to the sides at a strict 90-degree angle relative to the spine, parallel to the ground, forming a perfect straight line across the shoulders (rigid T-shape). Empty hands, open flat palms facing the floor, fingers separated and visible. Wide open negative space under the armpits, ensuring the arms are completely detached from the torso, tight-fitting sleeves, no cape, no hanging cloth. Both legs and flat leather boots are standing perfectly straight and parallel flat on the ground, pointing forward. Zero body rotation, perfectly flat mirror-like symmetry, looking directly into the camera, handsome clear face. The entire character, from the top of the head to the bottom of the boots, is fully contained and centered inside the frame, with generous empty space at the top, bottom, and sides so no part of the body is cut off or cropped. Modern high-quality 3D video game character model, digital sculpt, clean clay render, cinematic studio lighting. Isolated on a solid flat pure white background (#ffffff), strictly no floor shadows, ready for rigging, solo view, single character only.
```

#### 3. Маг (Mage) — Идеальная ортопедическая Т-поза БЕЗ ПОСОХА, БЕЗ ШИРОКИХ РУКАВОВ (Чистая подмышечная зона, руки строго в стороны):
```text
An absolute front-view straight isolated full-body head-to-toe shot of a wise old male wizard mage with a white beard in purple mystical robes with gold runes. Symmetrical flat orthopedic front-facing T-pose, both arms are perfectly raised and outstretched horizontally straight to the sides at a strict 90-degree angle relative to the spine, parallel to the ground, forming a perfect straight line across the shoulders (rigid T-shape). Empty hands, open flat palms facing the floor, fingers separated. Wide open negative space under the armpits, ensuring the arms are completely detached from the torso, tight-fitting sleeves, strictly no wide hanging cloth, no cape, no draped fabric. Both legs and boots are standing perfectly straight and parallel flat on the ground, pointing forward. Zero body rotation, perfectly flat mirror-like symmetry, looking directly into the camera, majestic face. The entire character, from the top of the head to the bottom of the boots, is fully contained and centered inside the frame, with generous empty space at the top, bottom, and sides so no part of the body is cut off or cropped. Modern high-quality 3D video game character model, digital sculpt, clean clay render, cinematic studio lighting. Isolated on a solid flat pure white background (#ffffff), strictly no floor shadows, ready for rigging, solo view, single character only.
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

## 🔮 КОРРЕКТИРОВКА КОСТЕЙ В BLENDER ПОД ПЕРСОНАЖА МАГА (ИНДИВИДУАЛЬНАЯ НАСТРОЙКА)

Когда вы используете автоматический генератор костей (`blender_rigging_helper.py`), он строит идеальный скелет по среднестатистическим пропорциям. Однако у конкретно вашей модели мага (особенно в свободных одеждах или с широко расставленными руками в А-позе или Т-позе) реальные руки могут находиться под небольшим углом вниз или быть шире стандартных расчетов. Из-за этого кости плеч, локтей и запястий могут оказаться смещены внутрь тела или не доходить до реальных суставов меша, вызывая сильные "резиновые" искажения при анимации.

### 🛑 РЕШЕНИЕ ПРОБЛЕМЫ: «Нажимаю Tab или ищу меню, но ничего не переключается!»
Если вы нажимаете клавишу **Tab**, но скелет не переходит в режим редактирования, а кости не превращаются в граненые октаэдры с шариками-суставами, это происходит по одной из трех причин в Blender:
1. **Выделена отдельная кость в дереве вместо самого объекта:** На вашем скриншоте в правой панели выделена строчка `Hips` (кость таза). Blender заблокировал режим, так как фокус находится на кости. 
   * **Решение:** Кликните левой кнопкой мыши по любой оранжевой кости прямо в главном центральном 3D-окне (или выделите верхнюю строку **`FateHumanoid_Armature`** в правом верхнем списке Scene Collection, а не раскрытые под-элементы типа Hips).
2. **Фокус клавиатуры ушел из 3D-окна:** Если вы кликнули по панели справа, клавиша Tab будет пытаться переключить кнопки интерфейса, а не режим скелета.
   * **Решение:** Просто наведите курсор мыши в центр главного 3D-окна с персонажем и нажмите **Tab**.
3. **Прямое переключение через меню (100% рабочий способ без клавиши Tab):**
   * В самом левом верхнем углу главного 3D-окна (прямо под надписью *View / Select / Add*) найдите выпадающий список режимов. На вашем скриншоте там сейчас написано **`Pose Mode`** (Режим Позы).
   * Кликните по этому списку левой кнопкой мыши и выберите **`Edit Mode`** (Режим Редактирования).
   * **Альтернатива:** Наведите мышь в 3D-окно и нажмите комбинацию клавиш **Ctrl + Tab**. Откроется круговое меню, где выберите пункт **Edit Mode** (вверху круга). Кости мгновенно станут октаэдрами!

---

### Активация симметрии (X-Axis Mirror):
В верхнем правом углу 3D-окна (3D Viewport) найдите и нажмите на круглую кнопку со значком бабочки **«X»** (Symmetry по оси X).
Теперь при перемещении любого сустава на левой стороне (например, локтя `LowerArm.L`), Blender будет автоматически и абсолютно зеркально перемещать правый сустав (`LowerArm.R`).

### Точечная подгонка суставов по форме меша мага:
1. Нажмите клавишу **Numpad 1** (вид строго спереди).
2. Выделите шарик начала плеча (`UpperArm.L`). Нажмите клавишу **G** (перемещение) и перетащите его точно в плечевой сустав мантии мага.
3. Выделите шарик локтя (стык между `UpperArm.L` и `LowerArm.L`). Нажмите **G** и сдвиньте его в центр локтевого сгиба рукава мага.
4. Выделите шарик запястья (стык между `LowerArm.L` и `Hand.L`). Нажмите **G** и передвиньте его точно к началу кисти мага.
5. Выделите конечный шарик пальцев (`Hand.L`) и перетащите его к кончикам пальцев модели.

### Проверка глубины (вид сбоку):
1. Нажмите клавишу **Numpad 3** (вид сбоку).
2. Убедитесь, что центральные кости позвоночника (`Spine`, `Chest`, `Neck`, `Head`) проходят ровно по середине объема туловища мага, а не выходят наружу через грудь или спину. При необходимости выделите их и сместите по оси Y (клавиши **G** -> **Y**).

### Выход из режима редактирования:
Нажмите **Tab** для возврата в **Object Mode** (или выберите Object Mode в том же выпадающем списке в левом верхнем углу). Все кости зафиксируются на своих идеальных местах!

---

## 🎭 РЕШЕНИЕ ПРОБЛЕМЫ: «Искажается или кривится лицо/борода мага при поворотах головы и тела»

**Почему это происходит:** 
При автоматической привязке костей (Auto-Weighting) Blender пытается угадать, какие кости управляют какими частями тела. Так как борода мага свисает очень близко к груди и плечам, программа по ошибке приписала влияние костей **`Chest`** (грудь), **`Shoulder.L` / `Shoulder.R`** (плечи) или **`Neck`** (шея) к вершинам лица и бороды. Когда маг поворачивает голову или тело, эти кости начинают тянуть лицо в разные стороны, вызывая сильные "резиновые" искажения, асимметрию и заломы меша.

**Решение:** Нам нужно сделать так, чтобы вся голова, лицо и борода на 100% подчинялись только кости **`Head`**, и абсолютно не зависели от груди, шеи или плеч.

### 🛠️ Способ 1: Использование Vertex Groups (Группы вершин) — Самый быстрый и 100% точный метод

Этот способ убирает "нежелательное влияние" на микроскопическом уровне без необходимости красить кистью вручную:

1. **Войдите в режим редактирования меша (Edit Mode):**
   * Перейдите в **Object Mode** (Объектный режим).
   * Кликните левой кнопкой мыши по самому **мешу мага** (его телу/мантии `node_0`), а не по скелету.
   * Нажмите клавишу **Tab** (или выберите **Edit Mode** в выпадающем меню в левом верхнем углу). Меш покроется сеткой из мелких точек.
2. **Выделите область головы и бороды:**
   * Снимите все выделения, кликнув в пустое место сцены (или нажмите **Alt + A**).
   * Нажмите клавишу **Numpad 1** для вида строго спереди.
   * Нажмите клавишу **B** (рамочное выделение) или зажмите клавишу **C** (выделение круглой кистью) и аккуратно обведите/закрасьте **всю голову мага целиком, включая волосы, лицо и всю бороду**. Все выделенные вершины станут ярко-оранжевыми.
3. **Назначьте 100% влияние на кость Head:**
   * На правой панели Blender найдите вкладку свойств со значком **зеленого треугольника** (это вкладка *Object Data Properties*).
   * Найдите вверху этой вкладки большой список под названием **`Vertex Groups`** (Группы вершин). Каждая группа там соответствует своей кости.
   * Прокрутите список или воспользуйтесь поиском и найдите группу с именем **`Head`**. Кликните по ней.
   * Под списком найдите ползунок **`Weight`** и убедитесь, что он выставлен ровно на **`1.000`**.
   * Нажмите кнопку **`Assign`** (Назначить) прямо под ползунком. Теперь вся выделенная голова и борода на 100% привязаны к кости головы.
4. **Уберите влияние мешающих костей (Главный шаг для исправления кривизны):**
   * **Не снимая выделения с оранжевой головы/бороды**, найдите в этом же списке Vertex Groups группу **`Neck`** (шея). Выберите её и нажмите кнопку **`Remove`** (Удалить) под списком.
   * Выберите группу **`Chest`** (грудь) и нажмите кнопку **`Remove`**.
   * Выберите группу **`Shoulder.L`** (левое плечо) и нажмите кнопку **`Remove`**.
   * Выберите группу **`Shoulder.R`** (правое плечо) и нажмите кнопку **`Remove`**.
   * Выберите группу **`Spine`** (позвоночник) и нажмите кнопку **`Remove`**.
5. **Проверка результата:**
   * Нажмите клавишу **Tab**, чтобы вернуться в **Object Mode**.
   * Теперь выделите скелет, перейдите в **Pose Mode** и покрутите кость `Head` или `Spine` (клавиша **R**). Лицо и борода больше никогда не будут кривиться и растягиваться — они будут поворачиваться идеально плавно и жестко следовать за головой!

---

### 🎨 Способ 2: Использование Weight Paint (Рисование весов кистью) — Наглядный метод

Если вы хотите визуально контролировать, какая кость куда тянет:

1. **Войдите в режим Weight Paint:**
   * В **Object Mode** выделите сначала **Скелет**, затем зажмите **Shift** и выделите **Меш** мага.
   * В верхнем левом углу выберите режим **`Weight Paint`** (или нажмите **Ctrl + Tab** -> **Weight Paint**).
2. **Поиск паразитных весов:**
   * Зажмите клавишу **Ctrl** и кликните левой кнопкой мыши по кости **`Head`**. Вы увидите, что голова и борода окрашены в красный цвет (100% влияние). Это отлично.
   * Теперь зажмите **Ctrl** и кликните по кости **`Chest`** или **`Shoulder.L` / `Shoulder.R`**. 
   * Посмотрите на бороду и лицо: если вы видите там участки желтого, зеленого, голубого или бирюзового цветов — значит, эти кости ошибочно влияют на лицо мага и заставляют его искривляться.
3. **Стирание лишнего влияния:**
   * Выберите на левой панели инструмент **`Draw`** (обычная кисть).
   * На верхней панели настроек кисти измените параметр **`Weight`** (вес) на **`0.000`** (полный ноль).
   * Аккуратно кистью закрасьте все цветные зоны на лице мага и на бороде для костей `Chest`, `Shoulder.L`, `Shoulder.R` и `Neck`. Цвет меша в этих местах должен стать чисто синим (это означает 0% влияния).
4. **Проверка:** Вернитесь в Pose Mode и проверьте анимацию. Все искажения полностью исчезнут!

---

## ⚔️ ЧАСТЬ 2. ПОШАГОВОЕ РУКОВОДСТВО ПО СОЗДАНИЮ И НАСТРОЙКЕ ВОИНА (WARRIOR)

Теперь приступаем к созданию Воина! Ниже представлена подробная пошаговая инструкция — от генерации картинки до Unity.

### Шаг 1. Генерация идеальной Т-позы в Leonardo.ai
Используйте этот точный ортопедический промпт, чтобы минимизировать появление оружия, плащей и широких элементов, мешающих риггингу воина:

**Позитивный промпт (Prompt):**
```text
An absolute front-view straight isolated full-body head-to-toe shot of a heroic medieval male human warrior knight in silver steel plate armor. Symmetrical flat orthopedic front-facing T-pose, both arms are perfectly raised and outstretched horizontally straight to the sides at a strict 90-degree angle relative to the spine, parallel to the ground, forming a perfect straight line across the shoulders (rigid T-shape). Empty hands, open flat palms facing the floor, fingers separated and visible. Wide open negative space under the armpits, ensuring the arms are completely detached from the torso, tight-fitting greaves, strictly no cape, no shields, no weapons, no hanging cloth. Both legs and iron sabatons boots are standing perfectly straight and parallel flat on the ground, pointing forward. Zero body rotation, perfectly flat mirror-like symmetry, looking directly into the camera, valiant brave face. The entire character, from the top of the head to the bottom of the boots, is fully contained and centered inside the frame, with generous empty space at the top, bottom, and sides so no part of the body is cut off or cropped. Modern high-quality 3D video game character model, digital sculpt, clean clay render, cinematic studio lighting. Isolated on a solid flat pure white background (#ffffff), strictly no floor shadows, ready for rigging, solo view, single character only.
```

**Негативный промпт (Negative Prompt):**
```text
holding weapon, sword, shield, cape, cloak, helmet covering face, low-poly, 2d, illustration, drawing, multiple angles, asymmetrical pose, background noise, floor shadows, platform, pedestal, cropped head, cropped feet.
```

> **🔥 Важное действие в Leonardo:** После генерации наведите курсор на лучшую картинку, нажмите кнопку **«Remove Background»** (Вырезать фон) для удаления белого фона и скачайте чистый PNG-файл вашего воина.

---

### Шаг 2. 3D-реконструкция в Tencent Hunyuan 3D
1. Откройте сайт [3d.hunyuan.tencent.com](https://3d.hunyuan.tencent.com/) и выберите режим **«Вэньшэнь 3D»** (Single image) на левой панели.
2. Загрузите полученный PNG-файл воина без фона.
3. Выберите модель **«Generation v3.1»**.
4. **Установите количество полигонов на `50k`** (50 тысяч) или максимум `500k` для оптимизации производительности в Unity (модели `1.5M` перегрузят VRAM).
5. Нажмите кнопку **«Generate Now»** и скачайте полученный `.fbx` файл воина на ваш компьютер.

---

### Шаг 3. Импорт воина в Blender и построение скелета
1. Откройте Blender, выделите стартовый куб и удалите его, нажав клавишу **X -> Delete**.
2. Импортируйте вашего воина: перейдите в **File -> Import -> FBX** (или OBJ/GLB, в зависимости от скачанного файла) и выберите модель воина.
3. Кликните левой кнопкой мыши по мешу воина в 3D-окне (он подсветится оранжевым контуром).
4. Перейдите во вкладку **Scripting** на самой верхней панели Blender.
5. Нажмите кнопку **New**, скопируйте и вставьте всё содержимое файла `blender_rigging_helper.py` из нашего проекта.
6. Нажмите треугольник **Run Script** (Play ▶️) в правом верхнем углу текстовой панели. Скелет `FateHumanoid_Armature` мгновенно построится и автоматически привяжется к воину, создав тестовые анимации!

---

### Шаг 4. Точечная подгонка суставов тяжелой брони Воина
Так как у воина латные плечи и тяжелая броня, автоматический скелет может оказаться чуть уже, чем реальные плечевые суставы доспехов. Исправим это вручную:
1. Выделите скелет **`FateHumanoid_Armature`** и перейдите в **Edit Mode** (выберите в выпадающем списке вверху слева или нажмите комбинацию **Ctrl + Tab -> Edit Mode**).
2. Включите **X-Axis Mirror** (значок бабочки в правом верхнем углу экрана), чтобы кости редактировались абсолютно симметрично.
3. Нажмите клавишу **Numpad 1** (вид строго спереди).
4. **Плечи воина:** Выделите круглый сустав на стыке плеча доспеха и руки (`UpperArm.L`). Нажмите клавишу **G** и сдвиньте его ровно в середину плечевого полусферического щитка (наплечника) воина.
5. **Локти воина:** Выделите сустав локтя (стык между `UpperArm.L` и `LowerArm.L`), нажмите клавишу **G** и сдвиньте его в центр локтевого сустава стальных лат.
6. **Ноги воина:** Латные поножи часто объемнее обычных ног. Выделите сустав колена (между `Thigh.L` и `Shin.L`), нажмите клавишу **G** и сдвиньте его в середину коленного щитка доспехов.
7. Нажмите клавишу **Tab**, чтобы вернуться в **Object Mode**.

---

### Шаг 5. Тонкая настройка весов (Weight Paint) стальных пластин
У воинов в латах есть специфическая проблема: при поднятии ног или сгибании рук стальные пластины доспехов (например, наплечники или нагрудник) могут неестественно растягиваться, как резина. Чтобы броня выглядела жесткой:
1. Выделите **Скелет**, зажмите клавишу **Shift**, выделите **Меш воина**.
2. В верхнем левом меню перейдите в режим **`Weight Paint`**.
3. Выберите инструмент **`Draw`** (кисть на панели слева).
4. На верхней панели настроек кисти установите значение **`Weight`** (вес) в **`0.000`** (полный ноль).
5. Зажмите клавишу **Ctrl** и кликните левой кнопкой мыши по кости бедра **`Thigh.L`**:
   * Закрасьте синим цветом (0% влияния) нижний край латной кирасы (нагрудника) воина, чтобы при движении ног стальной нагрудник не растягивался вниз, как резина.
6. Зажмите клавишу **Ctrl** и кликните по кости плеча **`UpperArm.L`**:
   * Закрасьте синим цветом бока тела (подмышки) и ребра кирасы, чтобы они не тянулись за руками при махах мечом.
7. Нажмите клавишу **Tab**, чтобы вернуться в **Object Mode**.

---

### 🛡️ РЕШЕНИЕ ПРОБЛЕМЫ: «Искажается или смазывается шлем Воина при поворотах головы и шеи»

**Почему это происходит:**
При автоматической привязке весов (Auto-Weights) Blender часто приписывает влияние костей шеи (`Neck`), плеч (`Shoulder.L` / `Shoulder.R`) или груди (`Chest`) к нижней части массивного металлического шлема воина. Из-за этого при поворотах головы шлем кривится, растягивается как пластилин или "вминается" в плечи.

**Решение:** Весь шлем должен быть жестко привязан к кости головы (`Head`) на 100%, и иметь 0% влияния от костей шеи, груди и плеч.

**Пошаговый алгоритм исправления (через Vertex Groups):**
1. В **Object Mode** выделите меш воина (кликнув по нему) и перейдите в **Edit Mode** (нажмите клавишу **Tab**).
2. Снимите все выделения, кликнув в пустое место сцены (или нажмите **Alt + A**).
3. Нажмите клавишу **Numpad 1** (вид строго спереди).
4. Нажмите клавишу **B** (рамочное выделение) или зажмите клавишу **C** (круг выделения) и аккуратно выделите **весь шлем воина целиком** (голову воина сверху до самых плеч). Все выделенные вершины шлема станут оранжевыми.
5. Нажмите вкладку со значком **зеленого треугольника** в правой колонке свойств (**Object Data Properties**).
6. В списке **`Vertex Groups`** (Группы вершин) найдите группу **`Head`**, установите ползунок **`Weight`** под списком на **`1.000`** и нажмите кнопку **`Assign`** (Назначить).
7. Теперь, **не снимая выделения со шлема**, выберите по очереди в этом же списке группы **`Neck`** (шея), **`Chest`** (грудь), **`Shoulder.L`** (левое плечо) и **`Shoulder.R`** (правое плечо) и нажимайте кнопку **`Remove`** (Удалить) под списком для каждой из них!
8. Нажмите клавишу **Tab**, чтобы вернуться в **Object Mode**. Теперь тяжелый шлем воина будет вращаться монолитно, красиво и жестко вместе с головой!

---

### Шаг 6. Проверка работоспособности и Экспорт в Unity
1. **Запуск теста движения:** Наведите курсор на 3D-вид и нажмите клавишу **Пробел (Space)**. Воин начнет плавно двигаться. Посмотрите, плавно ли движутся суставы, шлем и не растягиваются ли латы. Если всё выглядит жестко и гармонично — модель готова к экспорту!
2. **Экспорт модели:**
   * В объектном режиме выделите на сцене меш воина и его скелет (`FateHumanoid_Armature`).
   * Выберите в верхнем меню **File -> Export -> FBX (.fbx)**.
   * Настройте экспорт на правой панели в точности, как на вашем скриншоте:
     * **Selected Objects:** Обязательно поставьте галочку (чтобы экспортировать только выделенного воина и его скелет).
     * **Object Types:** Выделите только **`Armature`** and **`Mesh`** (зажмите Shift при выборе).
     * **Transform -> Apply Scalings:** Установите **`FBX All`** (самый надежный режим, устраняющий баг 100-кратного масштабирования в Unity).
     * **Armature -> Add Leaf Bones:** Снимите галочку (чтобы избежать создания лишних пустых костей-концевиков на пальцах и суставах).
     * **Animation -> Bake Animation:** Снимите галочку (для импорта чистой Т-позы со скелетом без тестовых кадров движения).
   * Дайте файлу имя `Warrior_Hero.fbx` и сохраните его прямо в папку вашего проекта Unity: `Assets/Models/Characters/`!

---

## 🏹 ЧАСТЬ 3. ПОШАГОВОЕ РУКОВОДСТВО ПО СОЗДАНИЮ И НАСТРОЙКЕ СТРЕЛКА (ARCHER)

Теперь приступаем к созданию и настройке нашего эльфийского Стрелка! Ниже представлена подробная пошаговая инструкция — от генерации картинки до Unity.

### Шаг 1. Генерация идеальной Т-позы в Leonardo.ai
Используйте этот точный ортопедический промпт, чтобы минимизировать появление оружия, лука, колчана, капюшона и широких элементов, мешающих риггингу стрелка:

**Позитивный промпт (Prompt):**
```text
An absolute front-view straight isolated full-body head-to-toe shot of a handsome male elven hunter ranger in tight leather armor with green trim. Symmetrical flat orthopedic front-facing T-pose, both arms are perfectly raised and outstretched horizontally straight to the sides at a strict 90-degree angle relative to the spine, parallel to the ground, forming a perfect straight line across the shoulders (rigid T-shape). Empty hands, open flat palms facing the floor, fingers separated and visible. Wide open negative space under the armpits, ensuring the arms are completely detached from the torso, tight-fitting sleeves, no cape, no hanging cloth. Both legs and flat leather boots are standing perfectly straight and parallel flat on the ground, pointing forward. Zero body rotation, perfectly flat mirror-like symmetry, looking directly into the camera, handsome clear face. The entire character, from the top of the head to the bottom of the boots, is fully contained and centered inside the frame, with generous empty space at the top, bottom, and sides so no part of the body is cut off or cropped. Modern high-quality 3D video game character model, digital sculpt, clean clay render, cinematic studio lighting. Isolated on a solid flat pure white background (#ffffff), strictly no floor shadows, ready for rigging, solo view, single character only.
```

**Негативный промпт (Negative Prompt):**
```text
holding weapon, bow, arrows, quiver, cape, cloak, hood covering face, low-poly, 2d, illustration, drawing, multiple angles, asymmetrical pose, background noise, floor shadows, platform, pedestal, cropped head, cropped feet.
```

> **🔥 Важное действие в Leonardo:** После генерации наведите курсор на лучшую картинку, нажмите кнопку **«Remove Background»** (Вырезать фон) для удаления белого фона и скачайте чистый PNG-файл вашего стрелка.

---

### Шаг 2. 3D-реконструкция в Tencent Hunyuan 3D
1. Откройте сайт [3d.hunyuan.tencent.com](https://3d.hunyuan.tencent.com/) и выберите режим **«Вэньшэнь 3D»** (Single image) на левой панели.
2. Загрузите полученный PNG-файл стрелка без фона.
3. Выберите модель **«Generation v3.1»**.
4. **Установите количество полигонов на `50k`** (50 тысяч) или максимум `500k` для оптимизации производительности в Unity (модели `1.5M` перегрузят VRAM).
5. Нажмите кнопку **«Generate Now»** и скачайте полученный `.fbx` файл стрелка на ваш компьютер.

---

### Шаг 3. Импорт стрелка в Blender и построение скелета
1. Откройте Blender, выделите стартовый куб и удалите его, нажав клавишу **X -> Delete**.
2. Импортируйте вашего стрелка: перейдите в **File -> Import -> FBX** (или OBJ/GLB, в зависимости от скачанного файла) и выберите модель стрелка.
3. Кликните левой кнопкой мыши по мешу стрелка в 3D-окне (он подсветится оранжевым контуром).
4. Перейдите во вкладку **Scripting** на самой верхней панели Blender.
5. Нажмите кнопку **New**, скопируйте и вставьте всё содержимое файла `blender_rigging_helper.py` из нашего проекта.
6. Нажмите треугольник **Run Script** (Play ▶️) в правом верхнем углу текстовой панели. Скелет `FateHumanoid_Armature` мгновенно построится и автоматически привяжется к стрелку, создав тестовые анимации!

---

### Шаг 4. Точечная подгонка суставов легкого доспеха Стрелка
Так как эльф-стрелок обладает стройным гибким телосложением, кости плеч и локтей автоскелета могут быть чуть шире или длиннее, чем тонкие руки меша. Откалибруем их:
1. Выделите скелет **`FateHumanoid_Armature`** и перейдите в **Edit Mode** (выберите в выпадающем списке вверху слева или нажмите комбинацию **Ctrl + Tab -> Edit Mode**).
2. Включите **X-Axis Mirror** (значок бабочки в правом верхнем углу экрана), чтобы кости редактировались абсолютно симметрично.
3. Нажмите клавишу **Numpad 1** (вид строго спереди).
4. **Тонкие плечи:** Выделите сустав плеча (`UpperArm.L`). Нажмите клавишу **G** и сдвиньте его вовнутрь, установив точно на стык тонкого плечевого сустава руки и туловища.
5. **Локти и запястья:** Выделите сустав локтя, сдвиньте его клавишей **G** в самый центр локтевого сгиба. Выделите сустав кисти (`Hand.L`) и переместите его точно на границу начала ладони Стрелка.
6. **Ноги и стопы:** Направьте коленные суставы (стык `Thigh.L` к `Shin.L`) ровно в центр коленей. Сустав лодыжки поставьте на уровень щиколотки, а носок `Foot.L` выровняйте по направлению его кожаного сапога.
7. Нажмите клавишу **Tab**, чтобы вернуться в **Object Mode**.

---

### Шаг 5. Настройка весов (Weight Paint) и устранение «резиновых» подмышек и капюшона/колчана
У Стрелков часто присутствуют элементы одежды, свисающие ремешки или легкие наплечные накидки. Исправим их привязку:
1. Выделите **Скелет**, зажмите клавишу **Shift**, выделите **Меш стрелка**.
2. В верхнем левом меню перейдите в режим **`Weight Paint`**.
3. **Ликвидация склеивания подмышек:** Выберите инструмент **Draw** на панели слева, установите **Weight** на верхней панели на **0.000**.
   * Нажмите **Ctrl + клик** по кости плеча **`UpperArm.L`**.
   * Аккуратно закрасьте синим цветом (0% влияния) боковые рёбра под мышкой Стрелка, чтобы при поднятии рук бока тела не тянулись вверх.
4. **Фиксация длинных ушей и капюшона (Vertex Groups):**
   * Чтобы длинные эльфийские уши или легкий капюшон не деформировались при поворотах, вернитесь в **Object Mode** (нажав **Tab**).
   * Выберите меш стрелка и перейдите в **Edit Mode** (клавиша **Tab**).
   * Снимите все выделения (**Alt + A**). С помощью рамочного выделения (**B**) выделите **всю голову Стрелка вместе с ушами и капюшоном** (сверху до шеи).
   * На правой панели Blender выберите вкладку со значком **зеленого треугольника** (**Object Data Properties**).
   * В списке **`Vertex Groups`** найдите группу **`Head`**, установите ползунок **Weight** на **1.000** и нажмите **`Assign`**.
   * Не снимая выделения со шлема/головы, выберите по очереди в этом же списке группы **`Neck`** (шея), **`Chest`** (грудь), **`Shoulder.L`** и **`Shoulder.R`** и нажимайте кнопку **`Remove`** (Удалить) под списком для каждой из них!
   * Вернитесь в **Object Mode** (нажав **Tab**). Теперь эльфийские уши и капюшон будут вращаться монолитно, красиво и жестко вместе с головой!

---

### Шаг 6. Проверка работоспособности и Экспорт в Unity
1. **Запуск теста движения:** Наведите курсор на 3D-вид и нажмите клавишу **Пробел (Space)**. Стрелок начнет плавно двигаться. Посмотрите, красиво ли движутся суставы и не растягивается ли одежда. Если всё выглядит идеально — модель готова к экспорту!
2. **Экспорт модели:**
   * В объектном режиме выделите на сцене меш стрелка и его скелет (`FateHumanoid_Armature`).
   * Выберите в верхнем меню **File -> Export -> FBX (.fbx)**.
   * Настройте экспорт на правой панели в точности, как на вашем скриншоте:
     * **Selected Objects:** Обязательно поставьте галочку (чтобы экспортировать только выделенного стрелка и его скелет).
     * **Object Types:** Выделите только **`Armature`** и **`Mesh`** (зажмите Shift при выборе).
     * **Transform -> Apply Scalings:** Установите **`FBX All`** (устраняет баг 100-кратного масштабирования в Unity).
     * **Armature -> Add Leaf Bones:** Снимите галочку (избегаем пустых лишних костей-концевиков).
     * **Animation -> Bake Animation:** Снимите галочку (для чистой Т-позы без тестовых кадров).
   * Дайте файлу имя `Archer_Hero.fbx` и сохраните его прямо в папку вашего проекта Unity: `Assets/Models/Characters/`!

---

## 🎨 РЕШЕНИЕ ПРОБЛЕМЫ: «Материалы Воина или Стрелка в Blender выглядят смазанными, слишком темными, металлическими или не соответствуют»

Если при импорте моделей Воина и Стрелка в Blender вы заметили, что они выглядят слишком черными, влажными, блестят как пластмасса, а текстура кажется смазанной или "вообще не соответствует" деталям, **не пугайтесь!** Это стандартная проблема импорта PBR-текстур ИИ (Tencent Hunyuan 3D) в Blender.

Ниже приведены **4 точечные причины** и пошаговый алгоритм их мгновенного исправления прямо в Blender.

---

### 🚨 Причина 1. Цветовое пространство (Color Space) вспомогательных карт (Главная причина "черноты" и смазанности)

По умолчанию при импорте PBR-текстур Blender считывает карты Металличности (Metallic), Шероховатости (Roughness) и Нормалей (Normal) в режиме цвета **`sRGB`**. Это грубая ошибка! В этих картах записан не цвет, а математические данные (векторы и коэффициенты отражения).

**Как исправить:**
1. Переключитесь на вкладку **Shading** (Затенение) на самой верхней панели Blender (рядом со Scripting) или откройте окно **Shader Editor**.
2. Выделите меш вашего персонажа (например, Стрелка). На панели шейдеров вы увидите ноды текстур, подключенные к главному блоку **Principled BSDF** (то, что видно у вас на скриншоте в свойствах материала).
3. Найдите блок ноды текстуры, который подключен к входу **`Metallic`** (Металличность):
   * Внутри этой ноды найдите выпадающий список **`Color Space`** (Цветовое пространство). Сейчас там стоит **`sRGB`**.
   * Измените его строго на **`Non-Color`** (Нецветные данные)!
4. Найдите блок ноды текстуры, подключенный к входу **`Roughness`** (Шероховатость):
   * Измените его **`Color Space`** со **`sRGB`** на **`Non-Color`**!
5. Найдите блок ноды текстуры, подключенный к ноде **`Normal Map`** (карта нормалей):
   * Измените его **`Color Space`** со **`sRGB`** на **`Non-Color`**!

---

### 🚨 КРИТИЧЕСКАЯ ОШИБКА ИМПОРТА: «Карта Металличности (Metallic) не подключена или перепутана» (ВИДНО НА ВАШЕМ СКРИНШОТЕ!)

Внимательно посмотрите на ваш скриншот в окне **Shader Editor** (режим **Shading**):
1. У вас нода **`texture_pbr_20250901_roughness.png`** (карта шероховатости) подключена **сразу к двум входам** блока **Principled BSDF** — и в **`Roughness`**, и в **`Metallic`**!
2. В это же время правильная нода **`texture_pbr_20250901_metallic.png`** (карта металличности) просто лежит внизу и **вообще никуда не подключена (её выход Color пуст)**!

Из-за этого у вас броня и кожа светятся неестественным пластиковым блеском, а настоящая металлическая карта воина игнорируется.

---

### 🔥 КРИТИЧЕСКАЯ ОШИБКА СЛЕДУЮЩИХ ШАГОВ (ТО, ЧТО ВИДНО НА ВАШЕМ НОВОМ ТРЕТЬЕМ СКРИНШОТЕ!):
«Вход цвета Base Color ошибочно подключен к ноде Metallic, а сама цветная текстура отключена!»

На вашем третьем скриншоте (где виден сам воин и ноды под ним) допущена критическая ошибка подключения:
* **Провод от входа `Base Color`** (желтый кружок в блоке *Principled BSDF*) теперь тянется прямо к выходу **`Color` ноды `texture_pbr_20250901_metallic.png`**!
* **Что это делает:** Вы заставили Blender использовать черно-белую металлическую карту в качестве основного цвета воина. Именно поэтому он выглядит полностью черно-серым, «железным» и грязным, как монолитный кусок чугуна, а настоящих цветов его одежды (золотой, синей, кожаной) вообще не видно!
* **Где цветная текстура:** Ваша основная цветная текстура **`texture_pbr_20250901.png`** (без приписок `_metallic` или `_roughness`) полностью отключена от зеленого блока Principled BSDF, либо её провод обрезан!

---

### 🛠️ ПОШАГОВЫЙ ПЛАН ПОЛНОГО ИСПРАВЛЕНИЯ ТЕКСТУРЫ ВОИНА (И СТРЕЛКА) В BLENDER:

Выполните следующие **3 простых действия** прямо сейчас в окне **Shader Editor** (вкладка **Shading**), чтобы вернуть воину его настоящий цвет и блеск:

#### Шаг 1. Отсоединяем неверные провода от Base Color и Metallic
1. Найдите зеленый блок **`Principled BSDF`**.
2. Посмотрите на его вход **`Base Color`** (желтый кружок). Из него идет провод к выходу ноды `texture_pbr_20250901_metallic.png`.
3. **Отсоедините его:** Нажмите левой кнопкой мыши по желтому кружку **`Base Color`** и оттащите провод в пустую область экрана, чтобы разорвать эту связь. (Или зажмите клавишу **Ctrl** и проведите правой кнопкой мыши, как ножом, поперек этой линии, чтобы разрезать её).
4. Теперь проверьте вход **`Metallic`** (серый кружок) на зеленом блоке `Principled BSDF`. К нему должен идти ОДИН провод от выхода **`Color`** ноды **`texture_pbr_20250901_metallic.png`**. Если туда ведет провод от ноды `roughness` — отключите его!

#### Шаг 2. Подключаем настоящую цветную текстуру (Base Color)
1. Найдите вашу главную текстуру **`texture_pbr_20250901.png`** (это исходная цветная картинка воина, в её названии нет приписок `_metallic`, `_roughness` или `_normal`).
   * *Если этой ноды нет в вашем окне Shader Editor:* Просто перетащите исходный файл картинки воина (например, `texture_pbr_20250901.png` или `base_color`) из левой папки проводника в Blender прямо в область нод. Она мгновенно появится как новая нода!
2. В созданной ноде цв�#### 🔗 КАК ПРИВЯЗАТЬ МЕШ ПЕРСОНАЖА К ВАШЕМУ НОВОМУ ОТРЕДАКТИРОВАННОМУ СКЕЛЕТУ? (Skinning / Скиннинг)

Если вы настроили кости, но при переходе в **Pose Mode** и вращении костей тело (меш) персонажа не двигается вслед за ними, значит, нарушена или отсутствует привязка вершин меша к костям скелета.

Чтобы полностью избежать ошибок с выделением (когда в меню нет нужного пункта) или ошибок геометрии ("Bone Heat Weighting: failed to find solution"), вы можете использовать **специально подготовленный автоматический Python-скрипт**, который сделает всё за вас в один клик!

---

##### 🤖 АВТОМАТИЧЕСКИЙ СПОСОБ: Запуск скрипта `blender_auto_binder.py` (Рекомендуется!)

Мы создали для вас скрипт `/blender_auto_binder.py`, который автоматически:
1. Очистит старые родительские связи меша (`node_0`).
2. Удалит мешающие или конфликтующие старые модификаторы Armature.
3. Объединит близкие вершины (`Merge by Distance`), предотвращая частую ошибку сбоя тепловых весов Blender.
4. Выделит Меш и Скелет в идеальном математическом порядке (Скелет станет активным).
5. Применит привязку **With Automatic Weights** (С автоматическими весами).

**Как его использовать в Blender:**
1. В верхнем меню Blender перейдите на вкладку **Scripting** (в самом верху экрана справа, рядом с вкладками Layout, Modeling, Rendering).
2. Нажмите кнопку **New** (Новый) вверху текстового редактора, чтобы создать новый файл скрипта.
3. Откройте в вашем проекте файл `/blender_auto_binder.py`, скопируйте весь его код и вставьте в текстовое окно Blender.
4. Убедитесь, что имя меша в списке объектов справа — `node_0`, а имя скелета — `FateHumanoid_Armature` (если они другие, скрипт автоматически попытается их найти, но лучше назвать их так).
5. Нажмите на кнопку **Run Script** (кнопка с треугольником воспроизведения ▶️ в правом верхнем углу текстового редактора Blender).
6. **Готово!** Посмотрите в консоль внизу — вы увидите сообщение `SUCCESS!`. Теперь перейдите во вкладку **Layout**, переключитесь в **Pose Mode** и покрутите кости — тело персонажа будет двигаться за ними идеально!

---

##### ✍️ РУЧНОЙ СПОСОБ: Если вы хотите сделать привязку вручную

Если вы хотите выполнить привязку вручную без использования скрипта, строго следуйте этому порядку:

##### 1️⃣ Шаг 1. Полная очистка старых родительских связей
Перед созданием новой привязки нужно удалить остатки старых связей и модификаторов:
1. Перейдите в **Object Mode** (Объектный режим) через левое верхнее меню.
2. Кликните левой кнопкой мыши по **Мешу (модели)** персонажа (например, `node_0`).
3. Нажмите комбинацию клавиш **`Alt + P`** и в выпадающем меню выберите пункт **`Clear and Keep Transformation`**.
4. Перейдите на вкладку **Modifier Properties** на панели справа (значок синего гаечного ключа 🔧).
5. Если в списке есть модификатор **Armature**, нажмите на крестик **`X`** в его правом верхнем углу, чтобы удалить его.

##### 2️⃣ Шаг 2. Правильное выделение для появления пункта «With Automatic Weights»
На вашем скриншоте в меню отображаются только простые варианты `Object`, `Vertex` из-за неверного порядка выделения.
1. Кликните в любое пустое место на экране, чтобы полностью снять выделение.
2. Кликните **левой кнопкой мыши** сначала по **Мешу** персонажа (`node_0`). Вокруг него появится тёмно-оранжевая обводка.
3. Зажмите и удерживайте клавишу **`Shift`**.
4. Кликните по **Скелету** (`FateHumanoid_Armature`). Скелет должен стать **светло-жёлтым** (активным), а меш остаться тёмно-оранжевым.
5. Теперь нажмите **`Ctrl + P`** — в появившемся меню «Set Parent To» гарантированно появится пункт **`With Automatic Weights`**!

##### ⚠️ Что делать, если появляется ошибка "Bone Heat Weighting: failed to find solution"?
Эта ошибка возникает из-за дублирующихся вершин импортированного меша:
1. Выделите только **Меш** (`node_0`) и перейдите в **Edit Mode** (клавиша **Tab**).
2. Нажмите **`A`**, чтобы выделить абсолютно все вершины модели.
3. Нажмите клавишу **`M`** (меню Merge / Объединить) и выберите **`By Distance`** (По расстоянию). Blender удалит дубликаты вершин.
4. Вернитесь в **Object Mode** (клавиша **Tab**) и повторите привязку: выделите Меш -> Shift + клик по Скелету -> **Ctrl + P** -> **With Automatic Weights**.

##### 3️⃣ Шаг 3. Исправление «резиновых» подмышек (Weight Paint):
   * Сначала выберите **Скелет** в объектном режиме (левым кликом), затем, удерживая клавишу **Shift**, кликните по **Мешу** персонажа.
   * Перейдите в режим **Weight Paint** (выберите его в выпадающем списке в левом верхнем углу окна Blender вместо *Object Mode*).
   * Зажмите клавишу **Ctrl** и кликните по плечевой кости (`UpperArm.L` или `UpperArm.R`). Вы увидите веса влияния кости: красный цвет означает 100% влияние, синий — 0% влияния.
   * Выберите кисть **Draw** на панели слева, установите параметр **Weight** (вес) на **`0.0`** и аккуратно сотрите влияние руки с боков туловища (рёбер и подмышек), чтобы при поднятии руки бока и рёбра персонажа не тянулись вслед за ней.ормате gLTF / GLB!
Это самый надежный способ, который решает проблему раз и навсегда без ручных правок.
1. В интерфейсе Tencent Hunyuan 3D найдите внизу синюю кнопку **«下载» (Скачать)**.
2. Кликните по ней и выберите формат **gLTF** (или **GLB**).
3. Скачанный файл импортируйте в Blender через меню: **`File` -> `Import` -> `gTF 2.0 (.gltf/.glb)`**.
4. **Результат:** Blender идеально прочитает координаты, сам перевернет текстуры как надо, и модель сразу откроется в идеальном виде, сочными цветами и без единой "кракозябры"!

---

#### 🏆 РЕШЕНИЕ №2 (Для исправления уже скачанного FBX): Скрипт с авто-переворотом UV

Если вы хотите работать именно с **FBX**-форматом, мы обновили наш Python-скрипт. Теперь он не только автоматически подключает все текстуры, но и **мгновенно исправляет «кракозябры»**, добавляя ноду отображения (Mapping) с переворотом текстуры по вертикали (Scale Y = -1, Location Y = 1)!

**Как запустить этот скрипт в Blender:**
1. Выделите в 3D окне модель вашего персонажа (например, **Воина** или **Стрелка**).
2. На самой верхней панели Blender переключитесь на вкладку **`Scripting`** (Скриптинг).
3. В центре экрана нажмите кнопку **`+ New`** (+ Создать), чтобы открыть текстовое поле.
4. Вставьте туда следующий обновленный код целиком:

```python
import bpy

def fix_pbr_material_and_vflip():
    # Получаем выделенный объект
    obj = bpy.context.active_object
    if not obj or obj.type != 'MESH':
        print("Ошибка: Пожалуйста, выделите 3D-модель персонажа в Blender!")
        return

    # Получаем его активный материал
    mat = obj.active_material
    if not mat or not mat.use_nodes:
        print(f"Ошибка: У объекта {obj.name} нет материала или не включены Nodes!")
        return

    nodes = mat.node_tree.nodes
    links = mat.node_tree.links

    # Ищем главный блок Principled BSDF
    bsdf = None
    for node in nodes:
        if node.type == 'BSDF_PRINCIPLED':
            bsdf = node
            break

    if not bsdf:
        print("Ошибка: Не найден блок Principled BSDF!")
        return

    # Создаем ноды координат для исправления перевернутых координат ("кракозябр")
    # 1. Texture Coordinate (Координаты текстур)
    tex_coord = None
    for node in nodes:
        if node.type == 'TEX_COORD':
            tex_coord = node
            break
    if not tex_coord:
        tex_coord = nodes.new(type='ShaderNodeTexCoord')
        tex_coord.location = (bsdf.location.x - 1000, bsdf.location.y)

    # 2. Mapping (Отображение) с переворотом по оси Y (V-Flip)
    mapping = None
    for node in nodes:
        if node.type == 'MAPPING':
            mapping = node
            break
    if not mapping:
        mapping = nodes.new(type='ShaderNodeMapping')
        mapping.location = (bsdf.location.x - 750, bsdf.location.y)
    
    # Ключевая настройка против "кракозябр": переворачиваем текстуру по вертикали
    mapping.inputs['Scale'].default_value[1] = -1.0  # Scale Y = -1
    mapping.inputs['Location'].default_value[1] = 1.0  # Location Y = 1
    
    # Соединяем координаты с маппингом
    links.new(tex_coord.outputs['UV'], mapping.inputs['Vector'])

    # Отключаем все неверные/старые связи со входами цвета, металла, шероховатости и нормали
    inputs_to_clear = ['Base Color', 'Metallic', 'Roughness', 'Normal']
    for input_name in inputs_to_clear:
        if input_name in bsdf.inputs:
            for link in bsdf.inputs[input_name].links:
                links.remove(link)

    # Ищем все ноды текстур и распределяем их по названию файлов
    tex_nodes = [node for node in nodes if node.type == 'TEX_IMAGE']
    
    base_color_node = None
    metallic_node = None
    roughness_node = None
    normal_node = None

    for node in tex_nodes:
        if not node.image:
            continue
        name = node.image.name.lower()
        
        # Подключаем маппинг к вектору каждой текстуры, чтобы переворот применился ко всем картам
        links.new(mapping.outputs['Vector'], node.inputs['Vector'])
        
        if "metallic" in name:
            metallic_node = node
        elif "roughness" in name:
            roughness_node = node
        elif "normal" in name:
            normal_node = node
        else:
            # Картинка без приписок или с припиской basecolor/albedo
            base_color_node = node

    # Настраиваем цветовые пространства и подключаем к BSDF
    # 1. Цвет (Base Color) - строго sRGB
    if base_color_node:
        base_color_node.image.colorspace_settings.name = 'sRGB'
        links.new(base_color_node.outputs['Color'], bsdf.inputs['Base Color'])
        print(f"[OK] Подключен цвет: {base_color_node.image.name}")

    # 2. Металл (Metallic) - строго Non-Color
    if metallic_node:
        metallic_node.image.colorspace_settings.name = 'Non-Color'
        links.new(metallic_node.outputs['Color'], bsdf.inputs['Metallic'])
        print(f"[OK] Подключен металл (Non-Color): {metallic_node.image.name}")

    # 3. Шероховатость (Roughness) - строго Non-Color
    if roughness_node:
        roughness_node.image.colorspace_settings.name = 'Non-Color'
        links.new(roughness_node.outputs['Color'], bsdf.inputs['Roughness'])
        print(f"[OK] Подключена шероховатость (Non-Color): {roughness_node.image.name}")

    # 4. Карта нормалей (Normal) - строго Non-Color через конвертер Normal Map
    if normal_node:
        normal_node.image.colorspace_settings.name = 'Non-Color'
        
        # Находим существующую ноду Normal Map или создаем новую
        normal_map_node = None
        for node in nodes:
            if node.type == 'NORMAL_MAP':
                normal_map_node = node
                break
        
        if not normal_map_node:
            normal_map_node = nodes.new(type='ShaderNodeNormalMap')
            normal_map_node.location = (bsdf.location.x - 280, bsdf.location.y - 300)
        
        # Смягчаем силу нормалей, чтобы убрать шум и сделать броню гладкой
        normal_map_node.inputs['Strength'].default_value = 0.15
        
        # Переподключаем
        links.new(normal_node.outputs['Color'], normal_map_node.inputs['Color'])
        links.new(normal_map_node.outputs['Normal'], bsdf.inputs['Normal'])
        print(f"[OK] Подключена карта нормалей (Non-Color, Сила=0.15): {normal_node.image.name}")

    print("=== УСПЕХ: Все текстуры подключены, а кракозябры успешно исправлены! ===")

fix_pbr_material_and_vflip()
```

5. Нажмите кнопку **`Run Script`** (кнопка воспроизведения ▶️ в правом верхнем углу окна скриптинга).
6. Вернитесь во вкладку **`Layout`** или **`Shading`** — ваш персонаж мгновенно станет ярким, правильно раскрашенным и лишится грязного чугунного налета! 
7. *Выделите следующего персонажа (например, Стрелка) и просто нажмите ▶️ снова!*

---

### 🌟 ПОЧЕМУ ВОИН ВСЁ РАВНО КАЖЕТСЯ ТЁМНЫМ В BLENDER (И КАК ЭТО ПОДТВЕРДИТЬ)?

Вы успешно применили скрипт! Если вы посмотрите на ваш второй и третий скриншот, в правой панели **Material Properties** все текстуры встали на свои места абсолютно идеально:
* **`Base Color`** подключен к цвету (`texture_pbr_20250901.png`)
* **`Metallic`** подключен к металлу (`texture_pbr_20250901_metallic.png`)
* **`Roughness`** подключен к шероховатости (`texture_pbr_20250901_roughness.png`)

Но в 3D окне модель все еще кажется темной/серой. Это происходит потому, что по умолчанию Blender отражает в металлической броне темный лес. Окно "Preferences" (Настройки), которое вы открыли на скриншоте, **не нужно** — закройте его на крестик.

Вот **два простых способа** настроить отображение прямо сейчас, чтобы ваш воин засиял:

---

#### 🏆 Способ 1. Выбираем светлую Студию вместо темного Леса (Самый красивый способ)

Прямо в том меню, которое открыто у вас на скриншоте справа (меню со сферой леса):

1. **Кликните прямо по круглой картинке с лесом** (зеленая сфера по центру вашего открытого меню Viewport Shading).
2. У вас откроется сетка из нескольких встроенных сфер (HDRI-карт окружения).
3. **Выберите светлую студийную сферу** (например, белую, серую или золотистую студию со светильниками).
4. **Результат:** Вместо темных лесных деревьев броня воина начнет отражать яркие белые софиты и засияет благородной сталью, как настоящий рыцарь!

---

#### 🏆 Способ 2. Как увидеть чистый, яркий цвет без теней и отражений (Плоский вид)

Вы искали "Flat" lighting, но искали его в режиме **Material Preview** (3-й шарик). Он находится в режиме **Solid** (2-й шарик):

1. **Переключите режим отображения:** В самом верхнем правом углу 3D-окна нажмите на **второй шарик слева** (простой серый круг — режим `Solid`).
2. **Откройте его меню:** Нажмите на маленькую стрелочку `v` сразу справа от этого серого шарика.
3. **Настройте плоский вид:**
   * В разделе **Lighting** (Освещение) выберите кнопку **`Flat`** (Плоское).
   * В разделе **Color** (Цвет) выберите кнопку **`Texture`** (Текстура).
4. **Результат:** Все тени, блики леса и отражения полностью отключатся! Вы увидите сочные, чистые, оригинальные цвета текстуры вашего Воина, как на картинке нейросети! Это идеальный режим для проверки правильности наложения текстур.

---

---

### 🚨 Причина 2. Перепутанные текстуры («Вообще не соответствует»)

Так как Tencent Hunyuan генерирует файлы с очень похожими названиями (например, `texture_pbr_2025090...`), при импорте нескольких персонажей в одну сцену Blender или при копировании материалов легко запутаться и случайно наложить текстуру Воина на модель Стрелка. Из-за этого одежда и лицо натянутся криво и "смажутся".

**Как исправить:**
1. В окне **Shader Editor** или в свойствах материала справа (вкладка со значком красного шарика **Material Properties**) посмотрите на название файла в ноде **`Base Color`** (Основной цвет).
2. Сравните его с файлами в вашей папке, куда Tencent Hunyuan скачал текстуры для этого конкретного персонажа.
3. Если там указан файл от другого героя:
   * Нажмите на значок папки в ноде Base Color.
   * Выберите правильный файл текстуры (обычно он заканчивается на `..._basecolor.png` или `..._albedo.png`), который принадлежит именно этому персонажу (Воину или Стрелку).

---

### 🚨 Причина 3. Эффект "жеваной фольги" из-за силы карты нормалей (Normal Map Strength)

Карты нормалей, генерируемые нейросетями, часто бывают слишком контрастными или шумными. При значении силы (Strength) равном **`1.0`** (по умолчанию) они создают на доспехах Воина или коже Стрелка некрасивые резкие тени, из-за чего текстура кажется "грязной", смазанной или пиксельной.

**Как исправить:**
1. В окне **Shader Editor** найдите ноду **`Normal Map`** (она стоит между текстурой нормалей и входом Normal блока Principled BSDF).
2. Уменьшите значение параметра **`Strength`** (Сила) до **`0.2`** или **`0.3`**!
3. Если и при этом значении текстура выглядит грязной — вы можете временно полностью отключить карту нормалей (отсоединить провод от входа *Normal*), оставив только чистую Base Color. Модель станет очень гладкой и аккуратной.

---

### 🚨 Причина 4. Размытие текстуры в режиме просмотра (Viewport Texture Filtering)

Blender по умолчанию использует фильтрацию текстур `Linear` (Линейная), которая сглаживает пиксели. На низкополигональных моделях с разрешением текстуры 1K это может создавать эффект "мыла" на стыках швов брони и лица.

**Как исправить:**
1. В окне **Shader Editor** найдите ноду текстуры **Base Color**.
2. В первом выпадающем списке ноды (где написано **`Linear`**) измените режим фильтрации на **`Smart`** или **`Closest`** (По ближайшим пикселям). Это сделает текстурные швы более четкими и контрастными в окне просмотра.

---

### 📦 Как правильно перенести материалы из Blender в Unity 6 без потери настроек

Чтобы ваши настроенные текстуры автоматически подхватились в Unity 6 и не отображались серыми манекенами:

1. **Экспорт текстур:** Положите файлы текстур (`..._basecolor.png`, `..._normal.png` и т.д.) в ту же папку проекта Unity `Assets/Models/Heroes_Battles/`, куда вы импортируете FBX-модель персонажа.
2. **Настройка импорта FBX в Unity:**
   * Выберите вашу модель (например, `Magic_Battles_Osnova`) в окне **Project** в Unity.
   * В окне **Inspector** перейдите во вкладку **Materials**.
   * Установите параметр **Location** на **`Use External Materials (Legacy)`** или нажмите кнопку **`Extract Materials...`** и укажите ту же папку. Unity создаст настраиваемые материалы прямо в папке проекта.
   * Нажмите кнопку **`Extract Textures...`** (Извлечь текстуры), чтобы Unity связала текстурные карты.
3. **Назначение карт в Unity:**
   * Кликните на созданный материал в Unity.
   * Перетащите карту цвета (`basecolor`) в слот **Albedo** (или Base Map).
   * Перетащите карту нормалей (`normal`) в слот **Normal Map**. Unity спросит: *"This texture is not marked as a normal map. Fix now?"* — обязательно нажмите **Fix Now**!
   * Установите гладкость (Smoothness) и металличность (Metallic) ползунками материала в Unity по вашему вкусу для идеального блеска стали у Воина и мягкой кожи у Стрелка!

---

## 📖 Пошаговое руководство: Перенос модели в Blender и запуск риггинга

### Шаг 1. Импорт модели в Blender
1. Откройте Blender (версии 2.80 и выше, включая Blender 4.x).
2. Удалите стандартный куб (выделите его и нажмите **X** -> **Delete**).
3. Импортируйте модель вашего персонажа (полученную из Leonardo или генераторов 3D вроде Hunyuan):
   * Перейдите в **File -> Import** -> Выберите нужный формат вашей модели:
     * **Wavefront (.obj)** — если у вас OBJ файл.
     * **gITF (.glb / .gltf)** — если у вас GLB файл.
     * **FBX (.fbx)** — если у вас FBX файл.

### Шаг 2. Запуск скрипта автоматизации
1. Выделите импортированный меш персонажа в 3D-виде кликом мыши (он должен подсветиться ярким оранжевым контуром).
2. Перейдите на вкладку **Scripting** в самом верхнем горизонтальном меню Blender.
3. Нажмите кнопку **New** (Создать новый файл) в верхней части текстового редактора.
4. Откройте файл `blender_rigging_helper.py` из нашего проекта, полностью скопируйте его содержимое и вставьте в текстовое поле Blender.
5. Нажмите кнопку **Run Script** (значок треугольника Play ▶️ в правом верхнем углу текстовой панели).
6. Скелет мгновенно построится, привяжется к модели и создаст тестовую анимацию!

### Шаг 3. Проверка и исправление недочетов
1. **Запуск теста движения:** Вернитесь во вкладку **Layout**, наведите курсор на 3D-вид и нажмите клавишу **Пробел (Space)**. Персонаж начнет плавно двигать руками, ногами и головой. Наблюдайте, где сетка растягивается неестественно.
2. **Корректировка костей:** Если руки или ноги вашего персонажа на меше шире или уже стандартных, выделите созданный скелет (`FateHumanoid_Armature`), перейдите в **Edit Mode** (клавиша **Tab**), выделите нужный сустав (круглый шарик на кости) и переместите его с помощью клавиши **G** точно в центр сустава вашего меша, как подробно описано выше.

#### 🚨 РЕШЕНИЕ ПРОБЛЕМЫ: Почему в Edit Mode скелет ровный, а при переходе в Object/Pose Mode руки и ноги уходят назад?

Вы проделали отличную работу и настроили кости идеально под модель в режиме **Edit Mode**! Но когда вы переключаетесь назад или запускаете анимацию, суставы персонажа скручиваются или выгибаются назад (как на вашем скриншоте на 32-м кадре).

**Почему это происходит:**
1. **Edit Mode (Режим редактирования)** настраивает базовую Т-позу скелета — так называемый **Rest Pose** (Позу покоя).
2. **Pose Mode / Object Mode (Режим позы/объекта)** отображает скелет с учетом активной анимации или ручного сдвига костей. 
3. На вашем скриншоте внизу шкала времени стоит на **32-м кадре**, и на скелете активна анимация (`Animation`). В этой анимации записаны углы поворота костей, созданные для старого, еще не отредактированного скелета. Когда Blender берет ваши новые ровные кости и применяет к ним повороты анимации из 32-го кадра, руки и ноги выворачиваются назад!

Вот **3 простых способа** мгновенно исправить это и вернуть скелет в идеально ровное состояние:

---

##### 🏆 Способ 1. Сброс позы в исходное положение (Clear Pose) — Самый надежный способ
Если вы хотите сбросить все анимационные повороты и вернуть скелет в чистое Т-положение, как в Edit Mode:
1. Выделите скелет и перейдите в **Pose Mode** (Режим позы) через меню в левом верхнем углу (или нажмите **Ctrl + Tab**).
2. Нажмите клавишу **`A`** на клавиатуре, чтобы выделить абсолютно все кости скелета (они должны подсветиться голубым/синим цветом).
3. **Сбросьте повороты:** Нажмите комбинацию клавиш **`Alt + R`** (очистит вращение).
4. **Сбросьте перемещения:** Нажмите комбинацию клавиш **`Alt + G`** (очистит сдвиги).
5. **Сбросьте масштаб:** Нажмите комбинацию клавиш **`Alt + S`** (очистит масштабирование).
6. **Результат:** Все кости моментально встанут идеально ровно по вашей модели в Т-позу, прямо как в Edit Mode!

---

##### 🏆 Способ 2. Временное принудительное включение Rest Position
Если на скелете записана анимация, которую вы не хотите удалять, но вам нужно временно увидеть скелет идеально ровным в объектном режиме:
1. Выберите ваш скелет в объектном режиме.
2. В правой панели найдите вкладку **Armature Data Properties** (зеленый значок бегущего человечка 🏃‍♂️, вкладка со свойствами скелета).
3. В самом верху в разделе **Pose** вы увидите две переключающиеся кнопки: **`Pose Position`** (Поза с анимацией) и **`Rest Position`** (Чистая исходная поза).
4. Нажмите на кнопку **`Rest Position`**!
5. **Результат:** Анимация временно отключится, скелет станет абсолютно ровным и совпадет с вашей моделью на 100%! *Не забудьте переключить обратно на Pose Position, когда будете экспортировать анимации.*

---

##### 🏆 Способ 3. Удаление мешающей тестовой анимации
Скрипт авто-генерации скелета создает тестовую анимацию покачивания, чтобы вы могли оценить сгибы. Если она вам больше не нужна и мешает:
1. Перейдите на вкладку **Animation** в верхней панели Blender или откройте окно **Dope Sheet** / **Action Editor**.
2. В выпадающем списке анимаций найдите активный экшн (например, `FateHumanoid_ArmatureAction` или `Animation`).
3. Нажмите на крестик **`X`** рядом с его названием, чтобы отвязать анимацию от скелета.
4. Переместите ползунок таймлайна внизу экрана на **0-й** или **1-й кадр**. Теперь кости не будут скручиваться!

---

#### 🔗 КАК ПРИВЯЗАТЬ МЕШ ПЕРСОНАЖА К ВАШЕМУ НОВОМУ ОТРЕДАКТИРОВАННОМУ СКЕЛЕТУ? (Skinning / Скиннинг)

Если вы настроили кости, но при переходе в **Pose Mode** и вращении костей тело (меш) персонажа не двигается вслед за ними, значит, нарушена или отсутствует привязка вершин меша к костям скелета.

Выполните следующие шаги для полной перепривязки модели:

##### 1️⃣ Шаг 1. Полная очистка старых родительских связей
Перед созданием новой правильной привязки нужно удалить остатки старых связей и модификаторов, чтобы они не конфликтовали:
1. Перейдите в **Object Mode** (Объектный режим) через левое верхнее меню.
2. Кликните левой кнопкой мыши по **Мешу (модели)** персонажа (например, `node_0`).
3. Нажмите комбинацию клавиш **`Alt + P`** и в выпадающем меню выберите пункт **`Clear and Keep Transformation`** (Очистить родителя с сохранением трансформаций).
4. Перейдите на вкладку **Modifier Properties** на панели справа (значок синего гаечного ключа 🔧).
5. Если в списке есть модификатор **Armature**, нажмите на крестик **`X`** в его правом верхнем углу, чтобы удалить его.

##### 2️⃣ Шаг 2. Создание новой привязки с автоматическими весами
1. В **Object Mode** выберите левым кликом мыши **Меш** персонажа (`node_0`).
2. Зажмите и удерживайте клавишу **`Shift`** на клавиатуре.
3. Не отпуская Shift, кликните по вашему **Скелету** (`FateHumanoid_Armature`).
   * *Критически важно:* Меш должен иметь тёмно-оранжевую подсветку, а скелет — светло-жёлтую (это значит, что скелет является активным главным объектом).
4. Нажмите комбинацию клавиш **`Ctrl + P`** на клавиатуре.
5. В появившемся меню «Set Parent To» выберите пункт **`With Automatic Weights`** (С автоматическими весами).
6. **Результат:** Blender автоматически сгенерирует веса влияния для каждой кости и добавит рабочий модификатор Armature.

##### 🛑 ЧТО ДЕЛАТЬ, ЕСЛИ НЕТ ПУНКТА «With Automatic Weights» ИЛИ СЛОЖНО ВЫДЕЛИТЬ ОБЪЕКТЫ?
На вашем скриншоте в меню отображаются только простые варианты `Object`, `Vertex` и т.д. 

**Причина:** Вы нарушили порядок выделения объектов. У вас активным объектом (выделен **светло-жёлтым** цветом) остался **Меш** (`node_0`), а скелет выделен тёмно-оранжевым. Для Blender это означает, что вы пытаетесь привязать скелет к мешу, а не меш к скелету.

Вы можете решить это двумя способами: либо вручную (Вариант А), либо **полностью автоматически в 1 клик с помощью нашего готового скрипта (Вариант Б)**, который сам выделит всё в правильном порядке, очистит старый мусор и привяжет скелет!

---

##### Вариант А. Ручное исправление порядка выделения:
1. Кликните в любое пустое место на экране, чтобы полностью снять выделение.
2. Кликните **левой кнопкой мыши** сначала по **Мешу** персонажа (`node_0`). Вокруг него появится тёмно-оранжевая обводка.
3. Зажмите и удерживайте клавишу **`Shift`**.
4. Кликните по **Скелету** (`FateHumanoid_Armature`). Скелет должен стать **светло-жёлтым**, а меш остаться тёмно-оранжевым.
   * *Внимание:* Если скелет не выделяется во вьюпорте, выделите его в списке слоёв (Outliner) справа: зажмите `Ctrl` или `Shift` и кликните на `FateHumanoid_Armature`. Главное, чтобы скелет стал активным (светлая подсветка/иконка в списке).
5. Теперь нажмите **`Ctrl + P`** — пункт **`With Automatic Weights`** гарантированно появится в меню!

---

##### Вариант Б. Автоматическая привязка скриптом (Рекомендуемый и 100% надёжный способ!):
Я написал для вас специальный скрипт авто-привязки `/blender_auto_binder.py`. Он полностью автоматизирует этот шаг: сам находит меш `node_0`, сам находит скелет, очищает старые родительские связи, исправляет ошибки вершин и привязывает модель с авто-весами!

**Как запустить его в Blender за 15 секунд:**
1. В верхней панели Blender перейдите на вкладку **`Scripting`** (скриптинг).
2. Нажмите кнопку **`New`** (Новый), чтобы создать текстовый файл.
3. Скопируйте и вставьте туда следующий Python-код:

```python
import bpy

def run_auto_binding():
    print("--- Starting Auto-Binding Script ---")
    if bpy.ops.object.mode_set.poll():
        bpy.ops.object.mode_set(mode='OBJECT')
    bpy.ops.object.select_all(action='DESELECT')
    
    mesh_obj = bpy.data.objects.get("node_0")
    arm_obj = bpy.data.objects.get("FateHumanoid_Armature")
    
    if not mesh_obj:
        for obj in bpy.data.objects:
            if obj.type == 'MESH' and (obj.name.startswith("node_") or "mesh" in obj.name.lower()):
                mesh_obj = obj
                break
    if not mesh_obj:
        meshes = [obj for obj in bpy.data.objects if obj.type == 'MESH']
        if len(meshes) > 0: mesh_obj = meshes[0]
            
    if not arm_obj:
        armatures = [obj for obj in bpy.data.objects if obj.type == 'ARMATURE']
        if len(armatures) > 0: arm_obj = armatures[0]

    if not mesh_obj or not arm_obj:
        print("ERROR: Mesh or Armature not found!")
        return False
        
    print(f"Using Mesh: {mesh_obj.name}, Armature: {arm_obj.name}")
    
    mesh_obj.select_set(True)
    bpy.context.view_layer.objects.active = mesh_obj
    bpy.ops.object.parent_clear(type='CLEAR_KEEP_TRANSFORM')
    
    arm_modifiers = [m for m in mesh_obj.modifiers if m.type == 'ARMATURE']
    for m in arm_modifiers:
        mesh_obj.modifiers.remove(m)
        
    print("Merging duplicate vertices to prevent Heat Weighting errors...")
    bpy.ops.object.select_all(action='DESELECT')
    mesh_obj.select_set(True)
    bpy.context.view_layer.objects.active = mesh_obj
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.remove_doubles(threshold=0.0001)
    bpy.ops.object.mode_set(mode='OBJECT')
    
    bpy.ops.object.select_all(action='DESELECT')
    mesh_obj.select_set(True)
    arm_obj.select_set(True)
    bpy.context.view_layer.objects.active = arm_obj
    
    print("Applying parenting with automatic weights...")
    try:
        bpy.ops.object.parent_set(type='WITH_AUTOMATIC_WEIGHTS')
        print("SUCCESS! Model is bound to armature successfully!")
        return True
    except Exception as e:
        print(f"Error: {e}")
        return False

run_auto_binding()
```

4. Нажмите на иконку **Запуска (треугольник «Play» ▶)** в верхней части панели скрипта.
5. **Готово!** Скрипт мгновенно выполнит за вас всю ручную работу. Вернитесь на вкладку **`Layout`**, перейдите в **Pose Mode** и проверьте: кости двигают тело безупречно!

---

##### ⚠️ Что делать, если появляется ошибка "Bone Heat Weighting: failed to find solution"?
Эта ошибка возникает, если меш импортированной модели имеет пересекающиеся или дублирующиеся вершины. Скрипт выше автоматически решает эту проблему за вас, очищая геометрию методом Merge By Distance! Если вы делаете это вручную:
1. Выделите только **Меш** (`node_0`) и перейдите в **Edit Mode** (клавиша **Tab**).
2. Нажмите **`A`**, чтобы выделить абсолютно все вершины модели (они подсветятся оранжевым).
3. Нажмите клавишу **`M`** (меню Merge / Объединить) и выберите **`By Distance`** (По расстоянию). Blender удалит дубликаты вершин (внизу появится отчет, например, *Removed 1240 vertices*).
4. Вернитесь в **Object Mode** (клавиша **Tab**) и повторите привязку: выделите Меш -> Shift + клик по Скелету -> **Ctrl + P** -> **With Automatic Weights**.

##### 3️⃣ Шаг 3. Исправление «резиновых» подмышек (Weight Paint):
   * Сначала выберите **Скелет** в объектном режиме (левым кликом), затем, удерживая клавишу **Shift**, кликните по **Мешу** персонажа.
   * Перейдите в режим **Weight Paint** (выберите его в выпадающем списке в левом верхнем углу окна Blender вместо *Object Mode*).
   * Зажмите клавишу **Ctrl** и кликните по плечевой кости (`UpperArm.L` или `UpperArm.R`). Вы увидите веса влияния кости: красный цвет означает 100% влияние, синий — 0% влияния.
   * Выберите кисть **Draw** на панели слева, установите параметр **Weight** (вес) на **`0.0`** и аккуратно сотрите влияние руки с боков туловища (рёбер и подмышек), чтобы при поднятии руки бока и рёбра персонажа не тянулись вслед за ней.

### Шаг 4. Настройки экспорта из Blender в Unity 6 для Воина, Стрелка и Мага

Когда скелет настроен и привязан к модели, нужно правильно экспортировать персонажей (Воина, Стрелка, Мага) в Unity 6, чтобы не сломались масштабы, текстуры и анимации.

#### 📋 Пошаговая инструкция экспорта в FBX:

1. В объектном режиме выделите **только** вашу модель персонажа (меш) и его скелет (`FateHumanoid_Armature`).
2. Перейдите в верхнее меню: **File -> Export -> FBX (.fbx)**.
3. In settings... (настройки):
   * Поставьте галочку **Selected Objects** (экспортировать только выделенные элементы).
   * В разделе **Object Types** выделите только **Armature** и **Mesh** (зажмите Shift при выборе).
   * В разделе **Transform**:
     * Установите **Scale** в значение **1.0**.
     * В поле **Apply Scalings** выберите **FBX All** (это сохранит масштаб в Unity равным ровно 1.0, без гигантских или микроскопических искажений).
     * Установите **Forward** на **-Z Forward** и **Up** на **Y Up** (это стандарт для Unity 6).
   * В разделе **Geometry**:
     * Измените **Smoothing** на **Face** (чтобы убрать некрасивые черные тени на доспехах воина).
   * В разделе **Armature**:
     * Поставьте галочку **Only Deform Bones** (только деформирующие кости).
     * **ОБЯЗАТЕЛЬНО снимите галочку «Add Leaf Bones»!** (иначе Blender добавит бесполезные пустые кости-пустышки на концах скелета, ломая риггинг Humanoid в Unity).
     * Установите **Primary Bone Axis = Y** и **Secondary Bone Axis = X**.
   * В разделе **Bake Animation**:
     * **Снимите галочку**, если вы хотите экспортировать чистый скин персонажа в Т-позе для последующей привязки любых сторонних анимаций (например, Mixamo).
4. Назовите файлы согласно классам:
   * **Воин:** `Warrior_Rigged_v1.fbx`
   * **Стрелок:** `Archer_Rigged_v1.fbx`
   * **Маг:** `Mage_Rigged_v1.fbx`
5. Экспортируйте файл прямо в вашу папку `Assets/Models/Characters/` в проекте Unity!

---

## 🎯 Обновленные промпты для Leonardo.ai для получения более ровных моделей

Если вы захотите перегенерировать персонажей в Leonardo перед импортом в Blender, используйте эти обновленные точные промпты. Они минимизируют появление нескольких ракурсов и заставляют модель стоять ровно по центру:

*(Все подробные СВЕРХ-СИММЕТРИЧНЫЕ промпты для Т-позы Воина, Стрелка и Мага находятся в начале этого руководства на ЭТАПЕ 1. Используйте их вместе с Универсальным Негативным Промптом для идеального результата!)*

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



## 💻 ЭТАП 5: Сверхлегкий C# Скрипт `TacticalUnitAnimator.cs` (С полной поддержкой боя)

Этот скрипт разработан с учетом требований к экстремальной производительности под Unity 6. Он полностью исключает выделение мусора (GC Alloc) за счет предварительного кэширования строк в числовые хэши параметров аниматора, бережет процессор за счет отключения анимации костей вне зоны видимости камеры, а также блокирует движение и атаки, если персонаж погиб.

```csharp
// [TACTICAL UNIT ANIMATOR v18.12.06]
// Оптимизированный менеджер анимаций для BattleScene (Бег, Атаки, Блоки, Получение урона, Смерть)
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
    
    // Новые хэши параметров для расширенной боевой системы
    private static readonly int HitTriggerHash = Animator.StringToHash("Hit");
    private static readonly int BlockTriggerHash = Animator.StringToHash("Block");
    private static readonly int DeathTriggerHash = Animator.StringToHash("Death");
    private static readonly int SuperDeathTriggerHash = Animator.StringToHash("SuperDeath");
    private static readonly int IsDeadBoolHash = Animator.StringToHash("IsDead");

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private bool isDead = false;

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
        if (isDead) return; // Если мертв — не двигаемся и не обновляем логику

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
        if (isDead) return;

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
        if (isDead) return;

        LookAtTargetInstant(lookAtTarget);
        animator.SetTrigger(AttackTriggerHash);
    }

    /// <summary>
    /// Воспроизведение суперспособности в сторону цели
    /// </summary>
    public void PlaySuperAttack(Vector3 lookAtTarget)
    {
        if (isDead) return;

        LookAtTargetInstant(lookAtTarget);
        animator.SetTrigger(SuperAttackTriggerHash);
    }

    /// <summary>
    /// Воспроизведение попадания (получение урона)
    /// </summary>
    public void PlayHit()
    {
        if (isDead) return;
        animator.SetTrigger(HitTriggerHash);
    }

    /// <summary>
    /// Воспроизведение блока удара щитом/оружием
    /// </summary>
    public void PlayBlock()
    {
        if (isDead) return;
        animator.SetTrigger(BlockTriggerHash);
    }

    /// <summary>
    /// Воспроизведение смерти персонажа
    /// </summary>
    /// <param name="isSuperAbility">Если true, воспроизводится смерть от мощного суперудара (падение/отлет)</param>
    public void PlayDeath(bool isSuperAbility)
    {
        if (isDead) return;
        isDead = true;
        isMoving = false;

        animator.SetFloat(SpeedHash, 0f);
        animator.SetBool(IsDeadBoolHash, true);

        if (isSuperAbility)
        {
            animator.SetTrigger(SuperDeathTriggerHash);
        }
        else
        {
            animator.SetTrigger(DeathTriggerHash);
        }
    }

    /// <summary>
    /// Полный сброс состояния (например, для переиспользования в пуле объектов)
    /// </summary>
    public void ResetUnit()
    {
        isDead = false;
        isMoving = false;
        animator.SetBool(IsDeadBoolHash, false);
        animator.SetFloat(SpeedHash, 0f);
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

## ⚔️ ЭТАП 6: Добавление боевых анимаций: удары, блоки, смерть и падения

Чтобы превратить простое перемещение в зрелищное тактическое сражение, вам понадобятся дополнительные анимации. Ниже приведена подробная инструкция по их поиску, скачиванию, импорту и настройке в аниматоре Unity.

### 🔍 1. Поисковые запросы на Mixamo для каждого класса
Зайдите на [Mixamo.com](https://www.mixamo.com/) и введите следующие поисковые запросы. Выбирайте те анимации, которые лучше всего подходят стилистике вашего персонажа:

#### 🛡️ Для Воина (Warrior):
* **Обычная атака (Sword Slash / Shield Bash):** 
  * Поиск: `sword slash` или `shield bash` или `one hand sword combo`.
  * Рекомендуемая анимация: **«Sword And Shield Slash»** или **«Standing Melee Kick»**.
* **Супер-атака (Heavy Jump Attack):**
  * Поиск: `heavy sword slash` или `jump attack`.
  * Рекомендуемая анимация: **«Great Sword Slash»** или **«Standing One-Handed Greatsword Slash»**.
* **Блок (Block):**
  * Поиск: `shield block` или `sword block`.
  * Рекомендуемая анимация: **«Standing Shield Block Pose»** or **«Sword And Shield Block To Hit»**.

#### 🏹 Для Стрелка (Archer):
* **Обычная атака (Bow Shot):**
  * Поиск: `standing draw bow` или `crossbow shoot`.
  * Рекомендуемая анимация: **«Standing Draw Bow»** (Убедитесь, что анимация воспроизводится стоя на месте!).
* **Супер-атака (Double / Rapid Shot):**
  * Поиск: `rapid bow shoot` или `bow combat combination`.
  * Рекомендуемая анимация: **«Standing Rapid Bow Fire»**.
* **Блок (Dodge / Evade):**
  * Поиск: `dodge` или `evade jump`.
  * Рекомендуемая анимация: **«Dodge Backwards»** (выберите параметр **In Place**, чтобы персонаж не улетал физически с клетки!).

#### 🔮 Для Мага (Mage):
* **Обычная атака (Magic Projectile):**
  * Поиск: `spell cast` или `standing magic fire`.
  * Рекомендуемая анимация: **«Standing Direct Magic Attack»** or **«Standing Magic Spell»**.
* **Супер-атака (Area of Effect Summon):**
  * Поиск: `summon spell` или `heavy magic summon`.
  * Рекомендуемая анимация: **«Summoning Ground Spells»** or **«Spell Casting High Energy»**.
* **Блок (Energy Shield / Ward):**
  * Поиск: `magic block` или `shield barrier`.
  * Рекомендуемая анимация: **«Standing Barrier Block»** (персонаж выставляет руку вперед, создавая магический щит).

#### 💀 Общие анимации (Для всех классов):
* **Получение урона (Hit Reaction):**
  * Поиск: `hit reaction` или `get hit standing`.
  * Рекомендуемая анимация: **«Standing Reaction Hit»** (быстрое вздрагивание корпуса от удара).
* **Смерть от обычного удара (Simple Death):**
  * Поиск: `death` или `standing death`.
  * Рекомендуемая анимация: **«Stagger Back And Die»** or **«Slightly Fold Death»** (персонаж падает вперед или на спину аккуратно в пределах своей клетки).
* **Смерть от суперспособности / Падение (Super Death / Knockdown):**
  * Поиск: `flying death` или `knockdown death` или `backward fall`.
  * Рекомендуемая анимация: **«Knocked Backwards And Death»** (мощный отлет назад с падением навзничь, идеально для смерти от взрыва или суперудара).

---

### 📥 2. Правильные параметры скачивания (Секрет экономии веса билда)
Когда вы скачиваете **дополнительные** анимации, соблюдайте эти правила:
1. **Первую анимацию (Idle / Скин)** вы скачиваете с настройкой **Format: FBX for Unity** и **Skin: With Skin** (чтобы получить 3D-модель).
2. **Все последующие анимации (удары, смерть, блоки)** скачивайте строго с настройкой **Skin: Without Skin**!
   * *Почему:* Файлы без скина содержат только математическую информацию о движении костей. Они весят по 50-100 КБ вместо 15-30 МБ! Это уменьшит вес вашей игры в 50 раз.
3. Всегда ставьте галочку **In Place** (На месте) для всех анимаций перемещений или уклонений, чтобы игровая логика Unity сама управляла положением фигурки на сетке, а анимация не уводила 3D-модель физически в сторону от её логической клетки.

---

### 📂 3. Настройка импорта анимаций в Unity
После того как вы перетащили файлы `.fbx` с анимациями в Unity:
1. Выделите файл анимации в окне **Project**.
2. В инспекторе перейдите во вкладку **Rig**:
   * **Animation Type:** Установите **Humanoid** (так как мы настроили Т-позу, кости идеально перенесутся!).
   * **Avatar Definition:** Выберите **Copy From Other Avatar** и укажите аватар вашей основной модели (например, `Warrior_Avatar`), который вы создали на Этапе 3. Это гарантирует 100% совместимость.
3. Перейдите во вкладку **Animation**:
   * Для анимаций смерти и урона **снимите** галочку **Loop Time** (они должны проиграться ровно один раз).
   * Для анимаций атак и блоков также **снимите** галочку **Loop Time**.
   * Нажмите **Apply** внизу инспектора.

---

### 🎨 4. Настройка Animator Controller (Логика переходов)

Создайте параметры в левой вкладке **Parameters** вашего Animator Controller:
1. `Speed` (Float) — для бега.
2. `IdleType` (Int) — стойка (0, 1, 2).
3. `Attack` (Trigger) — запуск атаки.
4. `SuperAttack` (Trigger) — запуск суперудара.
5. `Hit` (Trigger) — урон.
6. `Block` (Trigger) — блок.
7. `Death` (Trigger) — обычная смерть.
8. `SuperDeath` (Trigger) — жесткая смерть с отлетом.
9. `IsDead` (Bool) — флаг смерти.

#### 🗺️ Архитектура переходов (Transitions):

* **Переход в урон/блок (Any State -> Hit / Block):**
  * Создайте связь от узла **Any State** к состоянию **Hit** и к состоянию **Block**.
  * В условиях перехода (Conditions) укажите соответствующий триггер: `Hit` или `Block`.
  * Установите **Has Exit Time = false** (чтобы анимация срабатывала мгновенно, прерывая бег или покой).
  * Из состояний **Hit** и **Block** сделайте обратный переход в **Blend Tree (Idle/Run)** с включенной галочкой **Has Exit Time = true** (чтобы по окончании анимации персонаж автоматически вернулся в стойку покоя).

* **Переход в смерть (Any State -> Death / SuperDeath):**
  * Создайте связь от **Any State** к состояниям **Death** (обычное падение) и **SuperDeath** (отлет назад).
  * В условиях перехода укажите триггер `Death` или `SuperDeath`, а также обязательное условие `IsDead = true`.
  * Установите **Has Exit Time = false** и **Transition Duration = 0.1s** (для моментальной смерти).
  * **ВАЖНО:** Из состояний смерти **НЕ должно быть никаких выходящих стрелок!** Персонаж должен оставаться лежать на земле до тех пор, пока отряд не возродится или не исчезнет с поля боя.

---

## 🗜️ ЭТАП 7: Финальная оптимизация видеокарты и оперативной памяти в Unity 6

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

## 🛠️ РЕШЕНИЕ ПРОБЛЕМЫ: Растягивание («резиновый» шлем, воротник или плечи) в Mixamo

### 🔍 Почему это происходит?
Когда авто-риггер Mixamo рассчитывает привязку вершин меша к костям скелета (**Skin Weighting**), он использует автоматические алгоритмы расстояний. 
* Если у вашего рыцаря/воина **высокий воротник, массивные наплечники (pauldrons) или шлем со спускающимся забралом/подбородком**, автоматический алгоритм путается.
* Часть вершин шлема он привязывает к кости **Головы (Head)**, а часть нижних вершин — к костям **Шеи (Neck)** или **Груди (Spine/Chest)**.
* В результате при вращении головы часть шлема поворачивается правильно, а нижняя часть «прилипает» к шее или плечам, создавая ужасный эффект растянутой резины («смазанности»).

---

### 🚀 Как это исправить? 3 проверенных способа (от простого к профессиональному)

#### 🎯 Вариант 1: Правильная расстановка меток в Mixamo (Быстрый фикс)
1. Нажмите **Back** в Mixamo, чтобы вернуться к расстановке цветных маркеров.
2. **Маркер CHIN (Синий):** Поднимите его немного выше, чем обычно! Расположите его строго на уровне рта/забрала шлема, а не на самом нижнем кончике подбородка. Это «отрежет» влияние кости головы от шеи и воротника.
3. **Маркер GROIN (Оранжевый):** Убедитесь, что он стоит ровно по центру, не слишком низко.
4. В выпадающем меню **Skeleton LOD** выберите **No Fingers (25 bones)**. С упрощенным скелетом алгоритму Mixamo намного проще рассчитать правильные веса на голову и шею!

---

#### 💻 Вариант 2: Программный 1-клик фикс прямо в Unity (Скрипт `FateBoneWeightFixer.cs`)
Мы создали для вас специальный автоматический инструмент, который решает эту проблему прямо внутри Unity без использования Blender! Скрипт уже лежит в корне вашего проекта: `/FateBoneWeightFixer.cs`.

**Как запустить в Unity:**
1. Скопируйте файл `/FateBoneWeightFixer.cs` в ваш проект Unity в любую папку (например, `Assets/Editor/` или `Assets/Scripts/`).
2. Перетащите вашу риггеную модель (например, `Warrior_Rigged`) на сцену в Unity.
3. Найдите объект внутри модели, на котором висит компонент **SkinnedMeshRenderer** (это сам меш персонажа).
4. Добавьте на этот же объект наш скрипт **Fate Bone Weight Fixer** (нажмите *Add Component -> Fate Tools -> Fate Bone Weight Fixer*).
5. Настройте параметры в инспекторе:
   * **Head Bone:** Перетащите кость головы персонажа из иерархии (обычно называется `mixamorig:Head`).
   * **Height Threshold:** Установите пороговую высоту (по умолчанию `1.6` в локальных координатах). Все вершины меша выше этой отметки (то есть вся голова и верхняя часть шлема) будут принудительно привязаны 100% только к голове!
6. Нажмите правой кнопкой мыши по названию компонента **Fate Bone Weight Fixer** в инспекторе и выберите пункт меню **«Fix Head & Helmet Weights»** (или найдите контекстное меню).
7. Скрипт мгновенно очистит веса шеи со шлема, создаст новый оптимизированный меш в папке проекта и заменит его. Растягивание шлема полностью исчезнет!

---

#### 🖌️ Вариант 3: Ручная развесовка в Blender (Профессиональный индустриальный стандарт)
Если вы хотите идеального контроля над каждым миллиметром брони, сделайте ручную зачистку весов вершин (**Weight Paint**):

1. Импортируйте ваш полученный `.fbx` файл из Mixamo в Blender.
2. Выделите скелет (Armature), перейдите в **Pose Mode** и поверните кость головы (`Head`), чтобы увидеть, какие вершины тянутся.
3. Вернитесь в **Object Mode**, выделите меш брони, а затем зажмите `Shift` и выделите скелет.
4. Перейдите в режим **Weight Paint** (выпадающее меню слева вверху).
5. В правой панели найдите вкладку **Object Data Properties** (зеленый значок треугольной сетки) и раскройте список **Vertex Groups** (Группы вершин).
6. Найдите группу с именем головы (обычно `mixamorig:Head` или `Head`):
   * Выберите кисть **Draw** со значением **Weight = 1.0**.
   * Аккуратно закрасьте весь шлем красным цветом (красный цвет = 100% привязка к голове).
7. Найдите группу шеи (`mixamorig:Neck` или `Neck`) и груди (`mixamorig:Spine3`):
   * Выберите кисть **Subtract** со значением **Weight = 1.0** (или установите кисть Draw с Weight = 0.0).
   * Сотрите влияние шеи со всего шлема (он должен стать абсолютно синим в зоне шлема). Синий цвет означает нулевое влияние кости шеи.
8. Экспортируйте исправленную модель обратно в Unity в формате FBX. Теперь шлем будет сидеть как литой!

---

*Ваш проект полностью оптимизирован и готов к запуску в Unity 6!*

