const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, 'src', 'FateCastleManager.cs');
const content = fs.readFileSync(filePath, 'utf8');
const lines = content.split('\n');

let braceCount = 0;
const stack = [];

// Simple parser ignoring comments and strings for accurate brace tracing
let inString = false;
let inChar = false;
let inLineComment = false;
let inBlockComment = false;

for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    inLineComment = false;
    
    for (let j = 0; j < line.length; j++) {
        const char = line[j];
        
        if (inLineComment) {
            continue;
        }
        if (inBlockComment) {
            if (char === '*' && j + 1 < line.length && line[j+1] === '/') {
                inBlockComment = false;
                j++;
            }
            continue;
        }
        if (inString) {
            if (char === '\\') {
                j++;
                continue;
            }
            if (char === '"') {
                inString = false;
            }
            continue;
        }
        if (inChar) {
            if (char === '\\') {
                j++;
                continue;
            }
            if (char === "'") {
                inChar = false;
            }
            continue;
        }
        
        // Comment detection
        if (char === '/' && j + 1 < line.length && line[j+1] === '/') {
            inLineComment = true;
            j++;
            continue;
        }
        if (char === '/' && j + 1 < line.length && line[j+1] === '*') {
            inBlockComment = true;
            j++;
            continue;
        }
        
        // String/char detection
        if (char === '"') {
            inString = true;
            continue;
        }
        if (char === "'") {
            inChar = true;
            continue;
        }
        
        // Brace tracking
        if (char === '{') {
            braceCount++;
            stack.push({ line: i + 1, text: line.trim() });
            if (i + 1 >= 6500 && i + 1 <= 6835) {
                console.log(`PUSH line ${i + 1}: ${line.trim()} (Stack size: ${stack.length})`);
            }
        } else if (char === '}') {
            braceCount--;
            const popped = stack.pop();
            if (i + 1 >= 6500 && i + 1 <= 6835) {
                console.log(`POP line ${i + 1} matching PUSH from line ${popped ? popped.line : 'NONE'}: ${line.trim()} (Stack size: ${stack.length})`);
            }
            if (!popped) {
                console.log(`⚠️ Extra closed brace '}' at line ${i + 1}: ${line.trim()}`);
            }
            if (braceCount < 0) {
                braceCount = 0;
            }
        }
    }
}

console.log(`\n--- ANALYSIS COMPLETE ---`);
console.log(`Final brace count: ${braceCount}`);
if (stack.length > 0) {
    console.log(`Unclosed braces remaining in stack (${stack.length}):`);
    stack.forEach((b, index) => {
        console.log(`  [${index}] Line ${b.line}: ${b.text}`);
    });
} else {
    console.log("No unclosed braces found!");
}

