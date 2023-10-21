using MediatR;
using Microsoft.Extensions.Logging;
using Promag.Protobuf.Commons.V1;

namespace PersonalData.UseCases.Queries.Handlers;

public class PingHandler : IRequestHandler<PingQuery, PongReply>
{
    private readonly ILogger<PingHandler> _logger;

    public PingHandler(ILogger<PingHandler> logger)
    {
        _logger = logger;
    }

    public Task<PongReply> Handle(PingQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{HandlerName} - Pong!", nameof(PingHandler));

        return Task.FromResult(new PongReply
        {
            Message = "Personal Data Service sends pong"
        });
    }
}