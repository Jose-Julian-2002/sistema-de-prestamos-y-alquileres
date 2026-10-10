using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Pagos.MetodosPago.Commands
{
    public record CrearMetodoPagoCommand(string Nombre) : IRequest<int>;

    public class CrearMetodoPagoCommandHandler : IRequestHandler<CrearMetodoPagoCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CrearMetodoPagoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CrearMetodoPagoCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre del metodo de pago es obligatorio.");

            var nombre = request.Nombre.Trim();

            // El nombre es la clave del catalogo: "Efectivo" y "efectivo" son lo mismo.
            var nombreEnOtro = await _context.MetodosPago
                .AnyAsync(m => m.Nombre.ToLower() == nombre.ToLower(), cancellationToken);

            if (nombreEnOtro)
                throw new InvalidOperationException("Ya existe un metodo de pago con ese nombre.");

            var nuevoMetodo = new MetodoPago(nombre);

            _context.MetodosPago.Add(nuevoMetodo);
            await _context.SaveChangesAsync(cancellationToken);

            return nuevoMetodo.Id;
        }
    }

    public record ActualizarMetodoPagoCommand(int Id, string Nombre) : IRequest<Unit>;

    public class ActualizarMetodoPagoCommandHandler : IRequestHandler<ActualizarMetodoPagoCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public ActualizarMetodoPagoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(ActualizarMetodoPagoCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre del metodo de pago es obligatorio.");

            var metodo = await _context.MetodosPago
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (metodo is null)
                throw new InvalidOperationException($"No existe un metodo de pago con el Id {request.Id}.");

            metodo.Actualizar(request.Nombre);

            var nombreEnOtro = await _context.MetodosPago
                .AnyAsync(m => m.Nombre.ToLower() == metodo.Nombre.ToLower() && m.Id != request.Id, cancellationToken);

            if (nombreEnOtro)
                throw new InvalidOperationException("Ya existe otro metodo de pago con ese nombre.");

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }

    // El catalogo no tiene columna Estado, asi que aqui el borrado es fisico.
    // No se puede eliminar un metodo que tenga pagos registrados: la FK lo restringe.
    public record EliminarMetodoPagoCommand(int Id) : IRequest<Unit>;

    public class EliminarMetodoPagoCommandHandler : IRequestHandler<EliminarMetodoPagoCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public EliminarMetodoPagoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(EliminarMetodoPagoCommand request, CancellationToken cancellationToken)
        {
            var metodo = await _context.MetodosPago
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (metodo is null)
                throw new InvalidOperationException($"No existe un metodo de pago con el Id {request.Id}.");

            var tienePagos = await _context.Pagos
                .AnyAsync(p => p.MetodoPagoId == request.Id, cancellationToken);

            if (tienePagos)
                throw new InvalidOperationException("No se puede eliminar el metodo porque tiene pagos registrados.");

            _context.MetodosPago.Remove(metodo);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}