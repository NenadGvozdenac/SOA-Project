using MediatR;
using tours_service.src.Tours.API.DTOs;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Features.CreateCheckpoint;

public class CreateCheckpointHandler(ICrudRepository<Checkpoint> checkpointRepository) : IRequestHandler<CreateCheckpointCommand, Result<CreateCheckpointDTO>>
{
    public async Task<Result<CreateCheckpointDTO>> Handle(CreateCheckpointCommand request, CancellationToken cancellationToken)
    {
        if (request.UserDTO.Role != "Guide") // Samo autor (Guide) može da dodaje checkpoint
        {
            return Result<CreateCheckpointDTO>.Failure("Only author can add checkpoints.");
        }
        var checkpoint = new Checkpoint(
            request.Dto.TourId,
            request.Dto.Name,
            request.Dto.Description,
            request.Dto.Latitude,
            request.Dto.Longitude,
            request.Dto.ImageBase64
        );

        checkpointRepository.Create(checkpoint);
        
        var resultDto = new CreateCheckpointDTO
        {
            Id = checkpoint.Id,
            TourId = checkpoint.TourId,
            Name = checkpoint.Name,
            Description = checkpoint.Description,
            Latitude = checkpoint.Latitude,
            Longitude = checkpoint.Longitude,
            ImageBase64 = checkpoint.ImageBase64,
            CreatedAt = checkpoint.CreatedAt
        };

        return Result<CreateCheckpointDTO>.Success(resultDto);
    }
}
