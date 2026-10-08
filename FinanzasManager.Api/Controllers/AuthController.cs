using Microsoft.AspNetCore.Authorization;
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

        // Registro abierto: cualquier persona puede crear una cuenta (rol Cobrador por defecto)
        [AllowAnonymous]
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioCommand comando)
        {
            var usuarioId = await _mediator.Send(comando);
            return StatusCode(StatusCodes.Status201Created, new { id = usuarioId });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] IniciarSesionCommand comando)
        {
            var resultado = await _mediator.Send(comando);
            return Ok(resultado);
        }
    }
}
