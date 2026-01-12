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

### 3.1 Regla de Prioridad

El bump se determina por el cambio **más significativo**:

```
Breaking (!) > Feature (feat) > Fix (fix) > Otros
     ↓              ↓              ↓          ↓
   MAJOR          MINOR          PATCH    Sin bump
```

### 3.2 Ejemplos de Versionado

| Commits en el PR | Versión resultante |
|------------------|-------------------|
| 3 fixes | 1.5.11 → 1.5.12-preview |
| 5 fixes + 1 feat | 1.5.11 → 1.6.0-preview |
| fixes + feats + 1 breaking | 1.5.11 → 2.0.0-preview |
| solo docs/chore/test | ❌ Sin release |

### 3.3 Sufijo Preview

El `-preview-X` indica cuántos commits relevantes hay desde el último stable:

```
1.6.0-preview-3
      └─────┘ └─ 3 commits (feat/fix) desde último stable
         └─ MINOR bump por "feat"
```

### 3.4 Versionado Independiente

Ahora que Diagnostics es parte de `core`, cada paquete se versiona por separado:
- `MauiPdfGenerator` → versión basada en commits `(core)` (incluye Diagnostics)
- `SourceGenerators` → versión basada en commits `(sourcegen)`

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
