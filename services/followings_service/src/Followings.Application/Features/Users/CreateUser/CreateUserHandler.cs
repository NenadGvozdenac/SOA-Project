using followings_service.src.Followings.BuildingBlocks.Core.Domain;
using followings_service.src.Followings.BuildingBlocks.Infrastructure.Database;
using MediatR;
using Neo4j.Driver;

namespace followings_service.src.Followings.Application.Features.Users.CreateUser;

public class CreateUserHandler(IGraphDatabaseContext context) : IRequestHandler<CreateUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var query = @"
            MERGE (u:User {id: $userId})
            RETURN u.id as id
        ";

        var parameters = new Dictionary<string, object>
        {
            { "userId", request.UserId }
        };

        try
        {
            var resultCursor = await context.RunAsync(query, parameters);
            var result = await resultCursor.ToListAsync(cancellationToken);

            if (result.Any())
            {
                return Result<string>.Success(request.UserId);
            }

            return Result<string>.Failure("Failed to create user");
        }
        catch (Exception e)
        {
            return Result<string>.Failure(e.Message);
        }
    }
}
