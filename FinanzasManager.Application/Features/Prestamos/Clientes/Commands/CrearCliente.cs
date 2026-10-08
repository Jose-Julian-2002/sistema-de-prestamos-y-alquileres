using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanzasManager.Application.Features.Prestamos.Clientes.Commands
{
    public record CrearClienteCommand(string Nombre, string? DocumentoIdentidad, string? Telefono) : IRequest<int>;

    // 2. El Handler que guarda el cliente en SQL Server
    public class CrearClienteCommandHandler : IRequestHandler<CrearClienteCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CrearClienteCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CrearClienteCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre del cliente es obligatorio.");

            var nuevoCliente = new Cliente(request.Nombre, request.DocumentoIdentidad, request.Telefono);

            // La base tiene indice unico en DocumentoIdentidad. Se comprueba aqui para
            // devolver un 409 con un mensaje util en vez de un error de SQL al guardar.
            if (nuevoCliente.DocumentoIdentidad is not null)
            {
                var documentoEnOtro = await _context.Clientes
                    .AnyAsync(c => c.DocumentoIdentidad == nuevoCliente.DocumentoIdentidad, cancellationToken);

                if (documentoEnOtro)
                    throw new InvalidOperationException("Ya existe un cliente registrado con ese documento de identidad.");
            }

            _context.Clientes.Add(nuevoCliente);
            await _context.SaveChangesAsync(cancellationToken);

            return nuevoCliente.Id;
        }
    }
}