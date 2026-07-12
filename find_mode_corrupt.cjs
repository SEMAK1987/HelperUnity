const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, 'src', 'FateCastleManager.cs');
const content = fs.readFileSync(filePath, 'utf8');

const lines = content.split('\n');
let modeStack = ['code']; 
let inBlockComment = false;

for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    let inLineComment = false;

    const oldStackSize = modeStack.length;

    for (let j = 0; j < line.length; j++) {
        const char = line[j];
        const nextChar = line[j + 1];
        const currentMode = modeStack[modeStack.length - 1];

        if (inBlockComment) {
            if (char === '*' && nextChar === '/') {
                inBlockComment = false;
                j++;
            }
            continue;
        }

        if (inLineComment) {
            break;
        }

        if (currentMode === 'string') {
            if (char === '\\') {
                j++; // skip escaped
            } else if (char === '"') {
                modeStack.pop();
            }
            continue;
        }

        if (currentMode === 'char') {
            if (char === '\\') {
                j++;
            } else if (char === "'") {
                modeStack.pop();
            }
            continue;
        }

        if (currentMode === 'interpolated_string') {
            if (char === '\\') {
                j++;
            } else if (char === '{') {
                modeStack.push('code'); 
            } else if (char === '"') {
                modeStack.pop(); 
            }
            continue;
        }

        // Code mode
        if (char === '/' && nextChar === '/') {
            inLineComment = true;
            j++;
            continue;
        }
        if (char === '/' && nextChar === '*') {
            inBlockComment = true;
            j++;
            continue;
        }

        if (char === '$' && nextChar === '"') {
            modeStack.push('interpolated_string');
            j++;
            continue;
        }
        if (char === '"') {
            modeStack.push('string');
            continue;
        }
        if (char === "'") {
            modeStack.push('char');
            continue;
        }

        if (char === '}') {
            if (modeStack.length > 1 && modeStack[modeStack.length - 2] === 'interpolated_string') {
                modeStack.pop();
            }
        }
    }

    if (modeStack.length !== oldStackSize) {
        console.log(`Line ${i + 1}: Stack size changed from ${oldStackSize} to ${modeStack.length}. Stack: ${JSON.stringify(modeStack)}. Content: ${line.trim().substring(0, 80)}`);
    }
}
