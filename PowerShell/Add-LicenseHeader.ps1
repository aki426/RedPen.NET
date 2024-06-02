### Functions
function Test-ContainsHeader ($path, $header_str) {
    $origin = Get-Content $path -Encoding UTF8 | select -First 5
    foreach ($i in 0..2) {
        if ($origin[$i] -ne $header_str[$i]) {
            return $false
        }
    }

    return $true
}

function Concat-Header ($path, $header_str) {
    Write-Host $path -ForegroundColor Cyan
    Write-Host "(y/n)"
    $ans = Read-Host
    if ($ans.ToLower() -eq "y") {
        $origin = Get-Content $path -Encoding UTF8
        $header_str + $origin | Out-File $path -Encoding utf8
    }
}

### Loading
$header_str = Get-Content ..\LICENSE-HEADER -Encoding UTF8
#$header_str | select -First 3

# ls -Filter "*.cs" -Recurse ../RedPen.Net.Core
# ls -Filter "*.cs" -Recurse ../RedPen.Net.Core.Tests
# ls -Filter "*.cs" -Recurse ../RedPen.Net.CoreTests

### Process
foreach ($file in ls -Filter "*.cs" -Recurse ../RedPen.Net.Core) {
    if (-not (Test-ContainsHeader $file.FullName $header_str)) {
        Concat-Header $file.FullName $header_str
    }
}

foreach ($file in ls -Filter "*.cs" -Recurse ../RedPen.Net.Core.Tests) {
    if (-not (Test-ContainsHeader $file.FullName $header_str)) {
        Concat-Header $file.FullName $header_str
    }
}

foreach ($file in ls -Filter "*.cs" -Recurse ../RedPen.Net.CoreTests) {
    if (-not (Test-ContainsHeader $file.FullName $header_str)) {
        Concat-Header $file.FullName $header_str
    }
}
