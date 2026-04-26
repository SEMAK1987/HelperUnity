import bpy
import bmesh
import random
import math

# Project: Continent of Fate - Omniversal Divine Master v17.18.1
# Description: Automated generation of 4 continents with 12 races, heroes, and units.
# Includes: Relief generator, Castle levels (1-5), Roads, Cells, Heroes & Skills.

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
    
def create_road(start, end, race):
    # Create a path between points
    bpy.ops.curve.primitive_bezier_curve_add(enter_editmode=False, align='WORLD', location=(0, 0, 0))
    road = bpy.context.active_object
    road.name = f"Road_{race}_{start.name}_{end.name}"
    
    # Simple straight line road
    road.data.splines[0].bezier_points[0].co = start.location
    road.data.splines[0].bezier_points[1].co = end.location
    
    # Material
    mat = bpy.data.materials.new(name=f"Mat_Road_{race}")
    mat.diffuse_color = (0.2, 0.2, 0.2, 1.0) # Default dark road
    road.data.materials.append(mat)
    return road

def create_castle(name, location, race, level):
    # placeholder for castle levels 1-5
    size = 1 + level * 0.3
    bpy.ops.mesh.primitive_cube_add(size=size, location=location)
    castle = bpy.context.active_object
    castle.name = f"Castle_{race}_{name}_Lvl{level}"
    
    # Add towers for higher levels
    if level > 3:
        for i in range(4):
            angle = (i/4) * math.pi * 2
            t_loc = (location[0] + math.cos(angle)*size, location[1] + math.sin(angle)*size, location[2] + level*0.5)
            bpy.ops.mesh.primitive_cylinder_add(radius=0.4, depth=level, location=t_loc)
            tower = bpy.context.active_object
            tower.parent = castle
    
    mat = bpy.data.materials.get(f"Mat_Race_{race}")
    if not mat:
        mat = bpy.data.materials.new(name=f"Mat_Race_{race}")
        mat.diffuse_color = (random.random(), random.random(), random.random(), 1.0)
    castle.data.materials.append(mat)
    return castle

def create_hero(name, location, race, hero_class, is_main=False):
    # hero_class: 'Warrior', 'Archer', 'Mage'
    size = 0.8 if is_main else 0.6
    if hero_class == 'Warrior':
        bpy.ops.mesh.primitive_monkey_add(size=size, location=location)
    elif hero_class == 'Archer':
        bpy.ops.mesh.primitive_cone_add(radius1=0.4*size, depth=size*1.5, location=location)
    else: # Mage
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.5*size, location=location)
        
    hero = bpy.context.active_object
    hero.name = f"Hero_{race}_{hero_class}_{'Main' if is_main else 'Secondary'}_{name}"
    
    # Skill Metadata (Simulated via Custom Properties)
    hero["skill_1"] = "Common_Strike"
    hero["skill_2"] = "Common_Block"
    hero["skill_3"] = "Common_Heal"
    hero["skill_ult"] = f"{hero_class}_Ultimate"
    hero["skill_passive"] = f"{race}_Passive"
    
    return hero

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
            
            # Create main castle (Level 1-5 example)
            lvl = random.randint(1, 5)
            capital = create_castle("Capital", race_loc, race, lvl)
            
            # Create Heroes (Main + Secondaries)
            create_hero("Leader", (race_loc[0] + 1, race_loc[1], 1), race, "Warrior", True)
            create_hero("Vizier", (race_loc[0], race_loc[1] + 1, 1), race, "Mage", False)
            create_hero("Sentinel", (race_loc[0] - 1, race_loc[1], 1), race, "Archer", False)
            
            # Add simple roads between race hubs
            if i > 0:
                prev_angle = ((i-1) / 3) * math.pi * 2
                prev_offset_x = math.cos(prev_angle) * (c_size / 4)
                prev_offset_y = math.sin(prev_angle) * (c_size / 4)
                prev_loc = (c_loc[0] + prev_offset_x, c_loc[1] + prev_offset_y, 1)
                
                # Mock object for line
                bpy.ops.object.empty_add(location=prev_loc)
                prev_obj = bpy.context.active_object
                create_road(prev_obj, capital, race)
            
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

def create_procedural_relic(name, type_relic):
    # type_relic: "Orb", "Cube", "Totem"
    if type_relic == "Orb":
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.5)
    elif type_relic == "Cube":
        bpy.ops.mesh.primitive_cube_add(size=0.8)
    else:
        bpy.ops.mesh.primitive_cylinder_add(radius=0.3, depth=1.5)
        
    relic = bpy.context.active_object
    relic.name = f"Relic_{name}_{type_relic}"
    
    # Add neon effect
    mat = bpy.data.materials.new(name=f"Mat_Relic_{name}")
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    bsdf = nodes["Principled BSDF"]
    bsdf.inputs[19].default_value = (0, 1, 1, 1) # Emission color
    bsdf.inputs[20].default_value = 10.0 # Emission strength
    relic.data.materials.append(mat)
    return relic

# Execution
if __name__ == "__main__":
    setup_world()
