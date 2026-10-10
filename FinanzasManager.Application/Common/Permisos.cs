namespace FinanzasManager.Application.Common
{
    // Nombres tecnicos de los permisos del sistema.
    // Son la fuente de verdad compartida: el seed inserta estos nombres en la tabla
    // Permiso y Program.cs registra una policy por cada uno. Asi no pueden divergir.
    public static class Permisos
    {
        public const string Claim = "Permiso";

        // Nombre de la policy que exige el rol de administrador.
        public const string PolicyAdministrador = "RolAdministrador";

        // Nombres de los roles tal como se guardan en la tabla Rol.
        public const string NombreRolAdministrador = "Administrador";
        public const string NombreRolCobrador = "Cobrador";
        public const string NombreRolCliente = "Cliente";

        public static class Clientes
        {
            public const string Ver = "Clientes.Ver";
            public const string Crear = "Clientes.Crear";
            public const string Editar = "Clientes.Editar";
            public const string Eliminar = "Clientes.Eliminar";
        }

        public static class Prestamos
        {
            public const string Ver = "Prestamos.Ver";
            public const string Crear = "Prestamos.Crear";
            public const string Editar = "Prestamos.Editar";
            public const string Abonos = "Prestamos.Abonos";
            public const string Eliminar = "Prestamos.Eliminar";
        }

        public static class Contratos
        {
            public const string Ver = "Contratos.Ver";
            public const string Crear = "Contratos.Crear";
            public const string Editar = "Contratos.Editar";
            public const string Eliminar = "Contratos.Eliminar";
        }

        public static class Propiedades
        {
            public const string Ver = "Propiedades.Ver";
            public const string Crear = "Propiedades.Crear";
            public const string Editar = "Propiedades.Editar";
            public const string Eliminar = "Propiedades.Eliminar";
        }

        // Catalogo de tipos de propiedad (casa, apartamento, local...).
        // El nombre va en plural para seguir la misma convencion que el resto de grupos.
        public static class TiposPropiedad
        {
            public const string Ver = "TiposPropiedad.Ver";
            public const string Crear = "TiposPropiedad.Crear";
            public const string Editar = "TiposPropiedad.Editar";
            public const string Eliminar = "TiposPropiedad.Eliminar";
        }

        public static class Pagos
        {
            public const string Ver = "Pagos.Ver";
            public const string Crear = "Pagos.Crear";
            public const string Eliminar = "Pagos.Eliminar";
        }

        // Catalogo de metodos de pago (efectivo, transferencia...). Plural, igual que el resto.
        public static class MetodosPago
        {
            public const string Ver = "MetodosPago.Ver";
            public const string Crear = "MetodosPago.Crear";
            public const string Editar = "MetodosPago.Editar";
            public const string Eliminar = "MetodosPago.Eliminar";
        }

        public static class Usuarios
        {
            public const string Ver = "Usuarios.Ver";
            public const string Crear = "Usuarios.Crear";
            public const string Editar = "Usuarios.Editar";
            public const string GestionarRoles = "Usuarios.GestionarRoles";
        }

        public static class Roles
        {
            public const string Ver = "Roles.Ver";
            public const string Crear = "Roles.Crear";
            public const string Editar = "Roles.Editar";
            public const string Eliminar = "Roles.Eliminar";
            public const string AsignarPermisos = "Roles.AsignarPermisos";
        }

        public const string ReportesVer = "Reportes.Ver";

        // Lista completa. El seed usa esta enumeracion y Program.cs registra una
        // policy por cada elemento, asi que anadir un permiso aqui lo propaga a ambos.
        public static readonly IReadOnlyList<string> Todos = new List<string>
        {
            Clientes.Ver, Clientes.Crear, Clientes.Editar, Clientes.Eliminar,
            Prestamos.Ver, Prestamos.Crear, Prestamos.Editar, Prestamos.Abonos, Prestamos.Eliminar,
            Contratos.Ver, Contratos.Crear, Contratos.Editar, Contratos.Eliminar,
            Propiedades.Ver, Propiedades.Crear, Propiedades.Editar, Propiedades.Eliminar,
            TiposPropiedad.Ver, TiposPropiedad.Crear, TiposPropiedad.Editar, TiposPropiedad.Eliminar,
            Pagos.Ver, Pagos.Crear, Pagos.Eliminar,
            MetodosPago.Ver, MetodosPago.Crear, MetodosPago.Editar, MetodosPago.Eliminar,
            Usuarios.Ver, Usuarios.Crear, Usuarios.Editar, Usuarios.GestionarRoles,
            Roles.Ver, Roles.Crear, Roles.Editar, Roles.Eliminar, Roles.AsignarPermisos,
            ReportesVer
        };

        // Permisos del rol Administrador: todos.
        public static readonly IReadOnlyList<string> DeAdministrador = Todos;

        // Permisos del rol Cobrador: operacion diaria, sin tocar la configuracion de usuarios.
        public static readonly IReadOnlyList<string> DeCobrador = new List<string>
        {
            Clientes.Ver, Clientes.Crear, Clientes.Editar,
            Prestamos.Ver, Prestamos.Crear, Prestamos.Editar, Prestamos.Abonos,
            Contratos.Ver, Contratos.Crear, Contratos.Editar,
            Propiedades.Ver, Propiedades.Crear,
            // El cobrador necesita el catalogo para elegir metodo al registrar un pago.
            TiposPropiedad.Ver,
            MetodosPago.Ver,
            Pagos.Ver, Pagos.Crear,
            ReportesVer
        };

        // Permisos del rol Cliente: solo consulta.
        public static readonly IReadOnlyList<string> DeCliente = new List<string>
        {
            Prestamos.Ver,
            Contratos.Ver,
            Pagos.Ver
        };
    }
}