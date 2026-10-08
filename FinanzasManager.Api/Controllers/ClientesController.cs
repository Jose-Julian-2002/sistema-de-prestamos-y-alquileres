using FinanzasManager.Application.Common;
using FinanzasManager.Application.Features.Prestamos.Clientes.Commands;
using FinanzasManager.Application.Features.Prestamos.Clientes.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientesController(IMediator mediator) => _mediator = mediator;

        // GET api/Clientes
        [Authorize(Policy = Permisos.Clientes.Ver)]
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] ListarClientesQuery consulta)
        {
            var resultado = await _mediator.Send(consulta);
            return Ok(resultado);
        }

        // GET api/Clientes/5
        [Authorize(Policy = Permisos.Clientes.Ver)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var cliente = await _mediator.Send(new ObtenerClienteQuery(id));
            return Ok(cliente);
        }

        // POST api/Clientes
        [Authorize(Policy = Permisos.Clientes.Crear)]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearClienteCommand comando)
        {
            int clienteId = await _mediator.Send(comando);
            return CreatedAtAction(nameof(Obtener), new { id = clienteId }, clienteId);
        }

        // PUT api/Clientes/5
        [Authorize(Policy = Permisos.Clientes.Editar)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarClienteRequest cuerpo)
        {
            await _mediator.Send(new ActualizarClienteCommand(id, cuerpo.Nombre, cuerpo.DocumentoIdentidad, cuerpo.Telefono));
            return NoContent();
        }

        // PUT api/Clientes/5/estado  -> activa o desactiva
        [Authorize(Policy = Permisos.Clientes.Editar)]
        [HttpPut("{id:int}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoRequest cuerpo)
        {
            await _mediator.Send(new CambiarEstadoClienteCommand(id, cuerpo.Estado));
            return NoContent();
        }

        // DELETE api/Clientes/5  -> borrado logico, el cliente queda con Estado = false
        [Authorize(Policy = Permisos.Clientes.Eliminar)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _mediator.Send(new EliminarClienteCommand(id));
            return NoContent();
        }
    }

    // Cuerpo separado del command: el id viaja en la ruta, no en el JSON.
    // CambiarEstadoRequest se reusa de UsuariosController (mismo namespace).
    public record ActualizarClienteRequest(string Nombre, string? DocumentoIdentidad, string? Telefono);
}