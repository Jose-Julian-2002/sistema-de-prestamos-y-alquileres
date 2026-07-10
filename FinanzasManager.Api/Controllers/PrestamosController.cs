using Microsoft.AspNetCore.Mvc;
using MediatR;
using FinanzasManager.Application.Features.Prestamos.Commands;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinanzasManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrestamosController : ControllerBase
    {
        private readonly IMediator _mediator;

        // Inyectamos MediatR. Gracias a esto, el controlador no sabe qué base de datos existe
        // ni qué reglas se ejecutan; solo pasa el mensaje.
        public PrestamosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST api/<PrestamosController>
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearPrestamoCommand comando)
        {
            if (comando == null)
            {
                return BadRequest("Los datos del préstamo son requeridos.");
            }

            // Enviamos el comando a través del bus de MediatR hacia la capa de Aplicación
            Guid prestamoId = await _mediator.Send(comando);

            // Retornamos un estado 201 Created con el ID del nuevo préstamo
            return CreatedAtAction(nameof(Crear), new { id = prestamoId }, prestamoId);
        }

        // PUT api/<PrestamosController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PrestamosController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
