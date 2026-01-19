# Guía del Desarrollador - MauiPdfGenerator

Esta guía organiza todo lo que necesitas saber para trabajar en el proyecto, desde planificar hasta publicar.

---

## Índice Rápido

| ¿Qué quiero hacer? | Ir a sección |
|--------------------|--------------|
| Planificar una nueva funcionalidad | [1. Planificación](#1-planificación-roadmap-e-issues) |
| Crear un Issue | [1.2 Crear Issues](#12-crear-issues) |
| Hacer un commit | [2. Desarrollo](#2-desarrollo-commits) |
| Entender el versionado | [3. Publicación](#3-publicación-versionado) |
| Ver comandos útiles | [4. Referencia Rápida](#4-referencia-rápida) |

---

## 1. Planificación (Roadmap e Issues)

### 1.1 Roadmap del Proyecto

El **GitHub Project "MauiPdfGenerator Roadmap"** es tu punto de partida para ver qué hay pendiente.

```
Roadmap (Vista general)
    └── Milestones (Versiones objetivo)
            └── Issues (Tareas concretas)
                    └── Branch → Commits → PR → Merge
```

### 1.2 Crear Issues

Usa las plantillas disponibles según el tipo de trabajo:

| Plantilla | Cuándo usarla | Label automático |
|-----------|---------------|------------------|
| ✨ Nueva Funcionalidad | Agregar algo nuevo | `feat` |
| 🐞 Reporte de Error | Corregir un bug | `fix` |
| 🧰 Trabajo Interno | Docs, refactor, tests, CI, etc. | `chore` |

> [!TIP]
> La plantilla "Trabajo Interno" incluye un **selector de tipo** para indicar si es `docs`, `test`, `ci`, etc. Esto te ayudará a recordar qué prefijo usar en el commit.

### 1.3 Plan de Implementación

Cada plantilla tiene un campo **"Plan de Implementación"** para desglosar el trabajo:

> **Ejemplo:**
> - [ ] `feat(core)`: Crear modelos de datos
> - [ ] `feat(sourcegen)`: Actualizar generador
> - [ ] `docs(internal-task)`: Agregar ejemplo al README

### 1.4 Flujo de Trabajo Completo

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Issue      │ ──▶ │  Branch     │ ──▶ │  Commits    │ ──▶ │  PR         │ ──▶ │  Merge      │
│  planificar │     │  desde dev  │     │  (varios)   │     │  validar CI │     │  cierra todo│
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
```

**Paso a paso:**

1. **Selecciona Issue** del Roadmap → Muévelo a "In Progress"
2. **Crea branch** desde `development`:
   ```bash
   git checkout development
   git pull origin development
   git checkout -b feature/42-soporte-tablas
   ```
3. **Trabaja con commits** libres (no cierran nada todavía)
4. **Crea PR** hacia `development` — en la descripción escribe:
   ```markdown
   Closes #42
   ```
5. **CI valida** — si falla, haz más commits en el mismo branch
6. **Merge** — el Issue se cierra automáticamente

> [!IMPORTANT]
> **¿Por qué branch por Issue?**
> - Los commits en tu branch NO disparan releases preview
> - Puedes pausar y retomar cuando quieras
> - Solo al hacer merge a `development` se evalúa el versionado

---

## 2. Desarrollo (Commits)

### 2.1 Configuración Inicial

Configura la plantilla de commits (solo una vez):

```bash
git config commit.template .gitmessage
```

Ahora cada `git commit` (sin `-m`) mostrará la estructura correcta.

### 2.2 Estructura del Commit

```
<tipo>[alcance][!]: <descripción>

[cuerpo opcional]

[footer opcional]
```

**Ejemplo:**
```bash
feat(core): add PDF table rendering support
```

### 2.3 Tipos de Commit

#### Afectan Versionado (generan release)

| Tipo | Impacto | Cuándo usarlo |
|------|---------|---------------|
| `feat:` | MINOR (+0.1.0) | Nueva funcionalidad |
| `fix:` | PATCH (+0.0.1) | Corrección de bug |
| `feat!:` / `fix!:` | MAJOR (+1.0.0) | Cambio incompatible (breaking) |

#### NO Afectan Versionado (sin release)

| Tipo | Descripción |
|------|-------------|
| `docs:` | Documentación |
| `test:` | Tests |
| `refactor:` | Reestructurar código sin cambiar comportamiento |
| `perf:` | Mejoras de rendimiento |
| `build:` | Sistema de build |
| `ci:` | Configuración CI/CD |
| `chore:` | Mantenimiento general / mixto |

> [!NOTE]
> Usa `chore:` solo cuando mezcles varios tipos (ej: actualizar deps + arreglar linting). Si es una tarea enfocada, usa el tipo específico.

### 2.4 Alcances (Scopes)

| Alcance | Carpetas | ¿Publica paquete? |
|---------|----------|-------------------|
| `core` | `MauiPdfGenerator/` (incluye Diagnostics) | ✅ Sí |
| `sourcegen` | `MauiPdfGenerator.SourceGenerators/` | ✅ Sí |
| `internal-task` | `Sample/`, `.github/`, `Docs/` | ❌ No |

> [!IMPORTANT]
> **Regla de Oro:** Un commit = Un alcance. Si un Issue afecta múltiples componentes, haz un commit por cada uno.

### 2.5 Ejemplo: Issue que afecta varios componentes

**Issue:** "Agregar soporte de tablas"

❌ **Incorrecto:**
```bash
git commit -m "feat: add table support everywhere"
```

✅ **Correcto:**
```bash
git commit -m "feat(core): define Table entity models"
git commit -m "feat(sourcegen): update generator for tables"
```

---

## 3. Publicación (Versionado)

### 3.1 Arquitectura de Versionamiento

El sistema de versionamiento de MauiPdfGenerator usa una arquitectura de **fallback en cascada** para determinar la versión actual de cada paquete:

```
┌─────────────────────────────────────────────────────────────────┐
│                    FUENTES DE VERSIÓN                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. GitHub Artifact (MÁS RÁPIDO)                                 │
│     └─ Archivo: current-version.json                             │
│     └─ Contenido: { core: { version, publishedAt }, ... }       │
│     └─ Retención: 90 días                                        │
│                                                                 │
│  2. NuGet API REST (FALLBACK)                                    │
│     └─ Endpoint: api.nuget.org/v3/registration5-gz-semver2/     │
│     └─ Obtiene la versión más reciente (stable o preview)       │
│     └─ Ordena por fecha de publicación                           │
│                                                                 │
│  3. Versión Base (SI NO EXISTE NINGUNA)                          │
│     └─ Valor: 1.0.0                                              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 3.2 Flujo de Cálculo de Versión

```
Merge a development
        ↓
┌─────────────────────────────────────────┐
│  1. Analizar commits                     │
│     └─ Detectar feat/fix con scope      │
│     └─ Detectar breaking changes        │
└─────────────────┬───────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│  2. Para cada paquete con cambios:       │
│     └─ Obtener versión actual           │
│        (Artifact → NuGet API → 1.0.0)   │
│     └─ Determinar bump type             │
│        (MAJOR/MINOR/PATCH)              │
│     └─ Calcular increment               │
│        (PRs mergeados desde publish)    │
└─────────────────┬───────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│  3. Generar nueva versión               │
│     └─ Formato: X.Y.Z-preview-N         │
│     └─ Publicar a NuGet                 │
│     └─ Actualizar artifact              │
└─────────────────────────────────────────┘
```

### 3.3 Regla de Prioridad de Bump

El bump se determina por el cambio **más significativo**:

```
Breaking (!) > Feature (feat) > Fix (fix) > Otros
     ↓              ↓              ↓          ↓
   MAJOR          MINOR          PATCH    Sin bump
```

### 3.4 Fuentes de Datos para Cálculo

| Dato | Fuente | Descripción |
|------|--------|-------------|
| **Versión actual** | Artifact → NuGet API | Versión más reciente del paquete |
| **Fecha de publicación** | Artifact/NuGet | Fecha de la versión actual |
| **Bump type** | Análisis de commits | feat→MINOR, fix→PATCH, feat!/fix!→MAJOR |
| **Increment** | GitHub API (PRs) | PRs mergeados a development desde publishedAt |

### 3.5 Formato de Artifact `current-version.json`

```json
{
  "core": {
    "version": "1.5.12-preview-52",
    "publishedAt": "2026-01-18T10:30:00Z"
  },
  "sourcegen": {
    "version": "1.3.4-preview-0",
    "publishedAt": "2026-01-16T10:30:00Z"
  }
}
```

### 3.6 Cálculo del Increment

El número `N` en el sufijo `-preview-N` representa la cantidad de **PRs mergeados** a la rama `development` desde la fecha de publicación de la versión actual.

```
1.6.0-preview-3
      └─────┘ └─ 3 PRs mergeados desde última versión
         └─ MINOR bump por "feat"
```

### 3.7 Ejemplos de Versionado

| Commits en el PR | Versión Resultante |
|------------------|-------------------|
| 3 fixes | 1.5.11 → 1.5.12-preview-N |
| 5 fixes + 1 feat | 1.5.11 → 1.6.0-preview-N |
| fixes + feats + 1 breaking | 1.5.11 → 2.0.0-preview-N |
| solo docs/chore/test | ❌ Sin release |

### 3.8 Versionado Independiente

Cada paquete se versiona por separado según sus commits:

| Paquete | Scope de Commits | Artifact |
|---------|-----------------|----------|
| `MauiPdfGenerator` | `feat(core)`, `fix(core)`, etc. | `core.version` |
| `SourceGenerators` | `feat(sourcegen)`, `fix(sourcegen)`, etc. | `sourcegen.version` |

### 3.9 Scripts de Versionamiento

El script principal de versionamiento se encuentra en:

```
.github/scripts/Get-LatestPackageVersion.ps1
```

**Funciones del script:**

| Función | Descripción |
|---------|-------------|
| `Get-GitHubArtifactVersion` | Descarga `current-version.json` y extrae la versión del paquete |
| `Get-LatestNuGetVersion` | Consulta NuGet API REST para obtener la versión más reciente |
| `Calculate-VersionBump` | Analiza commits para determinar MAJOR/MINOR/PATCH |
| `Calculate-PreviewIncrement` | Cuenta PRs mergeados a development desde publishedAt |
| `Get-LatestPackageVersion` | Función principal que orquesta todo |

### 3.10 Logs de Versionamiento

El script genera logs estructurados para facilitar el debugging:

```powershell
[2026-01-18T10:30:00.000Z] [INFO] Iniciando cálculo de versión para: RandAMediaLabGroup.MauiPdfGenerator
[2026-01-18T10:30:00.001Z] [DEBUG] Intentando obtener versión desde GitHub Artifact...
[2026-01-18T10:30:00.002Z] [DEBUG] Versión obtenida desde artifact: 1.5.12-preview-52
[2026-01-18T10:30:00.003Z] [INFO] Analizando commits para bump...
[2026-01-18T10:30:00.004Z] [DEBUG] Feature detectado: feat(core): add PDF table rendering support
[2026-01-18T10:30:00.005Z] [INFO] Tipo de bump determinado: minor
[2026-01-18T10:30:00.006Z] [INFO] Calculando increment...
[2026-01-18T10:30:00.007Z] [DEBUG] PRs mergeados desde última versión: 3
[2026-01-18T10:30:00.008Z] [INFO] RESULTADO FINAL
[2026-01-18T10:30:00.009Z] [INFO]   Paquete: RandAMediaLabGroup.MauiPdfGenerator
[2026-01-18T10:30:00.010Z] [INFO]   Versión actual: 1.5.12-preview-52
[2026-01-18T10:30:00.011Z] [INFO]   Bump: minor
[2026-01-18T10:30:00.012Z] [INFO]   Increment: 3
[2026-01-18T10:30:00.013Z] [INFO]   Nueva versión: 1.6.0-preview-3
```

Los logs se muestran en la consola de GitHub Actions y también se escriben al archivo `$env:GITHUB_ENV` en caso de errores críticos.

### 3.11 Troubleshooting de Versionamiento

| Problema | Causa | Solución |
|----------|-------|----------|
| Error consultando NuGet | Problemas de red/API | Verificar conectividad; el workflow usará fallback |
| Artifact expirado (90 días) | Retención expirada | El workflow detectará y consultará NuGet |
| No se encuentran PRs | gh CLI no autenticado | Verificar que `GITHUB_TOKEN` esté disponible |
| Error crítico en script | Fallo en lógica | Revisar logs de GitHub Actions |

---

## 4. Referencia Rápida

### Flujo Completo

```
Issue → Branch (desde development) → Commits → PR → CI ✓ → Merge → Release
```

### Comandos Git

```bash
# Configurar plantilla (solo una vez)
git config commit.template .gitmessage

# Crear branch para Issue #42
git checkout development
git pull origin development
git checkout -b feature/42-descripcion-corta

# Ver commits desde último stable
git log main-v1.5.11..HEAD --oneline -- MauiPdfGenerator

# Ver solo feat/fix
git log main-v1.5.11..HEAD --grep="^feat\|^fix" --oneline
```

### Mapeo Labels ↔ Commits

| Label de Issue | Tipo(s) de Commit |
|----------------|-------------------|
| `feat` | `feat:` |
| `fix` | `fix:` |
| `chore` | `docs:`, `test:`, `refactor:`, `perf:`, `build:`, `ci:`, `chore:` |
| `breaking-change` | `feat!:`, `fix!:` |

---

## Preguntas Frecuentes

**P: ¿Qué pasa si hay 10 fixes y 1 feat?**
R: MINOR bump. El feat "gana" sobre los fixes.

**P: ¿El número preview se reinicia?**
R: Sí. `1.6.0-preview-8` → `1.6.0` → `1.7.0-preview-1`

**P: ¿Qué pasa si solo hay commits de docs/chore?**
R: No se publica nada. Sin bump = sin release.

---

## 5. Troubleshooting de CI/CD

Esta sección describe los errores más comunes en los workflows de CI/CD y cómo resolverlos.

### 5.1 Errores en Development Release (Preview)

| Error | Causa | Solución |
| --- | --- | --- |
| `has_sourcegen_changes = false` pero se publicó preview | Análisis de commits no detectó cambios | Verificar que los commits tengan el scope correcto: `feat(sourcegen)` o `fix(sourcegen)` |
| `publish-nuget` skipeado | No se generó paquete en build-main/build-sourcegen | Revisar logs del job anterior para ver por qué falló el build |
| Preview no se convierte a estable en Production Release | No hay commits nuevos en sourcegen | Este es comportamiento normal. El Production Release promociona previews automáticamente |

### 5.2 Errores en Production Release (Stable)

| Error | Causa | Solución |
| --- | --- | --- |
| `build-sourcegen` skipeado con preview pendiente | `has_sourcegen_changes = false` y no se detectó preview | Verificar que exista una tag preview: `git tag -l "gen-v*preview*"` |
| `publish-nuget` falla con "401 Unauthorized" | API key de NuGet incorrecta o expirada | Actualizar el secreto `NUGET_API_KEY` en GitHub Settings |
| `Verify NuGet Publication` timeout | Servicio de NuGet.org no responde | El workflow continúa con advertencia. Verificar manualmente más tarde |
| Multiple preview tags found | Se publicaron varias previews sin promocionar | Eliminar manualmente las tags más antiguas: `git tag -d gen-v1.3.4-preview-1 && git push origin :refs/tags/gen-v1.3.4-preview-1` |

### 5.3 Verificación de Publicación en NuGet

El workflow de Production Release crea automáticamente una issue con el label `status:preview-pending` cuando publica una versión preview. Para cerrarla:

1. Visita https://www.nuget.org/packages/RandAMediaLabGroup.MauiPdfGenerator
2. Verifica que la versión esté disponible
3. Confirma que la versión es la correcta
4. Elimina la tag de preview:
   ```bash
   git tag -d gen-v1.3.4-preview-1
   git push origin :refs/tags/gen-v1.3.4-preview-1
   ```
5. Cierra la issue de tracking

### 5.4 Previews Inferiores a Versión Estable

El workflow ignora versiones preview que sean menores que la última versión estable (ej: `gen-v1.3.3-preview-1` cuando existe `gen-v1.3.3`). Emite un warning en los logs.

**Ejemplo de warning:**
```
⚠ Preview más antigua que stable: gen-v1.3.3-preview-1 < gen-v1.3.3 (ignorando)
```

Para solucionar este problema:

1. Identificar la versión incorrecta:
   ```bash
   git tag -l "gen-v*preview*"
   ```
2. Eliminar las tags inválidas:
   ```bash
   git tag -d <tag-inválida>
   git push origin :refs/tags/<tag-inválida>
   ```
3. Volver a publicar la versión correcta (si es necesario)

### 5.5 Labels de Issues Tracking

| Label | Uso |
| --- | --- |
| `status:preview-pending` | Issue creada automáticamente al publicar una preview, cierra tras verificar en NuGet |

### 5.6 Mensajes de Error del Workflow

#### Error: "No se pudo determinar la versión"

**Causa:** El job de build no pudo obtener ni la versión calculada ni la pendiente de preview.

**Solución:**
1. Revisar los logs del job `analyze-commits`
2. Revisar los logs del job `check-pending-previews`
3. Verificar que existan tags estables: `git tag -l "main-v*" | git tag -l "gen-v*"`

#### Error: "Preview más antigua que stable (ignorando)"

**Causa:** Existe una tag preview con versión menor a la última versión estable. El workflow la ignora por seguridad.

**Solución:**
1. Identificar la tag inválida: `git tag -l "*preview*"`
2. Eliminarla: `git tag -d <tag> && git push origin :refs/tags/<tag>`
3. Volver a ejecutar el Production Release

#### Error: "ADVERTENCIA CRÍTICA: no pudo verificarse después de 3 intentos"

**Causa:** El paquete no está disponible en NuGet después de 30 minutos (3 intentos de 10 minutos).

**Solución:**
1. Verificar manualmente en NuGet: https://www.nuget.org/packages/RandAMediaLabGroup.MauiPdfGenerator
2. Revisar los logs del job `Publish to NuGet`
3. Si el paquete está en NuGet, cerrar la issue de tracking manualmente
4. Si no está, verificar el secreto `NUGET_API_KEY`

### 5.7 Comandos Útiles para Debug

```bash
# Listar todas las tags
git tag -l

# Listar solo tags de preview
git tag -l "*preview*"

# Listar tags estables
git tag -l "main-v*"
git tag -l "gen-v*"

# Ver commits desde última tag
git log main-v1.5.11..HEAD --oneline

# Verificar última tag
git describe --tags --abbrev=0

# Eliminar tag localmente
git tag -d <nombre-tag>

# Eliminar tag remotamente
git push origin :refs/tags/<nombre-tag>
```
