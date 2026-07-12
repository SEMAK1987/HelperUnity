const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, 'src', 'FateMapManager.cs');
const content = fs.readFileSync(filePath, 'utf8');
const lines = content.split('\n');

let braceCount = 0;
for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    for (let j = 0; j < line.length; j++) {
        if (line[j] === '{') {
            braceCount++;
        } else if (line[j] === '}') {
            braceCount--;
            if (braceCount < 0) {
                console.log(`Negative brace count at line ${i+1}: ${line.trim()}`);
                braceCount = 0;
            }
        }
    }
}
console.log(`Final brace count: ${braceCount}`);
