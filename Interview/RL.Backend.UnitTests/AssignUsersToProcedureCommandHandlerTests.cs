using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using RL.Backend.Commands;
using RL.Backend.Commands.Handlers;
using RL.Backend.Exceptions;
using RL.Data;

namespace RL.Backend.UnitTests;

[TestClass]
public class AssignUsersToProcedureCommandHandlerTests
{
    [TestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(int.MinValue)]
    public async Task AssignUsersToProcedureTests_InvalidPlanId_ReturnsBadRequest(int planId)
    {
        var context = new Mock<RLContext>();
        var sut = new AssignUsersToProcedureCommandHandler(context.Object);
        var request = new AssignUsersToProcedureCommand()
        {
            PlanId = planId,
            ProcedureId = 1,
            UserIds = new List<int> { 1 }
        };

        var result = await sut.Handle(request, new CancellationToken());

        result.Exception.Should().BeOfType(typeof(BadRequestException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(int.MinValue)]
    public async Task AssignUsersToProcedureTests_InvalidProcedureId_ReturnsBadRequest(int procedureId)
    {
        var context = new Mock<RLContext>();
        var sut = new AssignUsersToProcedureCommandHandler(context.Object);
        var request = new AssignUsersToProcedureCommand()
        {
            PlanId = 1,
            ProcedureId = procedureId,
            UserIds = new List<int> { 1 }
        };

        var result = await sut.Handle(request, new CancellationToken());

        result.Exception.Should().BeOfType(typeof(BadRequestException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(19)]
    [DataRow(35)]
    public async Task AssignUsersToProcedureTests_PlanNotFound_ReturnsNotFound(int planId)
    {
        var context = DbContextHelper.CreateContext();
        var sut = new AssignUsersToProcedureCommandHandler(context);
        var request = new AssignUsersToProcedureCommand()
        {
            PlanId = planId,
            ProcedureId = 1,
            UserIds = new List<int> { 1 }
        };

        var result = await sut.Handle(request, new CancellationToken());

        result.Exception.Should().BeOfType(typeof(NotFoundException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(19)]
    [DataRow(35)]
    public async Task AssignUsersToProcedureTests_ProcedureNotFound_ReturnsNotFound(int procedureId)
    {
        var context = DbContextHelper.CreateContext();
        var sut = new AssignUsersToProcedureCommandHandler(context);
        var request = new AssignUsersToProcedureCommand()
        {
            PlanId = 1,
            ProcedureId = procedureId,
            UserIds = new List<int> { 1 }
        };

        context.Plans.Add(new Data.DataModels.Plan { PlanId = 1 });
        await context.SaveChangesAsync();

        var result = await sut.Handle(request, new CancellationToken());

        result.Exception.Should().BeOfType(typeof(NotFoundException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(19)]
    [DataRow(35)]
    public async Task AssignUsersToProcedureTests_UserNotFound_ReturnsNotFound(int userId)
    {
        var context = DbContextHelper.CreateContext();
        var sut = new AssignUsersToProcedureCommandHandler(context);
        var request = new AssignUsersToProcedureCommand()
        {
            PlanId = 1,
            ProcedureId = 1,
            UserIds = new List<int> { userId }
        };

        context.Plans.Add(new Data.DataModels.Plan { PlanId = 1 });
        context.Procedures.Add(new Data.DataModels.Procedure { ProcedureId = 1 });
        await context.SaveChangesAsync();

        var result = await sut.Handle(request, new CancellationToken());

        result.Exception.Should().BeOfType(typeof(NotFoundException));
        result.Succeeded.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(19, 2)]
    [DataRow(35, 3)]
    public async Task AssignUsersToProcedureTests_ValidAssignment_ReturnsSuccess(int planId, int procedureId)
    {
        var context = DbContextHelper.CreateContext();
        var sut = new AssignUsersToProcedureCommandHandler(context);
        var userIds = new List<int> { 1, 2 };
        var request = new AssignUsersToProcedureCommand()
        {
            PlanId = planId,
            ProcedureId = procedureId,
            UserIds = userIds
        };

        context.Plans.Add(new Data.DataModels.Plan { PlanId = planId });
        context.Procedures.Add(new Data.DataModels.Procedure { ProcedureId = procedureId });
        context.Users.AddRange(
            new Data.DataModels.User { UserId = 1, Name = "User 1" },
            new Data.DataModels.User { UserId = 2, Name = "User 2" }
        );
        await context.SaveChangesAsync();

        var result = await sut.Handle(request, new CancellationToken());

        var assignments = await context.PlanProcedureUsers
            .Where(ppu => ppu.PlanId == planId && ppu.ProcedureId == procedureId)
            .ToListAsync();

        assignments.Should().HaveCount(2);
        assignments.Select(a => a.UserId).Should().BeEquivalentTo(userIds);
        result.Value.Should().BeOfType(typeof(Unit));
        result.Succeeded.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(19, 2)]
    [DataRow(35, 3)]
    public async Task AssignUsersToProcedureTests_ReplaceExistingAssignments_ReturnsSuccess(int planId, int procedureId)
    {
        var context = DbContextHelper.CreateContext();
        var sut = new AssignUsersToProcedureCommandHandler(context);
        var newUserIds = new List<int> { 3 };
        var request = new AssignUsersToProcedureCommand()
        {
            PlanId = planId,
            ProcedureId = procedureId,
            UserIds = newUserIds
        };

        context.Plans.Add(new Data.DataModels.Plan { PlanId = planId });
        context.Procedures.Add(new Data.DataModels.Procedure { ProcedureId = procedureId });
        context.Users.AddRange(
            new Data.DataModels.User { UserId = 1, Name = "User 1" },
            new Data.DataModels.User { UserId = 2, Name = "User 2" },
            new Data.DataModels.User { UserId = 3, Name = "User 3" }
        );
        context.PlanProcedureUsers.AddRange(
            new Data.DataModels.PlanProcedureUser { PlanId = planId, ProcedureId = procedureId, UserId = 1 },
            new Data.DataModels.PlanProcedureUser { PlanId = planId, ProcedureId = procedureId, UserId = 2 }
        );
        await context.SaveChangesAsync();

        var result = await sut.Handle(request, new CancellationToken());

        var assignments = await context.PlanProcedureUsers
            .Where(ppu => ppu.PlanId == planId && ppu.ProcedureId == procedureId)
            .ToListAsync();

        assignments.Should().HaveCount(1);
        assignments.Single().UserId.Should().Be(3);
        result.Value.Should().BeOfType(typeof(Unit));
        result.Succeeded.Should().BeTrue();
    }
}