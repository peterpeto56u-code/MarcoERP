# فحص SQL Server واتصال بقاعدة البيانات
Write-Host "=== فحص SQL Server ===" -ForegroundColor Cyan

# فحص خدمات SQL Server
$services = Get-Service -Name "MSSQL*" | Where-Object {$_.Status -eq 'Running'}
Write-Host "`nالخدمات العاملة:" -ForegroundColor Green
$services | Format-Table Name, DisplayName -AutoSize

# محاولة الاتصال بـ instances مختلفة
$instances = @(".", ".\MSSQLSERVER", "localhost", "(localdb)\MSSQLLocalDB")
$connStringTemplates = @(
    "Server={0};Database=master;Integrated Security=True;TrustServerCertificate=True;",
    "Server={0};Database=master;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;",
    "Server={0};Database=master;Integrated Security=SSPI;TrustServerCertificate=True;"
)

$connected = $false
$workingConnString = ""

foreach ($instance in $instances) {
    foreach ($template in $connStringTemplates) {
        $connStr = $template -f $instance
        try {
            $conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
            $conn.Open()
            Write-Host "`n✓ نجح الاتصال بـ: $instance" -ForegroundColor Green
            Write-Host "  Connection String: $connStr" -ForegroundColor Gray
            
            # فحص قاعدة البيانات
            $cmd = $conn.CreateCommand()
            $cmd.CommandText = "SELECT name FROM sys.databases WHERE name = 'MarcoERP'"
            $reader = $cmd.ExecuteReader()
            
            if ($reader.Read()) {
                Write-Host "✓ قاعدة البيانات MarcoERP موجودة" -ForegroundColor Green
                $reader.Close()
                
                # فحص عدد الجداول
                $conn.ChangeDatabase("MarcoERP")
                $cmd2 = $conn.CreateCommand()
                $cmd2.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
                $tableCount = $cmd2.ExecuteScalar()
                Write-Host "  عدد الجداول: $tableCount" -ForegroundColor Cyan
            } else {
                $reader.Close()
                Write-Host "✗ قاعدة البيانات MarcoERP غير موجودة" -ForegroundColor Yellow
                
                # إنشاء قاعدة البيانات
                $createCmd = $conn.CreateCommand()
                $createCmd.CommandText = "CREATE DATABASE [MarcoERP]"
                $createCmd.ExecuteNonQuery() | Out-Null
                Write-Host "✓ تم إنشاء قاعدة البيانات MarcoERP" -ForegroundColor Green
            }
            
            $workingConnString = $connStr.Replace("master", "MarcoERP")
            $conn.Close()
            $connected = $true
            break
        } catch {
            # محاولة التالية
        }
    }
    if ($connected) { break }
}

if ($connected) {
    Write-Host "`n=== Connection String للاستخدام ===" -ForegroundColor Cyan
    Write-Host $workingConnString -ForegroundColor Yellow
    
    # تحديث appsettings.json
    $appSettingsPath = "src\MarcoERP.WpfUI\appsettings.json"
    if (Test-Path $appSettingsPath) {
        $json = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
        $json.ConnectionStrings.DefaultConnection = $workingConnString
        $json | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
        Write-Host "`n✓ تم تحديث appsettings.json" -ForegroundColor Green
    }
} else {
    Write-Host "`n✗ فشل الاتصال بجميع الـ instances" -ForegroundColor Red
    exit 1
}
