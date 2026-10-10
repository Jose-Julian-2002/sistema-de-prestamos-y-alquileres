using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Contratos.Commands
{
    public record CrearContratoCommand(
        int PropiedadId,
        int ClienteId,
        int UsuarioId,
        decimal MontoAlquiler,
        decimal MontoGarantia,
        DateTime DiaPago,
        DateTime FechaInicio,
        DateTime FechaFin) : IRequest<int>;

    public class CrearContratoCommandHandler : IRequestHandler<CrearContratoCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CrearContratoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CrearContratoCommand request, CancellationToken cancellationToken)
        {
            ValidacionesContrato.Validar(
                request.MontoAlquiler,
                request.MontoGarantia,
                request.FechaInicio,
                request.FechaFin);

            await ValidacionesContrato.ValidarReferencias(
                _context,
                request.PropiedadId,
                request.ClienteId,
                request.UsuarioId,
                cancellationToken);

            var nuevoContrato = new Contrato(
                request.PropiedadId,
                request.ClienteId,
                request.UsuarioId,
                request.MontoAlquiler,
                request.MontoGarantia,
                request.DiaPago,
                request.FechaInicio,
                request.FechaFin);

            _context.Contratos.Add(nuevoContrato);
            await _context.SaveChangesAsync(cancellationToken);

            return nuevoContrato.Id;
        }
    }
}