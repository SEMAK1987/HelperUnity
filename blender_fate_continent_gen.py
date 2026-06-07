# ==============================================================================
#            FATE CONTINENT - 3D TACTICAL WORLD GENERATOR FOR BLENDER
# ==============================================================================
# Version: 18.11.7 (Tactical Playable Sectors Edition)
# Description: Generates 18 discrete, beautiful, thick 3D tactical sector tiles
#             matching the user's annotated boundary sketch.
#             - Completely removes all castle towers, trees, rocks, and circles.
#             - Divides Snowy Island into 4 separate sectors.
#             - Divides Main Continent into 14 separate sectors.
#             - Scales each sector down slightly (by 0.965) to leave crisp visual 
#               dividing lines ("разлиновка") between adjacent parts.
#             - Adds vertical thickness (depth) to each tile using Solidify
#               to prevent "objects flying in void" (thin flat sheets look bad).
#             - Automatically sets up 5 game-ready stylized flat-shaded materials.
# ==============================================================================

import bpy
import bmesh
import math

def delete_default_scene_objects():
    """Cleans up default scene objects for a fresh, professional generation."""
    bpy.ops.object.select_all(action='DESELECT')
    for name in ["Cube", "Camera", "Light"]:
        obj = bpy.data.objects.get(name)
        if obj:
            obj.select_set(True)
    bpy.ops.object.delete()

def get_or_create_material(name, color_rgb, roughness=1.0, metallic=0.0):
    """Creates a stylized, matte, low-poly material for bright vivid colors in Unity."""
    mat = bpy.data.materials.get(name)
    if mat is None:
        mat = bpy.data.materials.new(name=name)
        mat.use_nodes = True
        nodes = mat.node_tree.nodes
        principled = nodes.get("Principled BSDF")
        if principled:
            principled.inputs["Base Color"].default_value = color_rgb
            principled.inputs["Roughness"].default_value = roughness
            principled.inputs["Metallic"].default_value = metallic
    return mat

def get_warp(x, y):
    """Slight organic coordinate warping for natural-looking jagged sector borders."""
    wx = x + 0.4 * math.sin(0.7 * y) + 0.25 * math.cos(1.5 * x)
    wy = y + 0.4 * math.sin(0.7 * x) + 0.25 * math.cos(1.5 * y)
    return wx, wy

def get_land_fade(wx, wy):
    """
    Evaluates land presence and returns shoreline fade factor (0.0 to 1.0).
    Uses a union of overlapping geographic ellipsoids corresponding to the drawing.
    """
    # 1. Snowy Island (Top Left)
    fade_snow = 1.0 - (((wx - (-13.0)) / 4.8)**2 + ((wy - 8.5) / 2.8)**2)
    
    # 2. Main Continent Core and Regions
    fade_forest = 1.0 - (((wx - 0.5) / 4.5)**2 + ((wy - 1.5) / 4.0)**2)
    fade_crimson = 1.0 - (((wx - (-6.0)) / 4.0)**2 + ((wy - (-1.0)) / 3.8)**2)
    fade_crimson_n = 1.0 - (((wx - (-5.0)) / 2.5)**2 + ((wy - 3.5) / 2.5)**2)
    fade_ruins = 1.0 - (((wx - 8.0) / 4.0)**2 + ((wy - 2.5) / 3.2)**2)
    fade_ruins_n = 1.0 - (((wx - 9.0) / 2.5)**2 + ((wy - 5.5) / 2.5)**2)
    fade_desert_w = 1.0 - (((wx - 3.0) / 3.5)**2 + ((wy - (-6.5)) / 2.8)**2)
    fade_desert_c = 1.0 - (((wx - 8.5) / 4.5)**2 + ((wy - (-6.0)) / 3.2)**2)
    fade_desert_e = 1.0 - (((wx - 13.0) / 3.0)**2 + ((wy - (-5.5)) / 2.2)**2)
    
    # 3. Transitions / Bridges connecting regions into one solid landmass
    fade_bridge_1 = 1.0 - (((wx - 2.0) / 3.0)**2 + ((wy - (-2.5)) / 3.5)**2) # Center to Desert
    fade_bridge_2 = 1.0 - (((wx - 4.5) / 3.0)**2 + ((wy - 0.5) / 3.0)**2)  # Center to Ruins
    fade_bridge_3 = 1.0 - (((wx - (-2.5)) / 3.5)**2 + ((wy - 1.0) / 3.0)**2) # Center to Crimson
    
    max_fade = max(
        fade_snow, fade_forest, fade_crimson, fade_crimson_n, 
        fade_ruins, fade_ruins_n, fade_desert_w, fade_desert_c, 
        fade_desert_e, fade_bridge_1, fade_bridge_2, fade_bridge_3
    )
    
    if max_fade > 0.0:
        return True, min(1.0, max_fade * 5.0)
    return False, 0.0

# Define 18 Tactical Sector Centers with customized heights and materials
# Derived exactly from the hand-drawn dividing lines of the annotated image
sectors_def = [
    # 1. NORTHERN SNOWY ISLAND (4 sectors)
    {"id": "Sector_Snowy_West", "center": (-15.5, 9.5), "category": "ice_peak", "peak_h": 2.8},
    {"id": "Sector_Snowy_MidNorth", "center": (-13.0, 10.5), "category": "ice_peak", "peak_h": 2.4},
    {"id": "Sector_Snowy_MidSouth", "center": (-13.0, 7.5), "category": "ice_peak", "peak_h": 2.2},
    {"id": "Sector_Snowy_East", "center": (-10.5, 8.5), "category": "ice_peak", "peak_h": 1.9},
    
    # 2. CRIMSON WASTES (3 sectors)
    {"id": "Sector_Wastes_North", "center": (-6.0, 3.5), "category": "crimson_wastes", "peak_h": 2.4},
    {"id": "Sector_Wastes_Mid", "center": (-7.5, -1.0), "category": "crimson_wastes", "peak_h": 2.2},
    {"id": "Sector_Wastes_South", "center": (-6.5, -5.5), "category": "crimson_wastes", "peak_h": 2.0},
    
    # 3. FOREST KEEP (4 sectors)
    {"id": "Sector_Forest_North", "center": (-1.0, 4.5), "category": "forest_keep", "peak_h": 1.6},
    {"id": "Sector_Forest_Center", "center": (1.0, 1.0), "category": "forest_keep", "peak_h": 1.4},
    {"id": "Sector_Forest_East", "center": (3.0, 3.0), "category": "forest_keep", "peak_h": 1.5},
    {"id": "Sector_Forest_South", "center": (1.0, -3.0), "category": "forest_keep", "peak_h": 1.2},
    
    # 4. ANCIENT RUINS (3 sectors)
    {"id": "Sector_Ruins_North", "center": (6.5, 5.5), "category": "ancient_ruins", "peak_h": 2.6},
    {"id": "Sector_Ruins_Center", "center": (7.5, 1.5), "category": "ancient_ruins", "peak_h": 2.0},
    {"id": "Sector_Ruins_East", "center": (11.0, 2.0), "category": "ancient_ruins", "peak_h": 1.8},
    
    # 5. SOUTHERN DESERT (4 sectors)
    {"id": "Sector_Desert_West", "center": (3.0, -7.0), "category": "southern_desert", "peak_h": 1.0},
    {"id": "Sector_Desert_Center", "center": (7.0, -6.5), "category": "southern_desert", "peak_h": 1.1},
    {"id": "Sector_Desert_East", "center": (11.0, -5.0), "category": "southern_desert", "peak_h": 1.2},
    {"id": "Sector_Desert_FarEast", "center": (14.0, -6.0), "category": "southern_desert", "peak_h": 1.0},
]

def build_fate_continents():
    print("[World Architect] Initializing custom procedural generator...")
    
    # Create colors matching the user's high-contrast medieval fantasy layout
    materials_config = {
        "ice_peak": get_or_create_material("Mat_Ice_Peak", (0.85, 0.92, 0.98, 1.0)),       # Frosty White
        "crimson_wastes": get_or_create_material("Mat_Crimson_Wastes", (0.72, 0.28, 0.22, 1.0)), # Rocky Red-Orange
        "forest_keep": get_or_create_material("Mat_Forest_Keep", (0.12, 0.48, 0.22, 1.0)),     # Lush Green
        "ancient_ruins": get_or_create_material("Mat_Ancient_Ruins", (0.45, 0.46, 0.42, 1.0)),   # Slate Grey Stones
        "southern_desert": get_or_create_material("Mat_Southern_Desert", (0.88, 0.68, 0.38, 1.0)) # Warm Sandy Yellow
    }
    
    # Create dedicated collection in Outliner
    group_col = bpy.data.collections.get("Fate_Continent_Meshes")
    if group_col is None:
        group_col = bpy.data.collections.new("Fate_Continent_Meshes")
        bpy.context.scene.collection.children.link(group_col)
        
    print(f"[World Architect] Starting assembly of {len(sectors_def)} individual tactical puzzle tiles:")
    
    for sector in sectors_def:
        sec_id = sector["id"]
        cx, cy = sector["center"]
        category = sector["category"]
        peak_h = sector["peak_h"]
        
        print(f" -> Creating {sec_id} at ({cx}, {cy})...")
        
        # Spawn a finely detailed grid centered at the sector center
        bpy.ops.mesh.primitive_grid_add(
            x_subdivisions=35,
            y_subdivisions=35,
            size=9.0, # Generous padding to cleanly merge border cuts
            location=(cx, cy, 0.0)
        )
        
        obj = bpy.context.active_object
        obj.name = sec_id
        mesh = obj.data
        
        marked_for_deletion = set()
        
        # Sculpt vertices using land geometry masks and Voronoi assignment
        for vert in mesh.vertices:
            # Transition local coords to world coordinates
            wx = vert.co.x + cx
            wy = vert.co.y + cy
            
            # 1. Evaluate if this point falls inside the natural shorelines
            is_land, shore_fade = get_land_fade(wx, wy)
            
            if not is_land:
                marked_for_deletion.add(vert.index)
                continue
                
            # 2. Check closest sector using warped organic Voronoi coordinates
            warped_x, warped_y = get_warp(wx, wy)
            
            closest_sec = None
            min_dist = 99999.0
            
            for s_check in sectors_def:
                sc_x, sc_y = s_check["center"]
                dist = math.sqrt((warped_x - sc_x)**2 + (warped_y - sc_y)**2)
                if dist < min_dist:
                    min_dist = dist
                    closest_sec = s_check["id"]
            
            # If this coordinate belongs to another sector, delete it here
            if closest_sec != sec_id:
                marked_for_deletion.add(vert.index)
            else:
                # Calculate beautiful mountain elevation falling nicely at the edge
                c_dist = math.sqrt((wx - cx)**2 + (wy - cy)**2)
                domed_factor = max(0.0, 1.0 - (c_dist / 4.8))
                
                # Dynamic stylized landscape noise
                noise_1 = 0.35 * math.sin(wx * 0.9) * math.cos(wy * 0.9)
                noise_2 = 0.15 * math.sin(wx * 2.2)
                
                height = (0.2 + (peak_h - 0.2) * (domed_factor ** 1.3)) * shore_fade + (noise_1 + noise_2) * domed_factor
                vert.co.z = max(0.12, height)
                
        # Clean vertices using modern BMesh operations to prevent layout issues
        bm = bmesh.new()
        bm.from_mesh(mesh)
        bm.verts.ensure_lookup_table()
        
        to_delete = [bm.verts[v_idx] for v_idx in marked_for_deletion if v_idx < len(bm.verts)]
        bmesh.ops.delete(bm, geom=to_delete, context='VERTS')
        
        # Normal recalculation
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        bm.to_mesh(mesh)
        bm.free()
        mesh.update()
        
        # Ensure a clean mesh is left, otherwise skip solidifying
        if len(mesh.vertices) == 0:
            print(f"      [!] Warning: Sector {sec_id} generated empty. Deleting object.")
            bpy.data.objects.remove(obj, do_unlink=True)
            continue
            
        # Give the playing sector solid, premium 3D thickness (No thin floating paper!)
        solid_mod = obj.modifiers.new(name="Solidify_Sector", type='SOLIDIFY')
        solid_mod.thickness = 1.0
        solid_mod.offset = -1.0
        solid_mod.use_even_thickness = True
        
        # Apply Solidify Modifier to commit physical coordinate structure
        bpy.ops.object.modifier_apply(modifier="Solidify_Sector")
        
        # CREATE PLAYABLE SEPARATION GAPS ("разлиновка")
        # Shift origin point to sector geographic center for beautiful balanced scaling
        bpy.ops.object.origin_set(type='ORIGIN_GEOMETRY', center='BOUNDS')
        
        # Scale X/Y down by 3.5% to create crisp visual borders inside Unity 6
        obj.scale.x = 0.965
        obj.scale.y = 0.965
        obj.scale.z = 1.0
        bpy.ops.object.transform_apply(scale=True)
        
        # Clean up materials and apply the colorful stylized preset
        obj.data.materials.clear()
        obj.data.materials.append(materials_config[category])
        
        # Link to our main collection and unlink from active defaults
        bpy.context.collection.objects.unlink(obj)
        group_col.objects.link(obj)

    print("[World Architect] All 18 sectors generated, solidify applied, and scaled successfully!")

if __name__ == "__main__":
    delete_default_scene_objects()
    build_fate_continents()
    print("==============================================================================")
    print("SUCCESS: Full 18-Sector Playable Jigsaw Continent Generator Complete!")
    print("- Created exactly 18 standalone physical 3D sector objects.")
    # No more system coordinates or console port numbers to maintain artistic purity
    print("- Set up beautiful, glossy-free matte colors on Unity-ready materials.")
    print("==============================================================================")
