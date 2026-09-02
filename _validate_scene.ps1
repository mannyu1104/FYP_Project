# Validate new YAML blocks in MainGame.unity
$lines = Get-Content 'Assets\Scenes\MainGame.unity'
$newIds = @(2100000001,2100000002,2100000003,2100000011,2100000012,2100000013,2100000014,2100000015,2100000016,2100000017,2100000018,2100000019,2100000021,2100000022,2100000023,2100000024,2100000025,2100000026,2100000027,2100000028,2100000029,2100000031,2100000032,2100000033,2100000034,2100000035,2100000036,2100000037,2100000038,2100000039,2100000041,2100000042,2100000043,2100000044,2100000045,2100000046,2100000047,2100000048,2100000049,2100000051,2100000052,2100000053,2100000054,2100000055,2100000056,2100000057,2100000058,2100000059)
$typeNames = @('GameObject:','RectTransform:','Transform:','MonoBehaviour:','CanvasRenderer:','MeshFilter:','MeshRenderer:','Canvas:','CanvasScaler:','GraphicRaycaster:','Light:','SceneRoots:')

$blocks = @()
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match '^--- !u!\d+ &(\d+)$') {
        $id = [long]$Matches[1]
        $end = $lines.Count - 1
        for ($j = $i + 1; $j -lt $lines.Count; $j++) {
            if ($lines[$j] -match '^--- ') { $end = $j - 1; break }
        }
        $blocks += [pscustomobject]@{ id = $id; start = $i; end = $end }
    }
}
Write-Host "total blocks: $($blocks.Count)"

$problems = @()
foreach ($b in ($blocks | Where-Object { $newIds -contains $_.id })) {
    $hdr = $lines[$b.start+1].Trim()
    if ($hdr -notin $typeNames) { $problems += "BLOCK $($b.id) bad type header: [$hdr]" }
    for ($k = $b.start + 2; $k -le $b.end; $k++) {
        $t = $lines[$k]
        if ($t -eq '') { continue }
        if ($t.StartsWith('- ')) { continue }
        if (-not $t.StartsWith('  ')) { $problems += "BLOCK $($b.id) line $($k+1): no indent: [$t]" }
    }
}
if ($problems.Count -eq 0) { Write-Host 'ALL NEW BLOCKS OK' } else { $problems | ForEach-Object { Write-Host $_ } }