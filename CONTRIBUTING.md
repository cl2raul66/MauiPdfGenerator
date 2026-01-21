# Guía del Desarrollador - MauiPdfGenerator

Esta guía organiza todo lo que necesitas saber para trabajar en el proyecto, desde planificar hasta publicar.

---

## Índice Rápido

| ¿Qué quiero hacer? | Ir a sección |
|--------------------|--------------|
| Configurar Branch Protection | [0. Branch Protection](#0-branch-protection--reglas-estrictas) |
| Planificar una nueva funcionalidad | [1. Planificación](#1-planificación-roadmap-e-issues) |
| Crear un Issue | [1.2 Crear Issues](#12-crear-issues) |
| Hacer un commit | [2. Desarrollo](#2-desarrollo-commits) |
| Entender el versionado | [3. Publicación](#3-publicación-versionado) |
| Ver comandos útiles | [4. Referencia Rápida](#4-referencia-rápida) |

---

## 0. Branch Protection & Reglas Estrictas

Esta sección define las reglas de Branch Protection que garantizan el flujo DevOps determinista y sin ambigüedades.

### 0.1 Ramas Protegidas

| Rama | Propósito | Protección |
|------|-----------|------------|
| `development` | Desarrollo principal | ✅ Branch Protection Strict |
| `master` | Producción | ✅ Branch Protection Strict |

### 0.2 Reglas de Branch Protection Strict

Para garantizar el flujo ideal `anyBranch → development → master`, las siguientes reglas están **ACTIVAS**:

#### 0.2.1 Restricciones de Merge

| Regla | Descripción |
|-------|-------------|
| **Solo merges vía PR** | Los cambios a `development` y `master` solo pueden hacerse mediante Pull Requests |
| **Bloquear push directo** | No se permite `git push` directo a estas ramas |
| **Solo 1 PR pendiente** | Solo puede haber un PR abierto por rama protegida a la vez |

> [!IMPORTANT]
> **¿Por qué estas reglas?**
> - Garantizan que `git log ${{ github.event.before }}..HEAD` capture solo los commits del PR
> - Eliminan ambigüedades en el cálculo de versiones
> - Aseguran que el versionamiento sea determinista

#### 0.2.2 Requisitos de PR

| Requisito | Configuración |
|-----------|---------------|
| **Require PR reviews** | Mínimo 1 aprobación |
| **Require status checks** | `PR Validation` debe pasar |
| **Require branches up to date** | La rama debe estar actualizada con `development`/`master` antes de merge |
| **Dismiss stale reviews** | Re-aprobación requerida si hay cambios |

#### 0.2.3 Workflows Automáticos

| Workflow | Trigger | Ejecución Manual |
|----------|---------|------------------|
| `dev-release.yml` | Push a `development` | ❌ No disponible |
| `prod-release.yml` | Push a `master` | ❌ No disponible |
| `pr-validation.yml` | Pull Request a `development`/`master` | ✅ Disponible |

> [!WARNING]
> **Los workflows de release NO se pueden ejecutar manualmente.**
> Esto garantiza que:
> - Solo se publiquen versiones basadas en commits reales
> - No haya versiones inconsistentes o duplicadas
> - El cálculo de versiones sea siempre determinista

### 0.3 Variantes Prohibidas

| Variante | Por qué está prohibida | Qué hacer en su lugar |
|----------|------------------------|----------------------|
| Push directo a `development` | Rompe el flujo PR → CI → Release | Crear PR desde feature branch |
| Push directo a `master` | Rompe el flujo development → master → Production Release | Crear PR desde development a master |
| Múltiples PRs simultáneos a `development` | Commits pueden capturarse incorrectamente | Esperar a que se mergee el PR actual |
| Cherry-pick de `development` a `master` | Duplica commits en workflows | Usar el flujo PR development → master |
| Commits antes de crear PR | Branch Protection ya no aplica | Crear PR primero, luego hacer commits |

### 0.4 Flujo Ideal Garantizado

Con Branch Protection Strict activo, el flujo ideal SIEMPRE se cumple:

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Issue      │ ──▶ │  Branch     │ ──▶ │  Commits    │ ──▶ │  PR         │ ──▶ │  Merge      │
│  planificar │     │  desde dev  │     │  (varios)   │     │  validar CI │     │  cierra todo│
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                       │                                       │
                       ▼                                       ▼
                   feature/new-feature                     development
                                                                │
                                                                ▼
                                                       ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
                                                       │  PR         │ ──▶ │  Merge      │ ──▶ │  Release    │
                                                       │  dev→master │     │  cierra     │     │  Stable     │
                                                       └─────────────┘     └─────────────┘     └─────────────┘
                                                                                              │
                                                                                              ▼
                                                                                          master
```

### 0.5 Verificación de Branch Protection

Para verificar que Branch Protection está activo:

```bash
# Usar GitHub CLI
gh repo view --json branchProtectionRules

# Ver reglas específicas de development
gh api repos/:owner/:repo/branches/development/protection

# Ver reglas específicas de master
gh api repos/:owner/:repo/branches/master/protection
```

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
3. **Crea PR inmediatamente** hacia `development` — antes de hacer commits
    ```markdown
    Closes #42
    ```
4. **Trabaja con commits** (todos los commits pertenecerán al PR)
5. **CI valida** — si falla, haz más commits en el mismo branch
6. **Merge** — el Issue se cierra automáticamente y se ejecuta `dev-release.yml`

> [!IMPORTANT]
> **¿Por qué PR antes de commits?**
> - Garantiza que todos los commits pertenezcan al PR
> - Asegura que `git log ${{ github.event.before }}..HEAD` capture solo los commits del PR
> - Elimina ambigüedades en el cálculo de versiones
> - Branch Protection Strict bloquea múltiples PRs simultáneos

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

#### Development Release (Preview)

```
Merge a development (automático vía PR)
        ↓
┌─────────────────────────────────────────┐
│  1. Analizar commits                     │
│     └─ Detectar feat/fix con scope      │
│     └─ Detectar breaking changes        │
│     └─ git log ${{ github.event.before }}..HEAD │
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

> [!NOTE]
> **Este workflow se ejecuta AUTOMÁTICAMENTE** cuando se hace merge a `development`.
> No se puede ejecutar manualmente. Esto garantiza que el versionamiento sea determinista.

#### Production Release (Promoción)

```
Merge development → master (automático vía PR)
        ↓
┌─────────────────────────────────────────┐
│  1. Analizar commits                     │
│     └─ Detectar cambios en master           │
│     └─ Determinar paquetes a promocionar │
└─────────────────┬───────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│  2. Promocionar preview → stable          │
│     └─ Leer versión preview desde artifact │
│     └─ Extraer versión base (X.Y.Z)       │
│     └─ Publicar como estable              │
└─────────────────┬───────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│  3. Actualizar artifact con versión estable │
│     └─ Subir current-version.json          │
│     └─ Limpiar artifacts anteriores        │
└─────────────────────────────────────────┘
```

> [!NOTE]
> **Este workflow se ejecuta AUTOMÁTICAMENTE** cuando se hace merge a `master`.
> No se puede ejecutar manualmente. Esto garantiza que la promoción de versiones sea determinista.

---

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
| `Get-PromotedVersion` | Promociona versión preview a estable (producción) |
| `Get-LatestPackageVersion` | Función principal que orquesta todo |

**Modos de ejecución:**

| Modo | Parámetro | Uso |
| ---- | --------- | ---- |
| Preview | (default) | `dev-release.yml` - Calcula nuevas versiones preview |
| Promoción | `-PromotionMode` | `prod-release.yml` - Promociona preview a estable |

### 3.10 Logs de Versionamiento

El script genera logs estructurados para facilitar el debugging:

**Modo Preview (Development):**
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

**Modo Promoción (Production):**
```powershell
[2026-01-18T10:30:00.000Z] [INFO] MODO PROMOCIÓN: Intentando promocionar RandAMediaLabGroup.MauiPdfGenerator
[2026-01-18T10:30:00.001Z] [INFO] =============================================
[2026-01-18T10:30:00.002Z] [INFO] [PASO 1] Obteniendo versión preview actual...
[2026-01-18T10:30:00.003Z] [DEBUG] Versión obtenida desde artifact: 1.5.12-preview-52
[2026-01-18T10:30:00.004Z] [INFO] [PASO 2] Verificando que es versión preview...
[2026-01-18T10:30:00.005Z] [INFO] [PASO 3] Extrayendo versión estable...
[2026-01-18T10:30:00.006Z] [INFO]   Preview: 1.5.12-preview-52
[2026-01-18T10:30:00.007Z] [INFO]   Estable: 1.5.12
[2026-01-18T10:30:00.008Z] [INFO] [PASO 4] Verificando versión base...
[2026-01-18T10:30:00.009Z] [INFO] Versión base válida: 1.5.12
[2026-01-18T10:30:00.010Z] [INFO] RESULTADO DE PROMOCIÓN
[2026-01-18T10:30:00.011Z] [INFO]   Paquete: RandAMediaLabGroup.MauiPdfGenerator
[2026-01-18T10:30:00.012Z] [INFO]   Versión preview: 1.5.12-preview-52
[2026-01-18T10:30:00.013Z] [INFO]   Versión estable: 1.5.12
[2026-01-18T10:30:00.014Z] [INFO] =============================================
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

### 5.1 Nota Importante sobre Workflows Automáticos

> [!WARNING]
> **Los workflows de release NO se pueden ejecutar manualmente.**
>
> Si necesitas reintentar un workflow fallido:
> - Ve a la ejecución fallida en GitHub Actions
> - Haz clic en "Re-run failed jobs"
> - NO intentes ejecutar manualmente el workflow
>
> Los workflows `dev-release.yml` y `prod-release.yml` **NO tienen** `workflow_dispatch` activado por diseño.

### 5.2 Errores en Development Release (Preview)

| Error | Causa | Solución |
| --- | --- | --- |
| `has_sourcegen_changes = false` pero se publicó preview | Análisis de commits no detectó cambios | Verificar que los commits tengan el scope correcto: `feat(sourcegen)` o `fix(sourcegen)` |
| `publish-nuget` skipeado | No se generó paquete en build-main/build-sourcegen | Revisar logs del job anterior para ver por qué falló el build |
| Preview no se convierte a estable en Production Release | No hay commits nuevos en sourcegen | Este es comportamiento normal. El Production Release promociona previews automáticamente |

### 5.3 Errores en Production Release (Promoción)

| Error | Causa | Solución |
| --- | --- | --- |
| `No hay versión para promocionar` | No existe preview en artifact/NuGet | Verificar que se haya ejecutado `dev-release.yml` exitosamente |
| `La versión NO es preview` | La versión actual ya es estable | Verificar artifact `current-version.json` |
| `build-sourcegen` skipeado | `has_sourcegen_changes = false` | Verificar que el paquete tenga preview pendiente |

### 5.4 Troubleshooting de Promoción

#### Error: "No hay versión para promocionar"

**Causa:** No existe ninguna versión preview del paquete en el artifact ni en NuGet.

**Solución:**
1. Verificar que se haya ejecutado `dev-release.yml` exitosamente
2. Revisar artifact en GitHub Actions
3. Consultar NuGet directamente

#### Error: "La versión NO es preview"

**Causa:** El artifact contiene una versión estable, no preview.

**Solución:**
1. Verificar el contenido del artifact `current-version.json`
2. Si ya está estable, no hay nada que promocionar

#### Error: "Versión base inválida"

**Causa:** La versión extraída no tiene formato X.Y.Z válido.

**Solución:**
1. Revisar el formato de la versión en el artifact
2. Verificar que sea una versión semántica válida

### 5.5 Comandos Útiles para Debug

```bash
# Verificar contenido del artifact actual
gh artifact download current-version --pattern "current-version.json" --path ./debug

# Consultar NuGet API para verificar versiones
curl -s "https://api.nuget.org/v3-flatcontainer/RandAMediaLabGroup.MauiPdfGenerator/index.json"

# Verificar logs del workflow en GitHub Actions
# (Ir a Actions tab del repo)
```
