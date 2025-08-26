/**
 * Logging middleware for enhanced request/response tracking
 */

const morgan = require('morgan');

// Custom token for request ID
morgan.token('req-id', (req) => req.requestId || 'unknown');

// Custom token for user info
morgan.token('user', (req) => {
  if (req.user) {
    return `user:${req.user.id || req.user.userId || 'unknown'}`;
  }
  return 'anonymous';
});

// Custom token for response time in different format
morgan.token('response-time-ms', (req, res) => {
  if (!req._startAt || !res._startAt) {
    return '-';
  }
  
  const ms = (res._startAt[0] - req._startAt[0]) * 1e3 +
             (res._startAt[1] - req._startAt[1]) * 1e-6;
  
  return ms.toFixed(3) + 'ms';
});

// Development logging format
const developmentFormat = morgan((tokens, req, res) => {
  return [
    `[${new Date().toISOString()}]`,
    `[${tokens['req-id'](req, res)}]`,
    tokens.method(req, res),
    tokens.url(req, res),
    tokens.status(req, res),
    tokens['response-time-ms'](req, res),
    `- ${tokens['user'](req, res)}`,
    tokens['user-agent'](req, res)
  ].join(' ');
});

// Production logging format (JSON)
const productionFormat = morgan((tokens, req, res) => {
  const logObject = {
    timestamp: new Date().toISOString(),
    requestId: tokens['req-id'](req, res),
    method: tokens.method(req, res),
    url: tokens.url(req, res),
    status: parseInt(tokens.status(req, res)),
    responseTime: tokens['response-time-ms'](req, res),
    user: tokens['user'](req, res),
    userAgent: tokens['user-agent'](req, res),
    remoteAddr: tokens['remote-addr'](req, res),
    referrer: tokens.referrer(req, res)
  };
  
  return JSON.stringify(logObject);
});

// Error logging middleware
const logErrors = (err, req, res, next) => {
  const errorLog = {
    timestamp: new Date().toISOString(),
    requestId: req.requestId,
    error: {
      message: err.message,
      stack: err.stack,
      name: err.name
    },
    request: {
      method: req.method,
      url: req.url,
      headers: req.headers,
      body: req.body
    },
    user: req.user || 'anonymous'
  };
  
  console.error('Gateway Error:', JSON.stringify(errorLog, null, 2));
  next(err);
};

module.exports = {
  developmentFormat,
  productionFormat,
  logErrors
};
