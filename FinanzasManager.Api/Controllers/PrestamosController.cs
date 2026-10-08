using FinanzasManager.Application.Common;
using FinanzasManager.Application.Features.Prestamos.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrestamosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PrestamosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST api/<PrestamosController>
        [Authorize(Policy = Permisos.Prestamos.Crear)]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearPrestamoCommand comando)
        {
            if (comando == null)
            {
                return BadRequest("Los datos del préstamo son requeridos.");
            }

            // Enviamos el comando a través del bus de MediatR hacia la capa de Aplicación
            int prestamoId = await _mediator.Send(comando);

            // Retornamos un estado 201 Created con el ID del nuevo préstamo
            return CreatedAtAction(nameof(Crear), new { id = prestamoId }, prestamoId);
        }
    }
}