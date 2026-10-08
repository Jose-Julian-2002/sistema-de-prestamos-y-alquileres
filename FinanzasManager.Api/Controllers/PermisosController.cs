using FinanzasManager.Application.Common;
using FinanzasManager.Application.Features.Roles.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasManager.Api.Controllers
{
    // Los permisos son de solo lectura. El catalogo se define en Permisos.cs y el
    // seed lo siembra: permitir editarlos dejaria endpoints cuya policy ya no
    // puede satisfacerse por nadie.
    [Route("api/[controller]")]
    [ApiController]
    public class PermisosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PermisosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET api/Permisos
        [Authorize(Policy = Permisos.Roles.Ver)]
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] ListarPermisosQuery consulta)
        {
            var permisos = await _mediator.Send(consulta);
            return Ok(permisos);
        }
    }
}