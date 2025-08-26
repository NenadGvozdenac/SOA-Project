const express = require('express');
const ToursGrpcClient = require('../grpc/tours-client');

const router = express.Router();
const toursClient = new ToursGrpcClient();

// Helper function to extract user info from request
function getUserInfo(req) {
  // Extract user ID and auth token from headers or JWT
  const authHeader = req.headers.authorization;
  const userId = req.headers['x-user-id'] || req.user?.id || '1'; // fallback for testing
  
  return {
    userId: userId.toString(),
    authToken: authHeader || ''
  };
}

// RPC route for getting all tours
router.get('/tours/rpc', async (req, res) => {
  try {
    console.log(`[${new Date().toISOString()}] [${req.requestId}] RPC GetAllTours called`);
    
    const { userId, authToken } = getUserInfo(req);
    
    const grpcResponse = await toursClient.getAllTours(userId, authToken);
    
    res.status(200).json({
      success: grpcResponse.success,
      message: grpcResponse.message,
      data: grpcResponse.tours,
      protocol: 'gRPC',
      timestamp: new Date().toISOString(),
      requestId: req.requestId
    });
  } catch (error) {
    console.error(`[${new Date().toISOString()}] [${req.requestId}] RPC GetAllTours error:`, error);
    res.status(500).json({
      success: false,
      message: error.message || 'Internal server error',
      protocol: 'gRPC',
      timestamp: new Date().toISOString(),
      requestId: req.requestId
    });
  }
});

// RPC route for creating a tour
router.post('/tours/rpc', async (req, res) => {
  try {
    console.log(`[${new Date().toISOString()}] [${req.requestId}] RPC CreateTour called`);
    
    const { userId, authToken } = getUserInfo(req);
    const tourData = req.body;
    
    const grpcResponse = await toursClient.createTour(userId, authToken, tourData);
    
    res.status(grpcResponse.success ? 201 : 400).json({
      success: grpcResponse.success,
      message: grpcResponse.message,
      data: grpcResponse.tour,
      protocol: 'gRPC',
      timestamp: new Date().toISOString(),
      requestId: req.requestId
    });
  } catch (error) {
    console.error(`[${new Date().toISOString()}] [${req.requestId}] RPC CreateTour error:`, error);
    res.status(500).json({
      success: false,
      message: error.message || 'Internal server error',
      protocol: 'gRPC',
      timestamp: new Date().toISOString(),
      requestId: req.requestId
    });
  }
});

module.exports = router;
