$viewsPath = "e:\Smart erp\src\MarcoERP.WpfUI\Views"
$files = Get-ChildItem -Path $viewsPath -Filter "*.xaml" -Recurse
$updated = 0

foreach ($file in $files) {
    $content = [System.IO.File]::ReadAllText($file.FullName)
    $original = $content
    
    # Setter Value patterns: Property="Foreground" Value="#hex" and Property="Background" Value="#hex"
    # Primary
    $content = $content -replace '(Property="Foreground"\s+Value=")#3F51B5(")', '$1{StaticResource PrimaryBrush}$2'
    $content = $content -replace '(Property="Background"\s+Value=")#3F51B5(")', '$1{StaticResource PrimaryBrush}$2'
    # Danger
    $content = $content -replace '(Property="Foreground"\s+Value=")#F44336(")', '$1{StaticResource DangerBrush}$2'
    $content = $content -replace '(Property="Background"\s+Value=")#F44336(")', '$1{StaticResource DangerBrush}$2'
    # Success
    $content = $content -replace '(Property="Foreground"\s+Value=")#4CAF50(")', '$1{StaticResource SuccessBrush}$2'
    $content = $content -replace '(Property="Background"\s+Value=")#4CAF50(")', '$1{StaticResource SuccessBrush}$2'
    # Warning
    $content = $content -replace '(Property="Foreground"\s+Value=")#FF9800(")', '$1{StaticResource WarningBrush}$2'
    $content = $content -replace '(Property="Background"\s+Value=")#FF9800(")', '$1{StaticResource WarningBrush}$2'
    # Info
    $content = $content -replace '(Property="Foreground"\s+Value=")#2196F3(")', '$1{StaticResource InfoBrush}$2'
    # Background
    $content = $content -replace '(Property="Background"\s+Value=")#FAFAFA(")', '$1{StaticResource BackgroundBrush}$2'
    
    if ($content -ne $original) {
        [System.IO.File]::WriteAllText($file.FullName, $content)
        $updated++
        Write-Host "Updated: $($file.Name)"
    }
}

Write-Host ""
Write-Host "Total files updated: $updated"
