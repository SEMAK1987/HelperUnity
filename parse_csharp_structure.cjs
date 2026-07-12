const fs = require('fs');
const path = require('path');

const filePath = path.join(__dirname, 'src', 'FateMapManager.cs');
const content = fs.readFileSync(filePath, 'utf8');
const lines = content.split('\n');

let braceCount = 0;
const stack = [];

for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    let j = 0;
    // We want to skip string literals or comments if possible, but let's do a simple count first with lines and content
    while (j < line.length) {
        // Simple skip single-line comment
        if (line[j] === '/' && line[j+1] === '/') {
            break;
        }
        
        const char = line[j];
        if (char === '{') {
            braceCount++;
            stack.push({ line: i + 1, text: line.trim() });
        } else if (char === '}') {
            braceCount--;
            const popped = stack.pop();
            if (braceCount < 0) {
                console.log(`NEGATIVE BRACE COUNT at Line ${i + 1}: ${line.trim()}`);
                braceCount = 0;
            }
        }
        j++;
    }
}

if (stack.length > 0) {
    console.log("Unclosed braces remaining:");
    stack.forEach(item => {
        console.log(`Line ${item.line}: ${item.text}`);
    });
} else {
    console.log("No unclosed braces at end of file parsing.");
}
