using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FinanzasManager.Application.Features.Prestamos.Clientes.Commands;
using MediatR;

namespace FinanzasManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientesController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearClienteCommand comando)
        {
            Guid clienteId = await _mediator.Send(comando);
            return CreatedAtAction(nameof(Crear), new { id = clienteId }, clienteId);
        }
    }
}
