import bpy

def optimize_character_for_mixamo():
    """
    Разработчик: Fate Continent (Континент Судьбы) • Версия v18.7.4
    Скрипт для Blender 4.x / 5.x, автоматически подготавливающий AI-модели (Meshy, Tripo3D, CSM)
    к импорту и безошибочному риггингу на Mixamo.
    
    Что делает скрипт:
    1. Объединяет все разрозненные меши (доспехи, плащ, тело) в один цельный объект.
    2. Поворачивает персонажа лицом к экрану (устраняет разворот спиной в Unity).
    3. Выполняет сварку вершин (Merge Doubles/Weld) - главная причина сбоев Mixamo.
    4. Применяет модификатор Decimate для снижения веса сетки до оптимального для ИИ.
    5. Применяет все трансформации (Apply Location, Rotation, Scale).
    """
    print("=== ЗАПУСК ОПТИМИЗАЦИИ ГЕРОЯ ДЛЯ MIXAMO ===")
    
    # 1. Переключаемся в объектный режим
    if bpy.ops.object.mode_set.poll():
        bpy.ops.object.mode_set(mode='OBJECT')
        
    # Снимаем выделение со всего
    bpy.ops.object.select_all(action='DESELECT')
    
    # 2. Находим все полигональные меши в сцене
    mesh_objects = [obj for obj in bpy.data.objects if obj.type == 'MESH']
    
    if not mesh_objects:
        print("ОШИБКА: В сцене не найдено полигональных моделей (Mesh)!")
        return
        
    # Выделяем все меши
    for obj in mesh_objects:
        obj.select_set(True)
        
    # Делаем один из них активным
    bpy.context.view_layer.objects.active = mesh_objects[0]
    active_obj = bpy.context.active_object
    
    # 3. Объединяем (Join) все разрозненные куски в один меш
    if len(mesh_objects) > 1:
        print(f"Объединяем {len(mesh_objects)} объектов в один...")
        bpy.ops.object.join()
    else:
        print("Модель уже состоит из одного объекта.")

    # 4. Поворачиваем персонажа лицом к фронтальной камере Blender (Фронт = -Y)
    # Если в Unity модель стоит задом (-180 Y), в Blender ее нужно развернуть нужным образом.
    print("Выравниваем ориентацию персонажа лицом на передний план...")
    # Сбрасываем позицию в центр сцены
    active_obj.location = (0, 0, 0)
    
    # Убираем вращения и выставляем лицом к нам (по оси Y вперед)
    active_obj.rotation_euler = (0, 0, 0)
    # Если лицо все еще смотрит назад, разворачиваем на 180 по оси Z
    active_obj.rotation_euler[2] = 3.14159  # 180 градусов в радианах
    
    # Сбрасываем масштаб в 1.0
    active_obj.scale = (1, 1, 1)

    # Применяем трансформации (это запекает углы и размеры прямо в вершины)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

    # 5. Очистка геометрии и сварка разорванных AI-вершин (Remove Doubles)
    print("Выполняем сварку вершин (Welding)...")
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    # Сливаем вершины, находящиеся ближе чем 0.001м друг к другу
    bpy.ops.mesh.remove_doubles(threshold=0.001)
    bpy.ops.object.mode_set(mode='OBJECT')

    # 6. Оптимизация количества полигонов (Decimation)
    # AI-генераторы часто создают избыточную сетку (>150k полигонов), что крашит Mixamo.
    poly_count = len(active_obj.data.polygons)
    print(f"Исходное число полигонов: {poly_count}")
    
    if poly_count > 60000:
        print("Модель слишком тяжелая. Применяем Decimate для оптимизации...")
        decimate_ratio = 50000.0 / poly_count
        
        # Добавляем модификатор Decimate (Упрощение сетки)
        dec_mod = active_obj.modifiers.new(name="MixamoDecimate", type='DECIMATE')
        dec_mod.ratio = decimate_ratio
        
        # Применяем модификатор
        bpy.ops.object.modifier_apply(modifier="MixamoDecimate")
        print(f"Новое число полигонов: {len(active_obj.data.polygons)}")
    else:
        print("Сетка находится в оптимальных пределах. Пропускаем Decimate.")

    print("=== ОПТИМИЗАЦИЯ УСПЕШНО ЗАВЕРШЕНА! ===")
    print("Инструкция:")
    print("1. Нажмите File -> Export -> FBX.")
    print("2. В настройках экспорта (справа) выберите: 'Apply Scalings: FBX All'!")
    print("3. В разделе 'Path Mode' установите 'Copy' и нажмите на иконку коробки (Embed Textures), чтобы запечь текстуру внутрь файла.")
    print("4. Загрузите полученный .fbx файл в Mixamo - он сриггится без ошибок за 1 минуту!")

# Запуск
if __name__ == "__main__":
    optimize_character_for_mixamo()
