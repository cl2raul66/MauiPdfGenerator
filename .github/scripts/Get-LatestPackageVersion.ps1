<#
.SYNOPSIS
    Calcula la próxima versión de un paquete basándose en commits y artifacts/NuGet.

.DESCRIPTION
    Este script implementa la lógica de versionamiento para MauiPdfGenerator:
    1. Obtiene la versión actual desde GitHub Artifact (preferido) o NuGet API (fallback)
    2. Analiza commits para determinar tipo de bump (MAJOR/MINOR/PATCH)
    3. Cuenta PRs mergeados a development para el increment de preview
    4. Retorna la nueva versión calculada

.PARAMETER PackageName
    Nombre del paquete en NuGet (ej: "RandAMediaLabGroup.MauiPdfGenerator")

.PARAMETER Scope
    Alcance del cambio: "core" o "sourcegen"

.PARAMETER Commits
    Lista de mensajes de commits separados por coma

.PARAMETER PublishedAt
    Fecha de publicación de la versión actual (del artifact)

.EXAMPLE
    $result = & .github/scripts/Get-LatestPackageVersion.ps1 `
        -PackageName "RandAMediaLabGroup.MauiPdfGenerator" `
        -Scope "core" `
        -Commits "feat(core): add new feature"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$PackageName,

    [Parameter(Mandatory=$true)]
    [ValidateSet("core", "sourcegen")]
    [string]$Scope,

    [Parameter(Mandatory=$false)]
    [string]$Commits = "",

    [Parameter(Mandatory=$false)]
    [string]$PublishedAt = ""
)

$ErrorActionPreference = "Stop"
$script:VerbosePreference = "Continue"

function Write-Log {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Message,

        [Parameter(Mandatory=$false)]
        [ValidateSet("INFO", "WARN", "ERROR", "DEBUG")]
        [string]$Level = "INFO"
    )

    $timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
    $formattedMessage = "[$timestamp] [$Level] $Message"

    switch ($Level) {
        "ERROR" { Write-Host $formattedMessage -ForegroundColor Red }
        "WARN"  { Write-Host $formattedMessage -ForegroundColor Yellow }
        "DEBUG" { Write-Host $formattedMessage -ForegroundColor Cyan }
        default { Write-Host $formattedMessage }
    }

    # Escribir a archivo de log si existe la variable de entorno
    if ($env:GITHUB_ENV -and $Level -eq "ERROR") {
        Write-Output "VERSIONING_ERROR=$Message" >> $env:GITHUB_ENV
    }
}

function Get-GitHubArtifactVersion {
    <#
    .SYNOPSIS
        Descarga el artifact current-version.json y extrae la versión del paquete.
    #>

    param(
        [Parameter(Mandatory=$true)]
        [string]$PackageName
    )

    Write-Log -Message "Intentando obtener versión desde GitHub Artifact..." -Level "DEBUG"

    try {
        # Listar artifacts con nombre "current-version"
        $artifactsResponse = gh api repos/:owner/:repo/actions/artifacts `
            --jq '.artifacts[] | select(.name == "current-version")' 2>$null

        if ([string]::IsNullOrWhiteSpace($artifactsResponse)) {
            Write-Log -Message "No se encontró artifact 'current-version'" -Level "WARN"
            return $null
        }

        # Obtener el artifact más reciente
        $artifactId = ($artifactsResponse | Select-Object -First 1).id

        if (-not $artifactId) {
            Write-Log -Message "No se pudo del artifact" - obtener IDLevel "WARN"
            return $null
        }

        # Descargar artifact
        Write-Log -Message "Descargando artifact ID: $artifactId" -Level "DEBUG"
        $downloadUrl = gh api "repos/:owner/:repo/actions/artifacts/$artifactId/zip" --header "Accept: application/zip" -L 2>$null

        # Usar método alternativo: download con curl
        $zipPath = "/tmp/current-version.zip"
        $headers = @{
            "Authorization" = "Bearer $($env:GITHUB_TOKEN)"
            "Accept" = "application/vnd.github+json"
        }
        $artifactDownloadUrl = gh api "repos/:owner/:repo/actions/artifacts/$artifactId" --jq '.archive_download_url' 2>$null

        if ([string]::IsNullOrWhiteSpace($artifactDownloadUrl)) {
            Write-Log -Message "No se pudo obtener URL de descarga del artifact" -Level "WARN"
            return $null
        }

        # Intentar descargar usando la API
        $response = Invoke-RestMethod -Uri $artifactDownloadUrl -Headers $headers -Method Get
        # Nota: La descarga real de artifacts requiere autenticación adicional

        # Como alternativa, usar gh CLI para obtener el contenido del artifact
        Write-Log -Message "Obteniendo contenido del artifact..." -Level "DEBUG"

        # Este método es más simple: descargar y extraer
        $tempDir = "/tmp/version-artifact-$(Get-Random -Minimum 1000 -Maximum 9999)"
        New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

        # Usar gh artifact download (disponible en GitHub CLI 2.49+)
        $downloadResult = gh artifact download current-version --pattern "current-version.json" --dir $tempDir 2>&1

        if ($LASTEXITCODE -ne 0) {
            Write-Log -Message "No se pudo descargar artifact: $downloadResult" -Level "WARN"
            Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
            return $null
        }

        # Leer el archivo JSON
        $jsonPath = Join-Path $tempDir "current-version.json"
        if (Test-Path $jsonPath) {
            $jsonContent = Get-Content $jsonPath -Raw | ConvertFrom-Json

            # Extraer versión del paquete específico
            $version = if ($PackageName -like "*SourceGenerators*") {
                $jsonContent.sourcegen.version
            } else {
                $jsonContent.core.version
            }

            $publishedAt = if ($PackageName -like "*SourceGenerators*") {
                $jsonContent.sourcegen.publishedAt
            } else {
                $jsonContent.core.publishedAt
            }

            Write-Log -Message "Versión obtenida desde artifact: $version (publicado: $publishedAt)" -Level "DEBUG"

            Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue

            return @{
                Version = $version
                PublishedAt = $publishedAt
                Source = "artifact"
            }
        }

        Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
        return $null

    } catch {
        Write-Log -Message "Error descargando artifact: $($_.Exception.Message)" -Level "WARN"
        return $null
    }
}

function Get-LatestNuGetVersion {
    <#
    .SYNOPSIS
        Consulta la NuGet API REST para obtener la versión más reciente de un paquete.
    #>

    param(
        [Parameter(Mandatory=$true)]
        [string]$PackageName
    )

    Write-Log -Message "Consultando NuGet API para: $PackageName" -Level "DEBUG"

    $lowerName = $PackageName.ToLower()
    $url = "https://api.nuget.org/v3/registration5-gz-semver2/$lowerName/index.json"

    try {
        Write-Log -Message "Endpoint: $url" -Level "DEBUG"

        $response = Invoke-RestMethod -Uri $url -ErrorAction Stop

        $allVersions = @()

        # Recorrer todas las páginas de versiones
        foreach ($page in $response.items) {
            foreach ($item in $page.items) {
                $allVersions += [PSCustomObject]@{
                    Version   = $item.catalogEntry.version
                    Published = [datetime]$item.catalogEntry.published
                }
            }
        }

        if ($allVersions.Count -eq 0) {
            Write-Log -Message "No se encontraron versiones para $PackageName" -Level "WARN"
            return $null
        }

        # Ordenar por fecha descendente y obtener la más reciente
        $latestVersion = $allVersions | Sort-Object Published -Descending | Select-Object -First 1

        Write-Log -Message "Última versión en NuGet: $($latestVersion.Version) (publicado: $($latestVersion.Published))" -Level "DEBUG"

        return @{
            Version = $latestVersion.Version
            PublishedAt = $latestVersion.Published.ToString("yyyy-MM-ddTHH:mm:ssZ")
            Source = "nuget"
        }

    } catch {
        Write-Log -Message "Error consultando NuGet: $($_.Exception.Message)" -Level "WARN"
        return $null
    }
}

function Calculate-VersionBump {
    <#
    .SYNOPSIS
        Analiza los commits para determinar el tipo de bump (MAJOR/MINOR/PATCH).
    #>

    param(
        [Parameter(Mandatory=$true)]
        [string]$Commits,

        [Parameter(Mandatory=$true)]
        [string]$Scope
    )

    Write-Log -Message "Analizando commits para scope: $Scope" -Level "DEBUG"

    $bumpType = "none"
    $commitList = $Commits.Split(',', [StringSplitOptions]::RemoveEmptyEntries)

    foreach ($commit in $commitList) {
        $commit = $commit.Trim()

        # Detectar breaking changes con scope explícito
        if ($commit -match "^(feat|fix)(\($Scope\))?!:") {
            Write-Log -Message "Breaking change detectado: $commit" -Level "DEBUG"
            $bumpType = "major"
        }
        # Detectar features
        elseif ($commit -match "^feat\($Scope\):") {
            Write-Log -Message "Feature detectado: $commit" -Level "DEBUG"
            if ($bumpType -ne "major") {
                $bumpType = "minor"
            }
        }
        # Detectar fixes
        elseif ($commit -match "^fix\($Scope\):") {
            Write-Log -Message "Fix detectado: $commit" -Level "DEBUG"
            if ($bumpType -eq "none") {
                $bumpType = "patch"
            }
        }
    }

    Write-Log -Message "Tipo de bump determinado: $bumpType" -Level "INFO"

    return $bumpType
}

function Calculate-PreviewIncrement {
    <#
    .SYNOPSIS
        Cuenta los PRs mergeados a development desde la fecha de publicación.
    #>

    param(
        [Parameter(Mandatory=$true)]
        [string]$PublishedAt
    )

    Write-Log -Message "Calculando increment desde: $PublishedAt" -Level "DEBUG"

    if ([string]::IsNullOrWhiteSpace($PublishedAt)) {
        Write-Log -Message "No hay fecha de publicación, usando 1" -Level "WARN"
        return 1
    }

    try {
        # Convertir fecha a formato GitHub CLI
        $sinceDate = [datetime]$PublishedAt
        $sinceStr = $sinceDate.ToString("yyyy-MM-ddTHH:mm:ssZ")

        Write-Log -Message "Buscando PRs mergeados desde: $sinceStr" -Level "DEBUG"

        # Contar PRs mergeados a development desde la fecha
        $prs = gh pr list `
            --base development `
            --state merged `
            --merged "gte=$sinceStr" `
            --json number 2>&1

        if ($LASTEXITCODE -ne 0) {
            Write-Log -Message "Error consultando PRs: $prs" -Level "WARN"
            return 1
        }

        # Parsear respuesta JSON
        $prCount = ($prs | ConvertFrom-Json).Count

        Write-Log -Message "PRs mergeados desde última versión: $prCount" -Level "INFO"

        return [Math]::Max(1, $prCount)

    } catch {
        Write-Log -Message "Error calculando increment: $($_.Exception.Message)" -Level "WARN"
        return 1
    }
}

function Apply-VersionBump {
    <#
    .SYNOPSIS
        Aplica el bump a la versión base.
    #>

    param(
        [Parameter(Mandatory=$true)]
        [string]$Version,

        [Parameter(Mandatory=$true)]
        [string]$BumpType
    )

    Write-Log -Message "Aplicando bump '$BumpType' a versión: $Version" -Level "DEBUG"

    # Extraer componentes de la versión
    $versionWithoutPreview = $Version -replace "-preview-\d+$", ""
    $parts = $versionWithoutPreview.Split('.')

    if ($parts.Count -ne 3) {
        Write-Log -Message "Formato de versión inválido: $Version" -Level "ERROR"
        throw "Versión inválida: $Version"
    }

    $major = [int]$parts[0]
    $minor = [int]$parts[1]
    $patch = [int]$parts[2]

    # Aplicar bump
    switch ($BumpType) {
        "major" {
            $major += 1
            $minor = 0
            $patch = 0
        }
        "minor" {
            $minor += 1
            $patch = 0
        }
        "patch" {
            $patch += 1
        }
        "none" {
            # No hay cambios, retornar versión actual
            Write-Log -Message "No hay cambios que requieran version bump" -Level "INFO"
            return $null
        }
    }

    $newVersion = "$major.$minor.$patch"

    Write-Log -Message "Nueva versión base: $newVersion" -Level "DEBUG"

    return $newVersion
}

function Get-LatestPackageVersion {
    <#
    .SYNOPSIS
        Función principal que orquesta todo el proceso de versionamiento.
    #>

    param(
        [Parameter(Mandatory=$true)]
        [string]$PackageName,

        [Parameter(Mandatory=$true)]
        [string]$Scope,

        [Parameter(Mandatory=$false)]
        [string]$Commits = ""
    )

    Write-Log -Message "============================================" -Level "INFO"
    Write-Log -Message "Iniciando cálculo de versión para: $PackageName" -Level "INFO"
    Write-Log -Message "Scope: $Scope" -Level "INFO"
    Write-Log -Message "============================================" -Level "INFO"

    # 1. Obtener versión actual
    Write-Log -Message "[PASO 1] Obteniendo versión actual..." -Level "INFO"

    $currentVersion = Get-GitHubArtifactVersion -PackageName $PackageName

    if (-not $currentVersion) {
        Write-Log -Message "Artifact no disponible, consultando NuGet..." -Level "WARN"
        $currentVersion = Get-LatestNuGetVersion -PackageName $PackageName
    }

    if (-not $currentVersion) {
        Write-Log -Message "No se encontró versión en ninguna fuente. Usando 1.0.0." -Level "WARN"
        $currentVersion = @{
            Version = "1.0.0"
            PublishedAt = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
            Source = "default"
        }
    }

    Write-Log -Message "Versión actual: $($currentVersion.Version) (source: $($currentVersion.Source))" -Level "INFO"

    # 2. Analizar commits para determinar bump
    Write-Log -Message "[PASO 2] Analizando commits para bump..." -Level "INFO"

    $bumpType = Calculate-VersionBump -Commits $Commits -Scope $Scope

    if ($bumpType -eq "none") {
        Write-Log -Message "No hay cambios relevantes. No se publicará nueva versión." -Level "INFO"
        return @{
            Version = $null
            PublishedAt = $null
            BumpType = "none"
            Increment = 0
            Source = $currentVersion.Source
            ShouldPublish = $false
        }
    }

    # 3. Aplicar bump a la versión
    Write-Log -Message "[PASO 3] Aplicando bump..." -Level "INFO"

    $baseVersion = Apply-VersionBump -Version $currentVersion.Version -BumpType $bumpType

    if (-not $baseVersion) {
        return @{
            Version = $null
            PublishedAt = $null
            BumpType = $bumpType
            Increment = 0
            Source = $currentVersion.Source
            ShouldPublish = $false
        }
    }

    # 4. Calcular increment
    Write-Log -Message "[PASO 4] Calculando increment..." -Level "INFO"

    $increment = Calculate-PreviewIncrement -PublishedAt $currentVersion.PublishedAt

    # 5. Construir versión final
    $finalVersion = "$baseVersion-preview-$increment"

    Write-Log -Message "============================================" -Level "INFO"
    Write-Log -Message "RESULTADO FINAL" -Level "INFO"
    Write-Log -Message "  Paquete: $PackageName" -Level "INFO"
    Write-Log -Message "  Versión actual: $($currentVersion.Version)" -Level "INFO"
    Write-Log -Message "  Bump: $bumpType" -Level "INFO"
    Write-Log -Message "  Increment: $increment" -Level "INFO"
    Write-Log -Message "  Nueva versión: $finalVersion" -Level "INFO"
    Write-Log -Message "============================================" -Level "INFO"

    return @{
        Version = $finalVersion
        PublishedAt = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
        BumpType = $bumpType
        Increment = $increment
        Source = $currentVersion.Source
        ShouldPublish = $true
    }
}

# ============================================================================
# EJECUCIÓN PRINCIPAL
# ============================================================================

try {
    $result = Get-LatestPackageVersion `
        -PackageName $PackageName `
        -Scope $Scope `
        -Commits $Commits

    # Escribir outputs para GitHub Actions
    Write-Output "version=$($result.Version)"
    Write-Output "published_at=$($result.PublishedAt)"
    Write-Output "bump_type=$($result.BumpType)"
    Write-Output "increment=$($result.Increment)"
    Write-Output "source=$($result.Source)"
    Write-Output "should_publish=$($result.ShouldPublish)"

    # Si no debe publicar, escribir mensaje y salir
    if (-not $result.ShouldPublish) {
        Write-Log -Message "No hay cambios que requieran publicación. Saliendo." -Level "INFO"
        exit 0
    }

    exit 0

} catch {
    Write-Log -Message "ERROR CRÍTICO: $($_.Exception.Message)" -Level "ERROR"
    Write-Log -Message "Stack trace: $($_.ScriptStackTrace)" -Level "ERROR"
    exit 1
}
