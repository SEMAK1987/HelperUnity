const fs = require('fs');
const path = require('path');

// Recursive function to find all .cs files
function getFiles(dir, files_) {
    files_ = files_ || [];
    const files = fs.readdirSync(dir);
    for (const i in files) {
        const name = path.join(dir, files[i]);
        if (fs.statSync(name).isDirectory()) {
            if (files[i] !== 'node_modules' && files[i] !== 'dist' && files[i] !== '.git') {
                getFiles(name, files_);
            }
        } else if (name.endsWith('.cs')) {
            files_.push(name);
        }
    }
    return files_;
}

const allCsFiles = getFiles(__dirname);
console.log(`Found ${allCsFiles.length} C# files in workspace.`);

const classDefinitions = {};

const classRegex = /(class|struct|interface|enum)\s+(\w+)/g;

for (const filePath of allCsFiles) {
    const content = fs.readFileSync(filePath, 'utf8');
    const relativePath = path.relative(__dirname, filePath);
    
    let match;
    while ((match = classRegex.exec(content)) !== null) {
        const type = match[1];
        const name = match[2];
        if (name === 'Editor' || name === 'MonoBehaviour' || name === 'IEnumerator') continue;
        if (!classDefinitions[name]) {
            classDefinitions[name] = [];
        }
        classDefinitions[name].push({ file: relativePath, type });
    }
}

console.log("\n=== Duplicate Class/Struct/Enum/Interface Definitions in entire workspace ===");
for (const [name, occurrences] of Object.entries(classDefinitions)) {
    if (occurrences.length > 1) {
        console.log(`Type "${name}" is defined ${occurrences.length} times:`);
        occurrences.forEach(o => {
            console.log(`  - In ${o.file} as ${o.type}`);
        });
    }
}
