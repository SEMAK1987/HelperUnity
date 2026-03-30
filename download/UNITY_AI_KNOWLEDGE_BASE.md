# Unity AI Knowledge Base

This file contains the core knowledge for the Unity & Blender AI Assistant.

## Project Overview
The `unity-blender-ai-assistant-full` project is designed to bridge the gap between 3D modeling in Blender and game logic in Unity.

## Core Features
- **Automated C# Generation**: Generate Unity scripts for AI, movement, and UI.
- **Blender Python Automation**: Create custom Blender tools for asset export and procedural generation.
- **AI-Driven Workflows**: Use LLMs (like Gemini) to describe game mechanics and get ready-to-use code.

## Key Files
- `BaseAIController.cs`: The core AI logic for all mobs.
- `BlenderExporter.py`: Automates the export of models with correct Unity settings.
- `KnowledgeBase.json`: Stores project-specific metadata and AI instructions.

## Integration
- **Blender to Unity**: Assets are exported with metadata that Unity scripts can read.
- **Unity AI**: Mobs are automatically assigned skills from `ALL_MOBS_SKILLS.md`.
