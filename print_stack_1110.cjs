const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, 'src', 'FateMapManager.cs');
const content = fs.readFileSync(filePath, 'utf8');
const lines = content.split('\n');

let braceCount = 0;
const stack = [];

for (let i = 0; i < 1110; i++) {
    const line = lines[i];
    for (let j = 0; j < line.length; j++) {
        const char = line[j];
        if (char === '{') {
            braceCount++;
            stack.push({ line: i + 1, text: line.trim() });
        } else if (char === '}') {
            braceCount--;
            stack.pop();
        }
    }
}

console.log("Stack at line 1110:");
stack.forEach(item => {
    console.log(`Line ${item.line}: ${item.text}`);
});
