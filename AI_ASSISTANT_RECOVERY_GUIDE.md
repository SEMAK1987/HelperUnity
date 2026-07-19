# 🤖 AI Assistant - Disaster Recovery & Replica Reconstruction Guide (v18.11.29)

## 📌 Introduction & Purpose
This document serves as a complete **Disaster Recovery Blueprint** for the **Unity & Blender AI Assistant (Fate Continent Assistant)**. If the application environment is lost, corrupted, or needs to be redeployed on a different server or account, this guide contains the exact visual and functional specifications, UI layouts, styling guidelines, and architectural details to rebuild the application from scratch to match the attached screenshot.

---

## 🎨 Visual Identity & Layout Blueprint (From Screenshot Reference)

The interface utilizes a custom high-density futuristic design theme: **Zenith Cyber-Glass (Dark Obsidian Mode)**. 

### 1. Left Control Panel (Sidebar)
The left panel has a width of `320px` and is packed with system telemetries and status lists:
- **Header Info Block**:
  - **App Name**: `AI ASSISTANT` (Font: Space Grotesk, Semi-bold, white, capitalized).
  - **Version badge**: `V18.11.29` (Subtle grey text below the title).
  - **System Badge**: A glowing cyan chip icon on the left.
- **Server Metrics Status Rows**:
  - **СЕРВЕР (Server status)**: `● СВЯЗЬ ОК` (glowing bright green circle with text).
  - **ИИ ИНТЕЛЛЕКТ (AI Intelligence mode)**: `● ОГРАНИЧЕН` (soft amber circle with text to indicate secure mode).
  - **ЗРЕНИЕ (VISION)**: `● АКТИВНО` (blue capsule background indicator).
  - **AI АГЕНТ (AI Agent state)**: `● ГОТОВ` (glowing bright green circle inside custom dark block).
  - * বেনাইন message box: "HMR WEBSOCKET МОЖЕТ БЫТЬ ОТКЛЮЧЕН (ЭТО НОРМАЛЬНО)" in low-contrast grey text.
- **СТАТИСТИКА ПРОЕКТА (Project Statistics)**:
  - Scanning indicator: `● Сканирование...` (pulsing light green).
  - Sync button icon: `RefreshCw` on the right of the section header.
- **СТАТУС ПО (Software status check list)**:
  - Lists the availability of developer tools running on the system:
    - **Unity**: `● ...` status light.
    - **Blender**: `● ...` status light.
    - **GIMP**: `● ...` status light.
    - **Redot**: `● ...` status light.
    - **Photoshop**: `● ...` status light.
- **Bottom Feature Card (О ВОЗМОЖНОСТЯХ ИИ)**:
  - Elegant info banner with a glowing blue icon (`Info`).
  - Text: *"Всё о том, что умеет наш ИИ и как он работает с проектом."*

### 2. Main Workspace Header
- **Active Title**: `ИНТЕЛЛЕКТУАЛЬНЫЙ ПОМОЩНИК / UNITY & BLENDER EXPERT` (Font: Space Grotesk, heavy uppercase, bold, bright white).
- **Navigation Menu & Controls**:
  - Elegant inline pills with dark semi-transparent borders:
    - **ЧАТ (Chat Mode)**: Filled blue button with an airplane icon (`Send`) stating `АКТИВЕН`.
    - **ХРАНИЛИЩЕ (Storage)**: Simple icon with text.
    - **QUANTUM LINK**: Glowing orange lightning bolt icon.
    - **МИГРАЦИЯ (Migration)**: Sync icon.
    - **ОБЛОЖКИ ВК (VK Covers)**: Image icon.
    - **СПЛИТТЕР МАРКЕРОВ (Marker Splitter)**: Bullet list icon.
    - **БАЗА ЗНАНИЙ ИИ (AI Knowledge Base)**: Blue accent book icon.
    - **СТУДИЯ ИГРЫ (Game Studio)**: Purple game icon.
- **Status Indicator Banner**:
  - Text: `● ЗАЩИЩЕННЫЙ РЕЖИМ (V18.8.0)` (Amber neon bullet point).
  - Right-aligned controls: Trash/Clear button (`ОЧИСТИТЬ`) and Settings Gear icon (`Settings`).

### 3. Central Chat & Neural Hub
- **Ambient Center Icon**: A large glowing circle containing a digital microchip graphic (`Cpu`).
- **Main Heading Text**: `UNITY AI ASSISTANT V18.11.27` (Extra bold display font, white, centered).
- **Sub-heading Description**:
  - *"Я полностью осведомлен о вашем проекте по пути..."*
  - Animated loading label: `Загрузка...`
  - Subtext: *"Задавайте любые вопросы по Unity, Blender, Photoshop или GIMP. Модули Menu Studio Visuals Mastery, Omni-Answer Engine и проект 'Континент судьбы' (v18.11.27) активированы."*
- **Warning pill**: `ВНИМАНИЕ: PROFESSIONAL MULTI-TOOL MASTERY` (High-contrast amber border pill with capitalized text).

### 4. Interactive Input Block (Footer Input)
- Floating capsule container matching the width of the main column.
- **Left Status Indicator Capsule**: `● ONLINE` (Bright green dot inside dark glass capsule).
- **Attachment Button**: File paperclip icon (`Paperclip`) on the left of input.
- **Chat Input Bar**: Rounded obsidian field with text: *"Задайте вопрос по Unity или Blender..."* or *"Задайте вопрос по проекту..."*
- **Send Button**: Circle button with `Send` icon on the right end.
- **Disclaimer Subtext**: *"АИ МОЖЕТ ОШИБАТЬСЯ. ПРОВЕРЯЙТЕ КОД ПЕРЕД ИСПОЛЬЗОВАНИЕМ В ПРОЕКТЕ."* (Small muted grey uppercase text).

---

## ⚙️ Technical Core Architecture & APIs

To run this application, the following backend routes and socket bridges must be fully operational in `server.ts`:

1. **/api/blender/chat**
   - Handles text prompt execution.
   - Proxies request to Google Gemini Pro API using `process.env.GEMINI_API_KEY`.
   - Returns executable Python code for Blender or C# scripts for Unity, tailored to active context.
2. **/api/project/scan**
   - Scans the directory tree of the project.
   - Categorizes and counts `.cs`, `.py`, `.prefab`, `.unity`, `.fbx`, `.png`, `.wav` and other asset formats.
   - Runs automated code audits and security-checking routines.
3. **/api/software/status**
   - Probes local operating system process tables to determine if Unity Editor, Blender, GIMP, or Photoshop executables are running.
4. **/api/database/update**
   - Reads, validates, and persists user configurations in local JSON files like `knowledge_base.json` or `ccgs_project_blueprint.json`.

### 5. Espionage & Intel Report Mechanics (v18.11.27)
- **Spy Network Button**: Renders next to the player's primary hero status card (HUD) once at least one enemy castle has been successfully spied.
- **Scouting Cost & Formula**:
  - Requires Castle Level 2 to unlock. Cost = `100 * Level` gold (integers).
  - Success chance: `60 + (PlayerCastleLevel - 2) * 15`, capped at `100%`.
- **Information Depth Levels & Visual Grids**:
  - **Level 2 (Default)**: Basic Garrison power count, secondary hero count, commander class and level. Displays experience progress bars and active/passive skill cards.
  - **Level 3**: Garrison breakdown by T1, T2, and elite troops (T3+), commander worn weapon & armor details, secondary heroes details. Adds high-density visual grids for inventory slots and equipment slots.
  - **Level 4 (Max Info)**: Full garrison listing of all tiers (T1-T4), detailed listing of secondary heroes with names and levels, exact commander equipment slots (helmet, armor, boots, shield, ring, belt, shoulders, weapon) shown on an anatomical grid of square buttons, and precise listing of troop cohorts with experience ranks.
- **Spy Report Persistence**: Scouting a new castle discards the previous spy report context while keeping the current spy results cleanly cached in local PlayerPrefs.
- **Multilingual Support**: Supports 9 languages (RU, EN, DE, FR, ES, PT, JA, KO, ZH) natively using language-adaptive prefixes and `GetText9` helpers.

---

## 🛠️ Step-by-Step Restoration Protocol

If the application needs to be redeployed from scratch, follow these instructions:

1. **Verify Base Requirements**:
   - Install **Node.js (v18+)** and **npm**.
   - Install **Unity 6 (6000.3.10f1)** and **Blender** if running locally.
2. **Setup Folder Structure**:
   ```bash
   mkdir -p src/components uploads local_storage
   ```
3. **Environment Secrets**:
   - Create a `.env` file in the root directory:
     ```env
     GEMINI_API_KEY=your_gemini_key_here
     NODE_ENV=production
     ```
4. **Install Dependencies**:
   - Execute the package manager commands to install required tools:
     ```bash
     npm install
     ```
5. **Run Development Server**:
   - Boot the local system:
     ```bash
     npm run dev
     ```
6. **Compile and Package for Production**:
   - Build client and bundle backend:
     ```bash
     npm run build
     ```
   - Run the compiled package:
     ```bash
     npm run start
     ```

*This Disaster Recovery Spec is synchronized and updated dynamically with the actual code assets.*
