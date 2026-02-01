$rootPath = "D:\TodosProyectos\CSHARP\MauiPdfGenerator"
$projectsToAnalyze = @("MauiPdfGenerator", "MauiPdfGenerator.SourceGenerators")

$allCsFiles = @()

foreach ($project in $projectsToAnalyze) {
    $projectPath = Join-Path $rootPath $project
    $files = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse | 
             Where-Object { $_.FullName -notmatch '\\(obj|bin)\\' }
    $allCsFiles += $files
}

Write-Host "Analizando $($allCsFiles.Count) archivos..."

$typeRegex = [regex]::new('^\s*(?:public|internal|private|protected)?\s*(?:abstract|sealed|static|partial)?\s*(class|interface|enum|struct|record)\s+(\w+)', [System.Text.RegularExpressions.RegexOptions]::Multiline)

$allTypes = @{}

foreach ($file in $allCsFiles) {
    $content = Get-Content $file.FullName -Raw
    $matches = $typeRegex.Matches($content)
    $types = @($matches | ForEach-Object { $_.Groups[2].Value })
    
    if ($types.Count -gt 0) {
        $allTypes[$file.FullName] = @{
            File = $file.Name
            Types = $types
        }
    }
}

Write-Host "`n=== TIPOS HUÉRFANOS ===" -ForegroundColor Cyan

$orphanTypes = @{}
$totalOrphans = 0

foreach ($filePath in $allTypes.Keys) {
    $data = $allTypes[$filePath]
    $orphans = @()
    
    foreach ($type in $data.Types) {
        $found = $false
        foreach ($searchFile in $allCsFiles.FullName) {
            if ($searchFile -eq $filePath) { continue }
            
            $result = Select-String -Path $searchFile -Pattern "\b$type\b" -Quiet
            if ($result) { $found = $true; break }
        }
        
        if (-not $found) {
            $orphans += $type
            $totalOrphans++
        }
    }
    
    if ($orphans.Count -gt 0) {
        $orphanTypes[$filePath] = $orphans
        Write-Host "`n$($data.File)" -ForegroundColor Yellow
        Write-Host "  Ruta: $filePath" -ForegroundColor DarkGray
        $orphans | ForEach-Object { Write-Host "  - $_" }
    }
}

Write-Host "`n=== RESUMEN ===" -ForegroundColor Green
Write-Host "Archivos analizados: $($allCsFiles.Count)"
$totalTypes = ($allTypes.Values | ForEach-Object { $_.Types.Count } | Measure-Object -Sum).Sum
Write-Host "Total tipos: $totalTypes"
Write-Host "Tipos huérfanos: $totalOrphans"
if ($totalTypes -gt 0) {
    $pct = [math]::Round(($totalOrphans / $totalTypes) * 100, 2)
    Write-Host "Porcentaje: $pct%"
}
