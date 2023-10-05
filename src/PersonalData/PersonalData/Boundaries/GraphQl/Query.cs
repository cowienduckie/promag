using System.Security.Claims;
using System.Text;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PersonalData.Boundaries.GraphQl.Dtos;
using PersonalData.Boundaries.GraphQl.Filters;
using PersonalData.Boundaries.GraphQl.ObjectTypes;
using PersonalData.Common.Constants;
using PersonalData.Common.Enums;
using PersonalData.Services;
using PersonalData.UseCases.Queries;
using Promag.Protobuf.Identity.V1;
using Shared;
using Shared.Caching;
using Shared.CustomTypes;

namespace PersonalData.Boundaries.GraphQl;

public class Query
{
    [GraphQLName("Users")]
    [UseOffsetPaging(typeof(PersonType))]
    [UseFiltering(typeof(PersonFilterType))]
    [Authorize(AuthorizationPolicy.CAN_VIEW_USER)]
    public async Task<IQueryable<PersonDto>> GetUsers([Service] IMediator mediator)
    {
        return await mediator.Send(new GetPeopleQuery(UserType.User));
    }

    [GraphQLName("Person")]
    [GraphQLType(typeof(PersonType))]
    [Authorize(AuthorizationPolicy.CAN_VIEW_USER)]
    public async Task<PersonDto?> GetPersonById(
        Guid personId,
        [Service] ISender mediator,
        [Service] IDistributedCache distributedCache,
        [Service] IConfiguration configuration)
    {
        if (!configuration.GetOptions<CacheOptions>("Redis").Enabled)
        {
            return await mediator.Send(new GetPersonByIdQuery(personId));
        }

        var cacheKey = string.Format(PersonalDataCacheKeys.UserById, personId);

        PersonDto? personDto;

        var encodedUser = await distributedCache.GetAsync(cacheKey);

        if (encodedUser != null)
        {
            var serializedUser = Encoding.UTF8.GetString(encodedUser);

            personDto = JsonConvert.DeserializeObject<PersonDto>(serializedUser);
        }
        else
        {
            personDto = await mediator.Send(new GetPersonByIdQuery(personId));

            var serializedUser = JsonConvert.SerializeObject(personDto);
            encodedUser = Encoding.UTF8.GetBytes(serializedUser);

            await distributedCache.SetAsync(cacheKey, encodedUser, new DistributedCacheEntryOptions());
        }

        return personDto;
    }

    [GraphQLName("Me")]
    [GraphQLType(typeof(PersonType))]
    [Authorize(AuthorizationPolicy.CAN_VIEW_USER)]
    public async Task<PersonDto?> GetMyProfile(
        [Service] IHttpContextAccessor contextAccessor,
        [Service] ISender mediator)
    {
        var personIdFromContext = contextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

        if (personIdFromContext is null)
        {
            return null;
        }

        var personId = Guid.Parse(personIdFromContext);

        return await mediator.Send(new GetPersonByIdQuery(personId));
    }

    [GraphQLName("Roles")]
    [GraphQLType(typeof(ListType<RoleType>))]
    [Authorize(AuthorizationPolicy.CAN_VIEW_ROLE)]
    public async Task<List<RoleDto>> GetRoles(
        [Service] IHttpContextAccessor contextAccessor,
        [Service] IIdentityService identityService)
    {
        var roles = await identityService.FetchAllRoles();

        // Remove SUPER_USER_ROLE_NAME from roles if user is not a super user
        var identity = contextAccessor.HttpContext?.User.Identity as ClaimsIdentity;
        var roleClaim = identity?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);

        var superRole = roles.Find(x => x.Name == Roles.SUPER_USER_ROLE_NAME);

        if (roleClaim is not null &&
            superRole is not null &&
            !roleClaim.Value.Contains(Roles.SUPER_USER_ROLE_NAME))
        {
            roles.Remove(superRole);
        }

        return roles;
    }

    [GraphQLName("Role")]
    [GraphQLType(typeof(RoleType))]
    [Authorize(AuthorizationPolicy.CAN_VIEW_ROLE)]
    public async Task<RoleDto?> GetRoleById(
        Guid roleId,
        [Service] IIdentityService identityService)
    {
        var result = await identityService.FetchRoleById(roleId.ToString());

        return result.FirstOrDefault(x => x.RoleId == roleId.ToString());
    }

    [GraphQLName("Permissions")]
    [GraphQLType(typeof(ListType<StringType>))]
    [Authorize(AuthorizationPolicy.CAN_VIEW_ROLE)]
    public async Task<List<string>> GetRolePermissions(
        Guid roleId,
        [Service] ISender mediator)
    {
        return await mediator.Send(new GetRolePermissionsQuery(roleId)) as List<string> ?? new List<string>();
    }
}