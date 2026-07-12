const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, 'src', 'FateMapManager.cs');
const content = fs.readFileSync(filePath, 'utf8');
const lines = content.split('\n');

let braceCount = 0;
const stack = [];

for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    for (let j = 0; j < line.length; j++) {
        const char = line[j];
        if (char === '{') {
            braceCount++;
            stack.push({ line: i + 1, text: line.trim() });
        } else if (char === '}') {
            braceCount--;
            const popped = stack.pop();
            if (braceCount === 0 && i < 1139) {
                console.log(`Brace count became 0 at line ${i + 1}: ${line.trim()}`);
                console.log(`Open brace was at line ${popped.line}: ${popped.text}`);
            }
            if (braceCount < 0) {
                console.log(`Negative brace count at line ${i + 1}: ${line.trim()}`);
                braceCount = 0;
            }
        }
    }
}
