namespace followings_service.src.Followings.BuildingBlocks.Infrastructure.BlogsService;

public class ApiResponse<T>
{
    public string Message { get; set; } = string.Empty;
    public int Code { get; set; }
    public T Data { get; set; } = default!;
}
