using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using FinanzasManager.Application.Common.Interfaces;
using FinanzasManager.Domain.Entities;

namespace FinanzasManager.Application.Features.Prestamos.Clientes.Commands
{
    public record CrearClienteCommand(string Nombre, string DocumentoIdentidad, string Telefono) : IRequest<Guid>;

    // 2. El Handler que guarda el cliente en SQL Server
    public class CrearClienteCommandHandler : IRequestHandler<CrearClienteCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CrearClienteCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CrearClienteCommand request, CancellationToken cancellationToken)
        {
            var nuevoCliente = new Cliente(request.Nombre, request.DocumentoIdentidad, request.Telefono);

            _context.Clientes.Add(nuevoCliente);
            await _context.SaveChangesAsync(cancellationToken);

            return nuevoCliente.Id;
        }
    }
}
