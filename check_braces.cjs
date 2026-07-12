const fs = require('fs');
const path = require('path');

const srcDir = path.join(__dirname, 'src');
const files = fs.readdirSync(srcDir).filter(f => f.endsWith('.cs'));

for (const file of files) {
    const filePath = path.join(srcDir, file);
    const content = fs.readFileSync(filePath, 'utf8');
    
    // Check braces
    let braceCount = 0;
    let lineNum = 1;
    let colNum = 1;
    const lines = content.split('\n');
    for (let i = 0; i < lines.length; i++) {
        const line = lines[i];
        for (let j = 0; j < line.length; j++) {
            const char = line[j];
            if (char === '{') {
                braceCount++;
            } else if (char === '}') {
                braceCount--;
                if (braceCount < 0) {
                    console.log(`Error in ${file}: Negative brace count at line ${i+1}, col ${j+1}`);
                    braceCount = 0; // reset
                }
            }
        }
    }
    if (braceCount !== 0) {
        console.log(`Error in ${file}: Unbalanced braces! Final count is ${braceCount}`);
    }
}
console.log("Brace check completed.");
