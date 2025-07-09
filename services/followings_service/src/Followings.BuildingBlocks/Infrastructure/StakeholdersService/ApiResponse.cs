namespace followings_service.src.Followings.BuildingBlocks.Infrastructure.StakeholdersService;

public class ApiResponse<T>
{
    public string Message { get; set; } = string.Empty;
    public int Code { get; set; }
    public T Data { get; set; } = default!;
}
