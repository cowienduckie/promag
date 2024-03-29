using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portal.Data;

namespace Portal.UseCases.Mutations.Handlers;

public class UpdateTaskHandler : IRequestHandler<UpdateTaskCommand, bool>
{
    private readonly ILogger<UpdateTaskHandler> _logger;
    private readonly PortalContext _portalContext;

    public UpdateTaskHandler(ILogger<UpdateTaskHandler> logger, PortalContext portalContext)
    {
        _logger = logger;
        _portalContext = portalContext;
    }

    public async Task<bool> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{HandlerName} - Start", nameof(UpdateTaskHandler));

        var task = await _portalContext.Tasks.FirstOrDefaultAsync(t => t.Id == Guid.Parse(request.Id), cancellationToken);

        if (task == null)
        {
            _logger.LogInformation("{HandlerName} - Task not found", nameof(UpdateTaskHandler));

            return false;
        }

        task.Name = request.Name;
        task.Notes = request.Notes;
        task.Completed = request.Completed;
        task.StartOn = request.StartOn;
        task.DueOn = request.DueOn;
        task.AssigneeId = string.IsNullOrEmpty(request.AssigneeId) ? null : Guid.Parse(request.AssigneeId);

        _portalContext.Tasks.Update(task);
        await _portalContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{HandlerName} - Finish", nameof(UpdateTaskHandler));

        return true;
    }
}