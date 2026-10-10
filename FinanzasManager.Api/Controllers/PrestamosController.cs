using FinanzasManager.Application.Common;
using FinanzasManager.Application.Features.Prestamos.Commands;
using FinanzasManager.Application.Features.Prestamos.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrestamosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PrestamosController(IMediator mediator) => _mediator = mediator;

        // GET api/Prestamos
        [Authorize(Policy = Permisos.Prestamos.Ver)]
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] ListarPrestamosQuery consulta)
        {
            var resultado = await _mediator.Send(consulta);
            return Ok(resultado);
        }

        // GET api/Prestamos/5
        [Authorize(Policy = Permisos.Prestamos.Ver)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var prestamo = await _mediator.Send(new ObtenerPrestamoQuery(id));
            return Ok(prestamo);
        }

        // POST api/Prestamos
        [Authorize(Policy = Permisos.Prestamos.Crear)]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearPrestamoCommand comando)
        {
            var prestamoId = await _mediator.Send(comando);
            return CreatedAtAction(nameof(Obtener), new { id = prestamoId }, prestamoId);
        }

        // PUT api/Prestamos/5
        [Authorize(Policy = Permisos.Prestamos.Editar)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarPrestamoRequest cuerpo)
        {
            await _mediator.Send(new ActualizarPrestamoCommand(
                id,
                cuerpo.ClienteId,
                cuerpo.MontoCapital,
                cuerpo.Interes,
                cuerpo.TotalCuotas,
                cuerpo.FechaInicio));

            return NoContent();
        }

        // PUT api/Prestamos/5/estado  -> activa o desactiva
        [Authorize(Policy = Permisos.Prestamos.Editar)]
        [HttpPut("{id:int}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoRequest cuerpo)
        {
            await _mediator.Send(new CambiarEstadoPrestamoCommand(id, cuerpo.Estado));
            return NoContent();
        }

        // DELETE api/Prestamos/5  -> borrado logico, el prestamo queda con Estado = false
        [Authorize(Policy = Permisos.Prestamos.Eliminar)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _mediator.Send(new EliminarPrestamoCommand(id));
            return NoContent();
        }
    }

    // Cuerpo separado del command: el id viaja en la ruta, no en el JSON.
    // CambiarEstadoRequest se reusa de UsuariosController (mismo namespace).
    public record ActualizarPrestamoRequest(
        int ClienteId,
        decimal MontoCapital,
        decimal Interes,
        decimal TotalCuotas,
        DateTime FechaInicio);
}