param([Parameter(Mandatory=$true)][string]$path)
try
{
    [xml]$webConfig = Get-Content $path
    $envVarsEl = $webConfig.CreateElement('environmentVariables');
    $envVarEl = $webConfig.CreateElement('environmentVariables');
    $envVarEl.SetAttribute("name", "ASPNETCORE_ENVIROMNENT");
    $envVarEl.SetAttribute("value", $env:ASPNETCORE_ENVIRONMENT);
    $envVarsEl.AppendChild($envVarEl);
    $webConfig.configuration.location["system.webServer"].aspNetCore.appendChild($envVarsEl);
    $webConfig.Save($path)
}
catch
{
    Write-Host "$($_.Exception.Message)"  -ForegroundColor Red
    Write-Host "$($_.InvocationInfo.PositionMessage)" -ForegroundColor Red
    exit 1
}