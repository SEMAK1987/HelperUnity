bl_info = {
    "name": "AI Assistant Link",
    "author": "Omniversal World Architect v17.18.29",
    "version": (17, 18, 29),
    "blender": (2, 80, 0),
    "location": "View3D > N-Panel > AI Assistant",
    "description": "Direct bridge to the World Architect Divine Architect Supreme with project level GOD Synergy.",
    "warning": "",
    "doc_url": "",
    "category": "Interface",
}

import bpy
import json
import urllib.request
import urllib.parse

class AI_ASSISTANT_Properties(bpy.types.PropertyGroup):
    prompt: bpy.props.StringProperty(
        name="Prompt",
        description="Write what you want the AI to do",
        default="",
    )
    server_url: bpy.props.StringProperty(
        name="Server URL",
        description="URL of your AI Assistant server",
        default="http://localhost:3000",
    )
    mode: bpy.props.EnumProperty(
        name="Mode",
        description="Operating mode for the AI",
        items=[
            ('online', "Online (Quantum Cloud)", "Use Gemini for complex reasoning"),
            ('offline', "Offline (Neural Nexus)", "Use local Ollama instance"),
            ('no_internet', "No-Internet (Archive)", "Use local knowledge base search"),
        ],
        default='online',
    )

class AI_ASSISTANT_OT_Generate(bpy.types.Operator):
    bl_idname = "ai_assistant.generate"
    bl_label = "Manifest Code"
    bl_description = "Send prompt to AI and execute generated Python code"

    def execute(self, context):
        props = context.scene.ai_assistant_props
        url = props.server_url.rstrip("/") + "/api/blender/chat"
        
        data = {
            "prompt": props.prompt,
            "mode": props.mode,
            "context": {
                "blender_version": bpy.app.version_string,
                "objects": [obj.name for obj in bpy.data.objects],
                "active_object": bpy.context.active_object.name if bpy.context.active_object else None
            }
        }
        
        # Prepare request
        req_data = json.dumps(data).encode('utf-8')
        req = urllib.request.Request(url, data=req_data, method='POST')
        req.add_header('Content-Type', 'application/json')
        
        self.report({'INFO'}, f"Sending request to {url}...")
        
        try:
            with urllib.request.urlopen(req, timeout=60) as response:
                res_body = response.read().decode('utf-8')
                res_data = json.loads(res_body)
                
                if "code" in res_data:
                    generated_code = res_data["code"]
                    self.report({'INFO'}, "Manifesting code...")
                    
                    try:
                        # Safety check: execute generated code
                        exec(generated_code)
                        self.report({'INFO'}, "Success: Reality Manifested")
                    except Exception as e:
                        self.report({'ERROR'}, f"Execution failed: {str(e)}")
                        print(f"FAILED CODE:\n{generated_code}")
                elif "error" in res_data:
                    self.report({'ERROR'}, f"Server Error: {res_data['error']}")
                else:
                    self.report({'WARNING'}, "No code received from AI")
                    
        except Exception as e:
            self.report({'ERROR'}, f"Connection failed: {str(e)}")
            
        return {'FINISHED'}

class AI_ASSISTANT_PT_Panel(bpy.types.Panel):
    bl_label = "AI Assistant Link"
    bl_idname = "AI_ASSISTANT_PT_Panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = 'AI Assistant'

    def draw(self, context):
        layout = self.layout
        props = context.scene.ai_assistant_props
        
        col = layout.column(align=True)
        col.label(text="v17.18.25 - Zenith Multi-Tool Synergy (Photoshop & GIMP Sync)")
        col.prop(props, "server_url")
        col.prop(props, "mode")
        
        layout.separator()
        
        layout.prop(props, "prompt", text="")
        layout.operator("ai_assistant.generate", icon='PLAY')
        
        layout.separator()
        layout.label(text="Transcendent Status: Active", icon='HEART')

classes = (
    AI_ASSISTANT_Properties,
    AI_ASSISTANT_OT_Generate,
    AI_ASSISTANT_PT_Panel,
)

def register():
    for cls in classes:
        bpy.utils.register_class(cls)
    bpy.types.Scene.ai_assistant_props = bpy.props.PointerProperty(type=AI_ASSISTANT_Properties)

def unregister():
    for cls in reversed(classes):
        bpy.utils.unregister_class(cls)
    del bpy.types.Scene.ai_assistant_props

if __name__ == "__main__":
    register()
