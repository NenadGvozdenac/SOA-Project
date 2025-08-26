const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');
const path = require('path');

// Load the proto file
const PROTO_PATH = path.join(__dirname, '../../proto/tours.proto');

const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
  keepCase: true,
  longs: String,
  enums: String,
  defaults: true,
  oneofs: true
});

const toursProto = grpc.loadPackageDefinition(packageDefinition).tours;

class ToursGrpcClient {
  constructor() {
    // Create gRPC client
    this.client = new toursProto.ToursService(
      'tours_service:8082', // gRPC server address
      grpc.credentials.createInsecure()
    );
  }

  // Get all tours via gRPC
  async getAllTours(userId, authToken) {
    return new Promise((resolve, reject) => {
      const request = {
        user_id: userId,
        auth_token: authToken
      };

      this.client.GetAllTours(request, (error, response) => {
        if (error) {
          console.error('gRPC GetAllTours error:', error);
          reject(error);
        } else {
          resolve(response);
        }
      });
    });
  }

  // Create tour via gRPC
  async createTour(userId, authToken, tourData) {
    return new Promise((resolve, reject) => {
      const request = {
        user_id: userId,
        auth_token: authToken,
        tour_data: {
          name: tourData.name,
          description: tourData.description,
          difficulty: tourData.difficulty,
          tags: tourData.tags || []
        }
      };

      this.client.CreateTour(request, (error, response) => {
        if (error) {
          console.error('gRPC CreateTour error:', error);
          reject(error);
        } else {
          resolve(response);
        }
      });
    });
  }
}

module.exports = ToursGrpcClient;
