const fs = require('fs');
const path = require('path');

const srcDir = path.join(__dirname, 'src');
const files = fs.readdirSync(srcDir).filter(f => f.endsWith('.cs') && f !== 'FateCastleManager.cs');

console.log("=== References to FateCastleManager ===");
for (const file of files) {
    const filePath = path.join(srcDir, file);
    const content = fs.readFileSync(filePath, 'utf8');
    const lines = content.split('\n');
    for (let i = 0; i < lines.length; i++) {
        if (lines[i].includes('FateCastleManager')) {
            console.log(`[${file}:${i+1}] ${lines[i].trim()}`);
        }
    }
}
