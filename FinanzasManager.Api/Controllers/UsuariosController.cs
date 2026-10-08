using System.Security.Claims;
using FinanzasManager.Application.Common;
using FinanzasManager.Application.Features.Usuarios.Commands;
using FinanzasManager.Application.Features.Usuarios.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsuariosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Id del usuario del token. Lo pasamos a los comandos que necesitan
        // comparar contra el usuario objetivo para evitar autobloqueos.
        private int? SolicitanteId
        {
            get
            {
                var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(claim, out var id) ? id : null;
            }
        }

        // GET api/Usuarios
        [Authorize(Policy = Permisos.Usuarios.Ver)]
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] ListarUsuariosQuery consulta)
        {
            var resultado = await _mediator.Send(consulta);
            return Ok(resultado);
        }

        // GET api/Usuarios/5
        [Authorize(Policy = Permisos.Usuarios.Ver)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var usuario = await _mediator.Send(new ObtenerUsuarioQuery(id));
            return Ok(usuario);
        }

        // POST api/Usuarios
        [Authorize(Policy = Permisos.Usuarios.Crear)]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearUsuarioCommand comando)
        {
            var usuarioId = await _mediator.Send(comando);
            return CreatedAtAction(nameof(Obtener), new { id = usuarioId }, usuarioId);
        }

        // PUT api/Usuarios/5
        [Authorize(Policy = Permisos.Usuarios.Editar)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarUsuarioRequest cuerpo)
        {
            await _mediator.Send(new ActualizarUsuarioCommand(id, cuerpo.Nombre, cuerpo.Gmail));
            return NoContent();
        }

        // PUT api/Usuarios/5/rol
        [Authorize(Policy = Permisos.Usuarios.GestionarRoles)]
        [HttpPut("{id:int}/rol")]
        public async Task<IActionResult> CambiarRol(int id, [FromBody] CambiarRolRequest cuerpo)
        {
            await _mediator.Send(new CambiarRolUsuarioCommand(id, cuerpo.RolId, SolicitanteId));
            return NoContent();
        }

        // PUT api/Usuarios/5/estado
        [Authorize(Policy = Permisos.Usuarios.Editar)]
        [HttpPut("{id:int}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoRequest cuerpo)
        {
            await _mediator.Send(new CambiarEstadoUsuarioCommand(id, cuerpo.Estado, SolicitanteId));
            return NoContent();
        }

        // PUT api/Usuarios/5/password
        [Authorize(Policy = Permisos.Usuarios.Editar)]
        [HttpPut("{id:int}/password")]
        public async Task<IActionResult> CambiarPassword(int id, [FromBody] CambiarPasswordRequest cuerpo)
        {
            await _mediator.Send(new CambiarPasswordUsuarioCommand(id, cuerpo.PasswordActual, cuerpo.PasswordNueva));
            return NoContent();
        }

        // DELETE api/Usuarios/5  -> borrado logico, el usuario queda con Estado = false
        [Authorize(Policy = Permisos.Usuarios.Editar)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _mediator.Send(new EliminarUsuarioCommand(id, SolicitanteId));
            return NoContent();
        }
    }

    // Cuerpos separados de los commands: el id viaja en la ruta, no en el JSON.
    public record ActualizarUsuarioRequest(string Nombre, string Gmail);
    public record CambiarRolRequest(int RolId);
    public record CambiarEstadoRequest(bool Estado);
    public record CambiarPasswordRequest(string PasswordActual, string PasswordNueva);
}