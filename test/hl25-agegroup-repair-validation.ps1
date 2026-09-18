$ErrorActionPreference = 'Stop'
$repoRoot = Join-Path $PSScriptRoot '..'
$source = Get-Content -LiteralPath (Join-Path $repoRoot 'src/Genora.MultiTenancy.EntityFrameworkCore/Migrations/20260918093000_EnsureHl25ParticipantAgeGroup.cs') -Raw
$repair = [regex]::Match($source, 'migrationBuilder\.Sql\(@"([\s\S]*?)"\);').Groups[1].Value
if (!$repair) { throw 'Repair SQL not found.' }
# Exercise the actual migration SQL on session-local temp tables only.
$repair = $repair.Replace("N'[hl25].[AppHl25Participants]'", "N'tempdb..#Hl25AgeGroupValidation'")
$repair = $repair.Replace("N'hl25.AppHl25Participants'", "N'tempdb..#Hl25AgeGroupValidation'")
$repair = $repair.Replace('[hl25].[AppHl25Participants]', '#Hl25AgeGroupValidation')
$repair = $repair.Replace('sys.columns', 'tempdb.sys.columns')
$repair = $repair.Replace('DF_AppHl25Participants_AgeGroup', 'DF_Hl25AgeGroupValidation_' + [guid]::NewGuid().ToString('N'))
$settings = Get-Content -LiteralPath (Join-Path $repoRoot 'src/Genora.MultiTenancy.DbMigrator/appsettings.json') -Raw | ConvertFrom-Json
$builder = [System.Data.SqlClient.SqlConnectionStringBuilder]::new($settings.ConnectionStrings.Default)
$builder['Connect Timeout'] = 8
$conn = [System.Data.SqlClient.SqlConnection]::new($builder.ConnectionString)
function Execute-Sql([string]$sql) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sql
    $cmd.CommandTimeout = 15
    try { [void]$cmd.ExecuteNonQuery() } finally { $cmd.Dispose() }
}
function Scalar-Sql([string]$sql) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sql
    $cmd.CommandTimeout = 15
    try { return $cmd.ExecuteScalar() } finally { $cmd.Dispose() }
}
try {
    $conn.Open()
    Execute-Sql "CREATE TABLE #Hl25AgeGroupValidation (Id int NOT NULL, BirthDate datetime2 NULL); INSERT #Hl25AgeGroupValidation VALUES (1,'1990-01-01'),(2,NULL);"
    Execute-Sql $repair
    Execute-Sql $repair
    if ((Scalar-Sql "SELECT COUNT(*) FROM #Hl25AgeGroupValidation WHERE AgeGroup = 0") -ne 2) { throw 'Legacy row backfill failed.' }
    if ((Scalar-Sql "SELECT COUNT(*) FROM #Hl25AgeGroupValidation WHERE Id=1 AND BirthDate='1990-01-01'") -ne 1) { throw 'BirthDate data was changed.' }
    Write-Output 'PASS: legacy schema repaired, existing rows/BirthDate preserved, repeated repair succeeds.'
    Execute-Sql "DROP TABLE #Hl25AgeGroupValidation;"
    Execute-Sql "CREATE TABLE #Hl25AgeGroupValidation (Id int NOT NULL, AgeGroup tinyint NOT NULL);"
    Execute-Sql "INSERT #Hl25AgeGroupValidation (Id, AgeGroup) VALUES (1,2),(2,3);"
    Execute-Sql $repair
    if ((Scalar-Sql "SELECT SUM(CAST(AgeGroup AS int)) FROM #Hl25AgeGroupValidation") -ne 5) { throw 'Existing AgeGroup values were changed.' }
    Write-Output 'PASS: current schema and AgeGroup values preserved.'
    Execute-Sql "DROP TABLE #Hl25AgeGroupValidation;"
    Execute-Sql $repair
    if ((Scalar-Sql "SELECT OBJECT_ID(N'tempdb..#Hl25AgeGroupValidation', N'U')") -isnot [System.DBNull]) { throw 'Non-HL25 database was changed.' }
    Write-Output 'PASS: databases without HL25 participants skipped successfully.'
    Execute-Sql "CREATE TABLE #Hl25AgeGroupValidation (Id int NOT NULL, AgeGroup int NULL);"
    try { Execute-Sql $repair; throw 'Wrong-column-type guard failed.' }
    catch { if ($_.Exception.GetBaseException().Number -ne 51026) { throw } }
    Write-Output 'PASS: incompatible AgeGroup type/nullability rejected explicitly.'
} finally { $conn.Dispose() }
