/**
 * Gateway configuration
 */

module.exports = {
  server: {
    port: process.env.PORT || 3000,
    environment: process.env.NODE_ENV || 'development'
  },
  
  services: {
    blogs: {
      url: process.env.BLOGS_SERVICE_URL || 'http://localhost:8081',
      timeout: 10000,
      retries: 3
    },
    followings: {
      url: process.env.FOLLOWINGS_SERVICE_URL || 'http://localhost:8082',
      timeout: 10000,
      retries: 3
    },
    stakeholders: {
      url: process.env.STAKEHOLDERS_SERVICE_URL || 'http://localhost:8083',
      timeout: 10000,
      retries: 3
    },
    tours: {
      url: process.env.TOURS_SERVICE_URL || 'http://localhost:8084',
      timeout: 10000,
      retries: 3
    }
  },

  security: {
    cors: {
      origins: process.env.ALLOWED_ORIGINS?.split(',') || ['http://localhost:5173'],
      credentials: true
    },
    rateLimit: {
      windowMs: parseInt(process.env.RATE_LIMIT_WINDOW_MS) || 15 * 60 * 1000,
      maxRequests: parseInt(process.env.RATE_LIMIT_MAX_REQUESTS) || 100
    },
    jwt: {
      secret: process.env.JWT_SECRET || 'your-secret-key',
      expiresIn: process.env.JWT_EXPIRES_IN || '24h'
    },
    apiKeys: process.env.VALID_API_KEYS?.split(',') || []
  },

  monitoring: {
    healthCheck: {
      interval: 30000, // 30 seconds
      timeout: 5000 // 5 seconds
    },
    logging: {
      level: process.env.LOG_LEVEL || 'info',
      format: process.env.NODE_ENV === 'production' ? 'json' : 'dev'
    }
  }
};
