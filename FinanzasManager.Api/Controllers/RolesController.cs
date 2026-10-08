using FinanzasManager.Application.Common;
using FinanzasManager.Application.Features.Roles.Commands;
using FinanzasManager.Application.Features.Roles.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET api/Roles
        [Authorize(Policy = Permisos.Roles.Ver)]
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] ListarRolesQuery consulta)
        {
            var roles = await _mediator.Send(consulta);
            return Ok(roles);
        }

        // GET api/Roles/2
        [Authorize(Policy = Permisos.Roles.Ver)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var rol = await _mediator.Send(new ObtenerRolQuery(id));
            return Ok(rol);
        }

        // GET api/Roles/2/permisos -> solo los ids asignados, para los checkboxes
        [Authorize(Policy = Permisos.Roles.Ver)]
        [HttpGet("{id:int}/permisos")]
        public async Task<IActionResult> ObtenerPermisos(int id)
        {
            var permisos = await _mediator.Send(new ObtenerPermisosDeRolQuery(id));
            return Ok(permisos);
        }

        // POST api/Roles
        [Authorize(Policy = Permisos.Roles.Crear)]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearRolCommand comando)
        {
            var rolId = await _mediator.Send(comando);
            return CreatedAtAction(nameof(Obtener), new { id = rolId }, rolId);
        }

        // PUT api/Roles/2
        [Authorize(Policy = Permisos.Roles.Editar)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarRolCommand cuerpo)
        {
            await _mediator.Send(cuerpo with { Id = id });
            return NoContent();
        }

        // PUT api/Roles/2/permisos -> reemplaza el conjunto completo
        [Authorize(Policy = Permisos.Roles.AsignarPermisos)]
        [HttpPut("{id:int}/permisos")]
        public async Task<IActionResult> AsignarPermisos(int id, [FromBody] AsignarPermisosRequest cuerpo)
        {
            await _mediator.Send(new AsignarPermisosRolCommand(id, cuerpo.PermisoIds));
            return NoContent();
        }

        // DELETE api/Roles/2/permisos/5
        [Authorize(Policy = Permisos.Roles.AsignarPermisos)]
        [HttpDelete("{id:int}/permisos/{permisoId:int}")]
        public async Task<IActionResult> QuitarPermiso(int id, int permisoId)
        {
            await _mediator.Send(new QuitarPermisoRolCommand(id, permisoId));
            return NoContent();
        }

        // DELETE api/Roles/2 -> no se permite si es base o tiene usuarios
        [Authorize(Policy = Permisos.Roles.Eliminar)]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _mediator.Send(new EliminarRolCommand(id));
            return NoContent();
        }
    }

    public record AsignarPermisosRequest(IReadOnlyList<int> PermisoIds);
}