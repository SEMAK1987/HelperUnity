# [FATE CONTINENT - BLENDER AUTOMATIC RIGGING & TESTING SYSTEM v18.12.06]
# ======================================================================================
# Скрипт автоматического создания скелета (Armature), пропорционального размещения костей,
# автоматической привязки весов (Skinning) и создания тестовой анимации для проверки меша.
# ======================================================================================
# ИНСТРУКЦИЯ ПО ЗАПУСКУ:
# 1. Импортируйте вашего персонажа (OBJ, FBX или GLB) в Blender.
# 2. Выделите импортированный меш в 3D-виде (он должен стать активным).
# 3. Перейдите во вкладку Scripting вверху Blender, нажмите "New", вставьте этот код и нажмите кнопку "Run Script" (Play).
# ======================================================================================

import bpy
import math

def setup_automatic_rig():
    print("\n=== STARTING FATE AUTOMATIC RIGGING SYSTEM ===")
    
    # 1. Проверяем наличие активного объекта-меша
    active_obj = bpy.context.active_object
    if not active_obj or active_obj.type != 'MESH':
        # Если ничего не выделено, пытаемся найти любой крупный меш в сцене
        mesh_objs = [obj for obj in bpy.data.objects if obj.type == 'MESH']
        if mesh_objs:
            active_obj = mesh_objs[0]
            bpy.context.view_layer.objects.active = active_obj
            active_obj.select_set(True)
            print(f"[Fate Rig] Автоматически выбран меш: {active_obj.name}")
        else:
            print("[Fate ERROR] В сцене не найден меш! Импортируйте OBJ/FBX/GLB модель перед запуском скрипта.")
            return False

    mesh_obj = active_obj
    print(f"[Fate Rig] Подготовка к риггингу меша: '{mesh_obj.name}'")

    # Сбрасываем трансформации меша для точных расчетов костей
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

    # 2. Вычисляем размеры меша по его вершинам (Bounding Box) в локальном пространстве
    vertices = mesh_obj.data.vertices
    if len(vertices) == 0:
        print("[Fate ERROR] Выбранный меш не содержит вершин!")
        return False

    coords_x = [v.co.x for v in vertices]
    coords_y = [v.co.y for v in vertices]
    coords_z = [v.co.z for v in vertices]

    min_x, max_x = min(coords_x), max(coords_x)
    min_y, max_y = min(coords_y), max(coords_y)
    min_z, max_z = min(coords_z), max(coords_z)

    height = max_z - min_z
    width_x = max_x - min_x
    depth_y = max_y - min_y

    center_x = (min_x + max_x) / 2.0
    center_y = (min_y + max_y) / 2.0
    
    print(f"[Fate Rig] Размеры меша: Высота={height:.3f}m, Ширина={width_x:.3f}m, Глубина={depth_y:.3f}m")
    print(f"[Fate Rig] Центр меша: X={center_x:.3f}, Y={center_y:.3f}, Z_min={min_z:.3f} to Z_max={max_z:.3f}")

    # Удаляем старый скелет Fate, если он уже существовал, чтобы избежать дубликатов
    if "FateHumanoid_Armature" in bpy.data.objects:
        old_arm = bpy.data.objects["FateHumanoid_Armature"]
        print("[Fate Rig] Обнаружен старый скелет. Удаление перед повторным созданием...")
        bpy.data.objects.remove(old_arm, do_unlink=True)

    # 3. Создаем новую Арматуру (скелет)
    arm_data = bpy.data.armatures.new("FateHumanoid_ArmatureData")
    arm_obj = bpy.data.objects.new("FateHumanoid_Armature", arm_data)
    bpy.context.collection.objects.link(arm_obj)
    
    # Делаем кости видимыми ПОВЕРХ меша (Рентген / In Front)
    arm_obj.show_in_front = True
    arm_data.display_type = 'OCTAHEDRAL' # Классические октаэдрические кости
    arm_data.show_names = True           # Отображать имена костей для удобства отладки в Blender

    # 4. Переходим в Edit Mode для построения костей
    bpy.context.view_layer.objects.active = arm_obj
    bpy.ops.object.mode_set(mode='EDIT')

    # Пропорциональный расчет высоты суставов на основе роста меша
    # (Идеально подходит под любые размеры модели - от карликов до гигантов!)
    z_floor = min_z
    z_hips = z_floor + height * 0.52     # Таз
    z_spine = z_floor + height * 0.62    # Поясница
    z_chest = z_floor + height * 0.72    # Грудь
    z_neck = z_floor + height * 0.82     # Шея
    z_head = z_floor + height * 0.88     # Основание головы
    z_head_top = z_floor + height * 0.98 # Макушка

    # Руки (плечи, локти, кисти)
    shoulder_offset = width_x * 0.15     # Смещение плеча от центра
    elbow_offset = width_x * 0.38        # Смещение локтя от центра
    wrist_offset = width_x * 0.58        # Смещение запястья от центра
    hand_offset = width_x * 0.68         # Смещение кончиков пальцев
    
    # Ноги (бедра, колени, лодыжки, стопы)
    hip_width = width_x * 0.12           # Смещение ног от центральной оси
    z_knee = z_floor + height * 0.28     # Высота колен
    z_ankle = z_floor + height * 0.08    # Высота лодыжек
    y_toe = min_y - depth_y * 0.15       # Длина стопы вперед

    # Вспомогательная функция создания кости
    def create_bone(name, parent_name, head_pos, tail_pos):
        bone = arm_data.edit_bones.new(name)
        bone.head = head_pos
        bone.tail = tail_pos
        if parent_name:
            parent_bone = arm_data.edit_bones.get(parent_name)
            if parent_bone:
                bone.parent = parent_bone
        return bone

    # --- СТРОИМ ПОЗВОНОЧНИК ---
    # Таз (Hips) - главная корневая кость
    create_bone("Hips", None, 
                (center_x, center_y, z_hips), 
                (center_x, center_y, z_spine))

    # Поясница (Spine)
    create_bone("Spine", "Hips", 
                (center_x, center_y, z_spine), 
                (center_x, center_y, z_chest))

    # Грудь (Chest)
    create_bone("Chest", "Spine", 
                (center_x, center_y, z_chest), 
                (center_x, center_y, z_neck))

    # Шея (Neck)
    create_bone("Neck", "Chest", 
                (center_x, center_y, z_neck), 
                (center_x, center_y, z_head))

    # Голова (Head)
    create_bone("Head", "Neck", 
                (center_x, center_y, z_head), 
                (center_x, center_y, z_head_top))

    # --- ЛЕВАЯ СТОРОНА (Руки и Ноги) ---
    # Плечо (Shoulder_L)
    create_bone("Shoulder.L", "Chest", 
                (center_x, center_y, z_chest), 
                (center_x - shoulder_offset, center_y, z_chest))

    # Плечевая кость (UpperArm_L)
    create_bone("UpperArm.L", "Shoulder.L", 
                (center_x - shoulder_offset, center_y, z_chest), 
                (center_x - elbow_offset, center_y, z_chest))

    # Предплечье (Forearm_L)
    create_bone("LowerArm.L", "UpperArm.L", 
                (center_x - elbow_offset, center_y, z_chest), 
                (center_x - wrist_offset, center_y, z_chest))

    # Кисть руки (Hand_L)
    create_bone("Hand.L", "LowerArm.L", 
                (center_x - wrist_offset, center_y, z_chest), 
                (center_x - hand_offset, center_y, z_chest))

    # Бедро (Thigh_L)
    create_bone("Thigh.L", "Hips", 
                (center_x - hip_width, center_y, z_hips), 
                (center_x - hip_width, center_y, z_knee))

    # Голень (Shin_L)
    create_bone("Shin.L", "Thigh.L", 
                (center_x - hip_width, center_y, z_knee), 
                (center_x - hip_width, center_y, z_ankle))

    # Стопа (Foot_L)
    create_bone("Foot.L", "Shin.L", 
                (center_x - hip_width, center_y, z_ankle), 
                (center_x - hip_width, y_toe, z_ankle))


    # --- ПРАВАЯ СТОРОНА (Руки и Ноги) ---
    # Плечо (Shoulder_R)
    create_bone("Shoulder.R", "Chest", 
                (center_x, center_y, z_chest), 
                (center_x + shoulder_offset, center_y, z_chest))

    # Плечевая кость (UpperArm_R)
    create_bone("UpperArm.R", "Shoulder.R", 
                (center_x + shoulder_offset, center_y, z_chest), 
                (center_x + elbow_offset, center_y, z_chest))

    # Предплечье (Forearm_R)
    create_bone("LowerArm.R", "UpperArm.R", 
                (center_x + elbow_offset, center_y, z_chest), 
                (center_x + wrist_offset, center_y, z_chest))

    # Кисть руки (Hand_R)
    create_bone("Hand.R", "LowerArm.R", 
                (center_x + wrist_offset, center_y, z_chest), 
                (center_x + hand_offset, center_y, z_chest))

    # Бедро (Thigh_R)
    create_bone("Thigh.R", "Hips", 
                (center_x + hip_width, center_y, z_hips), 
                (center_x + hip_width, center_y, z_knee))

    # Голень (Shin_R)
    create_bone("Shin.R", "Thigh.R", 
                (center_x + hip_width, center_y, z_knee), 
                (center_x + hip_width, center_y, z_ankle))

    # Стопа (Foot_R)
    create_bone("Foot.R", "Shin.R", 
                (center_x + hip_width, center_y, z_ankle), 
                (center_x + hip_width, y_toe, z_ankle))

    # Выходим из Edit Mode для сохранения скелета
    bpy.ops.object.mode_set(mode='OBJECT')
    print("[Fate Rig] Скелет успешно построен и соразмерен!")

    # 5. СВЯЗЫВАЕМ МЕШ С СУПЕР-ТОЧНЫМ СКЕЛЕТОМ (Skinning с автоматическими весами вершин)
    # Сначала снимаем выделение со всего
    bpy.ops.object.select_all(action='DESELECT')
    
    # Выделяем меш персонажа (как ведомый объект)
    mesh_obj.select_set(True)
    # Выделяем арматуру (как ведущий активный объект)
    arm_obj.select_set(True)
    bpy.context.view_layer.objects.active = arm_obj

    # Выполняем привязку с автоматическими весами костей (работает на уровне C++ в ядре Blender)
    print("[Fate Rig] Выполнение автоматической развесовки (Armature Deform with Automatic Weights)...")
    try:
        bpy.ops.object.parent_set(type='ARMATURE_AUTO')
        print("[Fate Rig] Скиннинг успешно завершен!")
    except Exception as e:
        print(f"[Fate WARNING] Автоматическая привязка не удалась: {str(e)}")
        print("Попробуйте привязать вручную: выделите Меш, зажмите Shift, выделите Скелет -> нажмите Ctrl+P -> Выберите 'With Automatic Weights'")

    # 6. СОЗДАЕМ ТЕСТОВУЮ АНИМАЦИЮ (Движения рук, ног и головы для поиска недочетов меша!)
    print("[Fate Rig] Запись тестовой цикличной анимации на таймлайн...")
    
    # Устанавливаем длину таймлайна на 80 кадров
    bpy.context.scene.frame_start = 1
    bpy.context.scene.frame_end = 80
    bpy.context.scene.frame_set(1)

    # Очищаем анимационные данные скелета, если они были
    if arm_obj.animation_data:
        arm_obj.animation_data_clear()

    # Переходим в Pose Mode для анимации костей
    bpy.ops.object.mode_set(mode='POSE')

    bones_to_animate = {
        "UpperArm.L": [
            (1, (0.0, 0.0, 0.0)),
            (20, (0.0, 0.0, -1.1)), # Поднятие левой руки вверх на 60 градусов
            (40, (0.0, 0.0, 0.2)),  # Опускание левой руки вниз
            (60, (0.0, 0.0, -0.6)), # Среднее волновое движение
            (80, (0.0, 0.0, 0.0))  # Возврат в исходную Т-позу
        ],
        "UpperArm.R": [
            (1, (0.0, 0.0, 0.0)),
            (20, (0.0, 0.0, 1.1)),  # Поднятие правой руки вверх на 60 градусов
            (40, (0.0, 0.0, -0.2)), # Опускание правой руки вниз
            (60, (0.0, 0.0, 0.6)),  # Среднее волновое движение
            (80, (0.0, 0.0, 0.0))   # Возврат в исходную Т-позу
        ],
        "Head": [
            (1, (0.0, 0.0, 0.0)),
            (20, (0.0, 0.4, 0.0)),  # Поворот головы влево
            (40, (0.0, -0.4, 0.0)), # Поворот головы вправо
            (60, (0.2, 0.0, 0.0)),  # Наклон головы вперед
            (80, (0.0, 0.0, 0.0))   # Возврат головы прямо
        ],
        "Shin.L": [
            (1, (0.0, 0.0, 0.0)),
            (20, (0.8, 0.0, 0.0)),  # Изгиб левого колена
            (40, (0.0, 0.0, 0.0)),  # Выпрямление левого колена
            (60, (0.0, 0.0, 0.0)),
            (80, (0.0, 0.0, 0.0))
        ],
        "Shin.R": [
            (1, (0.0, 0.0, 0.0)),
            (20, (0.0, 0.0, 0.0)),
            (40, (0.8, 0.0, 0.0)),  # Изгиб правого колена
            (60, (0.0, 0.0, 0.0)),  # Выпрямление правого колена
            (80, (0.0, 0.0, 0.0))
        ]
    }

    # Прописываем ключи анимации по кадрам
    for bone_name, keyframes in bones_to_animate.items():
        pose_bone = arm_obj.pose.bones.get(bone_name)
        if pose_bone:
            # Используем классический режим вращения Euler XYZ для простоты C# и скрипта
            pose_bone.rotation_mode = 'XYZ'
            
            for frame, rot_val in keyframes:
                bpy.context.scene.frame_set(frame)
                pose_bone.rotation_euler = rot_val
                # Записываем ключ вращения кости в текущем кадре
                pose_bone.keyframe_insert(data_path="rotation_euler", group=bone_name)

    # Переключаем обратно в Object Mode
    bpy.ops.object.mode_set(mode='OBJECT')
    bpy.context.scene.frame_set(1)

    print("\n=======================================================")
    print("🏆 УСПЕХ: Автоматический риггинг Fate Continent завершен!")
    print("=======================================================")
    print("Что теперь делать в Blender:")
    print("1. Нажмите клавишу Пробел (Space) или кнопку Play под таймлайном.")
    print("   -> Персонаж начнет двигать руками, ногами и головой!")
    print("2. Чтобы исправить положение костей (если меш шире/уже):")
    print("   - Выделите скелет 'FateHumanoid_Armature'.")
    print("   - Нажмите Ctrl+Tab (или выберите 'Edit Mode' в верхнем левом углу).")
    print("   - Кликните по суставу (шарику на конце кости) и подвиньте его клавишей 'G'.")
    print("3. Чтобы исправить «резиновые растяжения» подмышками (Weight Painting):")
    print("   - Сначала выделите Скелет, затем зажмите Shift и кликните по Мешу.")
    print("   - Переключитесь в режим 'Weight Paint' (верхний левый угол).")
    print("   - Зажмите Ctrl и кликните по кости UpperArm.L или UpperArm.R.")
    print("   - Выберите кисть Draw с весом (Weight) = 0 и сотрите влияние руки на ребра туловища!")
    print("4. Экспорт: выделите меш и скелет -> File -> Export -> FBX (.fbx).")
    print("=======================================================\n")
    return True

if __name__ == "__main__":
    setup_automatic_rig()
