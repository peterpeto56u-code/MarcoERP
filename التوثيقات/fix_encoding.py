"""
Fix Arabic text encoding corruption in XAML files.
The corruption happened because PowerShell 5.1 read UTF-8 files using
the system's default codepage (cp1256 on Arabic Windows), then re-saved
as UTF-8 — double-encoding the Arabic text.

Fix: read corrupted UTF-8 → encode as cp1256 (recover original bytes) → decode as UTF-8.
"""
import os
import sys

VIEWS_DIR = r"E:\Smart erp\src\MarcoERP.WpfUI\Views"
fixed_count = 0
error_files = []
skipped_files = []

def has_mojibake(text: str) -> bool:
    """Detect the characteristic mojibake pattern: ظ/ط followed by cp1256-decoded bytes."""
    # Common patterns from UTF-8 Arabic read as cp1256
    markers = ['ظپ', 'ظˆ', 'ط§', 'طھ', 'ظٹ', 'ط±', 'ظ„', 'ط¨', 'ط¹', 'طµ', 'ظ†', 'ط¯', 'ط³']
    count = sum(1 for m in markers if m in text)
    return count >= 2  # at least 2 patterns found

def fix_file(filepath: str) -> bool:
    global fixed_count
    try:
        with open(filepath, 'r', encoding='utf-8-sig') as f:
            corrupted = f.read()
        
        if not has_mojibake(corrupted):
            skipped_files.append(filepath)
            return False
        
        # Reverse the double-encoding
        original_bytes = corrupted.encode('cp1256')
        fixed = original_bytes.decode('utf-8')
        
        # Write back as UTF-8 without BOM
        with open(filepath, 'w', encoding='utf-8', newline='') as f:
            f.write(fixed)
        
        fixed_count += 1
        print(f"  FIXED: {os.path.relpath(filepath, VIEWS_DIR)}")
        return True
    except (UnicodeDecodeError, UnicodeEncodeError) as e:
        error_files.append((filepath, str(e)))
        print(f"  ERROR: {os.path.relpath(filepath, VIEWS_DIR)} -> {e}")
        return False

# Process all XAML files
print("=" * 60)
print("Scanning XAML files for Arabic encoding corruption...")
print("=" * 60)

for root, dirs, files in os.walk(VIEWS_DIR):
    for fname in sorted(files):
        if fname.endswith('.xaml'):
            fix_file(os.path.join(root, fname))

# Also check AppStyles.xaml and other theme files
THEMES_DIR = r"E:\Smart erp\src\MarcoERP.WpfUI\Themes"
for root, dirs, files in os.walk(THEMES_DIR):
    for fname in sorted(files):
        if fname.endswith('.xaml'):
            fix_file(os.path.join(root, fname))

# Also check MainWindow ViewModel (navigation titles)
VM_DIR = r"E:\Smart erp\src\MarcoERP.WpfUI\ViewModels"
for root, dirs, files in os.walk(VM_DIR):
    for fname in sorted(files):
        if fname.endswith('.cs'):
            fix_file(os.path.join(root, fname))

# Check App.xaml.cs too
app_cs = r"E:\Smart erp\src\MarcoERP.WpfUI\App.xaml.cs"
if os.path.exists(app_cs):
    fix_file(app_cs)

print("\n" + "=" * 60)
print(f"RESULTS: Fixed {fixed_count} files, {len(error_files)} errors, {len(skipped_files)} skipped (clean)")
if error_files:
    print("\nERROR FILES:")
    for f, e in error_files:
        print(f"  {f}: {e}")
print("=" * 60)
