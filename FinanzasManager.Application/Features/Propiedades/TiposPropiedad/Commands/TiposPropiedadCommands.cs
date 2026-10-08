using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Propiedades.TiposPropiedad.Commands
{
    public record CrearTipoPropiedadCommand(string Nombre) : IRequest<int>;

    public class CrearTipoPropiedadCommandHandler : IRequestHandler<CrearTipoPropiedadCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CrearTipoPropiedadCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CrearTipoPropiedadCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre del tipo de propiedad es obligatorio.");

            var nombre = request.Nombre.Trim();

            // El nombre es la clave del catalogo: "Casa" y "casa" son el mismo tipo.
            var nombreEnOtro = await _context.TipoPropiedades
                .AnyAsync(t => t.Nombre.ToLower() == nombre.ToLower(), cancellationToken);

            if (nombreEnOtro)
                throw new InvalidOperationException("Ya existe un tipo de propiedad con ese nombre.");

            var nuevoTipo = new TipoPropiedad(nombre);

            _context.TipoPropiedades.Add(nuevoTipo);
            await _context.SaveChangesAsync(cancellationToken);

            return nuevoTipo.Id;
        }
    }

    public record ActualizarTipoPropiedadCommand(int Id, string Nombre) : IRequest<Unit>;

    public class ActualizarTipoPropiedadCommandHandler : IRequestHandler<ActualizarTipoPropiedadCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public ActualizarTipoPropiedadCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(ActualizarTipoPropiedadCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre del tipo de propiedad es obligatorio.");

            var tipo = await _context.TipoPropiedades
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (tipo is null)
                throw new InvalidOperationException($"No existe un tipo de propiedad con el Id {request.Id}.");

            tipo.Actualizar(request.Nombre);

            var nombreEnOtro = await _context.TipoPropiedades
                .AnyAsync(t => t.Nombre.ToLower() == tipo.Nombre.ToLower() && t.Id != request.Id, cancellationToken);

            if (nombreEnOtro)
                throw new InvalidOperationException("Ya existe otro tipo de propiedad con ese nombre.");

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }

    // El catalogo no tiene columna Estado, asi que aqui el borrado es fisico.
    // No se puede eliminar un tipo que tenga propiedades asociadas: la FK lo restringe.
    public record EliminarTipoPropiedadCommand(int Id) : IRequest<Unit>;

    public class EliminarTipoPropiedadCommandHandler : IRequestHandler<EliminarTipoPropiedadCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public EliminarTipoPropiedadCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(EliminarTipoPropiedadCommand request, CancellationToken cancellationToken)
        {
            var tipo = await _context.TipoPropiedades
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (tipo is null)
                throw new InvalidOperationException($"No existe un tipo de propiedad con el Id {request.Id}.");

            var tienePropiedades = await _context.Propiedades
                .AnyAsync(p => p.TipoPropiedadId == request.Id, cancellationToken);

            if (tienePropiedades)
                throw new InvalidOperationException("No se puede eliminar el tipo porque tiene propiedades asociadas.");

            _context.TipoPropiedades.Remove(tipo);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}