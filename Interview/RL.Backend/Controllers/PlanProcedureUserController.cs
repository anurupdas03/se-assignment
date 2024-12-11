using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlanProcedureUserController : ControllerBase
    {
        private readonly RLContext _context;

        public PlanProcedureUserController(RLContext context)
        {
            _context = context;
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignUsers([FromBody] AssignUsersRequest request)
        {
            var existingAssignments = await _context.PlanProcedureUsers
                .Where(ppu => ppu.PlanId == request.PlanId &&
                             ppu.ProcedureId == request.ProcedureId)
                .ToListAsync();

            _context.PlanProcedureUsers.RemoveRange(existingAssignments);

            foreach (var userId in request.UserIds)
            {
                await _context.PlanProcedureUsers.AddAsync(new PlanProcedureUser
                {
                    PlanId = request.PlanId,
                    ProcedureId = request.ProcedureId,
                    UserId = userId
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
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