/**
 * Basic tests for API Gateway
 * Run with: npm test
 */

const request = require('supertest');
const app = require('../server');

describe('API Gateway', () => {
  let server;

  beforeAll((done) => {
    server = app.listen(3001, done); // Use different port for testing
  });

  afterAll((done) => {
    server.close(done);
  });

  describe('Health Endpoints', () => {
    test('GET /health should return 200', async () => {
      const response = await request(app)
        .get('/health')
        .expect(200);
      
      expect(response.body).toHaveProperty('status');
      expect(response.body).toHaveProperty('timestamp');
      expect(response.body).toHaveProperty('gateway');
    });

    test('GET /services should return service configuration', async () => {
      const response = await request(app)
        .get('/services')
        .expect(200);
      
      expect(response.body).toHaveProperty('services');
      expect(response.body.services).toHaveProperty('blogs');
      expect(response.body.services).toHaveProperty('followings');
      expect(response.body.services).toHaveProperty('stakeholders');
      expect(response.body.services).toHaveProperty('tours');
    });

    test('GET /info should return gateway information', async () => {
      const response = await request(app)
        .get('/info')
        .expect(200);
      
      expect(response.body).toHaveProperty('name');
      expect(response.body).toHaveProperty('version');
      expect(response.body).toHaveProperty('environment');
      expect(response.body).toHaveProperty('uptime');
    });
  });

  describe('Route Handling', () => {
    test('GET /nonexistent should return 404', async () => {
      const response = await request(app)
        .get('/nonexistent')
        .expect(404);
      
      expect(response.body).toHaveProperty('error');
      expect(response.body.error).toBe('Route not found');
      expect(response.body).toHaveProperty('availableRoutes');
    });
  });

  describe('Security Headers', () => {
    test('Should include security headers', async () => {
      const response = await request(app)
        .get('/health')
        .expect(200);
      
      expect(response.headers).toHaveProperty('x-content-type-options');
      expect(response.headers).toHaveProperty('x-frame-options');
    });

    test('Should include request ID header', async () => {
      const response = await request(app)
        .get('/health')
        .expect(200);
      
      expect(response.headers).toHaveProperty('x-request-id');
    });
  });

  describe('Rate Limiting', () => {
    test('Should accept requests within limit', async () => {
      for (let i = 0; i < 5; i++) {
        await request(app)
          .get('/health')
          .expect(200);
      }
    });
  });
});
