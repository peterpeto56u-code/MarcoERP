$viewsPath = "e:\Smart erp\src\MarcoERP.WpfUI\Views"
$files = Get-ChildItem -Path $viewsPath -Filter "*.xaml" -Recurse
$updated = 0

foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content
    
    # Primary (#3F51B5)
    $content = $content -replace '\bForeground="#3F51B5"', 'Foreground="{StaticResource PrimaryBrush}"'
    $content = $content -replace '\bBackground="#3F51B5"', 'Background="{StaticResource PrimaryBrush}"'
    # Danger (#F44336)
    $content = $content -replace '\bForeground="#F44336"', 'Foreground="{StaticResource DangerBrush}"'
    $content = $content -replace '\bBackground="#F44336"', 'Background="{StaticResource DangerBrush}"'
    # Success (#4CAF50)
    $content = $content -replace '\bForeground="#4CAF50"', 'Foreground="{StaticResource SuccessBrush}"'
    $content = $content -replace '\bBackground="#4CAF50"', 'Background="{StaticResource SuccessBrush}"'
    $content = $content -replace 'Value="#4CAF50"', 'Value="{StaticResource SuccessBrush}"'
    # Warning (#FF9800)
    $content = $content -replace '\bForeground="#FF9800"', 'Foreground="{StaticResource WarningBrush}"'
    $content = $content -replace '\bBackground="#FF9800"', 'Background="{StaticResource WarningBrush}"'
    # Info (#2196F3)
    $content = $content -replace '\bForeground="#2196F3"', 'Foreground="{StaticResource InfoBrush}"'
    # Background (#FAFAFA) - \b ensures no match on AlternatingRowBackground
    $content = $content -replace '\bBackground="#FAFAFA"', 'Background="{StaticResource BackgroundBrush}"'
    
    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $updated++
        Write-Host "Updated: $($file.Name)"
    }
}

Write-Host ""
Write-Host "Total files updated: $updated"
