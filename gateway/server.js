require('dotenv').config();
const express = require('express');
const { createProxyMiddleware } = require('http-proxy-middleware');
const cors = require('cors');
const helmet = require('helmet');
const rateLimit = require('express-rate-limit');
const bodyParser = require('body-parser');

// Import custom middleware
const { verifyToken, verifyApiKey, addRequestId } = require('./middleware/auth');
const { developmentFormat, productionFormat, logErrors } = require('./middleware/logging');
const { healthChecker, checkServiceHealth, getHealthStatus } = require('./middleware/health');

const app = express();
const PORT = 3000;

// Security middleware
app.use(helmet());

// Request ID middleware
app.use(addRequestId);

// Debug middleware
app.use((req, res, next) => {
  console.log(`[DEBUG] Incoming request: ${req.method} ${req.url}`);
  next();
});

// CORS configuration
const corsOptions = {
  origin:['http://localhost:5173'],
  credentials: true,
  optionsSuccessStatus: 200
};
app.use(cors(corsOptions));

// Rate limiting
const limiter = rateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 100, // limit each IP to 100 requests per windowMs
  message: {
    error: 'Too many requests from this IP, please try again later.',
    retryAfter: Math.ceil((15 * 60 * 1000) / 1000)
  }
});
app.use(limiter);

// Logging middleware
// const isProduction = process.env.NODE_ENV === 'production';
// app.use(isProduction ? productionFormat : developmentFormat);

// Body parsing middleware
app.use(bodyParser.json({ limit: '10mb' }));
app.use(bodyParser.urlencoded({ extended: true, limit: '10mb' }));

// Test endpoint to check if gateway is working
app.post('/test', (req, res) => {
  console.log(`[${new Date().toISOString()}] Test endpoint called with body:`, req.body);
  res.status(200).json({
    message: 'Test endpoint working',
    receivedBody: req.body,
    requestId: req.requestId,
    timestamp: new Date().toISOString()
  });
});

// Authentication middleware (optional - can be enabled per route)
// app.use(verifyToken);
// app.use(verifyApiKey);

// Health check endpoint (enhanced)
app.get('/health', getHealthStatus);

// Service discovery endpoint
app.get('/services', (req, res) => {
  res.status(200).json({
    services: {
      blogs: 'http://blogs_service:8081',
      followings: 'http://followings_service:9090',
      stakeholders: 'http://stakeholders_service:8080',
      tours: 'http://tours_service:8082'
    },
    healthStatus: healthChecker.getAllServiceStatus()
  });
});

// Gateway info endpoint
app.get('/info', (req, res) => {
  res.status(200).json({
    name: 'SOA Project API Gateway',
    version: '1.0.0',
    environment: 'development',
    uptime: process.uptime(),
    timestamp: new Date().toISOString(),
    requestId: req.requestId
  });
});

// Proxy configurations
const proxyOptions = {
  changeOrigin: true,
  logLevel:'info',
  timeout: 30000, // 30 second timeout
  proxyTimeout: 30000, // 30 second proxy timeout
  onError: (err, req, res) => {
    console.error(`[${new Date().toISOString()}] [${req.requestId}] Proxy Error:`, err.message);
    if (!res.headersSent) {
      res.status(500).json({
        error: 'Service temporarily unavailable',
        message: 'The requested service is currently unavailable. Please try again later.',
        timestamp: new Date().toISOString(),
        requestId: req.requestId
      });
    }
  },
  onProxyRes: (proxyRes, req, res) => {
    // Add request ID to response
    res.setHeader('X-Request-ID', req.requestId);
    console.log(`[${new Date().toISOString()}] [${req.requestId}] Response ${proxyRes.statusCode} from ${req.url}`);
  }
};

// Blogs Service Proxy (with health check)
app.use('/api/blogs', 
  checkServiceHealth('blogs'),
  createProxyMiddleware({
    target: 'http://blogs_service:8081',
    pathRewrite: {
      '^/api/blogs': '/api'
    },
    onProxyReq: (proxyReq, req, res) => {
      // Add request ID to proxied request
      proxyReq.setHeader('X-Request-ID', req.requestId);
      
      // Handle POST body data
      if (req.body && (req.method === 'POST' || req.method === 'PUT' || req.method === 'PATCH')) {
        const bodyData = JSON.stringify(req.body);
        proxyReq.setHeader('Content-Type', 'application/json');
        proxyReq.setHeader('Content-Length', Buffer.byteLength(bodyData));
        proxyReq.write(bodyData);
      }
      
      console.log(`[${new Date().toISOString()}] [${req.requestId}] Proxying ${req.method} ${req.url} to ${proxyReq.path}`);
    },
    ...proxyOptions
  })
);

// Followings Service Proxy (with health check)
app.use('/api/followings',
  checkServiceHealth('followings'),
  createProxyMiddleware({
    target: 'http://followings_service:9090',
    pathRewrite: {
      '^/api/followings': '/api'
    },
    onProxyReq: (proxyReq, req, res) => {
      // Add request ID to proxied request
      proxyReq.setHeader('X-Request-ID', req.requestId);
      
      // Handle POST body data
      if (req.body && (req.method === 'POST' || req.method === 'PUT' || req.method === 'PATCH')) {
        const bodyData = JSON.stringify(req.body);
        proxyReq.setHeader('Content-Type', 'application/json');
        proxyReq.setHeader('Content-Length', Buffer.byteLength(bodyData));
        proxyReq.write(bodyData);
      }
      
      console.log(`[${new Date().toISOString()}] [${req.requestId}] Proxying ${req.method} ${req.url} to ${proxyReq.path}`);
    },
    ...proxyOptions
  })
);

// Test endpoint for debugging
app.post('/test-post', (req, res) => {
  console.log('Test POST received:', req.body);
  res.json({ 
    received: req.body, 
    headers: req.headers,
    method: req.method,
    url: req.url
  });
});

// Stakeholders Service Proxy (without health check for debugging)
app.use('/api/stakeholders',
  (req, res, next) => {
    console.log(`[DEBUG] Stakeholders middleware called for ${req.method} ${req.url}`);
    next();
  },
  createProxyMiddleware({
    target: 'http://stakeholders_service:8080',
    pathRewrite: {
      '^/api/stakeholders': '/api'
    },
    changeOrigin: true,
    logLevel: 'debug',
    timeout: 30000,
    proxyTimeout: 30000,
    onError: (err, req, res) => {
      console.error(`[${new Date().toISOString()}] [${req.requestId}] Stakeholders Proxy Error:`, err.message);
      res.status(500).json({
        error: 'Stakeholders service temporarily unavailable',
        message: err.message,
        timestamp: new Date().toISOString(),
        requestId: req.requestId
      });
    },
    onProxyReq: (proxyReq, req, res) => {
      console.log(`[DEBUG] onProxyReq called for ${req.method} ${req.url}`);
      proxyReq.setHeader('X-Request-ID', req.requestId);
      
      if (req.body && (req.method === 'POST' || req.method === 'PUT' || req.method === 'PATCH')) {
        console.log(`[DEBUG] Sending body:`, req.body);
        const bodyData = JSON.stringify(req.body);
        proxyReq.setHeader('Content-Type', 'application/json');
        proxyReq.setHeader('Content-Length', Buffer.byteLength(bodyData));
        proxyReq.write(bodyData);
      }
      
      console.log(`[${new Date().toISOString()}] [${req.requestId}] Proxying ${req.method} ${req.url} to ${proxyReq.path}`);
    },
    onProxyRes: (proxyRes, req, res) => {
      console.log(`[DEBUG] onProxyRes called with status ${proxyRes.statusCode}`);
      res.setHeader('X-Request-ID', req.requestId);
      console.log(`[${new Date().toISOString()}] [${req.requestId}] Response ${proxyRes.statusCode} from ${req.url}`);
    }
  })
);

// Tours Service Proxy (with health check)
app.use('/api/tours',
  checkServiceHealth('tours'),
  createProxyMiddleware({
    target: 'http://tours_service:8082',
    pathRewrite: {
      '^/api/tours': '/api'
    },
    onProxyReq: (proxyReq, req, res) => {
      // Add request ID to proxied request
      proxyReq.setHeader('X-Request-ID', req.requestId);
      
      // Handle POST body data
      if (req.body && (req.method === 'POST' || req.method === 'PUT' || req.method === 'PATCH')) {
        const bodyData = JSON.stringify(req.body);
        proxyReq.setHeader('Content-Type', 'application/json');
        proxyReq.setHeader('Content-Length', Buffer.byteLength(bodyData));
        proxyReq.write(bodyData);
      }
      
      console.log(`[${new Date().toISOString()}] [${req.requestId}] Proxying ${req.method} ${req.url} to ${proxyReq.path}`);
    },
    ...proxyOptions
  })
);

// Catch-all for undefined routes
app.use('*', (req, res) => {
  res.status(404).json({
    error: 'Route not found',
    message: `The route ${req.originalUrl} does not exist`,
    availableRoutes: [
      '/health',
      '/services',
      '/info',
      '/api/blogs/*',
      '/api/followings/*',
      '/api/stakeholders/*',
      '/api/tours/*'
    ],
    timestamp: new Date().toISOString(),
    requestId: req.requestId
  });
});

// Error logging middleware
app.use(logErrors);

// Global error handler
app.use((err, req, res, next) => {
  console.error(`[${new Date().toISOString()}] [${req.requestId}] Global Error Handler:`, err);
  res.status(500).json({
    error: 'Internal Server Error',
    message: 'An unexpected error occurred',
    timestamp: new Date().toISOString(),
    requestId: req.requestId
  });
});

// Start server
const server = app.listen(PORT, () => {
  console.log(`
🚀 API Gateway is running!
📍 Port: ${PORT}
🌍 Environment: 'development'
📊 Health Check: http://localhost:${PORT}/health
🔗 Services Discovery: http://localhost:${PORT}/services

📋 Available Routes:
  • /api/blogs/*     → http://localhost:8081
  • /api/followings/* → http://localhost:8083
  • /api/stakeholders/* → http://stakeholders_service:8080
  • /api/tours/*     → http://localhost:8082
  `);
});

// Graceful shutdown
process.on('SIGTERM', () => {
  console.log('SIGTERM received, shutting down gracefully');
  server.close(() => {
    console.log('Process terminated');
  });
});

process.on('SIGINT', () => {
  console.log('SIGINT received, shutting down gracefully');
  server.close(() => {
    console.log('Process terminated');
  });
});

module.exports = app;
