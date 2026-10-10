using System.Security.Claims;
using FinanzasManager.Application.Common;
using FinanzasManager.Application.Features.Pagos.Commands;
using FinanzasManager.Application.Features.Pagos.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PagosController(IMediator mediator) => _mediator = mediator;

        // Id del usuario del token. Se guarda como UsuarioId del pago:
        // registra quien cobro el recibo, no lo elige el cliente.
        private int? SolicitanteId
        {
            get
            {
                var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(claim, out var id) ? id : null;
            }
        }

        // GET api/Pagos
        [Authorize(Policy = Permisos.Pagos.Ver)]
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] ListarPagosQuery consulta)
        {
            var resultado = await _mediator.Send(consulta);
            return Ok(resultado);
        }

        // GET api/Pagos/5
        [Authorize(Policy = Permisos.Pagos.Ver)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var pago = await _mediator.Send(new ObtenerPagoQuery(id));
            return Ok(pago);
        }

        // POST api/Pagos
        [Authorize(Policy = Permisos.Pagos.Crear)]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearPagoRequest cuerpo)
        {
            if (!SolicitanteId.HasValue)
                return Unauthorized("No se pudo identificar al usuario del token.");

            var pagoId = await _mediator.Send(new CrearPagoCommand(
                cuerpo.Fecha,
                cuerpo.MetodoPagoId,
                SolicitanteId.Value,
                cuerpo.Detalles));

            return CreatedAtAction(nameof(Obtener), new { id = pagoId }, pagoId);
        }

        // DELETE api/Pagos/5  -> borrado fisico, se rechaza si no existe
        // No existe PUT: un recibo registrado no se edita, se anula y se rehace.
        [Authorize(Policy = Permisos.Pagos.Eliminar)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _mediator.Send(new EliminarPagoCommand(id));
            return NoContent();
        }
    }

    // Cuerpo separado del command: UsuarioId lo pone el servidor desde el token.
    public record CrearPagoRequest(
        DateTime Fecha,
        int MetodoPagoId,
        IReadOnlyList<DetallePagoRequest> Detalles);
}