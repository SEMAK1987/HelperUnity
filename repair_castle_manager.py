import re

def main():
    filepath = "src/FateCastleManager.cs"
    print(f"Reading {filepath}...")
    with open(filepath, "r", encoding="utf-8", errors="replace") as f:
        content = f.read()

    # Find start marker
    start_marker = 'string skillTypeLabel = "";\n            if (hoveredSkillType == "Ultimate")\n            {'
    start_idx = content.find(start_marker)
    if start_idx == -1:
        # Try with slightly different whitespace
        start_marker = 'string skillTypeLabel = "";'
        start_idx = content.find(start_marker)
        if start_idx == -1:
            print("Error: Could not find start marker")
            return
    
    # Find the labelStyle line which is after the start marker
    end_marker = "labelStyle.alignment = TextAnchor.MiddleLeft;"
    end_idx = content.find(end_marker, start_idx)
    if end_idx == -1:
        print("Error: Could not find end marker")
        return

    # We want to replace everything from the start_marker (inclusive) to the end_marker (exclusive)
    print(f"Replacing chunk from index {start_idx} to {end_idx}...")

    replacement = """string skillTypeLabel = "";
            if (hoveredSkillType == "Ultimate")
            {
                skillTypeLabel = (curLang == 0 ? "⚡ СУПЕРНАВЫК" : "⚡ ULTIMATE");
            }
            else if (hoveredSkillType == "Passive")
            {
                skillTypeLabel = (curLang == 0 ? "🛡️ ПАССИВНЫЙ" : "🛡️ PASSIVE");
            }
            else
            {
                skillTypeLabel = (curLang == 0 ? "⚔️ АКТИВНЫЙ" : "⚔️ ACTIVE");
            }
            GUI.Label(new Rect(tooltipX + 10, tooltipY + 28, tooltipWidth - 20, 16), skillTypeLabel, hoverTypeStyle);
            
            // Icon
            if (hoveredSkillIcon != null)
            {
                GUI.DrawTexture(new Rect(tooltipX + (tooltipWidth - 48f) / 2f, tooltipY + 48f, 48f, 48f), hoveredSkillIcon, ScaleMode.ScaleToFit);
            }
            
            // Description
            GUIStyle hoverDescStyle = new GUIStyle(GUI.skin.label);
            hoverDescStyle.fontSize = 11;
            hoverDescStyle.wordWrap = true;
            hoverDescStyle.normal.textColor = Color.white;
            hoverDescStyle.alignment = TextAnchor.UpperLeft;
            
            GUI.Label(new Rect(tooltipX + 10, tooltipY + 102, tooltipWidth - 20, tooltipHeight - 110), hoveredSkillDesc, hoverDescStyle);
        }
    }

    private void DrawStatRow(int curLang, string icon, string nameText, ref int statVal, ref int availablePoints, int minVal)
    {
        GUILayout.BeginHorizontal();
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        """

    new_content = content[:start_idx] + replacement + content[end_idx:]

    with open(filepath, "w", encoding="utf-8", errors="replace") as f:
        f.write(new_content)
    print("Successfully repaired.")

if __name__ == "__main__":
    main()
