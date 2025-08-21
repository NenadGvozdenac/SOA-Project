using MediatR;
using tours_service.src.Tours.Application.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.Domain;
using tours_service.src.Tours.BuildingBlocks.Core.UseCases;

namespace tours_service.src.Tours.Application.Features.DeleteCheckpoint;

public class DeleteCheckpointHandler(ICrudRepository<Checkpoint> checkpointRepository) : IRequestHandler<DeleteCheckpointCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteCheckpointCommand request, CancellationToken cancellationToken)
    {
        if (request.UserDTO.Role != "Guide")
        {
            return Result<bool>.Failure("Only author can delete checkpoints.");
        }

        var checkpoint = checkpointRepository.Get(request.CheckpointId);
        if (checkpoint == null)
        {
            return Result<bool>.Failure("Checkpoint not found.");
        }

        checkpointRepository.Delete(request.CheckpointId);

        return Result<bool>.Success(true);

    }
}