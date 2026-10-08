# API — Cliente, Propiedad y TipoPropiedad

Contrato para el frontend Angular. Verificado contra la API en ejecucion (`/openapi/v1.json`).

- Base: `http://localhost:5056/api`
- Todos los endpoints requieren `Authorization: Bearer {token}`.
- Todos los `id` son **int** (nunca Guid).
- Las fechas van en ISO 8601 (`2026-10-04T15:30:00`).

## Autenticacion

`POST /api/Auth/login`

```json
{ "gmail": "admin@finanzas.com", "password": "OnepieceJulian" }
```

Respuesta `200`:

```json
{
  "token": "eyJhbGciOi...",
  "nombre": "Administrador",
  "rol": "Administrador",
  "permisos": ["Clientes.Ver", "Clientes.Crear", "..."]
}
```

> `permisos` es la lista de permisos del rol. El frontend puede usarla para mostrar u
> ocultar botones, pero el servidor vuelve a validar en cada endpoint.

## Formato de error

Todos los errores devuelven `400`, `401`, `403` o `409` con el mismo cuerpo:

```json
{ "mensaje": "Ya existe un cliente registrado con ese documento de identidad." }
```

| Codigo | Cuando ocurre |
|--------|----------------|
| `400` | Faltan campos obligatorios o el cuerpo no es valido |
| `401` | No se envio token, o el token expiro |
| `403` | El rol no tiene el permiso del endpoint |
| `409` | Conflicto: no existe, documento repetido, referencia en uso |

## Listados paginados

Los tres recursos usan la misma envoltura, para que el frontend tenga un solo contrato:

```json
{
  "items": [ /* ... */ ],
  "total": 137,
  "pagina": 1,
  "tamano": 20
}
```

- `pagina` empieza en 1.
- `tamano` por defecto 20, maximo 200.

---

# Cliente

**Nota de negocio:** el borrado es **logico**. Un cliente con prestamos o contratos nunca
se elimina de la base, solo se marca `estado: false`. Por eso `DELETE` siempre responde `204`,
incluso si el cliente ya estaba desactivado (es idempotente).

## Modelo (`ClienteDto`)

```json
{
  "id": 1,
  "nombre": "Maria Lopez",
  "documentoIdentidad": "CC 12345678",
  "telefono": "3001234567",
  "fechaRegistro": "2026-10-04T15:30:00",
  "estado": true,
  "totalPrestamos": 2,
  "totalContratos": 1
}
```

| Campo | Tipo | Notas |
|-------|------|-------|
| `id` | int | |
| `nombre` | string | obligatorio, max 300 |
| `documentoIdentidad` | string \| null | max 40. **Unico en toda la base**: dos clientes no pueden compartirlo, y funciona como filtro unico, asi que **varios clientes pueden tenerlo en `null`**. Un cliente desactivado sigue reservando su documento. |
| `telefono` | string \| null | max 40 |
| `fechaRegistro` | dateTime \| null | lo asigna el servidor |
| `estado` | bool | `false` = desactivado |
| `totalPrestamos` | int | conteo que llega del servidor |
| `totalContratos` | int | conteo que llega del servidor |

## Endpoints

### GET `/api/Clientes`

Lista paginada. Requiere permiso `Clientes.Ver`.

| Query | Tipo | Default | Descripcion |
|-------|------|---------|-------------|
| `buscar` | string | null | Busca en `nombre` y `documentoIdentidad` |
| `soloActivos` | bool | null | `true` = solo activos, `false` = solo desactivados, ausente = todos |
| `pagina` | int | 1 | |
| `tamano` | int | 20 | max 200 |

```bash
GET /api/Clientes?buscar=maria&soloActivos=true&pagina=1&tamano=20
```

Respuesta `200`: `{ "items": [ClienteDto], "total": 137, "pagina": 1, "tamano": 20 }`

### GET `/api/Clientes/{id}`

Detalle. Requiere `Clientes.Ver`. Si no existe, `409`.

Respuesta `200`: `ClienteDto`

### POST `/api/Clientes`

Crea un cliente. Requiere `Clientes.Crear`.

```json
{ "nombre": "Maria Lopez", "documentoIdentidad": "CC 12345678", "telefono": "3001234567" }
```

Respuesta `201` con el **id como numero plano**:

```
42
```

Errores: `400` si `nombre` viene vacio. `409` si el documento ya existe.

### PUT `/api/Clientes/{id}`

Actualiza. Requiere `Clientes.Editar`. El `id` va en la ruta, **no** en el JSON.

```json
{ "nombre": "Maria Lopez Perez", "documentoIdentidad": "CC 12345678", "telefono": "3111111111" }
```

Respuesta `204` (sin cuerpo).

Errores: `400` nombre vacio. `409` si no existe o el documento pertenece a otro cliente.

### PUT `/api/Clientes/{id}/estado`

Activa o desactiva. Requiere `Clientes.Editar`. Es la forma de **reactivar** un cliente
eliminado logicamente.

```json
{ "estado": false }
```

Respuesta `204`.

### DELETE `/api/Clientes/{id}`

Borrado logico (`estado = false`). Requiere `Clientes.Eliminar`. Respuesta `204`.

---

# TipoPropiedad

Catalogo de tipos de propiedad (casa, apartamento, local...).

**Nota de negocio:** esta tabla **no tiene columna `estado`**, asi que el borrado aqui es
**fisico** y real. Se rechaza con `409` si el tipo tiene propiedades asociadas, porque la
clave foranea lo restringe.

## Modelo (`TipoPropiedadDto`)

```json
{ "id": 1, "nombre": "Casa", "totalPropiedades": 12 }
```

| Campo | Tipo | Notas |
|-------|------|-------|
| `id` | int | |
| `nombre` | string | obligatorio, max 100. Unico, sin distinguir mayusculas: `"Casa"` y `"casa"` son el mismo tipo. |
| `totalPropiedades` | int | Propiedades que usan este tipo |

## Endpoints

### GET `/api/TiposPropiedad`

Lista completa, **sin paginar** (es un catalogo corto, pensado para el desplegable del
formulario de propiedad). Requiere `TiposPropiedad.Ver`.

| Query | Tipo | Default | Descripcion |
|-------|------|---------|-------------|
| `soloConPropiedades` | bool | false | `true` = solo tipos que ya tienen propiedades |

Respuesta `200`: `TipoPropiedadDto[]`

```json
[ { "id": 1, "nombre": "Casa", "totalPropiedades": 12 },
  { "id": 2, "nombre": "Apartamento", "totalPropiedades": 0 } ]
```

### GET `/api/TiposPropiedad/{id}`

Detalle. Requiere `TiposPropiedad.Ver`. Respuesta `200`: `TipoPropiedadDto`

### POST `/api/TiposPropiedad`

Requiere `TiposPropiedad.Crear`.

```json
{ "nombre": "Local comercial" }
```

Respuesta `201` con el id plano. `409` si el nombre ya existe.

### PUT `/api/TiposPropiedad/{id}`

Requiere `TiposPropiedad.Editar`.

```json
{ "nombre": "Local" }
```

Respuesta `204`.

### DELETE `/api/TiposPropiedad/{id}`

Borrado fisico. Requiere `TiposPropiedad.Eliminar`. Respuesta `204`.

`409` si tiene propiedades asociadas:
`{ "mensaje": "No se puede eliminar el tipo porque tiene propiedades asociadas." }`

---

# Propiedad

**Nota de negocio:** el borrado es **logico** (`estado = false`). Ademas, si la propiedad tiene
contratos asociados, el `DELETE` se rechaza con `409` y hay que desactivarla con
`PUT /estado` en su lugar.

## Modelo (`PropiedadDto`)

```json
{
  "id": 1,
  "nombre": "Casa conjunto La Sabana",
  "direccion": "Calle 100 #20-35",
  "codigoLuz": "L883120",
  "codigoAgua": "A883120",
  "codigoGas": "G883120",
  "estado": true,
  "tipoPropiedadId": 1,
  "tipoPropiedad": "Casa",
  "totalContratos": 2
}
```

| Campo | Tipo | Notas |
|-------|------|-------|
| `id` | int | |
| `nombre` | string | obligatorio, max 100 |
| `direccion` | string \| null | max 512 |
| `codigoLuz` | string | **obligatorio**, max 100 |
| `codigoAgua` | string | **obligatorio**, max 100 |
| `codigoGas` | string \| null | max 100 |
| `estado` | bool | `false` = desactivada |
| `tipoPropiedadId` | int | FK a `TipoPropiedad` |
| `tipoPropiedad` | string | Nombre del tipo, ya resuelto por el servidor |
| `totalContratos` | int | conteo que llega del servidor |

## Endpoints

### GET `/api/Propiedades`

Lista paginada. Requiere `Propiedades.Ver`.

| Query | Tipo | Default | Descripcion |
|-------|------|---------|-------------|
| `buscar` | string | null | Busca en `nombre`, `direccion`, `codigoLuz`, `codigoAgua` y `codigoGas` |
| `tipoPropiedadId` | int | null | Filtra por tipo |
| `soloActivos` | bool | null | `true` = solo activas, `false` = solo desactivadas, ausente = todas |
| `pagina` | int | 1 | |
| `tamano` | int | 20 | max 200 |

Respuesta `200`: `{ "items": [PropiedadDto], "total": 42, "pagina": 1, "tamano": 20 }`

### GET `/api/Propiedades/{id}`

Detalle. Requiere `Propiedades.Ver`. Respuesta `200`: `PropiedadDto`

### POST `/api/Propiedades`

Requiere `Propiedades.Crear`.

```json
{
  "tipoPropiedadId": 1,
  "nombre": "Casa conjunto La Sabana",
  "direccion": "Calle 100 #20-35",
  "codigoLuz": "L883120",
  "codigoAgua": "A883120",
  "codigoGas": "G883120"
}
```

Respuesta `201` con el id plano.

Errores: `400` si falta `nombre`, `codigoLuz` o `codigoAgua`. `409` si `tipoPropiedadId` no existe.

### PUT `/api/Propiedades/{id}`

Requiere `Propiedades.Editar`. Cuerpo identico al del POST.

Respuesta `204`.

### PUT `/api/Propiedades/{id}/estado`

Activa o desactiva. Requiere `Propiedades.Editar`.

```json
{ "estado": false }
```

Respuesta `204`.

### DELETE `/api/Propiedades/{id}`

Borrado logico. Requiere `Propiedades.Eliminar`. Respuesta `204`.

`409` si tiene contratos:
`{ "mensaje": "No se puede eliminar la propiedad porque tiene contratos asociados. Desactivela en su lugar." }`

---

# Permisos

| Permiso | Permite |
|---------|---------|
| `Clientes.Ver` | GET `/api/Clientes` |
| `Clientes.Crear` | POST `/api/Clientes` |
| `Clientes.Editar` | PUT `/api/Clientes/{id}` y `/estado` |
| `Clientes.Eliminar` | DELETE `/api/Clientes/{id}` |
| `TiposPropiedad.Ver` | GET `/api/TiposPropiedad` |
| `TiposPropiedad.Crear` | POST `/api/TiposPropiedad` |
| `TiposPropiedad.Editar` | PUT `/api/TiposPropiedad/{id}` |
| `TiposPropiedad.Eliminar` | DELETE `/api/TiposPropiedad/{id}` |
| `Propiedades.Ver` | GET `/api/Propiedades` |
| `Propiedades.Crear` | POST `/api/Propiedades` |
| `Propiedades.Editar` | PUT `/api/Propiedades/{id}` y `/estado` |
| `Propiedades.Eliminar` | DELETE `/api/Propiedades/{id}` |

Reparto por rol base:

- **Administrador**: todos.
- **Cobrador**: `Clientes.Ver/Crear/Editar`, `Propiedades.Ver/Crear`, `TiposPropiedad.Ver`,
  mas prestamos, contratos, pagos y reportes. **No** puede eliminar propiedades ni tipos.
- **Cliente**: solo lectura de prestamos, contratos y pagos.

## Modelo para Angular

```ts
export interface Cliente {
  id: number;
  nombre: string;
  documentoIdentidad: string | null;
  telefono: string | null;
  fechaRegistro: string | null;
  estado: boolean;
  totalPrestamos: number;
  totalContratos: number;
}

export interface TipoPropiedad {
  id: number;
  nombre: string;
  totalPropiedades: number;
}

export interface Propiedad {
  id: number;
  nombre: string;
  direccion: string | null;
  codigoLuz: string;
  codigoAgua: string;
  codigoGas: string | null;
  estado: boolean;
  tipoPropiedadId: number;
  tipoPropiedad: string;
  totalContratos: number;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  pagina: number;
  tamano: number;
}

export interface ApiError { mensaje: string; }
```