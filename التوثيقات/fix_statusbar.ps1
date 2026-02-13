$viewsPath = "e:\Smart erp\src\MarcoERP.WpfUI\Views"
$files = Get-ChildItem -Path $viewsPath -Filter "*.xaml" -Recurse
$updated = 0

foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content
    
    # Replace standalone Background="#F5F5F5" (not AlternatingRow or other prefixed variants)
    $content = $content -replace '\bBackground="#F5F5F5"', 'Background="{StaticResource BackgroundBrush}"'
    
    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $updated++
        Write-Host "Updated: $($file.Name)"
    }
}

Write-Host ""
Write-Host "Total files updated: $updated"
