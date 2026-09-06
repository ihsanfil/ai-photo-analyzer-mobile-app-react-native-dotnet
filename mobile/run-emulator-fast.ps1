$ErrorActionPreference = 'Stop'
$adb = 'E:\Android\Sdk\platform-tools\adb.exe'
$apk = Join-Path $PSScriptRoot 'android\app\build\outputs\apk\release\app-release.apk'
if (-not (Test-Path $apk)) { throw "APK bulunamadı: $apk" }
if ((& $adb get-state 2>$null) -ne 'device') { throw 'Emülatör hazır değil. Önce Android Studio içinden Istanbul_API35 emülatörünü başlatın.' }
& $adb install -r $apk | Out-Null
& $adb shell am force-stop com.helloworld
& $adb shell monkey -p com.helloworld 1 | Out-Null
Write-Host 'AI Photo Analyzer emülatörde başlatıldı.'
