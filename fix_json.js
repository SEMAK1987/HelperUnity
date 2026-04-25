import fs from 'fs';
const data = fs.readFileSync('game_design.json', 'utf8');
const lines = data.split('\n');

// Fix line 653 (approx) - index 652
if (lines[652] && lines[652].includes('},')) {
    // Just find the line near there that looks mangled
    for (let i = 650; i < 660; i++) {
        if (lines[i] && lines[i].includes('},') && lines[i].length > 10) {
             lines[i] = '    },';
        }
    }
}

// Fix the big mess starting at 731
const corruptIdx = lines.findIndex(l => l.includes('{ "name": "Хранители жемч'));
if (corruptIdx !== -1) {
    const endIdx = lines.findIndex((l, idx) => idx > corruptIdx && l.includes('"monster_weight_modifier":'));
    if (endIdx !== -1) {
        const replacement = [
          '          { "name": "Хранители жемчужины", "base_weight": 96, "full_weight": 220, "gear": "Титановая броня, посох управления водой" },',
          '          { "name": "Адмиралы глубинных флотов", "base_weight": 94, "full_weight": 210, "gear": "Навигационные руны, командный трезубец" }',
          '        ]',
          '      },',
          '      "Эльфы": {',
          '        "light": [',
          '          { "name": "Лесные танцоры", "base_weight": 60, "full_weight": 70, "gear": "Лиственная броня, парные клинки" },',
          '          { "name": "Шепчущие тени", "base_weight": 55, "full_weight": 65, "gear": "Маскировочный плащ, кинжалы" }',
          '        ],',
          '        "medium": [',
          '          { "name": "Стражи вековых дубов", "base_weight": 75, "full_weight": 100, "gear": "Усиленная кора, копья" },',
          '          { "name": "Друиды-защитники", "base_weight": 70, "full_weight": 95, "gear": "Магические одежды, посохи" }',
          '        ],',
          '        "heavy": [',
          '          { "name": "Эльфийские воители", "base_weight": 85, "full_weight": 130, "gear": "Мифриловые латы, двуручный меч" }',
          '        ],',
          '        "ranged": [',
          '          { "name": "Мастера дальнего леса", "base_weight": 65, "full_weight": 85, "gear": "Длинный лук, зачарованные стрелы" }',
          '        ],',
          '        "legendary": [',
          '          { "name": "Владыки леса", "base_weight": 90, "full_weight": 180, "gear": "Реликвии древних, лук звездного света" }',
          '        ]',
          '      },',
          '      "Орки": {',
          '        "light": [',
          '          { "name": "Гоблины-налетчики", "base_weight": 50, "full_weight": 60, "gear": "Тряпье, ржавые ножи" }',
          '        ],',
          '        "medium": [',
          '          { "name": "Орки-рубаки", "base_weight": 85, "full_weight": 110, "gear": "Грубая кожа, топоры" }',
          '        ],',
          '        "heavy": [',
          '          { "name": "Черные орки", "base_weight": 100, "full_weight": 150, "gear": "Черное железо, гигантские тесаки" }',
          '        ],',
          '        "ranged": [',
          '          { "name": "Метатели камней", "base_weight": 80, "full_weight": 100, "gear": "Шкуры, пращи" }',
          '        ],',
          '        "legendary": [',
          '          { "name": "Вожди кровавой орды", "base_weight": 110, "full_weight": 220, "gear": "Артефакты хаоса, топор власти" }',
          '        ]',
          '      }',
          '    },'
        ];
        lines.splice(corruptIdx, endIdx - corruptIdx, ...replacement);
    }
}

fs.writeFileSync('game_design.json', lines.join('\n'));
console.log("Fixed!");
