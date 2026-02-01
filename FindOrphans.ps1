# Script para encontrar tipos huérfanos en proyectos C#
$projectPaths = @(
    "D:\TodosProyectos\CSHARP\MauiPdfGenerator\MauiPdfGenerator",
    "D:\TodosProyectos\CSHARP\MauiPdfGenerator\MauiPdfGenerator.SourceGenerators"
)

# Obtener todos los archivos .cs excluyendo directorios obj/
$allCsFiles = @()
foreach ($projectPath in $projectPaths) {
    $files = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse -File | 
             Where-Object { $_.FullName -notmatch "\\obj\\" }
    $allCsFiles += $files
}

Write-Host "Analizando $($allCsFiles.Count) archivos .cs..." -ForegroundColor Cyan

# Diccionario para almacenar tipos encontrados: Key = Tipo, Value = Info (Archivo, Línea)
$allTypes = @{}

# Expresión regular para encontrar definiciones de tipos
$typeRegex = [regex]::new('^\s*(?:public|internal|private|protected)?\s*(?:sealed|abstract|static|partial)?\s*(class|interface|enum|struct|record)\s+(\w+)', [System.Text.RegularExpressions.RegexOptions]::Multiline)

# Paso 1: Extraer todas las definiciones de tipos
Write-Host "`nExtrayendo definiciones de tipos..." -ForegroundColor Yellow

foreach ($file in $allCsFiles) {
    $content = Get-Content -Path $file.FullName -Raw
    $matches = $typeRegex.Matches($content)
    
    foreach ($match in $matches) {
        $typeName = $match.Groups[2].Value
        $typeKind = $match.Groups[1].Value
        $lineNumber = ($content.Substring(0, $match.Index).Split("`n").Length)
        
        $allTypes[$typeName] = @{
            Name = $typeName
            Kind = $typeKind
            File = $file.Name
            FilePath = $file.FullName
            Line = $lineNumber
            Referenced = $false
            ReferencedBy = @()
        }
    }
}

Write-Host "Se encontraron $($allTypes.Count) tipos definidos." -ForegroundColor Green

# Paso 2: Buscar referencias en todos los archivos
Write-Host "`nBuscando referencias..." -ForegroundColor Yellow

foreach ($file in $allCsFiles) {
    $content = Get-Content -Path $file.FullName -Raw
    $fileNameWithoutExtension = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    
    foreach ($typeName in $allTypes.Keys) {
        # No contar como referencia si es el mismo archivo
        if ($allTypes[$typeName].FilePath -eq $file.FullName) {
            continue
        }
        
        # Buscar el tipo como palabra completa en el contenido
        if ($content -match "\b$typeName\b") {
            $allTypes[$typeName].Referenced = $true
            $allTypes[$typeName].ReferencedBy += $file.Name
        }
    }
}

# Paso 3: Organizar tipos huérfanos por archivo
Write-Host "`n`n========================================" -ForegroundColor Cyan
Write-Host "TIPOS HUÉRFANOS (No referenciados)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$orphanTypesByFile = @{}
$totalOrphans = 0

foreach ($typeInfo in $allTypes.Values) {
    if (-not $typeInfo.Referenced) {
        if (-not $orphanTypesByFile.ContainsKey($typeInfo.FilePath)) {
            $orphanTypesByFile[$typeInfo.FilePath] = @()
        }
        $orphanTypesByFile[$typeInfo.FilePath] += $typeInfo
        $totalOrphans++
    }
}

# Mostrar resultados organizados por archivo
if ($orphanTypesByFile.Count -eq 0) {
    Write-Host "`nNo se encontraron tipos huérfanos." -ForegroundColor Green
} else {
    foreach ($filePath in $orphanTypesByFile.Keys | Sort-Object) {
        $orphans = $orphanTypesByFile[$filePath]
        Write-Host "`n----------------------------------------" -ForegroundColor Gray
        Write-Host "Archivo: $($orphanTypesByFile[$filePath][0].File)" -ForegroundColor White
        Write-Host "Ruta: $filePath" -ForegroundColor DarkGray
        Write-Host "----------------------------------------" -ForegroundColor Gray
        
        foreach ($orphan in $orphans) {
            Write-Host "  - $($orphan.Kind): $($orphan.Name) (línea $($orphan.Line))" -ForegroundColor Yellow
        }
    }
}

# Paso 4: Resumen final
Write-Host "`n`n========================================" -ForegroundColor Cyan
Write-Host "RESUMEN" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total de tipos analizados: $($allTypes.Count)" -ForegroundColor White
Write-Host "Total de tipos huérfanos: $totalOrphans" -ForegroundColor $(if ($totalOrphans -gt 0) { "Yellow" } else { "Green" })

if ($allTypes.Count -gt 0) {
    $percentage = [math]::Round(($totalOrphans / $allTypes.Count) * 100, 2)
    Write-Host "Porcentaje de huérfanos: $percentage%" -ForegroundColor $(if ($percentage -gt 10) { "Red" } elseif ($percentage -gt 5) { "Yellow" } else { "Green" })
}

Write-Host "`n========================================" -ForegroundColor Cyan
