import fs from 'fs';

const filePath = 'src/App.tsx';
let content = fs.readFileSync(filePath, 'utf8');

// We search for the specific closing tags before "AI GENERATOR PROMPTS"
const target = `                            </div>\n\n                          </div>\n                        </div>\n                      </div>\n\n                      {/* AI GENERATOR PROMPTS - COLLAPSIBLE EXPANSION CARDS */}`;
const targetLF = `                            </div>\n\n                          </div>\n                        </div>\n                      </div>\n\n                      {/* AI GENERATOR PROMPTS - COLLAPSIBLE EXPANSION CARDS */}`;
const targetCRLF = `                            </div>\r\n\r\n                          </div>\r\n                        </div>\r\n                      </div>\r\n\r\n                      {/* AI GENERATOR PROMPTS - COLLAPSIBLE EXPANSION CARDS */}`;

let index = content.indexOf(target);
let len = target.length;

if (index === -1) {
  index = content.indexOf(targetCRLF);
  len = targetCRLF.length;
}

if (index === -1) {
  // Let's search using a more flexible method: look for "AI GENERATOR PROMPTS" and backtrack
  const promptIndex = content.indexOf("{/* AI GENERATOR PROMPTS - COLLAPSIBLE EXPANSION CARDS */}");
  if (promptIndex !== -1) {
    const beforePart = content.slice(promptIndex - 200, promptIndex);
    console.log("Before part:", JSON.stringify(beforePart));
    // Let's replace the last occurrence of closed divs in the beforePart
    const searchStr = `                            </div>\r\n\r\n                          </div>\r\n                        </div>\r\n                      </div>`;
    const searchStrLF = `                            </div>\n\n                          </div>\n                        </div>\n                      </div>`;
    
    let subIdx = beforePart.indexOf(searchStr);
    let subLen = searchStr.length;
    if (subIdx === -1) {
      subIdx = beforePart.indexOf(searchStrLF);
      subLen = searchStrLF.length;
    }
    
    if (subIdx !== -1) {
      index = (promptIndex - 200) + subIdx;
      len = subLen;
    }
  }
}

if (index === -1) {
  console.error("Could not find the dangling tags target!");
  process.exit(1);
}

const replacement = `                            </div>\n\n                          </div>\n\n                      {/* AI GENERATOR PROMPTS - COLLAPSIBLE EXPANSION CARDS */}`;
content = content.slice(0, index) + replacement + content.slice(index + len);

fs.writeFileSync(filePath, content, 'utf8');
console.log("Successfully removed dangling tags!");
