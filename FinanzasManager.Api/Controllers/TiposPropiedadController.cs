using FinanzasManager.Application.Common;
using FinanzasManager.Application.Features.Propiedades.TiposPropiedad.Commands;
using FinanzasManager.Application.Features.Propiedades.TiposPropiedad.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiposPropiedadController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TiposPropiedadController(IMediator mediator) => _mediator = mediator;

        // GET api/TiposPropiedad
        [Authorize(Policy = Permisos.TiposPropiedad.Ver)]
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] ListarTiposPropiedadQuery consulta)
        {
            var resultado = await _mediator.Send(consulta);
            return Ok(resultado);
        }

        // GET api/TiposPropiedad/5
        [Authorize(Policy = Permisos.TiposPropiedad.Ver)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var tipo = await _mediator.Send(new ObtenerTipoPropiedadQuery(id));
            return Ok(tipo);
        }

        // POST api/TiposPropiedad
        [Authorize(Policy = Permisos.TiposPropiedad.Crear)]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearTipoPropiedadCommand comando)
        {
            int tipoId = await _mediator.Send(comando);
            return CreatedAtAction(nameof(Obtener), new { id = tipoId }, tipoId);
        }

        // PUT api/TiposPropiedad/5
        [Authorize(Policy = Permisos.TiposPropiedad.Editar)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarTipoPropiedadRequest cuerpo)
        {
            await _mediator.Send(new ActualizarTipoPropiedadCommand(id, cuerpo.Nombre));
            return NoContent();
        }

        // DELETE api/TiposPropiedad/5  -> borrado fisico, se rechaza si tiene propiedades
        [Authorize(Policy = Permisos.TiposPropiedad.Eliminar)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _mediator.Send(new EliminarTipoPropiedadCommand(id));
            return NoContent();
        }
    }

    public record ActualizarTipoPropiedadRequest(string Nombre);
}