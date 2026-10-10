using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Contratos.Commands
{
    // Edita los terminos del contrato. El cobrador que lo registro no cambia
    // aqui: UsuarioId se deja intacto.
    public record ActualizarContratoCommand(
        int Id,
        int PropiedadId,
        int ClienteId,
        decimal MontoAlquiler,
        decimal MontoGarantia,
        DateTime DiaPago,
        DateTime FechaInicio,
        DateTime FechaFin) : IRequest<Unit>;

    public class ActualizarContratoCommandHandler : IRequestHandler<ActualizarContratoCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public ActualizarContratoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(ActualizarContratoCommand request, CancellationToken cancellationToken)
        {
            ValidacionesContrato.Validar(
                request.MontoAlquiler,
                request.MontoGarantia,
                request.FechaInicio,
                request.FechaFin);

            var contrato = await _context.Contratos
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (contrato is null)
                throw new InvalidOperationException($"No existe un contrato con el Id {request.Id}.");

            await ValidacionesContrato.ValidarReferencias(
                _context,
                request.PropiedadId,
                request.ClienteId,
                contrato.UsuarioId,
                cancellationToken);

            contrato.Actualizar(
                request.PropiedadId,
                request.ClienteId,
                request.MontoAlquiler,
                request.MontoGarantia,
                request.DiaPago,
                request.FechaInicio,
                request.FechaFin);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}