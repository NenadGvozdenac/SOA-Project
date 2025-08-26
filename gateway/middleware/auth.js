/**
 * Authentication middleware for API Gateway
 * This middleware can be extended to handle JWT tokens, API keys, etc.
 */

const jwt = require('jsonwebtoken');

// JWT verification middleware (optional)
const verifyToken = (req, res, next) => {
  const token = req.headers.authorization?.split(' ')[1]; // Bearer TOKEN

  if (!token) {
    // For now, we'll allow requests without tokens
    // In production, you might want to enforce authentication for certain routes
    return next();
  }

  try {
    // Verify JWT token (you'll need to set JWT_SECRET in environment)
    const decoded = jwt.verify(token, process.env.JWT_SECRET || 'your-secret-key');
    req.user = decoded;
    next();
  } catch (error) {
    return res.status(401).json({
      error: 'Invalid token',
      message: 'The provided authentication token is invalid or expired',
      timestamp: new Date().toISOString()
    });
  }
};

// API Key middleware (optional)
const verifyApiKey = (req, res, next) => {
  const apiKey = req.headers['x-api-key'];
  
  if (!apiKey) {
    return next(); // Allow requests without API key for now
  }

  // In production, verify against a database or environment variable
  const validApiKeys = process.env.VALID_API_KEYS?.split(',') || [];
  
  if (!validApiKeys.includes(apiKey)) {
    return res.status(401).json({
      error: 'Invalid API key',
      message: 'The provided API key is invalid',
      timestamp: new Date().toISOString()
    });
  }

  next();
};

// Request ID middleware for tracing
const addRequestId = (req, res, next) => {
  req.requestId = `req_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  res.setHeader('X-Request-ID', req.requestId);
  next();
};

module.exports = {
  verifyToken,
  verifyApiKey,
  addRequestId
};
