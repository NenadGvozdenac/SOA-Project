using Grpc.Core;
using MediatR;
using tours_service.src.Tours.Application.Features.GetAllTours;
using tours_service.src.Tours.Application.Features.CreateTour;
using tours_service.src.Tours.Application.Features.DeleteCheckpoint;
using tours_service.src.Tours.Application.Features.ArchiveTour;
using tours_service.src.Tours.API.DTOs;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Infrastructure;
using tours_service.src.Tours.Application.Domain;
using Tours;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace tours_service.src.Tours.API.Grpc;

public class ToursGrpcService : ToursService.ToursServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ToursGrpcService> _logger;

    public ToursGrpcService(IMediator mediator, ILogger<ToursGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<GetAllToursResponse> GetAllTours(GetAllToursRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Auth token received: {Token}", string.IsNullOrEmpty(request.AuthToken) ? "EMPTY" : $"[{request.AuthToken.Length} chars]");

            // Extract user from JWT token using the same logic as ControllerBase extensions
            var user = GetUserFromToken(request.AuthToken);
            
            _logger.LogInformation("Extracted user: ID={UserId}, Role={Role}", user.Id, user.Role);
            
            var query = new GetAllToursQuery(user);
            var result = await _mediator.Send(query);
            
            _logger.LogInformation("gRPC GetAllTours query executed. Success: {Success}, Count: {Count}", 
                result.IsSuccess, result.IsSuccess ? result.Value?.Count() ?? 0 : 0);
            
            var response = new GetAllToursResponse
            {
                Success = result.IsSuccess,
                Message = result.IsSuccess ? "Tours retrieved successfully" : result.Error
            };

            if (result.IsSuccess && result.Value != null)
            {
                foreach (var tour in result.Value)
                {
                    response.Tours.Add(MapTourToGrpcTour(tour));
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in gRPC GetAllTours: {Message}", ex.Message);
            return new GetAllToursResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public override async Task<CreateTourResponse> CreateTour(CreateTourRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("gRPC CreateTour called for user: {UserId}", request.UserId);

            // Extract user from JWT token using the same logic as ControllerBase extensions
            var user = GetUserFromToken(request.AuthToken);

            // Map gRPC request to DTO
            var createdTourDTO = new CreatedTourDTO
            {
                Name = request.TourData.Name,
                Description = request.TourData.Description,
                Difficulty = request.TourData.Difficulty.ToString(), // Convert int to string
                Tags = request.TourData.Tags.ToList()
            };

            var command = new CreateTourCommand(user, createdTourDTO);
            var result = await _mediator.Send(command);

            _logger.LogInformation("gRPC CreateTour command executed. Success: {Success}, TourId: {TourId}", 
                result.IsSuccess, result.IsSuccess ? result.Value?.Id ?? 0 : 0);

            var response = new CreateTourResponse
            {
                Success = result.IsSuccess,
                Message = result.IsSuccess ? "Tour created successfully" : result.Error
            };

            if (result.IsSuccess && result.Value != null)
            {
                response.Tour = MapCreateTourToGrpcTour(result.Value);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in gRPC CreateTour");
            return new CreateTourResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public override async Task<DeleteCheckpointResponse> DeleteCheckpoint(DeleteCheckpointRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("gRPC DeleteCheckpoint called for checkpoint: {CheckpointId}", request.CheckpointId);

            // Extract user from JWT token
            var user = GetUserFromToken(request.AuthToken);

            var command = new DeleteCheckpointCommand(user, request.CheckpointId);
            var result = await _mediator.Send(command);

            _logger.LogInformation("gRPC DeleteCheckpoint command executed. Success: {Success}", result.IsSuccess);

            var response = new DeleteCheckpointResponse
            {
                Success = result.IsSuccess,
                Message = result.IsSuccess ? "Checkpoint deleted successfully" : result.Error
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in gRPC DeleteCheckpoint");
            return new DeleteCheckpointResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public override async Task<ArchiveTourResponse> ArchiveTour(ArchiveTourRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("gRPC ArchiveTour called for tour: {TourId}", request.TourId);

            // Extract user from JWT token
            var user = GetUserFromToken(request.AuthToken);

            var command = new ArchiveTourCommand(user, request.TourId);
            var result = await _mediator.Send(command);

            _logger.LogInformation("gRPC ArchiveTour command executed. Success: {Success}", result.IsSuccess);

            var response = new ArchiveTourResponse
            {
                Success = result.IsSuccess,
                Message = result.IsSuccess ? "Tour archived successfully" : result.Error
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in gRPC ArchiveTour");
            return new ArchiveTourResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    private global::Tours.Tour MapTourToGrpcTour(tours_service.src.Tours.Application.Features.GetAllTours.GetAllToursDTO tourDto)
    {
        var grpcTour = new global::Tours.Tour
        {
            Id = tourDto.Id,
            Name = tourDto.Name ?? "",
            Description = tourDto.Description ?? "",
            Price = (double)tourDto.Price,
            Difficulty = (int)tourDto.Difficulty,
            Status = tourDto.Status.ToString(),
            AuthorId = tourDto.AuthorId,
            CreatedAt = tourDto.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        // Add tags safely
        if (tourDto.Tags != null)
        {
            foreach (var tag in tourDto.Tags)
            {
                grpcTour.Tags.Add(tag ?? "");
            }
        }

        return grpcTour;
    }

    private global::Tours.Tour MapCreateTourToGrpcTour(tours_service.src.Tours.Application.Features.CreateTour.CreateTourDTO tourDto)
    {
        var grpcTour = new global::Tours.Tour
        {
            Id = tourDto.Id,
            Name = tourDto.Name ?? "",
            Description = tourDto.Description ?? "",
            Price = (double)tourDto.Price,
            Difficulty = (int)tourDto.Difficulty,
            Status = tourDto.Status.ToString(),
            AuthorId = tourDto.AuthorId,
            CreatedAt = tourDto.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        // Add tags safely
        if (tourDto.Tags != null)
        {
            foreach (var tag in tourDto.Tags)
            {
                grpcTour.Tags.Add(tag ?? "");
            }
        }

        return grpcTour;
    }

    /// <summary>
    /// Extract UserDTO from JWT token using the same logic as ControllerBaseExtensions.GetUser()
    /// </summary>
    private UserDTO GetUserFromToken(string authToken)
    {
        try
        {
            _logger.LogInformation("Starting JWT token extraction...");
            
            if (string.IsNullOrEmpty(authToken))
            {
                _logger.LogWarning("Empty auth token provided");
                throw new UnauthorizedAccessException("Auth token is required");
            }

            _logger.LogInformation("Token length: {Length}", authToken.Length);

            // Remove "Bearer " prefix if present
            if (authToken.StartsWith("Bearer "))
            {
                authToken = authToken.Substring(7);
                _logger.LogInformation("Removed Bearer prefix, new length: {Length}", authToken.Length);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            
            if (!tokenHandler.CanReadToken(authToken))
            {
                _logger.LogError("Cannot read JWT token format");
                throw new UnauthorizedAccessException("Invalid token format");
            }
            
            var jsonToken = tokenHandler.ReadJwtToken(authToken);
            _logger.LogInformation("JWT token parsed successfully, claims count: {Count}", jsonToken.Claims.Count());

            // Create ClaimsPrincipal from JWT token
            var claims = jsonToken.Claims.ToList();
            var identity = new ClaimsIdentity(claims, "jwt");
            var principal = new ClaimsPrincipal(identity);

            _logger.LogInformation("Available claims: {Claims}", string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}")));

            // Use the same extension methods as ControllerBaseExtensions.GetUser()
            var userId = principal.UserId();
            var email = principal.UserEmail();
            var role = principal.UserRole();
            var username = principal.UserName();

            _logger.LogInformation("Extracted user from JWT: ID={UserId}, Role={Role}, Email={Email}, Username={Username}", userId, role, email, username);

            return new UserDTO(userId, email, role, username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting user from JWT token: {Token}", authToken?.Substring(0, Math.Min(20, authToken?.Length ?? 0)));
            throw new UnauthorizedAccessException("Invalid or expired token");
        }
    }
}
