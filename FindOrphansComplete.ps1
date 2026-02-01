$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$csFiles = Get-ChildItem -Path $projectRoot -Filter "*.cs" -Recurse -File

$results = @{
    OrphanFiles = @()
    OrphanTypes = @()
    OrphanMethods = @()
    OrphanProperties = @()
}

Write-Host "Analizando proyecto en: $projectRoot" -ForegroundColor Cyan
Write-Host "Encontrados $($csFiles.Count) archivos .cs" -ForegroundColor Cyan
Write-Host ""

function Get-TypeDefinitions {
    param([string]$content)
    
    $patterns = @(
        '(?m)^\s*(public|internal|protected|private)?\s*(partial\s+)?(class|interface|struct|enum|record)\s+(\w+)'
    )
    
    $types = @()
    foreach ($pattern in $patterns) {
        $matches = [regex]::Matches($content, $pattern)
        foreach ($match in $matches) {
            $access = if ($match.Groups[1].Value) { $match.Groups[1].Value } else { "internal" }
            $keyword = $match.Groups[3].Value
            $name = $match.Groups[4].Value
            
            if ($access -in @("public", "internal") -and $keyword -eq "interface") {
                continue
            }
            
            $types += [PSCustomObject]@{
                Name = $name
                Access = $access
                Keyword = $keyword
                FullName = "$keyword $name"
            }
        }
    }
    return $types
}

function Get-MethodDefinitions {
    param([string]$content)
    
    $patterns = @(
        '(?m)^\s*(public|protected)\s+(?:override\s+)?(?:virtual\s+)?(?:async\s+)?(?:static\s+)?(?:new\s+)?[\w<>[\],\s]+\s+(\w+)\s*\(',
        '(?m)^\s*(public|protected)\s+(?:override\s+)?(?:virtual\s+)?(?:async\s+)?(?:static\s+)?(?:new\s+)?\w+\s+(\w+)\s*\('
    )
    
    $methods = @()
    foreach ($pattern in $patterns) {
        $matches = [regex]::Matches($content, $pattern)
        foreach ($match in $matches) {
            $access = $match.Groups[1].Value
            $name = $match.Groups[2].Value
            
            if ($name -match '^(get|set|add|remove)$') { continue }
            
            $methods += [PSCustomObject]@{
                Name = $name
                Access = $access
            }
        }
    }
    return $methods
}

function Get-PropertyDefinitions {
    param([string]$content)
    
    $patterns = @(
        '(?m)^\s*(public|protected)\s+[\w<>[\],\s]+\s+(\w+)\s*\{',
        '(?m)^\s*(public|protected)\s+(?:override\s+)?(?:virtual\s+)?[\w<>[\],\s]+\s+(\w+)\s*\{'
    )
    
    $properties = @()
    foreach ($pattern in $patterns) {
        $matches = [regex]::Matches($content, $pattern)
        foreach ($match in $matches) {
            $access = $match.Groups[1].Value
            $name = $match.Groups[2].Value
            
            $properties += [PSCustomObject]@{
                Name = $name
                Access = $access
            }
        }
    }
    return $properties
}

function Is-ReferencedInOtherFiles {
    param(
        [string]$name,
        [string]$currentFile,
        [string[]]$allFiles
    )
    
    foreach ($file in $allFiles) {
        if ($file -eq $currentFile) { continue }
        
        $matches = Select-String -Path $file -Pattern "\b$name\b" -ErrorAction SilentlyContinue
        if ($matches) {
            return $true
        }
    }
    return $false
}

function Get-FileImports {
    param([string]$filePath)
    
    $content = Get-Content $filePath -Raw
    $imports = @()
    
    $matches = [regex]::Matches($content, '(?m)^\s*using\s+([^;]+);')
    foreach ($match in $matches) {
        $import = $match.Groups[1].Value.Trim()
        if ($import -notmatch '^(System|Microsoft|Maui|Android|Apple|iOS)') {
            $imports += $import
        }
    }
    
    return $imports
}

$fileNames = $csFiles | Select-Object -ExpandProperty FullName
$fileBasenames = @{}

foreach ($file in $csFiles) {
    $basename = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    $fileBasenames[$file.FullName] = $basename
}

Write-Host "1. Buscando archivos completos huérfanos..." -ForegroundColor Yellow

foreach ($file in $csFiles) {
    $basename = $fileBasenames[$file.FullName]
    $isReferenced = $false
    
    foreach ($otherFile in $csFiles) {
        if ($otherFile.FullName -eq $file.FullName) { continue }
        
        $imports = Get-FileImports $otherFile.FullName
        foreach ($import in $imports) {
            $importName = $import -replace '.*\.'
            if ($importName -eq $basename -or $import -eq $basename) {
                $isReferenced = $true
                break
            }
        }
        
        if ($isReferenced) { break }
    }
    
    if (-not $isReferenced) {
        $results.OrphanFiles += $file.FullName.Replace($projectRoot, "").Trim("\")
    }
}

Write-Host "   Encontrados $($results.OrphanFiles.Count) archivos huérfanos" -ForegroundColor Gray
Write-Host ""

Write-Host "2. Buscando tipos huérfanos..." -ForegroundColor Yellow

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    $types = Get-TypeDefinitions $content
    
    foreach ($type in $types) {
        if ($type.Access -eq "public" -or $type.Keyword -eq "interface") {
            continue
        }
        
        if ($content -match "\[Generator\]") {
            continue
        }
        
        $isReferenced = Is-ReferencedInOtherFiles $type.Name $file.FullName $fileNames
        
        if (-not $isReferenced) {
            $relativePath = $file.FullName.Replace($projectRoot, "").Trim("\")
            $results.OrphanTypes += [PSCustomObject]@{
                File = $relativePath
                Type = $type.FullName
            }
        }
    }
}

Write-Host "   Encontrados $($results.OrphanTypes.Count) tipos huérfanos" -ForegroundColor Gray
Write-Host ""

Write-Host "3. Buscando métodos huérfanos..." -ForegroundColor Yellow

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    $methods = Get-MethodDefinitions $content
    
    foreach ($method in $methods) {
        if ($content -match "\[Test\]|\[Theory\]|\[Fact\]") {
            continue
        }
        
        $isReferenced = Is-ReferencedInOtherFiles $method.Name $file.FullName $fileNames
        
        if (-not $isReferenced) {
            $relativePath = $file.FullName.Replace($projectRoot, "").Trim("\")
            $results.OrphanMethods += [PSCustomObject]@{
                File = $relativePath
                Method = $method.Name
                Access = $method.Access
            }
        }
    }
}

Write-Host "   Encontrados $($results.OrphanMethods.Count) métodos huérfanos" -ForegroundColor Gray
Write-Host ""

Write-Host "4. Buscando propiedades huérfanas..." -ForegroundColor Yellow

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    $properties = Get-PropertyDefinitions $content
    
    foreach ($property in $properties) {
        $isReferenced = Is-ReferencedInOtherFiles $property.Name $file.FullName $fileNames
        
        if (-not $isReferenced) {
            $relativePath = $file.FullName.Replace($projectRoot, "").Trim("\")
            $results.OrphanProperties += [PSCustomObject]@{
                File = $relativePath
                Property = $property.Name
                Access = $property.Access
            }
        }
    }
}

Write-Host "   Encontradas $($results.OrphanProperties.Count) propiedades huérfanas" -ForegroundColor Gray
Write-Host ""

Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "RESULTADOS DETALLADOS" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host ""

if ($results.OrphanFiles.Count -gt 0) {
    Write-Host "ARCHIVOS COMPLETOS HUÉRFANOS" -ForegroundColor Magenta
    Write-Host "-" * 80 -ForegroundColor Magenta
    foreach ($file in $results.OrphanFiles | Sort-Object) {
        Write-Host "  $file" -ForegroundColor White
    }
    Write-Host ""
}

if ($results.OrphanTypes.Count -gt 0) {
    Write-Host "TIPOS HUÉRFANOS" -ForegroundColor Magenta
    Write-Host "-" * 80 -ForegroundColor Magenta
    
    $grouped = $results.OrphanTypes | Group-Object File
    foreach ($group in $grouped | Sort-Object Name) {
        Write-Host "  $($group.Name):" -ForegroundColor Cyan
        foreach ($type in $group.Group | Sort-Object Type) {
            Write-Host "    - $($type.Type)" -ForegroundColor White
        }
    }
    Write-Host ""
}

if ($results.OrphanMethods.Count -gt 0) {
    Write-Host "MÉTODOS HUÉRFANOS" -ForegroundColor Magenta
    Write-Host "-" * 80 -ForegroundColor Magenta
    
    $grouped = $results.OrphanMethods | Group-Object File
    foreach ($group in $grouped | Sort-Object Name) {
        Write-Host "  $($group.Name):" -ForegroundColor Cyan
        foreach ($method in $group.Group | Sort-Object Method) {
            Write-Host "    - $($method.Access) $($method.Method)()" -ForegroundColor White
        }
    }
    Write-Host ""
}

if ($results.OrphanProperties.Count -gt 0) {
    Write-Host "PROPIEDADES HUÉRFANAS" -ForegroundColor Magenta
    Write-Host "-" * 80 -ForegroundColor Magenta
    
    $grouped = $results.OrphanProperties | Group-Object File
    foreach ($group in $grouped | Sort-Object Name) {
        Write-Host "  $($group.Name):" -ForegroundColor Cyan
        foreach ($property in $group.Group | Sort-Object Property) {
            Write-Host "    - $($property.Access) $($property.Property)" -ForegroundColor White
        }
    }
    Write-Host ""
}

Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "RESUMEN DE ESTADÍSTICAS" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host ""

Write-Host "Total de archivos .cs analizados: $($csFiles.Count)" -ForegroundColor White
Write-Host ""
Write-Host "Archivos completos huérfanos: $($results.OrphanFiles.Count)" -ForegroundColor $(if ($results.OrphanFiles.Count -gt 0) { "Red" } else { "Green" })
Write-Host "Tipos huérfanos: $($results.OrphanTypes.Count)" -ForegroundColor $(if ($results.OrphanTypes.Count -gt 0) { "Red" } else { "Green" })
Write-Host "Métodos huérfanos: $($results.OrphanMethods.Count)" -ForegroundColor $(if ($results.OrphanMethods.Count -gt 0) { "Red" } else { "Green" })
Write-Host "Propiedades huérfanas: $($results.OrphanProperties.Count)" -ForegroundColor $(if ($results.OrphanProperties.Count -gt 0) { "Red" } else { "Green" })
Write-Host ""

$totalOrphans = $results.OrphanFiles.Count + $results.OrphanTypes.Count + $results.OrphanMethods.Count + $results.OrphanProperties.Count
Write-Host "TOTAL DE ELEMENTOS HUÉRFANOS: $totalOrphans" -ForegroundColor $(if ($totalOrphans -gt 0) { "Yellow" } else { "Green" })

Write-Host ""
Write-Host "Análisis completado." -ForegroundColor Green
