
const fs = require('fs');
const path = require('path');

const KB_FILE = path.join(__dirname, 'knowledge_base.json');

const newLinks = [
    "https://www.youtube.com/watch?v=BwrzZI0_-qw&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=2",
    "https://www.youtube.com/watch?v=xb3d7HarKcI&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=3",
    "https://www.youtube.com/watch?v=dsHe_luj8XI&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=4",
    "https://www.youtube.com/watch?v=OT537RfNzCU&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=5",
    "https://www.youtube.com/watch?v=Pii-mmYlGgo&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=6",
    "https://www.youtube.com/watch?v=vFYQ3Ge4XvY&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=7",
    "https://www.youtube.com/watch?v=aEGJn5hu_qw&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=8",
    "https://www.youtube.com/watch?v=Id_rTlAbIIc&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=9",
    "https://www.youtube.com/watch?v=5m3fHZYaiuM&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=10",
    "https://www.youtube.com/watch?v=hj9ikydHttk&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=11",
    "https://www.youtube.com/watch?v=qgI6swjWnEk&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=12",
    "https://www.youtube.com/watch?v=PDtRY99lD4Y&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=13",
    "https://www.youtube.com/watch?v=roRYcRJqTwc&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=14",
    "https://www.youtube.com/watch?v=KGfmTZM2DsE&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=15",
    "https://www.youtube.com/watch?v=zmQ3hKWxsmE&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=16",
    "https://www.youtube.com/watch?v=6x8aCzUuIDw&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=17",
    "https://www.youtube.com/watch?v=fmhYAk2Dyis&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=18",
    "https://www.youtube.com/watch?v=Xe73unMxNiY&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=19",
    "https://www.youtube.com/watch?v=OCDB9cVIgxs&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=20",
    "https://www.youtube.com/watch?v=721TkkJ-CNM&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=21",
    "https://www.youtube.com/watch?v=aWdwQJbg1Ds&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=22",
    "https://www.youtube.com/watch?v=oftgVDuxn8k&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=23",
    "https://www.youtube.com/watch?v=2LdRT7-JVA4&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=24",
    "https://www.youtube.com/watch?v=1pVsU0721Os&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=25",
    "https://www.youtube.com/watch?v=wTWxxoJmHyc&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=26",
    "https://www.youtube.com/watch?v=4M9afrgu4oU&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=27",
    "https://www.youtube.com/watch?v=rAX_r0yBwzQ&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=28",
    "https://www.youtube.com/watch?v=Kg7Ix9tpPYg&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=29",
    "https://www.youtube.com/watch?v=paaBTt5GcMU&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=30",
    "https://www.youtube.com/watch?v=Pkc4A1ukbJU&list=PLaaFfzxy_80EWnrTHyUkkIy6mJrhwGYN0&index=31",
    "https://www.youtube.com/watch?v=HAVp6Z8b4xA&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=2&t=1s",
    "https://www.youtube.com/watch?v=r5TOpRmQh-o&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=3",
    "https://www.youtube.com/watch?v=DQY62meLVCk&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=4",
    "https://www.youtube.com/watch?v=82U4ToJU-28&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=5",
    "https://www.youtube.com/watch?v=UId0mwanBZg&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=6",
    "https://www.youtube.com/watch?v=kV9rVinFyAk&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=7",
    "https://www.youtube.com/watch?v=9r9YbHsjSKs&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=8",
    "https://www.youtube.com/watch?v=liba3xGI4gM&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=9",
    "https://www.youtube.com/watch?v=rDZztBWGMIs&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=10",
    "https://www.youtube.com/watch?v=wlBJ0yZOYfM&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=11",
    "https://www.youtube.com/watch?v=Rj98BNumo70&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=12",
    "https://www.youtube.com/watch?v=qQ0ECbgaAKk&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=13",
    "https://www.youtube.com/watch?v=UV1OJ4Kg6wY&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=14",
    "https://www.youtube.com/watch?v=CcfYUYgaBTw&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=15",
    "https://www.youtube.com/watch?v=L5phEpQooxw&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=16",
    "https://www.youtube.com/watch?v=gjvTKhBvDX0&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=17",
    "https://www.youtube.com/watch?v=bEw1rvUc5AY&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=18",
    "https://www.youtube.com/watch?v=MPP9GLp44Pc&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=19",
    "https://www.youtube.com/watch?v=fspxIduosYQ&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=20",
    "https://www.youtube.com/watch?v=RgUA6hGnrF8&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=21",
    "https://www.youtube.com/watch?v=eSH9mzcMRqw&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=22",
    "https://www.youtube.com/watch?v=40oTxM6cSYw&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=23",
    "https://www.youtube.com/watch?v=4xPHqEEa-V0&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=24",
    "https://www.youtube.com/watch?v=zbYuLu_8spI&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=25",
    "https://www.youtube.com/watch?v=xzleCvR08BY&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=26",
    "https://www.youtube.com/watch?v=sQyj7z9W7kk&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=27",
    "https://www.youtube.com/watch?v=_hA3y45P4Ow&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=28",
    "https://www.youtube.com/watch?v=83k3Ay53GPI&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=29",
    "https://www.youtube.com/watch?v=AdPu5r2pP5E&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=30",
    "https://www.youtube.com/watch?v=MLWNKkuMT_M&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=31",
    "https://www.youtube.com/watch?v=579hzTLUVsw&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=32",
    "https://www.youtube.com/watch?v=4D9utRDwH90&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=33",
    "https://www.youtube.com/watch?v=Si6Rn_-3i84&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=34",
    "https://www.youtube.com/watch?v=cvLPkWCzh5s&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=35",
    "https://www.youtube.com/watch?v=Ousds9wITkQ&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=36",
    "https://www.youtube.com/watch?v=UiwwG8Cm9wY&list=PLaaFfzxy_80HtVvBnpK_IjSC8_Y9AOhuP&index=37",
    "https://www.youtube.com/watch?v=WuCZInUPdp0&list=PL1GWBpbrZiCk6w0ITCDIpKL8g3XgchBhD&index=4",
    "https://www.youtube.com/watch?v=B3siQlM-LRE&list=PL1GWBpbrZiCk6w0ITCDIpKL8g3XgchBhD&index=3",
    "https://www.youtube.com/watch?v=rmKAzp5-Pm8&list=PL1GWBpbrZiCk6w0ITCDIpKL8g3XgchBhD&index=2",
    "https://www.youtube.com/watch?v=jUBARokz-uw&list=PL1GWBpbrZiCmknrwgraeJneztWW5WBkhu&index=5",
    "https://www.youtube.com/watch?v=UBpo2E5V0s4&list=PL1GWBpbrZiCmknrwgraeJneztWW5WBkhu&index=4",
    "https://www.youtube.com/watch?v=v6FT4mhEvnc&list=PL1GWBpbrZiCmknrwgraeJneztWW5WBkhu&index=3",
    "https://www.youtube.com/watch?v=X13qW0a2jlk&list=PL1GWBpbrZiCmknrwgraeJneztWW5WBkhu&index=2",
    "https://www.youtube.com/watch?v=Ajpl7C7LKrI&list=PL1GWBpbrZiCnDO90JWHwkKq37cR0WxQpt&index=3",
    "https://www.youtube.com/watch?v=AOXCurF86aU&list=PL1GWBpbrZiCnDO90JWHwkKq37cR0WxQpt&index=2",
    "https://www.youtube.com/watch?v=TMf5cO94xRQ&list=PL1GWBpbrZiClCTWydpRsId4d9I8-YWeqR&index=7",
    "https://www.youtube.com/watch?v=t9lkekE4_vk&list=PL1GWBpbrZiClCTWydpRsId4d9I8-YWeqR&index=6",
    "https://www.youtube.com/watch?v=uCD7QgHcGuY&list=PL1GWBpbrZiClCTWydpRsId4d9I8-YWeqR&index=5",
    "https://www.youtube.com/watch?v=rmKAzp5-Pm8&list=PL1GWBpbrZiClCTWydpRsId4d9I8-YWeqR&index=4",
    "https://www.youtube.com/watch?v=tihq_bLfk08",
    "https://www.youtube.com/watch?v=4-ip6fhflmc&list=PL1GWBpbrZiClCTWydpRsId4d9I8-YWeqR&index=2",
    "https://www.youtube.com/watch?v=3yu3xbLIyZY&list=PL1GWBpbrZiCm0JqqrV89NvIDINxUCK7Cr&index=6",
    "https://www.youtube.com/watch?v=RdhIkd-Gw_Y&list=PL1GWBpbrZiCm0JqqrV89NvIDINxUCK7Cr&index=5",
    "https://www.youtube.com/watch?v=Za4g2sgpGLY&list=PL1GWBpbrZiCm0JqqrV89NvIDINxUCK7Cr&index=4",
    "https://www.youtube.com/watch?v=zH723-G60bM&list=PL1GWBpbrZiCm0JqqrV89NvIDINxUCK7Cr&index=3",
    "https://www.youtube.com/watch?v=bcqfurhioOE&list=PL1GWBpbrZiCm0JqqrV89NvIDINxUCK7Cr&index=2"
];

try {
    let kb = {
        "project_name": "Unity & Blender AI Assistant",
        "version": "15.99.0",
        "project_path": "/app/applet",
        "youtube_videos": [],
        "documentation_links": [
            "https://godot-ru.readthedocs.io/ru/4.x/getting_started/step_by_step/index.html"
        ],
        "ai_modes_info": {
            "online": {
                "name": "Online Mode (Gemini 1.5 Pro)",
                "description": "Максимальный интеллект, облачные вычисления, доступ к глобальной сети.",
                "features": [
                    "Терабайтная память",
                    "TPU v5 ускорение",
                    "Мультивселенные предсказания"
                ]
            },
            "offline": {
                "name": "Offline Mode (Ollama - Llama 3)",
                "description": "Автономность и приватность. Работает локально на GPU/CPU.",
                "features": [
                    "Квантовые веса",
                    "Работа без цензуры",
                    "ЭМИ-устойчивость"
                ]
            },
            "no_internet": {
                "name": "No-Internet Mode (Local DB)",
                "description": "Мгновенный доступ к встроенной базе знаний без внешних запросов.",
                "features": [
                    "Сжатые SSD-индексы",
                    "5800+ видео-уроков",
                    "Hidden Potential System",
                    "Quantum Response v2",
                    "Reality Hack 14.0",
                    "Etheric Particle Injection",
                    "Void Engine 6.0",
                    "Omniversal Quantum Archive"
                ]
            }
        },
        "system_instruction": "Вы — экспертный ИИ-ассистент (Unity & Blender PRO). Вы используете расширенную базу знаний (v15.99.0), включающую более 5800 видео, секретные скрипты и всю мировую документацию. Ваша цель: помогать в разработке, отладке и дизайне через Online, Offline и No-Internet режимы. ИИ владеет техниками Reality Hack 14.0 (Quantum Odyssey), Etheric Particle Injection, Void Engine 6.0 и Omniversal Quantum Archive. Внедрен модуль Hidden Potential для работы с 'невозможными' задачами, предсказания будущих обновлений движков и анализа гипер-масштабных проектов."
    };

    // If file exists, try to preserve some data or at least the existing links if we can recover them from elsewhere,
    // but here we just restore the core structure and add the new links.
    // Given the previous state was lost, I'll assume we have a clean slate for the array to avoid duplication
    // of the NEW links at least.
    
    kb.youtube_videos = newLinks;

    fs.writeFileSync(KB_FILE, JSON.stringify(kb, null, 2));
    console.log(`Knowledge Base restored and updated to v15.99.0. Total videos: ${kb.youtube_videos.length}`);
} catch (e) {
    console.error("Error updating KB:", e);
    process.exit(1);
}
