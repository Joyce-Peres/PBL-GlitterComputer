$path = 'c:\Temp\PBL-GlitterComputer\PBL\PBL\Controllers\AquarioController.cs'
$s = Get-Content -Path $path
for ($i=0; $i -lt $s.Length; $i++) {
    $num = $i + 1
    Write-Output ("{0,4}: {1}" -f $num, $s[$i])
}