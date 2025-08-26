using Grpc.Core;
using Grpc.Net.Client;
using GatewayNet.Grpc;

namespace GatewayNet.Controllers
{
    public class ToursProtoController : ToursService.ToursServiceBase
    {
        private readonly ILogger<ToursProtoController> _logger;
        private readonly IConfiguration _configuration;

        public ToursProtoController(ILogger<ToursProtoController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public override async Task<GetAllToursResponse> GetAllTours(GetAllToursRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Forwarding GetAllTours request to tours service");

                // Get the tours service URL from configuration
                var toursServiceUrl = _configuration["TOURS_SERVICE_URL"] ?? "http://tours_service:8083";
                
                // Create gRPC channel (with insecure connection for development)
                var httpHandler = new HttpClientHandler();
                httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                
                using var channel = GrpcChannel.ForAddress(toursServiceUrl, new GrpcChannelOptions 
                { 
                    HttpHandler = httpHandler 
                });

                var client = new ToursService.ToursServiceClient(channel);
                var response = await client.GetAllToursAsync(request);

                _logger.LogInformation($"Received {response.Tours.Count} tours from service");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding GetAllTours request");
                return new GetAllToursResponse
                {
                    Success = false,
                    Message = $"Gateway error: {ex.Message}"
                };
            }
        }

        public override async Task<CreateTourResponse> CreateTour(CreateTourRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation("Forwarding CreateTour request to tours service");

                // Get the tours service URL from configuration
                var toursServiceUrl = _configuration["TOURS_SERVICE_URL"] ?? "http://tours_service:8083";
                
                // Create gRPC channel (with insecure connection for development)
                var httpHandler = new HttpClientHandler();
                httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                
                using var channel = GrpcChannel.ForAddress(toursServiceUrl, new GrpcChannelOptions 
                { 
                    HttpHandler = httpHandler 
                });

                var client = new ToursService.ToursServiceClient(channel);
                var response = await client.CreateTourAsync(request);

                _logger.LogInformation($"Tour creation result: {response.Success}");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forwarding CreateTour request");
                return new CreateTourResponse
                {
                    Success = false,
                    Message = $"Gateway error: {ex.Message}"
                };
            }
        }
    }
}
