import bpy
import bmesh
import random
import math

# Project: Continent of Fate - Omniversal Divine Master v17.16.0
# Description: Automated generation of 4 continents with 12 races, heroes, and units.
# Includes: Relief generator, Castle levels, Battle zones, Procedural Weapons.

def clear_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete()

def create_continent(name, location, size, r_color):
    # Create a base mesh for the continent
    bpy.ops.mesh.primitive_plane_add(size=size, location=location)
    cont = bpy.context.active_object
    cont.name = f"Continent_{name}"
    
    # Material
    mat = bpy.data.materials.new(name=f"Mat_{name}")
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    nodes["Principled BSDF"].inputs[0].default_value = r_color
    cont.data.materials.append(mat)
    
    # Add Displacement for Relief (Landscape Architect Module)
    bpy.ops.object.modifier_add(type='DISPLACE')
    disp = cont.modifiers["Displace"]
    tex = bpy.data.textures.new(f"Noise_{name}", type='CLOUDS')
    tex.noise_scale = 1.5
    disp.texture = tex
    disp.strength = 5.0 # Height of mountains

    # Add cells (grid)
    create_grid(cont, 10)
    return cont

def create_grid(parent, divisions):
    bpy.ops.mesh.primitive_grid_add(size=parent.dimensions.x, x_subdivisions=divisions, y_subdivisions=divisions, location=parent.location)
    grid = bpy.context.active_object
    grid.name = f"Grid_{parent.name}"
    grid.parent = parent
    grid.display_type = 'WIRE'
    
def create_castle(name, location, race, level):
    # Simple placeholder for castle levels
    bpy.ops.mesh.primitive_cube_add(size=1 + level*0.2, location=location)
    castle = bpy.context.active_object
    castle.name = f"Castle_{race}_{name}_Lvl{level}"
    
    # Material based on race
    mat = bpy.data.materials.get(f"Mat_Race_{race}")
    if not mat:
        mat = bpy.data.materials.new(name=f"Mat_Race_{race}")
        mat.diffuse_color = (random.random(), random.random(), random.random(), 1.0)
    castle.data.materials.append(mat)
    return castle

def create_unit(name, location, race, type_u):
    # type_u: 'warrior', 'archer', 'mage'
    if type_u == 'warrior':
        bpy.ops.mesh.primitive_monkey_add(size=0.5, location=location)
    elif type_u == 'archer':
        bpy.ops.mesh.primitive_cone_add(radius1=0.3, depth=1, location=location)
    else: # mage
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.4, location=location)
        
    unit = bpy.context.active_object
    unit.name = f"Unit_{race}_{type_u}_{name}"
    return unit

def setup_world():
    clear_scene()
    
    continents_info = [
        ("Northern_Frost", (-20, 20, 0), 30, (0.8, 0.9, 1.0, 1.0)),
        ("Southern_Sand", (20, -20, 0), 30, (0.9, 0.7, 0.3, 1.0)),
        ("Eastern_Forest", (20, 20, 0), 30, (0.2, 0.5, 0.1, 1.0)),
        ("Western_Iron", (-20, -20, 0), 30, (0.4, 0.4, 0.4, 1.0))
    ]
    
    races = {
        "Northern_Frost": ["Orcs", "Dwarves", "Elementals"],
        "Southern_Sand": ["Humans", "Naga", "Demons"],
        "Eastern_Forest": ["Elves", "Fairies", "Centaurs"],
        "Western_Iron": ["Undead", "Cyborgs", "Goblins"]
    }
    
    for c_name, c_loc, c_size, c_color in continents_info:
        cont = create_continent(c_name, c_loc, c_size, c_color)
        
        # Place 3 races per continent
        continent_races = races[c_name]
        for i, race in enumerate(continent_races):
            # Calculate position for race hub
            angle = (i / 3) * math.pi * 2
            offset_x = math.cos(angle) * (c_size / 4)
            offset_y = math.sin(angle) * (c_size / 4)
            race_loc = (c_loc[0] + offset_x, c_loc[1] + offset_y, 1)
            
            # Create main castle (Level 1)
            create_castle("Capital", race_loc, race, 1)
            
            # Create Heroes
            create_unit("MainHero", (race_loc[0] + 1, race_loc[1], 1), race, "warrior")
            create_unit("Mage", (race_loc[0], race_loc[1] + 1, 1), race, "mage")
            create_unit("Archer", (race_loc[0] - 1, race_loc[1], 1), race, "archer")
            
            # Add some units
            for j in range(3):
                create_unit(f"Troop_{j}", (race_loc[0] + random.uniform(-2,2), race_loc[1] + random.uniform(-2,2), 1), race, "warrior")

    # Add Bandit camps
    for i in range(10):
        loc = (random.uniform(-40, 40), random.uniform(-40, 40), 0.5)
        create_unit(f"Bandit_{i}", loc, "Bandits", "warrior")

    export_layout()
    print("World Architect: 4 Continents Manifested Successfully.")

def export_layout():
    import json
    import os
    
    data = {"objects": []}
    for obj in bpy.data.objects:
        if "Continent" in obj.name: continue # Skip continents for simple bridge
        
        # Determine type
        o_type = "Unit"
        if "Castle" in obj.name: o_type = "Castle"
        
        # Determine race (hacked from name)
        race = "Neutral"
        parts = obj.name.split('_')
        if len(parts) > 1:
            race = parts[1]
            
        data["objects"].append({
            "name": obj.name,
            "type": o_type,
            "position": {
                "x": obj.location.x,
                "y": obj.location.y,
                "z": obj.location.z
            },
            "race": race
        })
    
    # Save to file
    filepath = os.path.join(bpy.path.abspath("//"), "world_layout.json")
    with open(filepath, 'w') as f:
        json.dump(data, f, indent=4)
    print(f"Layout exported to: {filepath}")

def create_procedural_weapon(name, rarity):
    # rarity: "Common", "Rare", "Epic", "Legendary"
    bpy.ops.mesh.primitive_cube_add(size=1)
    weapon = bpy.context.active_object
    weapon.name = f"Weapon_{name}_{rarity}"
    
    # Scale based on rarity
    scale_factor = 1.0
    if rarity == "Rare": scale_factor = 1.2
    elif rarity == "Epic": scale_factor = 1.5
    elif rarity == "Legendary": scale_factor = 2.0
    
    weapon.scale = (0.1, 0.1, scale_factor)
    
    # Add material based on rarity
    mat = bpy.data.materials.new(name=f"Mat_{rarity}")
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    color = (0.5, 0.5, 0.5, 1) # Gray for common
    if rarity == "Rare": color = (0, 0.5, 1, 1) # Blue
    elif rarity == "Epic": color = (0.5, 0, 1, 1) # Purple
    elif rarity == "Legendary": color = (1, 0.8, 0, 1) # Gold
    
    nodes["Principled BSDF"].inputs[0].default_value = color
    weapon.data.materials.append(mat)
    return weapon

# Execution
if __name__ == "__main__":
    setup_world()
