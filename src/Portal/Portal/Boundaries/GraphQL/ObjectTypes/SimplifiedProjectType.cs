using HotChocolate.Types;
using Portal.Boundaries.GraphQL.Dtos.Projects;

namespace Portal.Boundaries.GraphQL.ObjectTypes;

public class SimplifiedProjectType : ObjectType<SimplifiedProjectDto>
{
    protected override void Configure(IObjectTypeDescriptor<SimplifiedProjectDto> descriptor)
    {
    }
}