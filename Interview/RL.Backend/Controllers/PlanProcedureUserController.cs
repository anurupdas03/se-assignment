using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Commands;
using RL.Backend.Models;
using RL.Data;

namespace RL.Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlanProcedureUserController : ControllerBase
    {
        private readonly RLContext _context;
        private readonly IMediator _mediator;

        public PlanProcedureUserController(RLContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignUsers([FromBody] AssignUsersRequest request)
        {
            var command = new AssignUsersToProcedureCommand
            {
                PlanId = request.PlanId,
                ProcedureId = request.ProcedureId,
                UserIds = request.UserIds
            };

            var response = await _mediator.Send(command);
            return response.ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignedUsers([FromQuery] int planId, [FromQuery] int procedureId)
        {
            var users = await _context.PlanProcedureUsers
                .Include(ppu => ppu.User)
                .Where(ppu => ppu.PlanId == planId &&
                             ppu.ProcedureId == procedureId)
                .Select(ppu => ppu.User)
                .ToListAsync();

            return Ok(users);
        }
    }

    public class AssignUsersRequest
    {
        public int PlanId { get; set; }
        public int ProcedureId { get; set; }
        public List<int> UserIds { get; set; }
    }
}