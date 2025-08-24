/**
 * Service health checking middleware
 */

const http = require('http');
const https = require('https');

class HealthChecker {
  constructor() {
    this.serviceStatus = new Map();
    this.checkInterval = 30000; // 30 seconds
    this.timeout = 5000; // 5 seconds
    
    this.services = {
      blogs: process.env.BLOGS_SERVICE_URL,
      followings: process.env.FOLLOWINGS_SERVICE_URL,
      stakeholders: process.env.STAKEHOLDERS_SERVICE_URL,
      tours: process.env.TOURS_SERVICE_URL
    };
    
    // Initialize health checking
    this.startHealthChecking();
  }

  async checkServiceHealth(serviceName, serviceUrl) {
    return new Promise((resolve) => {
      const url = new URL(`${serviceUrl}/api/health`);
      const client = url.protocol === 'https:' ? https : http;
      
      const req = client.request({
        hostname: url.hostname,
        port: url.port,
        path: url.pathname,
        method: 'GET',
        timeout: this.timeout
      }, (res) => {
        resolve({
          name: serviceName,
          status: res.statusCode === 200 ? 'healthy' : 'unhealthy',
          statusCode: res.statusCode,
          lastChecked: new Date().toISOString()
        });
      });

      req.on('error', () => {
        resolve({
          name: serviceName,
          status: 'unhealthy',
          statusCode: null,
          lastChecked: new Date().toISOString(),
          error: 'Connection failed'
        });
      });

      req.on('timeout', () => {
        req.destroy();
        resolve({
          name: serviceName,
          status: 'unhealthy',
          statusCode: null,
          lastChecked: new Date().toISOString(),
          error: 'Timeout'
        });
      });

      req.end();
    });
  }

  async checkAllServices() {
    const promises = Object.entries(this.services).map(([name, url]) => {
      if (url) {
        return this.checkServiceHealth(name, url);
      }
      return Promise.resolve({
        name,
        status: 'not_configured',
        lastChecked: new Date().toISOString()
      });
    });

    const results = await Promise.all(promises);
    
    results.forEach(result => {
      this.serviceStatus.set(result.name, result);
    });

    return results;
  }

  startHealthChecking() {
    // Initial check
    this.checkAllServices();
    
    // Periodic checks
    setInterval(() => {
      this.checkAllServices();
    }, this.checkInterval);
  }

  getServiceStatus(serviceName) {
    return this.serviceStatus.get(serviceName);
  }

  getAllServiceStatus() {
    const status = {};
    this.serviceStatus.forEach((value, key) => {
      status[key] = value;
    });
    return status;
  }

  isServiceHealthy(serviceName) {
    const status = this.getServiceStatus(serviceName);
    return status && status.status === 'healthy';
  }
}

// Create singleton instance
const healthChecker = new HealthChecker();

// Middleware to check service health before proxying
const checkServiceHealth = (serviceName) => {
  return (req, res, next) => {
    if (!healthChecker.isServiceHealthy(serviceName)) {
      return res.status(503).json({
        error: 'Service Unavailable',
        message: `The ${serviceName} service is currently unavailable`,
        service: serviceName,
        timestamp: new Date().toISOString()
      });
    }
    next();
  };
};

// Health status endpoint handler
const getHealthStatus = (req, res) => {
  const allStatus = healthChecker.getAllServiceStatus();
  const overallHealth = Object.values(allStatus).every(service => 
    service.status === 'healthy' || service.status === 'not_configured'
  );

  res.status(overallHealth ? 200 : 503).json({
    status: overallHealth ? 'healthy' : 'degraded',
    timestamp: new Date().toISOString(),
    services: allStatus,
    gateway: {
      status: 'healthy',
      uptime: process.uptime(),
      version: '1.0.0'
    }
  });
};

module.exports = {
  healthChecker,
  checkServiceHealth,
  getHealthStatus
};
