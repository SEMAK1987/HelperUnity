import os

def check_brackets(filepath):
    with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()
    
    braces = 0
    parens = 0
    brackets = 0
    
    # Simple brace/paren tracking ignoring comments and strings
    in_string = False
    in_char = False
    in_line_comment = False
    in_block_comment = False
    
    i = 0
    n = len(content)
    while i < n:
        c = content[i]
        
        if in_line_comment:
            if c == '\n':
                in_line_comment = False
            i += 1
            continue
            
        if in_block_comment:
            if c == '*' and i + 1 < n and content[i+1] == '/':
                in_block_comment = False
                i += 2
            else:
                i += 1
            continue
            
        if in_string:
            if c == '\\':
                i += 2
                continue
            if c == '"':
                in_string = False
            i += 1
            continue
            
        if in_char:
            if c == '\\':
                i += 2
                continue
            if c == "'":
                in_char = False
            i += 1
            continue
            
        # Check comments
        if c == '/' and i + 1 < n and content[i+1] == '/':
            in_line_comment = True
            i += 2
            continue
        if c == '/' and i + 1 < n and content[i+1] == '*':
            in_block_comment = True
            i += 2
            continue
            
        # Check strings/chars
        if c == '"':
            in_string = True
            i += 1
            continue
        if c == "'":
            in_char = True
            i += 1
            continue
            
        # Count braces
        if c == '{':
            braces += 1
        elif c == '}':
            braces -= 1
        elif c == '(':
            parens += 1
        elif c == ')':
            parens -= 1
        elif c == '[':
            brackets += 1
        elif c == ']':
            brackets -= 1
            
        i += 1
        
    return braces, parens, brackets

print("Checking C# files for unmatched brackets/braces/parens:")
for root, dirs, files in os.walk('.'):
    for file in files:
        if file.endswith('.cs'):
            path = os.path.join(root, file)
            b, p, br = check_brackets(path)
            if b != 0 or p != 0 or br != 0:
                print(f"⚠️ {path}: braces={b}, parens={p}, brackets={br}")
            else:
                print(f"✅ {path}: OK")
