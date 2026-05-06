# Estructura de Issues en GitHub

Guía para crear, etiquetar y gestionar issues en el repositorio de manera profesional.

## 📋 Tipos de Issues

### 1. Feature Request

Solicitud de nueva funcionalidad.

**Template**:
```markdown
## Descripción
[Descripción clara y concisa de la funcionalidad deseada]

## Motivación
¿Por qué necesitamos esta funcionalidad?
- [Caso de uso 1]
- [Caso de uso 2]

## Solución Propuesta
[Cómo debería implementarse]

## Alternativas Consideradas
[Otras formas de abordar esto]

## Aceptación
- [ ] Feature implementado
- [ ] Tests agregados (>70% coverage)
- [ ] Documentación actualizada
- [ ] PR revisado y aprobado

## Estimación
- Puntos de historia: [?]
- Esfuerzo: [1-2 días / 2-5 días / 1-2 semanas]
```

**Ejemplo**:
```markdown
## Descripción
Implementar autenticación JWT para proteger endpoints API.

## Motivación
- Seguridad: Actualmente la API es pública
- Compliance: Requerimiento de cliente
- Auditoría: Necesidad de rastrear quién accede a datos

## Solución Propuesta
1. Agregar middleware de autenticación JWT
2. Generar tokens en endpoint de login
3. Validar tokens en cada request
4. Agregar refresh token mechanism

## Alternativas
- OAuth 2.0: Más complejo, mejor para integración con terceros
- Basic Auth: Menos seguro

## Estimación
Puntos de historia: 5
Esfuerzo: 2-3 días
```

### 2. Bug Report

Reporte de comportamiento incorrecto.

**Template**:
```markdown
## Descripción
[Descripción clara del bug]

## Pasos para Reproducir
1. [Paso 1]
2. [Paso 2]
3. [Paso 3]

## Comportamiento Esperado
[Qué debería pasar]

## Comportamiento Real
[Qué está pasando]

## Entorno
- OS: [Windows/Mac/Linux]
- .NET Version: [10.0]
- PostgreSQL Version: [15]
- Rama: [main/develop]

## Logs/Stack Trace
\`\`\`
[Pegue logs aquí]
\`\`\`

## Archivos Afectados
- [Archivo 1]
- [Archivo 2]

## Prioridad
- [ ] Crítica (API no funciona)
- [ ] Alta (Funcionalidad rota)
- [ ] Media (Comportamiento incorrecto)
- [ ] Baja (Cosmético)

## Posible Solución
[Si tienes ideas]
```

**Ejemplo**:
```markdown
## Descripción
El cálculo de OEE retorna NaN cuando la cantidad de unidades es 0.

## Pasos para Reproducir
1. Crear orden de producción
2. No incrementar ningún contador
3. Calcular OEE

## Comportamiento Esperado
OEE = 0 o mensaje de error

## Comportamiento Real
OEE = NaN (Not a Number)

## Logs
```
ArgumentException: OEE cannot be NaN
at MetricsCalculator.Calculate()
```

## Prioridad
- [x] Alta (Métrica crítica)
```

### 3. Improvement

Mejora a funcionalidad existente.

**Template**:
```markdown
## Descripción
[Descripción de la mejora]

## Beneficio
[Por qué esto es una mejora]
- [Razón 1]
- [Razón 2]

## Cambios Propuestos
- [Cambio 1]
- [Cambio 2]

## Impacto
- Breaking changes: [ ] Sí [x] No
- Afecta APIs existentes: [x] Sí [ ] No

## Estimación
Puntos: [?]
Esfuerzo: [?]
```

### 4. Documentation

Actualización o creación de documentación.

**Template**:
```markdown
## Tipo de Documentación
- [ ] README
- [ ] API docs
- [ ] Architecture
- [ ] Setup guide
- [ ] Otro: [_____]

## Descripción
[Qué documentar o actualizar]

## Razón
[Por qué es necesario]

## Secciones a Cubrir
- [ ] Sección 1
- [ ] Sección 2
- [ ] Sección 3
```

## 🏷️ Etiquetas (Labels)

### Por Tipo

| Label | Color | Descripción |
|-------|-------|-------------|
| `type/feature` | 🟢 Verde | Nueva funcionalidad |
| `type/bug` | 🔴 Rojo | Bug report |
| `type/improvement` | 🟡 Amarillo | Mejora existente |
| `type/docs` | 🔵 Azul | Documentación |
| `type/chore` | ⚫ Negro | Mantenimiento |

### Por Prioridad

| Label | Descripción |
|-------|-------------|
| `priority/critical` | Bloquea ejecución |
| `priority/high` | Importante para sprint |
| `priority/medium` | Puede esperar |
| `priority/low` | Nice to have |

### Por Fase

| Label | Descripción |
|-------|-------------|
| `phase/1-api` | Fase 1: API endpoints |
| `phase/2-data` | Fase 2: Datos de prueba |
| `phase/3-mqtt` | Fase 3: Integración MQTT |
| `phase/4-auth` | Fase 4: Mejoras API |
| `phase/5-gpio` | Fase 5: GPIO handlers |
| `phase/6-machines` | Fase 6: Múltiples máquinas |
| `phase/7-ui` | Fase 7: Dashboard |
| `phase/8-enterprise` | Fase 8: Integración |

### Por Estado

| Label | Descripción |
|-------|-------------|
| `status/blocked` | Bloqueado por otro issue |
| `status/in-progress` | Siendo trabajado |
| `status/review` | En review |
| `status/ready` | Listo para implementar |

### Por Dificultad

| Label | Descripción |
|-------|-------------|
| `difficulty/easy` | < 1 día |
| `difficulty/medium` | 1-3 días |
| `difficulty/hard` | 3-5 días |
| `difficulty/epic` | > 5 días (múltiples PRs) |

## 📊 Puntos de Historia

Sistema de estimación:

```
1 punto = 4 horas
2 puntos = 1 día
3 puntos = 1-2 días
5 puntos = 2-3 días
8 puntos = 4-5 días
13 puntos = 1-2 semanas
21 puntos = 2-3 semanas
```

**Asignación**:
- Tasks muy pequeñas (< 4 horas) = 0.5 o 1 punto
- Usar secuencia de Fibonacci para consistencia
- Si algo es > 13, considerar dividirlo en subtasks

## 🔄 Ciclo de Vida del Issue

```
IDEA → BACKLOG → READY → IN_PROGRESS → REVIEW → DONE
 ↓
Discusión inicial

              ↓
         Estimado
         Etiquetado
         Triaged
         
                   ↓
              Asignado
              En Sprint
              
                        ↓
                   Creado PR
                   Tests
                   
                              ↓
                         Revisado
                         Aprobado
                         
                                    ↓
                               Merged
                               Cerrado
```

### Transiciones

**IDEA → BACKLOG**:
- [ ] Descripción clara
- [ ] Criterios de aceptación definidos
- [ ] Estimado

**BACKLOG → READY**:
- [ ] Etiquetado correctamente
- [ ] Dependencias identificadas
- [ ] Aceptado por el equipo

**READY → IN_PROGRESS**:
- [ ] Asignado a persona
- [ ] Rama creada
- [ ] PR creado como draft (opcional)

**IN_PROGRESS → REVIEW**:
- [ ] Código compilado
- [ ] Tests pasan
- [ ] PR creado y enlazado

**REVIEW → DONE**:
- [ ] Aprobaciones recibidas
- [ ] Comentarios resueltos
- [ ] Merged a main
- [ ] Issue automáticamente cerrado

## 📝 Crear Issues en Masa

### CLI (gh)

```bash
# Crear issue simple
gh issue create \
  --title "[Fase 4.1] Implementar JWT" \
  --body "Descripción del issue" \
  --label "type/feature,priority/high,phase/4-auth"

# Crear múltiples issues
for i in {1..5}; do
  gh issue create \
    --title "[Feature $i] Title" \
    --label "type/feature"
done
```

### Excel → Issues

Convertir spreadsheet a issues:

```powershell
# Con CSV
$csv = Import-Csv "issues.csv"

foreach ($row in $csv) {
  gh issue create `
    --title $row.Title `
    --body $row.Description `
    --label $row.Labels
}
```

## 🎯 Kanban Board Recomendado

**Columnas**:

1. **📋 Backlog** (Nuevas ideas, sin estimar)
2. **🎯 Ready** (Estimado, listo para Sprint)
3. **🔄 In Progress** (Siendo trabajado)
4. **👀 Review** (Esperando review)
5. **✅ Done** (Completado, merged)

**Límites**:
- In Progress: 1-3 por persona
- Review: Sin límite (se revisa ASAP)

## 📊 Reportes Útiles

### Velocity

```bash
# Issues cerrados en último sprint
gh issue list --state closed --label "Fase-X" --limit 50
```

### Burndown

```bash
# Issues por estado
gh issue list --state open --label "type/bug"
gh issue list --state open --label "type/feature"
```

### Blockers

```bash
# Issues bloqueados
gh issue list --label "status/blocked" --state open
```

## ✅ Checklist para Nueva Feature

Cuando crees un issue para nueva feature:

- [ ] Título claro y conciso (< 60 caracteres)
- [ ] Descripción detallada con contexto
- [ ] Criterios de aceptación específicos
- [ ] Links a issues relacionados
- [ ] Labels aplicados (tipo, prioridad, fase)
- [ ] Estimación en puntos
- [ ] Asignado (si está listo)
- [ ] No hay duplicados abiertos

## 🚫 Antipatrones

❌ **Evitar**:

```markdown
# Issue mal formado
Tengo un problema con el código

# Falta información
Agregar una función

# Criterios vagos
Hacer que funcione mejor

# Sin contexto
BUG!!!

# Mal etiquetado
Feature con label de bug

# Estimación irreal
21 puntos = 1 día
```

## 📚 Documentación Relacionada

- [CONTRIBUTING.md](./CONTRIBUTING.md) - Guía para colaboradores
- [KANBAN_UPDATE.md](./KANBAN_UPDATE.md) - Issues planeados
- [README.md](./README.md) - Overview del proyecto

---

**Última actualización**: 2026-05-06
