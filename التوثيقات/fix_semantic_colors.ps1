$viewsPath = "e:\Smart erp\src\MarcoERP.WpfUI\Views"
$files = Get-ChildItem -Path $viewsPath -Filter "*.xaml" -Recurse
$updated = 0

foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content
    
    # Error foreground: #D32F2F and #C62828 -> ErrorForegroundBrush
    $content = $content -replace 'Foreground="#D32F2F"', 'Foreground="{StaticResource ErrorForegroundBrush}"'
    $content = $content -replace 'Foreground="#C62828"', 'Foreground="{StaticResource ErrorForegroundBrush}"'
    $content = $content -replace '(Property="Foreground"\s+Value=")#D32F2F(")', '$1{StaticResource ErrorForegroundBrush}$2'
    $content = $content -replace '(Property="Foreground"\s+Value=")#C62828(")', '$1{StaticResource ErrorForegroundBrush}$2'
    
    # Error background: #FFEBEE -> ErrorBackgroundBrush
    $content = $content -replace 'Background="#FFEBEE"', 'Background="{StaticResource ErrorBackgroundBrush}"'
    $content = $content -replace '(Property="Background"\s+Value=")#FFEBEE(")', '$1{StaticResource ErrorBackgroundBrush}$2'
    
    # Subtitle: #78909C -> SubtitleBrush
    $content = $content -replace 'Foreground="#78909C"', 'Foreground="{StaticResource SubtitleBrush}"'
    
    # Status text: #546E7A -> StatusTextBrush
    $content = $content -replace 'Foreground="#546E7A"', 'Foreground="{StaticResource StatusTextBrush}"'
    
    # Muted text: #90A4AE -> MutedTextBrush
    $content = $content -replace 'Foreground="#90A4AE"', 'Foreground="{StaticResource MutedTextBrush}"'
    
    # Secondary text: #B0BEC5 -> SecondaryTextBrush
    $content = $content -replace 'Foreground="#B0BEC5"', 'Foreground="{StaticResource SecondaryTextBrush}"'
    
    # Dark surface: #37474F (Foreground only, not Background in sidebar)
    $content = $content -replace 'Foreground="#37474F"', 'Foreground="{StaticResource DarkSurfaceBrush}"'
    
    # Overlay: #40000000
    $content = $content -replace 'Background="#40000000"', 'Background="{StaticResource OverlayBrush}"'
    $content = $content -replace 'Background="#60000000"', 'Background="{StaticResource OverlayBrush}"'
    
    # Unsaved warning: #E65100
    $content = $content -replace 'Foreground="#E65100"', 'Foreground="{StaticResource UnsavedWarningBrush}"'
    $content = $content -replace 'Background="#E65100"', 'Background="{StaticResource UnsavedWarningBrush}"'
    
    # Unsaved warning bg: #FFF3E0
    $content = $content -replace 'Background="#FFF3E0"', 'Background="{StaticResource UnsavedWarningBackgroundBrush}"'
    
    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $updated++
        Write-Host "Updated: $($file.Name)"
    }
}

Write-Host ""
Write-Host "Total files updated: $updated"
