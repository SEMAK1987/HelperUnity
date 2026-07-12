const fs = require('fs');
const path = require('path');

const srcDir = path.join(__dirname, 'src');
const files = fs.readdirSync(srcDir).filter(f => f.endsWith('.cs'));

for (const file of files) {
    const filePath = path.join(srcDir, file);
    const content = fs.readFileSync(filePath, 'utf8');
    const lines = content.split('\n');
    
    let braceCount = 0;
    const stack = [];
    
    for (let i = 0; i < lines.length; i++) {
        const line = lines[i];
        let j = 0;
        // Skip strings and comments to get a cleaner count
        let inString = false;
        let inComment = false;
        while (j < line.length) {
            if (inComment) {
                if (line[j] === '*' && line[j+1] === '/') {
                    inComment = false;
                    j += 2;
                    continue;
                }
                j++;
                continue;
            }
            if (line[j] === '/' && line[j+1] === '/') {
                break; // rest of line is comment
            }
            if (line[j] === '/' && line[j+1] === '*') {
                inComment = true;
                j += 2;
                continue;
            }
            
            const char = line[j];
            if (char === '"' && line[j-1] !== '\\') {
                inString = !inString;
            }
            
            if (!inString) {
                if (char === '{') {
                    braceCount++;
                    stack.push({ line: i + 1, text: line.trim() });
                } else if (char === '}') {
                    braceCount--;
                    if (stack.length > 0) {
                        stack.pop();
                    }
                    if (braceCount < 0) {
                        console.log(`[${file}] Extra closing brace '}' at line ${i+1}`);
                        braceCount = 0;
                    }
                }
            }
            j++;
        }
    }
    
    if (stack.length > 0) {
        console.log(`[${file}] Unbalanced braces! Final count: ${braceCount}. Remaining unclosed:`);
        stack.forEach(item => {
            console.log(`  Line ${item.line}: ${item.text}`);
        });
    }
}
console.log("Brace analysis done.");
