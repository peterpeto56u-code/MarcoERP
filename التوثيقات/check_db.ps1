# فحص واتصال بقاعدة البيانات MarcoERP
$connectionString = "Server=.;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "✓ الاتصال بـ SQL Server نجح" -ForegroundColor Green
    
    # فحص وجود قاعدة البيانات
    $checkDbCommand = $connection.CreateCommand()
    $checkDbCommand.CommandText = "SELECT database_id FROM sys.databases WHERE name = 'MarcoERP'"
    $result = $checkDbCommand.ExecuteScalar()
    
    if ($null -eq $result) {
        Write-Host "✗ قاعدة البيانات MarcoERP غير موجودة - سيتم إنشاؤها" -ForegroundColor Yellow
        
        # إنشاء قاعدة البيانات
        $createDbCommand = $connection.CreateCommand()
        $createDbCommand.CommandText = "CREATE DATABASE [MarcoERP]"
        $createDbCommand.ExecuteNonQuery() | Out-Null
        Write-Host "✓ تم إنشاء قاعدة البيانات MarcoERP بنجاح" -ForegroundColor Green
    } else {
        Write-Host "✓ قاعدة البيانات MarcoERP موجودة" -ForegroundColor Green
        
        # فحص عدد الجداول
        $connection.ChangeDatabase("MarcoERP")
        $tablesCommand = $connection.CreateCommand()
        $tablesCommand.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
        $tableCount = $tablesCommand.ExecuteScalar()
        Write-Host "  - عدد الجداول: $tableCount" -ForegroundColor Cyan
    }
    
    $connection.Close()
    
} catch {
    Write-Host "✗ خطأ: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
