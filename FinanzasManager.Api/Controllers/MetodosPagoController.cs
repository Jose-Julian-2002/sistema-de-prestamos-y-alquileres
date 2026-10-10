using FinanzasManager.Application.Common;
using FinanzasManager.Application.Features.Pagos.MetodosPago.Commands;
using FinanzasManager.Application.Features.Pagos.MetodosPago.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetodosPagoController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MetodosPagoController(IMediator mediator) => _mediator = mediator;

        // GET api/MetodosPago
        [Authorize(Policy = Permisos.MetodosPago.Ver)]
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] ListarMetodosPagoQuery consulta)
        {
            var resultado = await _mediator.Send(consulta);
            return Ok(resultado);
        }

        // GET api/MetodosPago/5
        [Authorize(Policy = Permisos.MetodosPago.Ver)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var metodo = await _mediator.Send(new ObtenerMetodoPagoQuery(id));
            return Ok(metodo);
        }

        // POST api/MetodosPago
        [Authorize(Policy = Permisos.MetodosPago.Crear)]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearMetodoPagoCommand comando)
        {
            int metodoId = await _mediator.Send(comando);
            return CreatedAtAction(nameof(Obtener), new { id = metodoId }, metodoId);
        }

        // PUT api/MetodosPago/5
        [Authorize(Policy = Permisos.MetodosPago.Editar)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarMetodoPagoRequest cuerpo)
        {
            await _mediator.Send(new ActualizarMetodoPagoCommand(id, cuerpo.Nombre));
            return NoContent();
        }

        // DELETE api/MetodosPago/5  -> borrado fisico, se rechaza si tiene pagos
        [Authorize(Policy = Permisos.MetodosPago.Eliminar)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _mediator.Send(new EliminarMetodoPagoCommand(id));
            return NoContent();
        }
    }

    public record ActualizarMetodoPagoRequest(string Nombre);
}