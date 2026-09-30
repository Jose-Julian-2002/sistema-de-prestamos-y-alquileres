# Diseño: Roles, Permisos y alineación del esquema real

**Fecha:** 2026-09-30
**Estado:** pendiente de revisión

## Problema

La base de datos `FinanzasDB` (servidor `DESKTOP-C85ET8B`) fue rediseñada con 12 tablas
en nomenclatura **singular**, todas con `int` IDENTITY como PK y **actualmente vacías**.
El código C# quedó atrás: 8 entidades con `Guid` como Id, nombres de tabla en plural,
`Email`/`PasswordHash` en vez de `Gmail`/`Password`, y sin tablas de permisos.

La solución **no compila**: 12 errores, dimulai por `Id = int();` en los constructores
de `Usuario` y `Rol`.

La migración existente `Inicial_CrearTablasFinanzas` crea `Clientes`, `Prestamos` y
`TransaccionesPrestamos` con columnas que no existen en la base real. `TransaccionPrestamo.cs`
y `TipoTransaccion.cs` fueron borrados del código pero el DbContext los referencia.

## Esquema real (fuente de verdad)

Servidor `DESKTOP-C85ET8B`, base `FinanzasDB`. Todos los decimales son `decimal(10,2)`.
Las tablas `Usuario`, `Contrato`, `Prestamo` y `Propiedad` tienen `Estado bit` con
default `1`.

| Tabla | PK | Columnas |
|---|---|---|
| `Rol` | `id` int (hoy **sin** IDENTITY) | `Nombre` nvarchar(100) NOT NULL |
| `Permiso` | `Id` int IDENTITY | `Nombre` nvarchar(100) NOT NULL |
| `RolPermiso` | compuesta (`RolId`, `PermisoId`) | ambas FK |
| `Usuario` | `Id` int IDENTITY | `Nombre` nvarchar(100), `Gmail` nvarchar(200), `Password` nvarchar(512), `Estado` bit, `RolId` int |
| `TipoPropiedad` | `Id` int IDENTITY | `Nombre` nvarchar(100) |
| `Propiedad` | `Id` int IDENTITY | `Nombre` nvarchar(100), `Direccion` nvarchar(512) NULL, `CodigoLuz` nvarchar(100), `CodigoAgua` nvarchar(100), `CodigoGas` nvarchar(100) NULL, `Estado` bit, `TipoPropiedadId` int |
| `Cliente` | `Id` int IDENTITY | `Nombre` nvarchar(300), `DocumentoIdentidad` nvarchar(40) NULL, `Telefono` nvarchar(40) NULL, `FechaRegistro` datetime NULL |
| `Prestamo` | `Id` int IDENTITY | `MontoCapital` decimal(10,2), `Interes` decimal(10,2), `TotalCuotas` decimal(10,2), `FechaInicio` date, `Estado` bit, `ClienteId` int |
| `Contrato` | `Id` int IDENTITY | `MontoAlquiler` decimal(10,2), `MontoGarantia` decimal(10,2), `DiaPago` date, `FechaInicio` date, `FechaFin` date, `Estado` bit, `PropiedadId` int, `ClienteId` int, `UsuarioId` int |
| `MetodoPago` | `Id` int IDENTITY | `Nombre` nvarchar(100) |
| `Pago` | `Id` int IDENTITY | `Fecha` date, `MontoTotal` decimal(10,2), `MetodoPagoId` int, `UsuarioId` int |
| `PagoDetalle` | `Id` int IDENTITY | `MontoAplicado` decimal(10,2), `FechaPago` date, `PagoId` int, `PrestamoId` int NULL, `ContratoId` int NULL |

Claves foráneas existentes: `Usuario→Rol`, `RolPermiso→Rol`, `RolPermiso→Permiso`,
`Propiedad→TipoPropiedad`, `Prestamo→Cliente`, `Contrato→Propiedad`, `Contrato→Cliente`,
`Contrato→Usuario`, `Pago→MetodoPago`, `Pago→Usuario`, `PagoDetalle→Pago`,
`PagoDetalle→Prestamo`, `PagoDetalle→Contrato`.

**No existe índice único en `Usuario.Gmail`.** Hay que agregarlo: el login depende de
que el correo sea único y el código actual ya lo asume.

## Decisiones tomadas

1. Los nombres de columna mandan: la entidad `Usuario` usa `Gmail` y `Password`.
   `Password` almacena el hash BCrypt, nunca texto plano.
2. Se mapean las 12 tablas, incluyendo las entidades nuevas `MetodoPago`, `Pago` y
   `PagoDetalle`.
3. `Rol.id` se convierte a IDENTITY mediante `ALTER TABLE`, quedando consistente con
   las otras 11 tablas. La tabla está vacía, no hay riesgo de pérdida.
4. La autorización pasa a ser por permisos: los permisos viajan como claims del JWT y
   cada endpoint se protege con una policy.
5. Se corrigen los 12 errores de compilación como parte del trabajo.
6. La migración vieja se reemplaza por un baseline que refleja el esquema real.
7. Se siembran roles y permisos del sistema.
8. Se corrigen dos problemas de seguridad detectados en el camino (ver §6).

## Diseño

### 1. Capa de dominio

Doce entidades en `FinanzasManager.Domain/Entities/`, una por tabla, con el nombre de
la clase en singular y las propiedades nombradas exactamente como las columnas.

Todas siguen el patrón ya presente en el repositorio: constructor privado para EF,
constructor público que valida y asigna, y navegación declarada como `ICollection<T>`.

Navegaciones:

- `Rol.Permisos` → `ICollection<RolPermiso>`
- `Permiso.Roles` → `ICollection<RolPermiso>`
- `RolPermiso.Rol` / `.Permiso` → `Rol` / `Permiso`
- `Usuario.Rol` → `Rol`, `Usuario.Contratos` → `ICollection<Contrato>`,
  `Usuario.Pagos` → `ICollection<Pago>`
- `TipoPropiedad.Propiedades` → `ICollection<Propiedad>`
- `Propiedad.TipoPropiedad`, `Propiedad.Contratos`
- `Cliente.Prestamos`, `Cliente.Contratos`
- `Prestamo.Cliente`, `Prestamo.PagoDetalles`
- `Contrato.Propiedad`, `Contrato.Cliente`, `Contrato.Usuario`
- `MetodoPago.Pagos`
- `Pago.MetodoPago`, `Pago.Usuario`, `Pago.Detalles`
- `PagoDetalle.Pago`, `PagoDetalle.Prestamo` (nullable), `PagoDetalle.Contrato` (nullable)

Cambios de tipo respecto al código actual, todos forzosos porque la columna es `int`:
`Cliente.Id`, `Prestamo.Id`, `Contrato.Id`, `RolPermiso` dejan de ser `Guid`.

`Contrato.UsuarioId` pasa de `Guid` a `int`, y `Contrato.DiaPago` de `int` a `DateTime`
porque la columna es `date`, no el día del mes.

`Prestamo` se renombra según la base: `PorcentajeInteres` → `Interes`,
`PlazoMeses` (int) → `TotalCuotas` (decimal, el importe de la cuota),
`Activo` → `Estado`.

Se **eliminan** las referencias a `TransaccionPrestamo` y `TipoTransaccion`. La base no
tiene tabla de transacciones, así que la entidad correcta es no recrear los archivos.
`PagoDetalle` cubre el registro de pagos con dos FKs opcionales.

### 2. Capa de persistencia

`ApplicationDbContext` expone las 12 `DbSet<T>`, todas en singular
(`Usuarios`, `Clientes`, ... siguiendo la convención ya usada en `Usuarios`).

`OnModelCreating` aplica, por entidad: la PK, los `HasMaxLength` derivados del tipo de
columna, los `decimal(10,2)`, los `HasDefaultValue(true)` de los campos `Estado`, y un índice único en
`Gmail` más índices sobre las FKs que se consultan con frecuencia.

La relación `RolPermiso` se configura con clave primaria compuesta
`HasKey(rp => new { rp.RolId, rp.PermisoId })` y `DeleteBehavior.Cascade` en ambos lados:
al borrar un rol se van sus permisos y al borrar un permiso se van sus asignaciones.

`Usuario → Rol` usa `DeleteBehavior.Restrict` para que no se pueda borrar un rol que
aún tenga usuarios. Las FKs de negocio (`Prestamo→Cliente`, `Contrato→Cliente`,
`Contrato→Propiedad`, `Pago→Usuario`, `PagoDetalle→*`) también en `Restrict` para no
borrar en cascada historial financiero. `PagoDetalle → Pago` sí va en `Cascade`.

### 3. Autenticación

`IJwtTokenGenerator.GenerarToken` pasa a
`GenerarToken(int usuarioId, string nombre, string rol, IEnumerable<string> permisos)`.

`JwtTokenGenerator` emite:

- `sub` = id del usuario
- `ClaimTypes.Name` = nombre
- `ClaimTypes.Role` = nombre del rol (se mantiene para `[Authorize(Roles = ...)]`)
- un claim `Permiso` por cada permiso del usuario

Se añade `MapInboundClaims = false` en las opciones de JwtBearer para que los claims
conserven sus nombres y no se reescriban.

`IniciarSesionCommand` busca por `Gmail` con `Estado == true`, trae el rol y sus
permisos en una sola consulta con `Include(u => u.Rol).ThenInclude(r => r.Permisos)`, y
devuelve `AuthResponseDto(Token, Nombre, Rol, Permisos)` para que el frontend Angular
pueda ocultar botones sin tener que decodificar el token.

`RegistrarUsuarioCommand` resuelve el rol **en el servidor** por nombre (`Cobrador`)
y devuelve `int`.

### 4. Autorización por permisos

Los nombres de permiso son constantes de código en
`Application/Common/Permisos.cs`, para que el seed y las policies no puedan divergir:

```
Clientes.Ver, Clientes.Crear, Clientes.Editar, Clientes.Eliminar
Prestamos.Ver, Prestamos.Crear, Prestamos.Editar, Prestamos.Abonos
Contratos.Ver, Contratos.Crear, Contratos.Editar
Propiedades.Ver, Propiedades.Crear, Propiedades.Editar
Pagos.Ver, Pagos.Crear
Usuarios.Ver, Usuarios.Crear, Usuarios.Editar, Usuarios.GestionarRoles
Reportes.Ver
```

En `Program.cs` se recorre esa lista y se registra una policy por permiso que exige un
claim `Permiso` con ese valor. Se registra además la policy `RolAdministrador` para
las acciones de gestión de usuarios.

Se evita consultar la base al arrancar: las policies se derivan de las constantes, no de
la tabla. La tabla `Permiso` sigue siendo la fuente de verdad en tiempo de ejecución
porque los claims salen de ella en el login.

Los controladores pasan de `[Authorize]` a `[Authorize(Policy = Permisos.Clientes.Crear)]`.

### 5. Migración y seed

Se borran `Inicial_CrearTablasFinanzas`, su `.Designer.cs` y
`ApplicationDbContextModelSnapshot`, que son inservibles: describen tablas y columnas
que no existen en la base real.

Se genera un baseline nuevo que produce el esquema de las 12 tablas. Como las tablas ya
existen en la base, **no se ejecuta `database update` automáticamente**: se genera el
script idempotente con `dotnet ef migrations script --idempotent` y se revisa a mano.

El seed va en un `DbInitializer` explícito que corre al arrancar la API, no en `OnModelCreating`.
Inserta los permisos de la lista de constantes, los roles `Administrador` (todos los
permisos), `Cobrador` (operación de clientes, préstamos y pagos) y `Cliente` (solo
consulta de sus propios datos), y es idempotente: no duplica si ya existen.

El `ALTER TABLE` de `Rol.id` a IDENTITY y el índice único de `Gmail` van en un script
SQL aparte que se ejecuta antes de regenerar el baseline, para que la base y el modelo
coincidan exactamente.

### 6. Correcciones de seguridad

Estas dos no fueron el encargo original pero se detectaron al leer el flujo de
autenticación y son baratas de arreglar ahora:

**Escalada de privilegios en el registro.** `POST /api/Auth/registrar` es `[AllowAnonymous]`
y acepta un parámetro `Rol` del cliente. Cualquiera podía registrarse como `Administrador`.
Se elimina el parámetro; el rol se resuelve en el servidor por nombre.

**Secreto JWT en texto plano.** `JwtSettings:Secret` está en `appsettings.json`, archivo
versionado en git. Se mueve a *user secrets*, con `appsettings.json` dejando solo issuer,
audience y duración.

Se corrige además `DesignTimeDbContextFactory`, que apunta a `FinanzasManagerDb`, base
que no existe. Pasa a leer `FinanzasDb` de la configuración.

## Fuera de alcance

- Endpoints CRUD para `Pago`, `PagoDetalle`, `MetodoPago`, `Contrato` y `Propiedad`.
  Se mapean las entidades y sus relaciones, pero no se escriben comandos MediatR nuevos.
- Migración de datos. Las tablas están vacías, no hay nada que conservar.
- Pruebas automatizadas. No hay proyecto de tests en el repositorio.
- Quitar el scaffolding de `WeatherForecastController` y los `Put`/`Delete` vacíos de
  `PrestamosController`. Se puede hacer aparte.

## Verificación

1. `dotnet build` termina sin errores ni advertencias nuevas.
2. El script `ALTER TABLE` se aplica y `Rol.id` pasa a ser IDENTITY.
3. `dotnet ef migrations script --idempotent` genera DDL que coincide con el esquema
   real tabla por tabla, comparado contra `sys.columns` y `sys.foreign_keys`.
4. Arranca la API: el seed completa sin duplicar y `SELECT COUNT(*)` de `Rol`, `Permiso`
   y `RolPermiso` devuelve los valores esperados.
5. `POST /api/Auth/registrar` crea un usuario con rol `Cobrador` e `ignora` cualquier
   `Rol` enviado en el cuerpo.
6. `POST /api/Auth/login` devuelve token con claims de rol y permisos.
7. `POST /api/Clientes` sin token → 401. Con token de `Administrador` → 201.
   Con token de un rol sin permiso `Clientes.Crear` → 403.