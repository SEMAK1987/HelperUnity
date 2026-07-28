import bpy

def run_auto_binding():
    print("--- Starting Auto-Binding Script ---")
    
    # 1. Ensure we are in Object Mode
    if bpy.ops.object.mode_set.poll():
        bpy.ops.object.mode_set(mode='OBJECT')
    
    # 2. Deselect everything first
    bpy.ops.object.select_all(action='DESELECT')
    
    # 3. Find the mesh (node_0 or similar) and armature (FateHumanoid_Armature)
    mesh_obj = bpy.data.objects.get("node_0")
    arm_obj = bpy.data.objects.get("FateHumanoid_Armature")
    
    # Fallback if names are slightly different (e.g. if mesh has suffix or is the only mesh)
    if not mesh_obj:
        for obj in bpy.data.objects:
            if obj.type == 'MESH' and (obj.name.startswith("node_") or "mesh" in obj.name.lower()):
                mesh_obj = obj
                break
    if not mesh_obj:
        # Just find any mesh in the scene if node_0 wasn't found
        meshes = [obj for obj in bpy.data.objects if obj.type == 'MESH']
        if len(meshes) > 0:
            mesh_obj = meshes[0]
            
    if not arm_obj:
        # Find any armature
        armatures = [obj for obj in bpy.data.objects if obj.type == 'ARMATURE']
        if len(armatures) > 0:
            arm_obj = armatures[0]

    if not mesh_obj:
        print("ERROR: Mesh object not found! Please import or rename your character model mesh.")
        return False
    if not arm_obj:
        print("ERROR: Armature (skeleton) not found!")
        return False
        
    print(f"Using Mesh: {mesh_obj.name}")
    print(f"Using Armature: {arm_obj.name}")
    
    # 4. Clean up old parenting
    # Select mesh and clear parent
    mesh_obj.select_set(True)
    bpy.context.view_layer.objects.active = mesh_obj
    bpy.ops.object.parent_clear(type='CLEAR_KEEP_TRANSFORM')
    
    # Remove any existing Armature modifier to avoid double deformation
    arm_modifiers = [m for m in mesh_obj.modifiers if m.type == 'ARMATURE']
    for m in arm_modifiers:
        mesh_obj.modifiers.remove(m)
        print(f"Removed old armature modifier: {m.name}")
        
    # 5. Fix potential "Bone Heat Weighting" error by merging duplicate vertices
    print("Merging duplicate vertices on mesh to prevent binding errors...")
    bpy.ops.object.select_all(action='DESELECT')
    mesh_obj.select_set(True)
    bpy.context.view_layer.objects.active = mesh_obj
    
    # Switch to edit mode to merge vertices
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.remove_doubles(threshold=0.0001) # Merges double vertices
    bpy.ops.object.mode_set(mode='OBJECT')
    
    # 6. Perform the parenting with automatic weights
    bpy.ops.object.select_all(action='DESELECT')
    
    # Step 1: Select Mesh first
    mesh_obj.select_set(True)
    # Step 2: Select Armature second
    arm_obj.select_set(True)
    # Step 3: Set Armature as Active Object (Crucial for Parenting)
    bpy.context.view_layer.objects.active = arm_obj
    
    print("Applying parenting with automatic weights...")
    try:
        bpy.ops.object.parent_set(type='WITH_AUTOMATIC_WEIGHTS')
        print("SUCCESS! Mesh is now perfectly bound to the skeleton with automatic weights!")
        
        # Fix unweighted vertices to prevent Unity "ImportFBX Warnings: vertices with no weight"
        print("Checking for unweighted vertices...")
        bpy.ops.object.mode_set(mode='OBJECT')
        
        # Find a suitable root or center bone from the armature to assign unweighted vertices to
        target_bone_name = None
        for name in ["Hips", "Spine", "Pelvis", "Root", "mixamorig:Hips", "mixamorig:Spine", "Spine.001"]:
            if name in arm_obj.data.bones:
                target_bone_name = name
                break
        if not target_bone_name and len(arm_obj.data.bones) > 0:
            target_bone_name = arm_obj.data.bones[0].name
            
        default_group = None
        if target_bone_name:
            if target_bone_name in mesh_obj.vertex_groups:
                default_group = mesh_obj.vertex_groups[target_bone_name]
            else:
                default_group = mesh_obj.vertex_groups.new(name=target_bone_name)
                print(f"Created missing vertex group '{target_bone_name}' on the mesh to map to bone.")
        
        if not default_group and len(mesh_obj.vertex_groups) > 0:
            default_group = mesh_obj.vertex_groups[0]
            
        if default_group:
            unweighted_count = 0
            for v in mesh_obj.data.vertices:
                if len(v.groups) == 0:
                    default_group.add([v.index], 1.0, 'REPLACE')
                    unweighted_count += 1
            if unweighted_count > 0:
                print(f"Fixed {unweighted_count} unweighted vertices by assigning them to '{default_group.name}' vertex group!")
        else:
            print("Warning: No vertex groups or bones found to bind unweighted vertices.")
            
        print("Now you can switch to POSE MODE and rotate bones - the body will move!")
        return True
    except Exception as e:
        print(f"Error during parenting with weights: {e}")
        print("Attempting fallback: Parenting with empty groups...")
        try:
            bpy.ops.object.parent_set(type='WITH_EMPTY')
            print("Successfully parented with empty groups as fallback.")
        except Exception as ex:
            print(f"Fallback also failed: {ex}")
        return False

run_auto_binding()
