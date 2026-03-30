import express from "express";
import { createServer as createViteServer } from "vite";
import path from "path";
import fs from "fs-extra";
import cors from "cors";
import dotenv from "dotenv";
import multer from "multer";

dotenv.config();

// Configure storage for uploads
const storage = multer.diskStorage({
  destination: (req, file, cb) => {
    const uploadDir = path.join(process.cwd(), "uploads");
    fs.ensureDirSync(uploadDir);
    cb(null, uploadDir);
  },
  filename: (req, file, cb) => {
    cb(null, `${Date.now()}-${file.originalname}`);
  }
});

const upload = multer({ 
  storage,
  limits: {
    fileSize: 500 * 1024 * 1024 // 500MB max per file (highest limit)
  }
});

async function startServer() {
  const app = express();
  const PORT = process.env.PORT || 3000; // Dynamic port: 3000 for cloud, overrideable for local

  app.use(cors());
  app.use(express.json());

  const kbPath = path.join(process.cwd(), "knowledge_base.json");
  const blueprintPath = path.join(process.cwd(), "ccgs_project_blueprint.json");
  const providedApiKey = "AIzaSyAp7Y-PCxX_qPBzpOB_Dn4dvIacjRa2R4M";

  const updateBlueprint = async () => {
    try {
      const kb = await fs.readJson(kbPath);
      const metadata = await fs.readJson(path.join(process.cwd(), "metadata.json"));
      const pkg = await fs.readJson(path.join(process.cwd(), "package.json"));
      
      const blueprint = {
        project_name: metadata.name,
        description: metadata.description,
        version: kb.version,
        dependencies: pkg.dependencies,
        interface_structure: {
          tabs: ["studio", "kb", "commands", "files"],
          sidebar: "Mini Sidebar with quick access to tabs and status indicators",
          top_bar: "Project name, API key status, Hierarchy button, Update button",
          right_sidebar: "Unity status and System logs"
        },
        agents_count: kb.levels.reduce((acc: number, l: any) => acc + l.agents.length, 0),
        knowledge_base: kb,
        last_updated: new Date().toISOString(),
        recovery_instructions: "To restore, create a new project and replace knowledge_base.json and metadata.json with the data from this file."
      };
      
      await fs.writeJson(blueprintPath, blueprint, { spaces: 2 });
      console.log("[CCGS] Project blueprint updated.");
    } catch (error) {
      console.error("[CCGS] Failed to update blueprint:", error);
    }
  };

  // Ensure KB exists
  if (!(await fs.pathExists(kbPath))) {
    await fs.writeJson(kbPath, { 
      version: "0.3.0",
      levels: [
        {
          id: 1,
          name: "Management",
          agents: [
            { id: "cd1", name: "Creative Director", role: "Vision & Strategy", model: "Gemini 3.1 Pro" }
          ]
        }
      ], 
      commands: [
        { cmd: "/build", desc: "Build Unity Project" },
        { cmd: "/test", desc: "Run Unit Tests" }
      ], 
      unity_status: { is_running: false, version: "unknown" } 
    });
  }

  // File Upload Endpoint
  app.post("/api/upload", upload.array("files", 10), (req, res) => {
    try {
      const files = req.files as Express.Multer.File[];
      if (!files || files.length === 0) {
        return res.status(400).json({ error: "No files uploaded" });
      }

      // Validation logic (simplified for demo, usually done in middleware)
      const results = files.map(f => ({
        name: f.originalname,
        size: f.size,
        type: f.mimetype,
        path: f.path
      }));

      res.json({ success: true, files: results });
      updateBlueprint(); // Update blueprint after upload
    } catch (error) {
      res.status(500).json({ error: "Upload failed" });
    }
  });

  app.get("/api/kb", async (req, res) => {
    try {
      const data = await fs.readJson(kbPath);
      res.json(data);
    } catch (error) {
      res.status(500).json({ error: "Failed to read knowledge base" });
    }
  });

  app.post("/api/kb", async (req, res) => {
    try {
      await fs.writeJson(kbPath, req.body, { spaces: 2 });
      await updateBlueprint();
      res.json({ success: true });
    } catch (error) {
      res.status(500).json({ error: "Failed to save knowledge base" });
    }
  });

  app.get("/api/config", (req, res) => {
    res.json({
      apiKey: process.env.GEMINI_API_KEY || providedApiKey,
      version: "0.3.0",
      appUrl: process.env.APP_URL,
      port: 3001
    });
  });

  // Recovery Endpoint
  app.post("/api/recovery", async (req, res) => {
    try {
      const { blueprint } = req.body;
      if (!blueprint || !blueprint.knowledge_base) {
        return res.status(400).json({ error: "Invalid blueprint data" });
      }

      await fs.writeJson(kbPath, blueprint.knowledge_base, { spaces: 2 });
      await fs.writeJson(path.join(process.cwd(), "metadata.json"), {
        name: blueprint.project_name,
        description: blueprint.description,
        requestFramePermissions: []
      }, { spaces: 2 });

      res.json({ success: true });
    } catch (error) {
      res.status(500).json({ error: "Recovery failed" });
    }
  });

  // Unity Detection
  app.get("/api/unity/status", async (req, res) => {
    // Check if unity_version.txt exists (created by start_studio.bat)
    const versionPath = path.join(process.cwd(), "unity_version.txt");
    let isRunning = false;
    let version = "unknown";
    
    if (await fs.pathExists(versionPath)) {
      version = (await fs.readFile(versionPath, "utf-8")).trim();
      isRunning = true;
    } else {
      // Fallback for demo
      isRunning = Math.random() > 0.5;
      version = isRunning ? "2022.3.62f2" : "unknown";
    }
    
    const kb = await fs.readJson(kbPath);
    const oldStatus = JSON.stringify(kb.unity_status);
    kb.unity_status = { is_running: isRunning, version, project_path: "C:/Projects/MyGame" };
    
    if (oldStatus !== JSON.stringify(kb.unity_status)) {
      await fs.writeJson(kbPath, kb);
      await updateBlueprint();
    }
    
    res.json(kb.unity_status);
  });

  app.get("/api/download/ccgs", (req, res) => {
    res.setHeader('Content-Type', 'application/zip');
    res.setHeader('Content-Disposition', `attachment; filename=CCGS_Studio_v0.3.0.zip`);
    res.send(Buffer.from("Mock CCGS Studio Archive Content"));
  });

  // Vite middleware
  if (process.env.NODE_ENV !== "production") {
    const vite = await createViteServer({
      server: { middlewareMode: true },
      appType: "spa",
    });
    app.use(vite.middlewares);
  } else {
    const distPath = path.join(process.cwd(), "dist");
    app.use(express.static(distPath));
    app.get("*", (req, res) => {
      res.sendFile(path.join(distPath, "index.html"));
    });
  }

  app.listen(PORT, "0.0.0.0", () => {
    console.log(`Server running on http://localhost:${PORT}`);
  });
}

startServer();
