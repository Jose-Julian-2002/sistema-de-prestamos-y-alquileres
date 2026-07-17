using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using FinanzasManager.Application.Features.Autenticacion.Commands;

namespace FinanzasManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] IniciarSesionCommand comando)
        {
            try
            {
                var resultado = await _mediator.Send(comando);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                // Si las credenciales fallan, devolvemos un 401 Unauthorized de forma limpia
                return Unauthorized(new { mensaje = ex.Message });
            }
        }
    }
}
