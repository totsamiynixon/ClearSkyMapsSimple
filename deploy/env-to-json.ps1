param([Parameter(Mandatory=$true)][string]$outputPath, [string]$prefix = $null)
try
{
	$prefix = $prefix 
	
    $jsonRequest = @{};
    $envVars = get-childitem env:\;
	$envVars | ForEach-Object {
	    $key = $_.Key;
		if($prefix){
		    if(!$_.Key.StartsWith($prefix)) {
			    return;
			}
			$key = $_.Key.Remove(0, $prefix.Length);
			$key = $key.Replace("__", ":");
		}
		$jsonRequest[$key] = $_.Value;
	}
	
	Write-Host "Generated result: " -ForegroundColor Yellow
	Write-Host ($jsonRequest | Format-Table | Out-String) -ForegroundColor Yellow
	
    $jsonRequest | ConvertTo-Json -depth 100 | Out-File $outputPath;
}
catch
{
    Write-Host "$($_.Exception.Message)"  -ForegroundColor Red
    Write-Host "$($_.InvocationInfo.PositionMessage)" -ForegroundColor Red
    exit 1
}