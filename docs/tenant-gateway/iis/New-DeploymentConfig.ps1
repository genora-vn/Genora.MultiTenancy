# Run on the deployment machine. Generates files only; never changes IIS or deploys.
[CmdletBinding()]
param(
    [ValidateSet('Staging','Production')][string]$Environment = 'Staging',
    [Parameter(Mandatory=$true)][string]$OutputDirectory,
    [ValidateRange(1,65535)][int]$BackendPort = 8868,
    [ValidateRange(1,65535)][int]$GatewayPort = 5088
)
$ErrorActionPreference = 'Stop'
$output = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $output) { throw 'Choose a NEW output directory; existing files will not be overwritten.' }
$templates = Split-Path $PSScriptRoot -Parent
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$gateway = Get-Content -Raw -LiteralPath (Join-Path $templates "gateway.$Environment.example.json") | ConvertFrom-Json
$guard = Get-Content -Raw -LiteralPath (Join-Path $templates "abp-guard.$Environment.example.json") | ConvertFrom-Json
$random = [Security.Cryptography.RandomNumberGenerator]::Create()
try {
    foreach ($name in @('hl25','hlg')) {
        $bytes = New-Object byte[] 32
        $random.GetBytes($bytes)
        $key = [Convert]::ToBase64String($bytes)
        $gateway.TenantGateway.Tenants.$name.SharedKey = $key
        $gateway.TenantGateway.Tenants.$name.BackendAddress = "http://127.0.0.1:$BackendPort/"
        $guard.TenantGatewayGuard.Tenants.$name.SharedKey = $key
    }
} finally { $random.Dispose() }
[xml]$gatewayWeb = Get-Content -Raw -LiteralPath (Join-Path $repo 'src/Genora.MultiTenancy.Gateway/web.config')
$gatewayWeb.configuration.location.'system.webServer'.aspNetCore.environmentVariables.environmentVariable |
    Where-Object { $_.name -eq 'ASPNETCORE_ENVIRONMENT' } | ForEach-Object { $_.value = $Environment }
$ingress = [IO.File]::ReadAllText((Join-Path $PSScriptRoot "ingress.$Environment.web.config"))
$ingress = $ingress.Replace('http://127.0.0.1:8868/', "http://127.0.0.1:$BackendPort/").Replace('http://127.0.0.1:5088/', "http://127.0.0.1:$GatewayPort/")
foreach ($folder in @('gateway','abp','ingress')) { New-Item -ItemType Directory -Path (Join-Path $output $folder) -Force | Out-Null }
$utf8 = New-Object Text.UTF8Encoding($false)
[IO.File]::WriteAllText((Join-Path $output "gateway/appsettings.$Environment.json"), ($gateway | ConvertTo-Json -Depth 16), $utf8)
[IO.File]::WriteAllText((Join-Path $output "abp/guard.$Environment.fragment.json"), ($guard | ConvertTo-Json -Depth 16), $utf8)
$gatewayWeb.Save((Join-Path $output 'gateway/web.config'))
[IO.File]::WriteAllText((Join-Path $output 'ingress/web.config'), $ingress, $utf8)
Write-Output "Prepared $Environment files in: $output"
Write-Output 'Secrets generated independently per tenant and matched between Gateway and ABP. Values are not logged.'
Write-Output 'Protect this directory with deployment/IIS read ACLs. Do not commit, share or paste generated secret files.'
Write-Output 'Merge the ABP fragment into its existing environment settings; it is not a complete ABP appsettings file.'
