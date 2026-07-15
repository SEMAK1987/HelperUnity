def main():
    filepath = "src/FateCastleManager.cs"
    print(f"Reading {filepath}...")
    with open(filepath, "r", encoding="utf-8") as f:
        content = f.read()

    # Find the start of GetAICommanderItem
    target_start = "private InventoryItem GetAICommanderItem("
    start_idx = content.find(target_start)
    if start_idx == -1:
        # try with extra spaces
        target_start = "private InventoryItem GetAICommanderItem"
        start_idx = content.find(target_start)

    if start_idx == -1:
        print("Error: Could not find start of GetAICommanderItem")
        return

    # Find the start of SpyReportWindowFunction
    target_end = "private void SpyReportWindowFunction(int windowID)"
    match_end = content.find(target_end)
    if match_end == -1:
        target_end = "void SpyReportWindowFunction(int windowID)"
        match_end = content.find(target_end)
    
    if match_end == -1:
        print("Error: Could not find start of SpyReportWindowFunction")
        return

    print(f"Replacing chunk from index {start_idx} to {match_end}...")

    # The new closed GetAICommanderItem, slot drawing, and DrawSpyReportInterior opening
    replacement_code = """private InventoryItem GetAICommanderItem(CastleInstance castle, string cmdClass, int slotType)
     {
         InventoryItem item = new InventoryItem();
         item.slotType = slotType;
         item.level = castle.aiArmorTier; // Tier 1-4
         item.count = 1;
         item.statBonus = castle.aiArmorTier * 3;
         
         bool isArcher = cmdClass.ToLower().Contains("archer") || cmdClass.ToLower().Contains("strelok") || cmdClass.ToLower().Contains("ranger");
         bool isMage = cmdClass.ToLower().Contains("mage") || cmdClass.ToLower().Contains("wizard") || cmdClass.ToLower().Contains("mag");
         bool isWarrior = !isArcher && !isMage;

         switch (slotType)
         {
             case 1: // Helmet
                 item.id = "ai_helmet";
                 item.name = isArcher ? GetText9("Кожаный Капюшон", "Leather Hood", "Lederkapuze", "Capuche de cuir", "Capucha de cuero", "Capuz de Couro", "フード", "가죽 후드", "皮质软皮兜帽") :
                             isMage ? GetText9("Диадема Мудреца", "Sage Diadem", "Diadem", "Diadème de sage", "Diadema de sabio", "Diadema de Sábio", "知性の王冠", "현자의 다이아뎀", "睿智学者法冠") :
                                      GetText9("Стальной Шлем", "Steel Greathelm", "Stahlhelm", "Heaume d'acier", "Yelmo de acero", "Elmo de Aço", "グレートヘルム", "강철 그ейтхельм", "十字军精钢重盔");
                 break;
             case 2: // Amulet
                 item.id = "ai_amulet";
                 item.name = GetText9("Рунический Амулет", "Runic Amulet", "Runenamulett", "Amulette runique", "Amuleto rúnico", "Amuleto Rúnico", "ルーンアミュレット", "룬 아뮬렛", "远古卢恩护身符");
                 break;
             case 3: // Pauldrons
                 item.id = "ai_pauldrons";
                 item.name = isArcher ? GetText9("Кожаные Наплечники", "Leather Pauldrons", "Lederschultern", "Épaulières de cuir", "Hombreras de cuero", "Ombreiras de Couro", "レザーショルダー", "가죽 어깨보호대", "轻便游侠护肩") :
                             isMage ? GetText9("Вышитые Манжеты", "Embroidered Mantle", "Stickmantel", "Mantelet brodé", "Manto bordado", "Manto Bordado", "刺繍のマント", "자수 멘틀", "魔法刺绣披肩") :
                                      GetText9("Латы Охранника", "Heavy Pauldrons", "Plattenschultern", "Spallières de plaques", "Hombreras de placas", "Ombreiras de Placa", "プレートショルダー", "중갑 어깨보호대", "圣殿十字军肩铠");
                 break;
             case 4: // Armor
                 item.id = "ai_armor";
                 if (castle.aiArmorTier == 1)
                     item.name = GetText9("Кожаный Нагрудник", "Leather Chest", "Lederbrust", "Plastron de cuir", "Peto de cuero", "Colete de Couro", "レザーチェスト", "가죽 갑옷", "皮质防具护胸");
                 else if (castle.aiArmorTier == 2)
                     item.name = GetText9("Кольчужный Доспех", "Chainmail", "Kettenhemd", "Cotte de mailles", "Cota de malla", "Cota de Malha", "チェインメイル", "사슬 갑옷", "精木重铠锁子甲");
                 else if (castle.aiArmorTier == 3)
                     item.name = GetText9("Латы Рыцаря", "Knight Plate", "Plattenrüstung", "Harnois de plaques", "Armadura de placas", "Armadura de Placa", "プレートアーマー", "판금 갑옷", "骑士精钢板甲");
                 else
                     item.name = GetText9("Броня Небожителя", "Celestial Armor", "Himmlische Rüstung", "Armure Céleste", "Armadura Celestial", "Armadura Celestial", "セレスティアルアーマー", "천상 갑옷", "神界金缕玉衣");
                 break;
             case 5: // Ring
                 item.id = "ai_ring";
                 item.name = GetText9("Золотое Кольцо", "Gold Ring", "Goldring", "Bague d'or", "Anillo de oro", "Anel de Ouro", "金の指輪", "황금 반지", "镶金至尊魔戒");
                 break;
             case 6: // Belt
                 item.id = "ai_belt";
                 item.name = GetText9("Усиленный Пояс", "Reinforced Belt", "Gürtel", "Ceinture renforcée", "Cinturón reforzado", "Cinto Reforçado", "ヘビーベルト", "강화 허ри띠", "巨兽坚韧腰带");
                 break;
             case 7: // Boots
                 item.id = "ai_boots";
                 item.name = GetText9("Кованые Сапоги", "Plated Sabatons", "Stiefel", "Sabatons", "Sabatones", "Soleretes", "サバトン", "강철 판금장화", "厚重精钢战靴");
                 break;
             case 8: // Weapon
                 item.id = "ai_weapon";
                 if (isArcher)
                 {
                     if (castle.aiArmorTier == 1) item.name = GetText9("Короткий Лук", "Short Bow", "Bogen", "Arc court", "Arco corto", "Arco Curto", "ショートボウ", "숏보우", "精木短弓");
                     else if (castle.aiArmorTier == 2) item.name = GetText9("Композитный Лук", "Composite Bow", "Bogen", "Arc composite", "Arco compuesto", "Arco Composto", "コンポジットボウ", "컴포지트 보우", "反曲猎弓");
                     else if (castle.aiArmorTier == 3) item.name = GetText9("Охотничий Лук", "Hunter Bow", "Hunter Bow", "Arc de chasse", "Arco de caza", "Arco de Caça", "ハンティングボウ", "헌터 보우", "追风长弓");
                     else item.name = GetText9("Лук Зенита", "Zenith Divine Bow", "Zenit-Bogen", "Arc céleste", "Arco del Cénit", "Arco do Zênite", "ゼニスボウ", "제니스 활", "至臻封神裁决之弓");
                 }
                 else if (isMage)
                 {
                     if (castle.aiArmorTier == 1) item.name = GetText9("Деревянный Посох", "Wooden Staff", "Stab", "Bâton de bois", "Bastón de madera", "Cajado de Madeira", "ウッドスタッフ", "나무 지팡и", "学徒圆木法杖");
                     else if (castle.aiArmorTier == 2) item.name = GetText9("Ученик Посох", "Apprentice Staff", "Stab", "Bâton d'apprenti", "Bastón de aprendiz", "Cajado de Aprendiz", "アプレンティス", "수습생 지팡이", "元素秘法短杖");
                     else if (castle.aiArmorTier == 3) item.name = GetText9("Мифриловый Посох", "Mithril Staff", "Mithril-Stab", "Bâton de mithril", "Bastón de mitril", "Cajado de Mitril", "ミスリルスタッフ", "미스릴 지팡이", "璀璨星辰神杖");
                     else item.name = GetText9("Посох Архимага", "Archmage Divine Staff", "Stab", "Bâton d'archimage", "Bastón de archimago", "Cajado do Arquimago", "アークメイジスタッフ", "아크메이지 지팡이", "至臻天启奥法之杖");
                 }
                 else
                 {
                     if (castle.aiArmorTier == 1) item.name = GetText9("Бронзовый Меч", "Bronze Sword", "Schwert", "Épée de bronze", "Espada de bronze", "Espada de Bronze", "ブロンズソード", "청동 검", "铜质单手剑");
                     else if (castle.aiArmorTier == 2) item.name = GetText9("Стальной Молот", "Steel Mace", "Mace", "Masse d'acier", "Maza de acero", "Maça de Aço", "สチールメイス", "강철 메이스", "双刃双手钢斧");
                     else if (castle.aiArmorTier == 3) item.name = GetText9("Мифриловый Двуручник", "Mithril Greatsword", "Schwert", "Espadon de mithril", "Espadón de mitril", "Montante de Mitril", "ミスリル大剣", "미스リル 대검", "秘银骑士阔剑");
                     else item.name = GetText9("Клинок Зенита", "Divine Zenith Blade", "Göttliche Zenith-Klinge", "Lame de Zénith Divine", "Espada del Cénit Divina", "Espada do Zênite Divina", "神聖ゼニスブレード", "신성 제니스 블레이д", "至臻封神裁决之刃");
                 }
                 break;
         }
         return item;
     }

    private void DrawAIEquippedSlotButtonAnatomical(CastleInstance castle, string cmdClass, int slotType, string nameRU, string nameEN, int lang, GUIStyle slotStyle, float width, float height)
    {
        InventoryItem item = GetAICommanderItem(castle, cmdClass, slotType);
        
        GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Width(width), GUILayout.Height(height));
        
        Rect btnRect = GUILayoutUtility.GetRect(width - 8f, height - 8f, GUILayout.Width(width - 8f), GUILayout.Height(height - 8f));
        GUI.color = new Color(0.1f, 0.12f, 0.2f, 1.0f);
        GUI.Box(btnRect, "", GUI.skin.box);
        GUI.color = Color.white;

        if (item != null && !string.IsNullOrEmpty(item.id))
        {
            Texture2D itemIcon = GetItemIconTexture(item);
            if (itemIcon != null)
            {
                GUI.DrawTexture(btnRect, itemIcon, ScaleMode.ScaleToFit);
            }
            else
            {
                GUI.Label(btnRect, "🛡️", s_spyBoxLabelStyle);
            }

            if (Event.current.type == EventType.Repaint && btnRect.Contains(Event.current.mousePosition))
            {
                SetHoveredItem(item, lang);
            }
        }
        else
        {
            GUIStyle emptySlotS = new GUIStyle(GUI.skin.label);
            emptySlotS.alignment = TextAnchor.MiddleCenter;
            emptySlotS.normal.textColor = Color.gray;
            emptySlotS.fontSize = 9;
            GUI.Label(btnRect, lang == 0 ? nameRU : nameEN, emptySlotS);
        }

        GUILayout.EndHorizontal();
    }

    private void DrawSpyReportPopup(int curLang)
    {
        InitSpyStyles();
        
        float panelWidth = 840f;
        float panelHeight = 580f;
        float px = (Screen.width - panelWidth) / 2f;
        float py = (Screen.height - panelHeight) / 2f;

        string spyReportTitle = GetText9(
            "🕵️ ОТЧЕТ ШПИОНАЖА • ГЛАВНЫЙ ШТАБ РАЗВЕДКИ", "🕵️ ESPIONAGE REPORT • MILITARY INTELLIGENCE",
            "🕵️ SPIONAGEBERICHT • GEHEIMDIENST-ZENTRALE", "🕵️ RAPPORT D'ESPIONNAGE • CENTRE DE RENSEIGNEMENT",
            "🕵️ INFORME DE ESPIONAJE • INTELIGENCIA MILITAR", "🕵️ RELATÓRIO DE ESPIONAGEM • CENTRAL DI INTELIGÊNCIA",
            "🕵️ 敵情偵察報告書・軍事情報总局", "🕵️ 첩보 보고서 • 군사 정보국 사령부",
            "🕵️ 军事情报总局 • 敌特渗透防线深度报告书"
        );

        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.fontSize = 14;
        windowStyle.fontStyle = FontStyle.Bold;

        GUI.backgroundColor = new Color(0.04f, 0.08f, 0.16f, 0.98f);
        GUI.Window(101, new Rect(px, py, panelWidth, panelHeight), SpyReportWindowFunction, spyReportTitle, windowStyle);
        GUI.backgroundColor = Color.white;
    }

    private void InitSpyStyles()
    {
        if (s_spyCardBgStyle == null)
        {
            s_spyCardBgStyle = new GUIStyle(GUI.skin.box);
            s_spyCardBgStyle.padding = new RectOffset(10, 10, 10, 10);
        }
        if (s_spySectionTitleStyle == null)
        {
            s_spySectionTitleStyle = new GUIStyle(GUI.skin.label);
            s_spySectionTitleStyle.fontSize = 12;
            s_spySectionTitleStyle.fontStyle = FontStyle.Bold;
            s_spySectionTitleStyle.normal.textColor = new Color(0.2f, 0.8f, 1.0f, 1.0f); // neon cyan
        }
        if (s_spyDetailLabelStyle == null)
        {
            s_spyDetailLabelStyle = new GUIStyle(GUI.skin.label);
            s_spyDetailLabelStyle.fontSize = 11;
            s_spyDetailLabelStyle.richText = true;
            s_spyDetailLabelStyle.normal.textColor = Color.white;
            s_spyDetailLabelStyle.wordWrap = true;
        }
        if (s_spyBoxLabelStyle == null)
        {
            s_spyBoxLabelStyle = new GUIStyle(GUI.skin.label);
            s_spyBoxLabelStyle.fontSize = 18;
            s_spyBoxLabelStyle.fontStyle = FontStyle.Bold;
            s_spyBoxLabelStyle.alignment = TextAnchor.MiddleCenter;
            s_spyBoxLabelStyle.normal.textColor = Color.gray;
        }
        if (s_spyBarTextStyle == null)
        {
            s_spyBarTextStyle = new GUIStyle(GUI.skin.label);
            s_spyBarTextStyle.fontSize = 9;
            s_spyBarTextStyle.fontStyle = FontStyle.Bold;
            s_spyBarTextStyle.alignment = TextAnchor.MiddleCenter;
            s_spyBarTextStyle.normal.textColor = Color.white;
        }
    }

    private void DrawSpyReportInterior(int curLang, CastleInstance castle, int spyInfoLvl, string cmdClass)
    {
"""

    new_content = content[:start_idx] + replacement_code + content[match_end:]
    
    with open(filepath, "w", encoding="utf-8") as f:
        f.write(new_content)
    print("Successfully replaced.")

if __name__ == "__main__":
    main()
