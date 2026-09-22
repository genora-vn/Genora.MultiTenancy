# Requires PowerShell 7. Sends exactly four read-only requests; never a load test.
[CmdletBinding()]
param([switch]$ConfirmStaging)
$ErrorActionPreference = 'Stop'
if (-not $ConfirmStaging) { throw 'Use -ConfirmStaging after confirming this is the intended staging environment.' }
$targets = @(
    @{ Name = 'HL25'; Base = 'https://duocphamhoalinh-staging.genora.vn'; Tenant = '650ccd37-aeb4-63e7-bac7-3a23a72f9cbc'; Path = '/api/mini-app/hl25/config' },
    @{ Name = 'HLG'; Base = 'https://hoalinh-staging.genora.vn'; Tenant = '8cfc81eb-4693-2434-c5b2-3a21cccfe131'; Path = '/api/mini-app/hlg/knowledge/categories' }
)
foreach ($target in $targets) {
    $config = Invoke-RestMethod -Uri ($target.Base + '/api/abp/application-configuration?includeLocalizationResources=false') -MaximumRedirection 0 -TimeoutSec 20
    if ($config.currentTenant.id -ne $target.Tenant) { throw ($target.Name + ': TenantId mismatch. Stop before load testing.') }
    $data = Invoke-RestMethod -Uri ($target.Base + $target.Path) -MaximumRedirection 0 -TimeoutSec 20
    $ok = if ($target.Name -eq 'HL25') { $data.success -eq $true } else { $null -eq $data.error -and $null -ne $data.data }
    if (-not $ok) { throw ($target.Name + ': business response failed. Inspect staging logs; no load test yet.') }
    [pscustomobject]@{ Tenant = $target.Name; TenantId = $config.currentTenant.id; Smoke = 'PASS'; GatewayPathVerified = 'Requires 429/origin403 test' }
}
