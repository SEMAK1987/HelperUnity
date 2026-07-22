# ==============================================================================
#      FATE CONTINENT - AUTOMATED TEXTURE BAKE & UV UNWRAP FOR BLENDER 5.1.1
# ==============================================================================
# Version: v18.12.05 (One-Click Bake System)
# Description: This script fully automates the complex baking process:
#             1. Automatically switches render engine to Cycles & GPU.
#             2. Selects all cells inside the "Battle_Arena_Grid" collection.
#             3. Performs non-overlapping Smart UV Projection with custom margin.
#             4. Instantiates and selects the bake target node in ALL tile materials.
#             5. Runs the diffuse texture bake and saves "Grid_Bake_Color.png".
# ==============================================================================

import bpy
import os

def run_automated_bake():
    print("--- Starting Automated Battle Grid Bake ---")
    
    # 1. Switch Render Engine to Cycles
    scene = bpy.context.scene
    scene.render.engine = 'CYCLES'
    
    # Enable GPU computing if available
    try:
        preferences = bpy.context.preferences
        cycles_preferences = preferences.addons['cycles'].preferences
        cycles_preferences.compute_device_type = 'CUDA' # or 'METAL' / 'OPTIX'
        scene.cycles.device = 'GPU'
        print("Cycles switched to GPU compute mode.")
    except Exception as e:
        scene.cycles.device = 'CPU'
        print("GPU not available, baking using CPU.")

    # 2. Find or Create the target bake texture
    image_name = "Grid_Bake_Color"
    image_width = 2048
    image_height = 2048
    
    if image_name in bpy.data.images:
        image = bpy.data.images[image_name]
        print(f"Using existing image: {image_name}")
    else:
        image = bpy.data.images.new(name=image_name, width=image_width, height=image_height, alpha=False)
        print(f"Created new image texture: {image_name}")

    # 3. Target Collection Verification
    collection_name = "Battle_Arena_Grid"
    grid_collection = bpy.data.collections.get(collection_name)
    if not grid_collection:
        print(f"ERROR: Collection '{collection_name}' not found!")
        return
        
    # Deselect all objects first
    bpy.ops.object.select_all(action='DESELECT')
    
    # Select only grid cells
    cells = []
    for obj in grid_collection.objects:
        if obj.type == 'MESH' and obj.name.startswith("Grid_Cell_"):
            obj.select_set(True)
            cells.append(obj)
            
    if not cells:
        print("ERROR: No 'Grid_Cell_XX_YY' meshes found in the collection!")
        return
        
    # Make the first cell active for operator execution context
    bpy.context.view_layer.objects.active = cells[0]
    print(f"Selected {len(cells)} grid cells for UV Unwrapping and Baking.")

    # 4. Automate non-overlapping Smart UV Unwrapping
    print("Executing Smart UV Project...")
    # Enter Edit Mode
    bpy.ops.object.mode_set(mode='EDIT')
    # Select all geometry
    bpy.ops.mesh.select_all(action='SELECT')
    # Perform Smart UV Project with 0.02 Island Margin to prevent bleeding
    bpy.ops.uv.smart_project(island_margin=0.02)
    # Exit Edit Mode back to Object Mode
    bpy.ops.object.mode_set(mode='OBJECT')
    print("UV Unwrapping completed successfully with custom margin 0.02.")

    # 5. Inject and Select Image Texture Node in all associated materials
    # We collect all unique materials used by our cells
    materials = set()
    for cell in cells:
        for slot in cell.material_slots:
            if slot.material:
                materials.add(slot.material)
                
    print(f"Found {len(materials)} unique materials. Preparing node trees...")
    
    for mat in materials:
        mat.use_nodes = True
        nodes = mat.node_tree.nodes
        
        # Check if the automated node already exists to avoid clutter
        bake_node = nodes.get("Grid_Bake_Texture")
        if not bake_node:
            bake_node = nodes.new(type='ShaderNodeTexImage')
            bake_node.name = "Grid_Bake_Texture"
            bake_node.label = "Grid_Bake_Texture"
            
        # Assign our baking image
        bake_node.image = image
        
        # VERY CRITICAL: Make this node active in the material tree!
        nodes.active = bake_node
        print(f"Assigned and selected bake target node in material: {mat.name}")

    # 6. Execute Cycles Diffuse Bake
    print("Initiating Cycles Diffuse Bake... Please wait.")
    
    # Force samples to 1 to speed up baking 100x and prevent CPU freezes
    scene.cycles.samples = 1
    
    # Configure bake settings
    scene.cycles.bake_type = 'DIFFUSE'
    scene.render.bake.use_pass_direct = False
    scene.render.bake.use_pass_indirect = False
    scene.render.bake.use_pass_color = True # Only bake raw color
    scene.render.bake.margin = 4 # Pixel margin to prevent seams
    
    # Run bake
    bpy.ops.object.bake(type='DIFFUSE')
    print("Bake completed successfully!")

    # 7. Save baked image to project root or default desktop path
    blend_file_path = bpy.data.filepath
    if blend_file_path:
        save_dir = os.path.dirname(blend_file_path)
    else:
        save_dir = os.path.expanduser("~/Desktop")
        
    save_path = os.path.join(save_dir, "Grid_Bake_Color.png")
    
    # Save the image file
    image.filepath_raw = save_path
    image.file_format = 'PNG'
    image.save()
    
    print(f"SUCCESS: Baked texture saved to: {save_path}")
    print("You can now safely drag 'Grid_Bake_Color.png' into your Unity project Assets folder!")

# Run the automatic bake
if __name__ == "__main__":
    run_automated_bake()
