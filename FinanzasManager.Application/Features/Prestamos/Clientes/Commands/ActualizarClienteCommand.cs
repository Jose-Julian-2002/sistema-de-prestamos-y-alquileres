using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Prestamos.Clientes.Commands
{
    // Edita los datos del cliente. El estado NO se toca aqui:
    // activar o desactivar va en su propio comando.
    public record ActualizarClienteCommand(int Id, string Nombre, string? DocumentoIdentidad, string? Telefono) : IRequest<Unit>;

    public class ActualizarClienteCommandHandler : IRequestHandler<ActualizarClienteCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public ActualizarClienteCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(ActualizarClienteCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre del cliente es obligatorio.");

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (cliente is null)
                throw new InvalidOperationException($"No existe un cliente con el Id {request.Id}.");

            cliente.Actualizar(request.Nombre, request.DocumentoIdentidad, request.Telefono);

            // Tras normalizar ya se sabe cual es el documento final. Se excluye el propio
            // cliente de la comprobacion para no chocar consigo mismo al no cambiarlo.
            if (cliente.DocumentoIdentidad is not null)
            {
                var documentoEnOtro = await _context.Clientes
                    .AnyAsync(c => c.DocumentoIdentidad == cliente.DocumentoIdentidad && c.Id != request.Id, cancellationToken);

                if (documentoEnOtro)
                    throw new InvalidOperationException("Ya existe otro cliente registrado con ese documento de identidad.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}