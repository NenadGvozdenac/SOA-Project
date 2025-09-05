using MediatR;
using tours_service.src.Tours.API.DTOs;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.Application.Features.UpdateCheckpoint;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Features.CreateCheckpoint;

public class UpdateCheckpointHandler(ICrudRepository<Checkpoint> checkpointRepository) : IRequestHandler<UpdateCheckpointCommand, Result<UpdateCheckpointDTO>>
{
    public async Task<Result<UpdateCheckpointDTO>> Handle(UpdateCheckpointCommand request, CancellationToken cancellationToken)
    {
        if (request.UserDTO.Role != "Guide") // Samo autor (Guide) može da dodaje checkpoint
        {
            return Result<UpdateCheckpointDTO>.Failure("Only author can update checkpoints.");
        }
        var checkpoint = checkpointRepository.Get(request.CheckpointId);
        if (checkpoint == null)
        {
            return Result<UpdateCheckpointDTO>.Failure("Checkpoint not found.");
        }
        if (checkpoint.TourId != request.CreatedCheckpointDTO.TourId)
        {
            return Result<UpdateCheckpointDTO>.Failure("Checkpoint does not belong to the specified tour.");
        }
        checkpoint.Name = request.CreatedCheckpointDTO.Name;
        checkpoint.Description = request.CreatedCheckpointDTO.Description;
        checkpoint.Latitude = request.CreatedCheckpointDTO.Latitude;
        checkpoint.Longitude = request.CreatedCheckpointDTO.Longitude;
        checkpoint.ImageBase64 = request.CreatedCheckpointDTO.ImageBase64;

        checkpointRepository.Update(checkpoint);

        var resultDto = new UpdateCheckpointDTO
        {
            Id = checkpoint.Id,
            TourId = checkpoint.TourId,
            Name = checkpoint.Name,
            Description = checkpoint.Description,
            Latitude = checkpoint.Latitude,
            Longitude = checkpoint.Longitude,
            ImageBase64 = checkpoint.ImageBase64
        };

        return Result<UpdateCheckpointDTO>.Success(resultDto);
    }
}
