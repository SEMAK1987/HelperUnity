const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, 'src', 'FateCastleManager.cs');
const content = fs.readFileSync(filePath, 'utf8');

const lines = content.split('\n');
let braceCount = 0;

// To handle string interpolation, we need a stack of modes: 'code' or 'string' or 'interpolated_string'
let modeStack = ['code']; 
let inBlockComment = false;

for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    let inLineComment = false;

    // We can also print method boundaries to see where brace count jumps unexpectedly
    const methodMatch = line.match(/^\s*(public|private|protected|internal)\s+(static\s+)?(void|string|int|float|bool|Vector2|Rect|Texture2D|IEnumerator|class|struct)\s+(\w+)\s*\(/);
    if (methodMatch && modeStack[modeStack.length - 1] === 'code') {
        console.log(`Line ${i + 1}: Method/Class "${methodMatch[4]}" defined. Current brace count: ${braceCount}`);
    }

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
                modeStack.push('code'); // Switch to code inside interpolation
            } else if (char === '"') {
                modeStack.pop(); // End of interpolated string
            }
            continue;
        }

        // Inside code mode
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

        if (char === '{') {
            braceCount++;
        } else if (char === '}') {
            braceCount--;
            // If we are inside code mode but it's nested in an interpolated string, we might hit '}' which returns us to string
            if (braceCount < 0) {
                // If the previous mode was interpolated_string, we pop code and return to interpolated_string
                if (modeStack.length > 1 && modeStack[modeStack.length - 2] === 'interpolated_string') {
                    modeStack.pop();
                    // Do not decrement brace count if it was the closing brace of interpolation
                    braceCount++; 
                } else {
                    console.error(`ERROR: Extra closing brace '}' at line ${i + 1}: ${line.trim()}`);
                    braceCount = 0;
                }
            } else {
                // Check if this '}' actually closed a code section in interpolated string
                if (modeStack.length > 1 && modeStack[modeStack.length - 2] === 'interpolated_string' && braceCount < modeStack.filter(m => m === 'code').length - 1) {
                    modeStack.pop();
                    // Restore brace count since it was interpolation close, not block close
                    braceCount++;
                }
            }
        }
    }
}

console.log(`\nFinal check of FateCastleManager.cs:
Final Brace Count: ${braceCount}
Active Mode Stack: ${JSON.stringify(modeStack)}
`);
