using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using FinanzasManager.Domain.Entities;
using FinanzasManager.Application.Common.Interfaces;

namespace FinanzasManager.Application.Features.Prestamos.Commands
{
    public record CrearPrestamoCommand(
    Guid ClienteId,
    decimal MontoCapital,
    decimal PorcentajeInteres
) : IRequest<Guid>;

    public class CrearPrestamoCommandHandler : IRequestHandler<CrearPrestamoCommand, Guid>
    {
        // Inyectamos la interfaz que creamos en el paso anterior
        private readonly IApplicationDbContext _context;

        public CrearPrestamoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CrearPrestamoCommand request, CancellationToken cancellationToken)
        {
            // 1. Creamos la entidad (Aquí adentro ya se genera la primera transacción de "Desembolso")
            var nuevoPrestamo = new Prestamo(
                request.ClienteId,
                request.MontoCapital,
                request.PorcentajeInteres
            );

            // 2. Lo agregamos al DbSet a través de la interfaz
            _context.Prestamos.Add(nuevoPrestamo);

            // 3. Guardamos los cambios de forma asíncrona en la Base de Datos
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Retornamos el ID
            return nuevoPrestamo.Id;
        }
    }
}
