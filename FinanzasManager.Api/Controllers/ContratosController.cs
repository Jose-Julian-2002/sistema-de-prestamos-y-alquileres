using System.Security.Claims;
using FinanzasManager.Application.Common;
using FinanzasManager.Application.Features.Contratos.Commands;
using FinanzasManager.Application.Features.Contratos.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContratosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContratosController(IMediator mediator) => _mediator = mediator;

        // Id del usuario del token. Se guarda como UsuarioId del contrato:
        // registra quien creo el contrato, no lo elige el cliente.
        private int? SolicitanteId
        {
            get
            {
                var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(claim, out var id) ? id : null;
            }
        }

        // GET api/Contratos
        [Authorize(Policy = Permisos.Contratos.Ver)]
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] ListarContratosQuery consulta)
        {
            var resultado = await _mediator.Send(consulta);
            return Ok(resultado);
        }

        // GET api/Contratos/5
        [Authorize(Policy = Permisos.Contratos.Ver)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var contrato = await _mediator.Send(new ObtenerContratoQuery(id));
            return Ok(contrato);
        }

        // POST api/Contratos
        [Authorize(Policy = Permisos.Contratos.Crear)]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearContratoRequest cuerpo)
        {
            if (!SolicitanteId.HasValue)
                return Unauthorized("No se pudo identificar al usuario del token.");

            var contratoId = await _mediator.Send(new CrearContratoCommand(
                cuerpo.PropiedadId,
                cuerpo.ClienteId,
                SolicitanteId.Value,
                cuerpo.MontoAlquiler,
                cuerpo.MontoGarantia,
                cuerpo.DiaPago,
                cuerpo.FechaInicio,
                cuerpo.FechaFin));

            return CreatedAtAction(nameof(Obtener), new { id = contratoId }, contratoId);
        }

        // PUT api/Contratos/5
        [Authorize(Policy = Permisos.Contratos.Editar)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarContratoRequest cuerpo)
        {
            await _mediator.Send(new ActualizarContratoCommand(
                id,
                cuerpo.PropiedadId,
                cuerpo.ClienteId,
                cuerpo.MontoAlquiler,
                cuerpo.MontoGarantia,
                cuerpo.DiaPago,
                cuerpo.FechaInicio,
                cuerpo.FechaFin));

            return NoContent();
        }

        // PUT api/Contratos/5/estado  -> activa o desactiva
        [Authorize(Policy = Permisos.Contratos.Editar)]
        [HttpPut("{id:int}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoRequest cuerpo)
        {
            await _mediator.Send(new CambiarEstadoContratoCommand(id, cuerpo.Estado));
            return NoContent();
        }

        // DELETE api/Contratos/5  -> borrado logico, el contrato queda con Estado = false
        [Authorize(Policy = Permisos.Contratos.Eliminar)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _mediator.Send(new EliminarContratoCommand(id));
            return NoContent();
        }
    }

    // Cuerpos separados de los commands: el id viaja en la ruta y UsuarioId lo
    // pone el servidor desde el token, nunca llega en el JSON.
    // CambiarEstadoRequest se reusa de UsuariosController (mismo namespace).
    public record CrearContratoRequest(
        int PropiedadId,
        int ClienteId,
        decimal MontoAlquiler,
        decimal MontoGarantia,
        DateTime DiaPago,
        DateTime FechaInicio,
        DateTime FechaFin);

    public record ActualizarContratoRequest(
        int PropiedadId,
        int ClienteId,
        decimal MontoAlquiler,
        decimal MontoGarantia,
        DateTime DiaPago,
        DateTime FechaInicio,
        DateTime FechaFin);
}