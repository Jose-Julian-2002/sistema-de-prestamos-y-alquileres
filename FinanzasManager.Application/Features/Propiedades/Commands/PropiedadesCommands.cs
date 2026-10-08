using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Propiedades.Commands
{
    public record CrearPropiedadCommand(
        int TipoPropiedadId,
        string Nombre,
        string? Direccion,
        string CodigoLuz,
        string CodigoAgua,
        string? CodigoGas) : IRequest<int>;

    public class CrearPropiedadCommandHandler : IRequestHandler<CrearPropiedadCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CrearPropiedadCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CrearPropiedadCommand request, CancellationToken cancellationToken)
        {
            ValidarCampos(request.Nombre, request.CodigoLuz, request.CodigoAgua);

            // La FK es restrictiva: si el tipo no existe, el insert fallaria con
            // un error de SQL. Se comprueba antes para devolver un mensaje claro.
            var tipoExiste = await _context.TipoPropiedades
                .AnyAsync(t => t.Id == request.TipoPropiedadId, cancellationToken);

            if (!tipoExiste)
                throw new InvalidOperationException($"No existe un tipo de propiedad con el Id {request.TipoPropiedadId}.");

            var nuevaPropiedad = new Propiedad(
                request.TipoPropiedadId,
                request.Nombre,
                request.Direccion,
                request.CodigoLuz,
                request.CodigoAgua,
                request.CodigoGas);

            _context.Propiedades.Add(nuevaPropiedad);
            await _context.SaveChangesAsync(cancellationToken);

            return nuevaPropiedad.Id;
        }

        internal static void ValidarCampos(string nombre, string codigoLuz, string codigoAgua)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la propiedad es obligatorio.");

            if (string.IsNullOrWhiteSpace(codigoLuz))
                throw new ArgumentException("El codigo de luz es obligatorio.");

            if (string.IsNullOrWhiteSpace(codigoAgua))
                throw new ArgumentException("El codigo de agua es obligatorio.");
        }
    }

    public record ActualizarPropiedadCommand(
        int Id,
        int TipoPropiedadId,
        string Nombre,
        string? Direccion,
        string CodigoLuz,
        string CodigoAgua,
        string? CodigoGas) : IRequest<Unit>;

    public class ActualizarPropiedadCommandHandler : IRequestHandler<ActualizarPropiedadCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public ActualizarPropiedadCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(ActualizarPropiedadCommand request, CancellationToken cancellationToken)
        {
            CrearPropiedadCommandHandler.ValidarCampos(request.Nombre, request.CodigoLuz, request.CodigoAgua);

            var propiedad = await _context.Propiedades
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (propiedad is null)
                throw new InvalidOperationException($"No existe una propiedad con el Id {request.Id}.");

            var tipoExiste = await _context.TipoPropiedades
                .AnyAsync(t => t.Id == request.TipoPropiedadId, cancellationToken);

            if (!tipoExiste)
                throw new InvalidOperationException($"No existe un tipo de propiedad con el Id {request.TipoPropiedadId}.");

            propiedad.Actualizar(
                request.TipoPropiedadId,
                request.Nombre,
                request.Direccion,
                request.CodigoLuz,
                request.CodigoAgua,
                request.CodigoGas);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }

    // Activa o desactiva la propiedad. No borra la fila: si tiene contratos
    // vigentes debe seguir existiendo para el historial.
    public record CambiarEstadoPropiedadCommand(int Id, bool Estado) : IRequest<Unit>;

    public class CambiarEstadoPropiedadCommandHandler : IRequestHandler<CambiarEstadoPropiedadCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public CambiarEstadoPropiedadCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(CambiarEstadoPropiedadCommand request, CancellationToken cancellationToken)
        {
            var propiedad = await _context.Propiedades
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (propiedad is null)
                throw new InvalidOperationException($"No existe una propiedad con el Id {request.Id}.");

            if (propiedad.Estado == request.Estado)
                return Unit.Value;

            propiedad.Estado = request.Estado;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }

    public record EliminarPropiedadCommand(int Id) : IRequest<Unit>;

    public class EliminarPropiedadCommandHandler : IRequestHandler<EliminarPropiedadCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public EliminarPropiedadCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(EliminarPropiedadCommand request, CancellationToken cancellationToken)
        {
            var propiedad = await _context.Propiedades
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (propiedad is null)
                throw new InvalidOperationException($"No existe una propiedad con el Id {request.Id}.");

            // Con contratos la fila no se toca. Desactivarla es la unica salida,
            // y el frontend deberia ofrecerla en vez de un borrado que va a fallar.
            var tieneContratos = await _context.Contratos
                .AnyAsync(c => c.PropiedadId == request.Id, cancellationToken);

            if (tieneContratos)
                throw new InvalidOperationException("No se puede eliminar la propiedad porque tiene contratos asociados. Desactivela en su lugar.");

            if (!propiedad.Estado)
                return Unit.Value;

            propiedad.Estado = false;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}