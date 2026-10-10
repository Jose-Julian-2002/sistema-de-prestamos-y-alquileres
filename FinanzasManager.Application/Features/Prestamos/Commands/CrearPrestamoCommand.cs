using System;
using System.Threading;
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
        decimal TotalCuotas,
        DateTime FechaInicio) : IRequest<int>;

    public class CrearPrestamoCommandHandler : IRequestHandler<CrearPrestamoCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CrearPrestamoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CrearPrestamoCommand request, CancellationToken cancellationToken)
        {
            ValidacionesPrestamo.Validar(request.MontoCapital, request.Interes, request.TotalCuotas);

            await ValidacionesPrestamo.ValidarClienteActivo(_context, request.ClienteId, cancellationToken);

            var nuevoPrestamo = new Prestamo(
                request.ClienteId,
                request.MontoCapital,
                request.Interes,
                request.TotalCuotas,
                request.FechaInicio);

            _context.Prestamos.Add(nuevoPrestamo);
            await _context.SaveChangesAsync(cancellationToken);

            return nuevoPrestamo.Id;
        }
    }
}