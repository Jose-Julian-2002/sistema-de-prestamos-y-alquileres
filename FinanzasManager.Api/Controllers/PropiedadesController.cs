using FinanzasManager.Application.Common;
using FinanzasManager.Application.Features.Propiedades.Commands;
using FinanzasManager.Application.Features.Propiedades.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropiedadesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PropiedadesController(IMediator mediator) => _mediator = mediator;

        // GET api/Propiedades
        [Authorize(Policy = Permisos.Propiedades.Ver)]
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] ListarPropiedadesQuery consulta)
        {
            var resultado = await _mediator.Send(consulta);
            return Ok(resultado);
        }

        // GET api/Propiedades/5
        [Authorize(Policy = Permisos.Propiedades.Ver)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var propiedad = await _mediator.Send(new ObtenerPropiedadQuery(id));
            return Ok(propiedad);
        }

        // POST api/Propiedades
        [Authorize(Policy = Permisos.Propiedades.Crear)]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearPropiedadCommand comando)
        {
            int propiedadId = await _mediator.Send(comando);
            return CreatedAtAction(nameof(Obtener), new { id = propiedadId }, propiedadId);
        }

        // PUT api/Propiedades/5
        [Authorize(Policy = Permisos.Propiedades.Editar)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarPropiedadRequest cuerpo)
        {
            await _mediator.Send(new ActualizarPropiedadCommand(
                id,
                cuerpo.TipoPropiedadId,
                cuerpo.Nombre,
                cuerpo.Direccion,
                cuerpo.CodigoLuz,
                cuerpo.CodigoAgua,
                cuerpo.CodigoGas));

            return NoContent();
        }

        // PUT api/Propiedades/5/estado  -> activa o desactiva
        [Authorize(Policy = Permisos.Propiedades.Editar)]
        [HttpPut("{id:int}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoRequest cuerpo)
        {
            await _mediator.Send(new CambiarEstadoPropiedadCommand(id, cuerpo.Estado));
            return NoContent();
        }

        // DELETE api/Propiedades/5  -> borrado logico, salvo que tenga contratos
        [Authorize(Policy = Permisos.Propiedades.Eliminar)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _mediator.Send(new EliminarPropiedadCommand(id));
            return NoContent();
        }
    }

    // Cuerpo separado del command: el id viaja en la ruta, no en el JSON.
    // CambiarEstadoRequest se reusa de UsuariosController (mismo namespace).
    public record ActualizarPropiedadRequest(
        int TipoPropiedadId,
        string Nombre,
        string? Direccion,
        string CodigoLuz,
        string CodigoAgua,
        string? CodigoGas);
}