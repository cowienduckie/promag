using MasterData.Boundaries.GraphQl.Dtos;
using MediatR;

namespace MasterData.UseCases.Queries;

public class GetActivityLogsQuery : IRequest<IQueryable<ActivityLogDto>>;