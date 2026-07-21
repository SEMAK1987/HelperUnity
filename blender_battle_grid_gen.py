# ==============================================================================
#            FATE CONTINENT - BATTLE ARENA GRID GENERATOR FOR BLENDER
# ==============================================================================
# Version: 18.12.03 (BattleScene Grid Edition)
# Description: Generates a fully adjustable tactical battle grid of rounded squares
#             with custom team-colored materials and central pedestals.
#             Features an interactive N-Panel UI with real-time viewport updates.
#             Perfect for pathfinding design in BattleScene!
# ==============================================================================

bl_info = {
    "name": "Battle Arena Grid Generator",
    "author": "Zenith World Architect v18.12.03",
    "version": (18, 12, 3),
    "blender": (2, 80, 0),
    "location": "View3D > N-Panel > Battle Grid",
    "description": "Generates real-time customizable battle arenas with rounded tiles.",
    "warning": "",
    "doc_url": "",
    "category": "Mesh",
}

import bpy
import bmesh
import math

# Callback to automatically rebuild grid on slider tweak
def trigger_rebuild(self, context):
    if context.scene.battle_grid_props.auto_update:
        bpy.ops.battle_grid.generate('EXEC_DEFAULT')

class BattleGridProperties(bpy.types.PropertyGroup):
    rows: bpy.props.IntProperty(
        name="Rows (Up/Down)",
        description="Number of vertical rows in the combat grid",
        default=3,
        min=1,
        max=30,
        update=trigger_rebuild
    )
    cols: bpy.props.IntProperty(
        name="Columns (Left/Right)",
        description="Number of horizontal columns in the combat grid",
        default=4,
        min=1,
        max=30,
        update=trigger_rebuild
    )
    tile_size: bpy.props.FloatProperty(
        name="Tile Size",
        description="Length of each square tile side",
        default=1.5,
        min=0.2,
        max=5.0,
        update=trigger_rebuild
    )
    spacing: bpy.props.FloatProperty(
        name="Spacing",
        description="Gap distance between adjacent tiles",
        default=0.2,
        min=0.0,
        max=2.0,
        update=trigger_rebuild
    )
    corner_radius: bpy.props.FloatProperty(
        name="Corner Roundness",
        description="Bevel radius of tile corners",
        default=0.3,
        min=0.0,
        max=1.0,
        update=trigger_rebuild
    )
    thickness: bpy.props.FloatProperty(
        name="Thickness",
        description="Extrusion height of the base tiles",
        default=0.2,
        min=0.02,
        max=2.0,
        update=trigger_rebuild
    )
    pedestal_width: bpy.props.FloatProperty(
        name="Pedestal Width",
        description="Width of the center selector peg",
        default=0.25,
        min=0.05,
        max=1.0,
        update=trigger_rebuild
    )
    pedestal_height: bpy.props.FloatProperty(
        name="Pedestal Height",
        description="Height of the center selector peg",
        default=0.15,
        min=0.0,
        max=1.0,
        update=trigger_rebuild
    )
    auto_update: bpy.props.BoolProperty(
        name="Auto-Update Viewport",
        description="Instantly rebuild grid upon tweaking parameters",
        default=True
    )

class BATTLE_GRID_OT_Generate(bpy.types.Operator):
    bl_idname = "battle_grid.generate"
    bl_label = "Generate Battle Grid"
    bl_description = "Procedurally spawn the tactical battle grid"
    bl_options = {'REGISTER', 'UNDO'}

    def get_or_create_material(self, name, r, g, b, emission_strength=0.0):
        mat = bpy.data.materials.get(name)
        if mat is None:
            mat = bpy.data.materials.new(name=name)
            mat.use_nodes = True
            nodes = mat.node_tree.nodes
            principled = nodes.get("Principled BSDF")
            if principled:
                principled.inputs["Base Color"].default_value = (r, g, b, 1.0)
                principled.inputs["Roughness"].default_value = 0.4
                if emission_strength > 0.0:
                    # Support for emission glow
                    if "Emission Color" in principled.inputs:
                        principled.inputs["Emission Color"].default_value = (r, g, b, 1.0)
                    elif "Emission" in principled.inputs:
                        principled.inputs["Emission"].default_value = (r, g, b, 1.0)
                    
                    if "Emission Strength" in principled.inputs:
                        principled.inputs["Emission Strength"].default_value = emission_strength
        return mat

    def execute(self, context):
        props = context.scene.battle_grid_props
        
        # 1. Clean up or create dedicated collection
        col_name = "Battle_Arena_Grid"
        collection = bpy.data.collections.get(col_name)
        if collection:
            # Safely remove old objects in this collection to avoid duplicating memory
            for obj in list(collection.objects):
                bpy.data.objects.remove(obj, do_unlink=True)
        else:
            collection = bpy.data.collections.new(col_name)
            bpy.context.scene.collection.children.link(collection)

        # 2. Setup colors matching user's reference image
        # Left columns are player spawn zones (Blue), right are enemy zones (Red), neutral are grey/green
        mat_blue = self.get_or_create_material("Mat_Grid_Blue", 0.08, 0.45, 0.95, 0.3)
        mat_red = self.get_or_create_material("Mat_Grid_Red", 0.85, 0.12, 0.18, 0.3)
        mat_green = self.get_or_create_material("Mat_Grid_Green", 0.12, 0.65, 0.28, 0.1)
        mat_grey = self.get_or_create_material("Mat_Grid_Grey", 0.32, 0.38, 0.45, 0.0)
        mat_pedestal = self.get_or_create_material("Mat_Grid_Pedestal", 0.22, 0.26, 0.30, 0.0)

        # Calculate offsets to center the grid around (0, 0, 0) in Blender
        total_w = props.cols * props.tile_size + (props.cols - 1) * props.spacing
        total_d = props.rows * props.tile_size + (props.rows - 1) * props.spacing
        start_x = -total_w / 2.0 + props.tile_size / 2.0
        start_y = -total_d / 2.0 + props.tile_size / 2.0

        for r in range(props.rows):
            for c in range(props.cols):
                # Calculate coordinates
                posX = start_x + c * (props.tile_size + props.spacing)
                posY = start_y + r * (props.tile_size + props.spacing)
                
                cell_name = f"Grid_Cell_{r:02d}_{c:02d}"
                
                # Create a rounded base tile
                bpy.ops.mesh.primitive_cube_add(
                    size=1.0, 
                    location=(posX, posY, props.thickness / 2.0)
                )
                tile_obj = bpy.context.active_object
                tile_obj.name = cell_name
                
                # Apply size transformations
                tile_obj.scale = (props.tile_size, props.tile_size, props.thickness)
                bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
                
                # Bevel vertical corners using bmesh
                me = tile_obj.data
                bm = bmesh.new()
                bm.from_mesh(me)
                
                # Filter vertical edges
                vertical_edges = []
                for edge in bm.edges:
                    v1, v2 = edge.verts
                    if abs(v1.co.x - v2.co.x) < 0.001 and abs(v1.co.y - v2.co.y) < 0.001:
                        vertical_edges.append(edge)
                
                if vertical_edges and props.corner_radius > 0.0:
                    # Apply bevel radius bounded to half tile size
                    safe_radius = min(props.corner_radius, props.tile_size / 2.1)
                    bmesh.ops.bevel(bm, geom=vertical_edges, offset=safe_radius, segments=6, affect='EDGES')
                
                bm.to_mesh(me)
                bm.free()
                
                # Assign material based on position (similar to tactical map)
                # Leftmost columns: Blue; Rightmost: Red; Top/bottom center: Green; Center default: Grey
                if props.cols >= 3:
                    if c == 0:
                        tile_obj.data.materials.append(mat_blue)
                    elif c == props.cols - 1:
                        tile_obj.data.materials.append(mat_red)
                    elif r in (0, props.rows - 1) and c in (1, props.cols - 2):
                        tile_obj.data.materials.append(mat_green)
                    else:
                        tile_obj.data.materials.append(mat_grey)
                else:
                    # Simple alternate
                    if c % 2 == 0:
                        tile_obj.data.materials.append(mat_blue)
                    else:
                        tile_obj.data.materials.append(mat_red)

                # Move base tile into the custom grid collection
                for col in tile_obj.users_collection:
                    col.objects.unlink(tile_obj)
                collection.objects.link(tile_obj)

                # Create center pedestal
                if props.pedestal_height > 0.0:
                    ped_z = props.thickness + props.pedestal_height / 2.0
                    bpy.ops.mesh.primitive_cube_add(
                        size=1.0,
                        location=(posX, posY, ped_z)
                    )
                    ped_obj = bpy.context.active_object
                    ped_obj.name = f"{cell_name}_Pedestal"
                    
                    # Styled elongated pedestal matching user's image (slightly stretched on Y axis)
                    ped_obj.scale = (props.pedestal_width, props.pedestal_width * 1.4, props.pedestal_height)
                    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
                    
                    # Bevel top edges of the pedestal for smoothness
                    bm_p = bmesh.new()
                    bm_p.from_mesh(ped_obj.data)
                    top_edges = []
                    for edge in bm_p.edges:
                        # Top horizontal edges
                        v1, v2 = edge.verts
                        if abs(v1.co.z - v2.co.z) < 0.001 and v1.co.z > 0.0:
                            top_edges.append(edge)
                    if top_edges:
                        bmesh.ops.bevel(bm_p, geom=top_edges, offset=0.03, segments=3, affect='EDGES')
                    bm_p.to_mesh(ped_obj.data)
                    bm_p.free()
                    
                    ped_obj.data.materials.append(mat_pedestal)
                    
                    # Move pedestal to grid collection
                    for col in ped_obj.users_collection:
                        col.objects.unlink(ped_obj)
                    collection.objects.link(ped_obj)
                    
                    # Parent pedestal to base tile to keep hierarchy clean and solid
                    ped_obj.parent = tile_obj
                    ped_obj.matrix_parent_inverse = tile_obj.matrix_world.inverted()

                # Add a Bevel Modifier for glossy edge highlights (Unity/Blender game look)
                bev_mod = tile_obj.modifiers.new(name="TileBevel", type='BEVEL')
                bev_mod.width = 0.02
                bev_mod.segments = 2
                
                # Make smooth shaded
                tile_obj.select_set(True)
                bpy.context.view_layer.objects.active = tile_obj
                bpy.ops.object.shade_smooth()

        self.report({'INFO'}, f"Successfully generated {props.rows}x{props.cols} Battle Grid!")
        return {'FINISHED'}

class BATTLE_GRID_PT_Panel(bpy.types.Panel):
    bl_label = "Battle Grid Architect"
    bl_idname = "BATTLE_GRID_PT_Panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = 'Battle Grid'

    def draw(self, context):
        layout = self.layout
        props = context.scene.battle_grid_props
        
        box = layout.box()
        box.label(text="GRID DIMENSIONS", icon='GRID')
        col = box.column(align=True)
        col.prop(props, "rows")
        col.prop(props, "cols")
        
        box = layout.box()
        box.label(text="TILE DESIGN", icon='MESH_CUBE')
        col = box.column(align=True)
        col.prop(props, "tile_size")
        col.prop(props, "spacing")
        col.prop(props, "corner_radius")
        col.prop(props, "thickness")
        
        box = layout.box()
        box.label(text="PEDESTAL SETTINGS", icon='CONE')
        col = box.column(align=True)
        col.prop(props, "pedestal_width")
        col.prop(props, "pedestal_height")
        
        layout.separator()
        layout.prop(props, "auto_update")
        
        # Hard trigger button
        layout.operator("battle_grid.generate", icon='PLAY', text="REBUILD GRID NOW")
        
        layout.separator()
        layout.label(text="Unity BattleScene-Ready", icon='CHECKMARK')

classes = (
    BattleGridProperties,
    BATTLE_GRID_OT_Generate,
    BATTLE_GRID_PT_Panel,
)

def register():
    for cls in classes:
        bpy.utils.register_class(cls)
    bpy.types.Scene.battle_grid_props = bpy.props.PointerProperty(type=BattleGridProperties)

def unregister():
    for cls in reversed(classes):
        bpy.utils.unregister_class(cls)
    del bpy.types.Scene.battle_grid_props

if __name__ == "__main__":
    register()
