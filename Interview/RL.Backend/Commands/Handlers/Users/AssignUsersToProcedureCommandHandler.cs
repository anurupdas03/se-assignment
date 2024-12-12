using MediatR;
using Microsoft.EntityFrameworkCore;
using RL.Backend.Exceptions;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Commands.Handlers
{
    public class AssignUsersToProcedureCommandHandler : IRequestHandler<AssignUsersToProcedureCommand, ApiResponse<Unit>>
    {
        private readonly RLContext _context;

        public AssignUsersToProcedureCommandHandler(RLContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<Unit>> Handle(AssignUsersToProcedureCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.PlanId < 1)
                    return ApiResponse<Unit>.Fail(new BadRequestException("Invalid PlanId"));
                if (request.ProcedureId < 1)
                    return ApiResponse<Unit>.Fail(new BadRequestException("Invalid ProcedureId"));
                if (request.UserIds == null)
                    return ApiResponse<Unit>.Fail(new BadRequestException("UserIds cannot be null"));

                var plan = await _context.Plans.FirstOrDefaultAsync(p => p.PlanId == request.PlanId);
                if (plan is null)
                    return ApiResponse<Unit>.Fail(new NotFoundException($"PlanId: {request.PlanId} not found"));

                var procedure = await _context.Procedures.FirstOrDefaultAsync(p => p.ProcedureId == request.ProcedureId);
                if (procedure is null)
                    return ApiResponse<Unit>.Fail(new NotFoundException($"ProcedureId: {request.ProcedureId} not found"));

                foreach (var userId in request.UserIds)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                    if (user is null)
                        return ApiResponse<Unit>.Fail(new NotFoundException($"UserId: {userId} not found"));
                }

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

                return ApiResponse<Unit>.Succeed(new Unit());
            }
            catch (Exception e)
            {
                return ApiResponse<Unit>.Fail(e);
            }
        }
    }
}