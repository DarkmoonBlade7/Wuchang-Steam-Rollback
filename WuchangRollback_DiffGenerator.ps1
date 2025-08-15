param(
    [string]$OldPath = "<Path to V 1.4 of the game ending in \Wuchang Fallen Feathers>",
    [string]$NewPath = "<Path to V 1.4 of the game ending in \Wuchang Fallen Feathers>",
    [string]$PatchPath = "<Provide a folder to store the diffs>"
)

$changedDir = Join-Path $PatchPath "changed"
$deletedDir = Join-Path $PatchPath "deleted"
$newFilesList = Join-Path $PatchPath "new_files.txt"

New-Item -ItemType Directory -Force -Path $changedDir | Out-Null
New-Item -ItemType Directory -Force -Path $deletedDir | Out-Null
Remove-Item $newFilesList -ErrorAction SilentlyContinue

$oldFiles = Get-ChildItem -Recurse $OldPath | Where-Object { -not $_.PSIsContainer }
$newFiles = Get-ChildItem -Recurse $NewPath | Where-Object { -not $_.PSIsContainer }
$oldMap = @{}
$newMap = @{}

foreach ($f in $oldFiles) {
    $rel = $f.FullName.Substring($OldPath.Length).TrimStart("\")
    $oldMap[$rel] = $f
}
foreach ($f in $newFiles) {
    $rel = $f.FullName.Substring($NewPath.Length).TrimStart("\")
    $newMap[$rel] = $f
}

# 1. Changed or deleted files
foreach ($rel in $oldMap.Keys) {
    if ($newMap.ContainsKey($rel)) {
        $oldHash = (Get-FileHash $oldMap[$rel].FullName -Algorithm SHA256).Hash
        $newHash = (Get-FileHash $newMap[$rel].FullName -Algorithm SHA256).Hash
        if ($oldHash -ne $newHash) {
            $dest = Join-Path $changedDir $rel
            New-Item -ItemType Directory -Force -Path (Split-Path $dest) | Out-Null
            Copy-Item $oldMap[$rel].FullName $dest -Force
        }
    } else {
        # Deleted in new version — keep copy
        $dest = Join-Path $deletedDir $rel
        New-Item -ItemType Directory -Force -Path (Split-Path $dest) | Out-Null
        Copy-Item $oldMap[$rel].FullName $dest -Force
    }
}

# 2. New files in update
foreach ($rel in $newMap.Keys) {
    if (-not $oldMap.ContainsKey($rel)) {
        Add-Content $newFilesList $rel
    }
}

(Get-FileHash "$NewPath\Project_Plague\Binaries\Win64\Project_Plague-Win64-Shipping.exe" -Algorithm SHA256).Hash | Out-File "$PatchPath\ProjectPlague_Hash.txt"

Write-Host "Patch created at $PatchPath"
