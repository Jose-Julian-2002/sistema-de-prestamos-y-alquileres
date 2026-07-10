using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using FinanzasManager.Domain.Entities;
namespace FinanzasManager.Application.Features.Prestamos.Commands
{
    public record CrearPrestamoCommand(
        Guid ClienteId,
        decimal MontoCapital,
        decimal PorcentajeInteres
    ) : IRequest<Guid>;

    public class CrearPrestamoCommandHandler : IRequestHandler<CrearPrestamoCommand, Guid>
    {
        public async Task<Guid> Handle(CrearPrestamoCommand request, CancellationToken cancellationToken)
        {
            // Aplicamos las reglas del negocio usando la entidad de Dominio
            var nuevoPrestamo = new Prestamo(
                request.ClienteId,
                request.MontoCapital,
                request.PorcentajeInteres
            );

            // TODO: Aquí llamaremos al repositorio de Infraestructura para guardarlo en SQL Server.
            // Simulamos un retraso asíncrono temporal para que compile sin errores
            await Task.CompletedTask;

            // Retornamos el ID del préstamo recién creado
            return nuevoPrestamo.Id;
        }
    }
}
