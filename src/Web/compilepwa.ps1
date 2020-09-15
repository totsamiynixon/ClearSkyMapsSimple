param([Parameter(Mandatory=$true)][string]$rootPath)

[PSCustomObject]$configJSON = Get-Content -Path ($rootPath + "pwasettings.json") | ConvertFrom-Json;

$version = $configJSON."Application__Version";

$manifestJSON = Get-Content -Path ($pwaRootPath + "Areas/PWA/manifest.json") | ConvertFrom-Json;
$manifestJSON."name" = $manifestJSON."name" + " " + $configJSON."Application__Environment";
$manifestJSON."short_name" = $manifestJSON."short_name" + " " + $configJSON."Application__Environment";
$manifestJSON | Add-Member -NotePropertyName gsm_sender_id -NotePropertyValue  $configJSON."FirebaseCloudMessaging__MessagingSenderId";

$manifestJSON | ConvertTo-Json -depth 100 | Out-File ($pwaRootPath  + "wwwroot/pwa/manifest.json");


$serviceWorker =  Get-Content -Path ($pwaRootPath  + "Areas/PWA/service-worker.js");
$serviceWorker = $serviceWorker -replace '%MessagingSenderId%', $configJSON."FirebaseCloudMessaging__MessagingSenderId";
$serviceWorker = $serviceWorker -replace '%Version%', $version;

$serviceWorker | Set-Content -Path ($pwaRootPath + "wwwroot/pwa/service-worker.js");
