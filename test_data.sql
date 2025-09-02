-- Insert test user
INSERT INTO users (name, surname, email, username, password, role, created_at) 
VALUES ('Test', 'User', 'test@example.com', 'testuser', 'password123', 'TOURIST', NOW());

-- Insert test tour  
INSERT INTO tours."Tours" ("AuthorId", "Name", "Description", "Difficulty", "Tags", "Status", "Price", "PublishedAt", "ArchivedAt", "LengthKm", "CreatedAt")
VALUES (1, 'Test Tour', 'A test tour for debugging', 1, ARRAY['test', 'debugging'], 1, 25.99, NOW(), NOW() + INTERVAL '30 days', 5.5, NOW());

-- Insert checkpoints for the tour
INSERT INTO tours."Checkpoints" ("TourId", "Name", "Description", "Latitude", "Longitude", "ImageBase64", "CreatedAt")
VALUES 
(1, 'Start Point', 'Starting point of the tour', 44.7866, 20.4489, NULL, NOW()),
(1, 'Checkpoint 1', 'First checkpoint', 44.7876, 20.4499, NULL, NOW() + INTERVAL '1 minute'),
(1, 'End Point', 'Final destination', 44.7886, 20.4509, NULL, NOW() + INTERVAL '2 minutes');

-- Insert tour purchase token (assuming user id 1)
INSERT INTO tours."TourPurchaseTokens" ("UserId", "TourId", "TokenValue", "CreatedAt")
VALUES (1, 1, 'test-token-123', NOW());
