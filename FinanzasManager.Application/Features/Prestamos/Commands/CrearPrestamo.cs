using System;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Prestamos.Commands
{
    public record CrearPrestamoCommand(
        int ClienteId,
        decimal MontoCapital,
        decimal Interes,
        decimal TotalCuotas
    ) : IRequest<int>;

    public class CrearPrestamoCommandHandler : IRequestHandler<CrearPrestamoCommand, int>
    {
        // Inyectamos la interfaz que creamos en el paso anterior
        private readonly IApplicationDbContext _context;

        public CrearPrestamoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CrearPrestamoCommand request, CancellationToken cancellationToken)
        {
            if (request.MontoCapital <= 0)
                throw new ArgumentException("El monto de capital debe ser mayor que cero.");

            if (request.Interes < 0)
                throw new ArgumentException("El interes no puede ser negativo.");

            if (request.TotalCuotas <= 0)
                throw new ArgumentException("El total de la cuota debe ser mayor que cero.");

            // 1. El prestamo siempre pertenece a un cliente existente.
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == request.ClienteId, cancellationToken);

            if (cliente is null)
                throw new ArgumentException($"No existe un cliente con el Id {request.ClienteId}.");

            // 2. Creamos la entidad
            var nuevoPrestamo = new Prestamo(
                request.ClienteId,
                request.MontoCapital,
                request.Interes,
                request.TotalCuotas,
                DateTime.Now
            );

            // 3. Lo agregamos al DbSet a traves de la interfaz
            _context.Prestamos.Add(nuevoPrestamo);

            // 4. Guardamos los cambios de forma asíncrona en la Base de Datos
            await _context.SaveChangesAsync(cancellationToken);

            // 5. Retornamos el ID
            return nuevoPrestamo.Id;
        }
    }
}