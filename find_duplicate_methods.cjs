const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, 'src', 'FateCastleManager.cs');
const content = fs.readFileSync(filePath, 'utf8');

const lines = content.split('\n');
const methodCounts = {};

const methodRegex = /^\s*(public|private|protected|internal)\s+(static\s+)?(void|string|int|float|bool|Vector2|Rect|Texture2D|IEnumerator|class|struct)\s+(\w+)\s*\(/;

for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    const match = line.match(methodRegex);
    if (match) {
        const methodName = match[4];
        if (!methodCounts[methodName]) {
            methodCounts[methodName] = [];
        }
        methodCounts[methodName].push(i + 1);
    }
}

console.log("--- Duplicate Methods in FateCastleManager.cs ---");
for (const [name, lines] of Object.entries(methodCounts)) {
    if (lines.length > 1) {
        console.log(`Method "${name}" is defined ${lines.length} times at lines: ${lines.join(', ')}`);
    }
}
