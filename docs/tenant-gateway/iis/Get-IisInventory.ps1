# Run in elevated 64-bit Windows PowerShell on the IIS server. Read-only; no secrets/config dumps.
[CmdletBinding()]
param([string[]]$SiteNames = @('Genora.Ingress.Staging','Genora.Tenant.Gateway','Genora.MultiTenancy'))
$ErrorActionPreference = 'Stop'
Import-Module WebAdministration
foreach ($siteName in $SiteNames) {
    $site = Get-Website -Name $siteName
    if ($null -eq $site) { Write-Output "Site missing: $siteName"; continue }
    $pool = Get-Item -LiteralPath (Join-Path 'IIS:\AppPools' $site.applicationPool)
    $environment = 'not set in web.config'
    $root = [Environment]::ExpandEnvironmentVariables($site.physicalPath)
    $webPath = Join-Path $root 'web.config'
    if (Test-Path -LiteralPath $webPath) {
        try {
            [xml]$xml = Get-Content -Raw -LiteralPath $webPath
            $values = @($xml.SelectNodes('//environmentVariable') | Where-Object { $_.name -in @('ASPNETCORE_ENVIRONMENT','DOTNET_ENVIRONMENT') } | ForEach-Object {
                $_.name + '=' + $(if ($_.value -in @('Staging','Production','Development')) { $_.value } else { '(custom value)' })
            })
            if ($values.Count) { $environment = $values -join '; ' }
        } catch { $environment = 'web.config XML unreadable/invalid (details omitted)' }
    }
    [pscustomobject]@{
        Site = $site.name; State = [string]$site.state; AppPool = $site.applicationPool
        Bindings = @($site.bindings.Collection | ForEach-Object { $_.protocol + ' ' + $_.bindingInformation })
        ManagedRuntime = $pool.managedRuntimeVersion; MaxWorkers = $pool.processModel.maxProcesses
        DisallowOverlappingRotation = $pool.recycling.disallowOverlappingRotation
        WebConfigEnvironment = $environment
    } | ConvertTo-Json -Depth 4
}
foreach ($property in @('enabled','preserveHostHeader')) {
    try {
        $value = Get-WebConfigurationProperty -PSPath 'MACHINE/WEBROOT/APPHOST' -Filter 'system.webServer/proxy' -Name $property
        Write-Output ("ARR $property = " + $value.Value)
    } catch { Write-Output "ARR $property unavailable; confirm ARR is installed/configured." }
}
Write-Output 'App Pool/machine environment overrides, certificates and live routing must also be checked. No secret values were read from appsettings.'
