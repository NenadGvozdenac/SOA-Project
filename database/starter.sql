DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS roles;

CREATE TABLE IF NOT EXISTS roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL
);

CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    surname VARCHAR(255) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    username VARCHAR(255) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    biography VARCHAR(255) NOT NULL,
    moto VARCHAR(255) NOT NULL,
    profile_picture BYTEA,
    role_id SERIAL,

    blocked BOOLEAN NOT NULL DEFAULT FALSE,

    FOREIGN KEY (role_id) references roles(id)
);

INSERT INTO roles (id, name)
    VALUES (0, 'Admin'), (1, 'Guide'), (2, 'Tourist');

INSERT INTO users (name, surname, email, username, password, biography, moto, blocked, role_id)
    VALUES ('Admin', 'User', 'admin@gmail.com', 'admin', '$2b$12$RRgRi/SXn1LPRQwggm/.9OgWn7oPkoWeky5L1dXK3FQ9.jiMGq.DS', 'This is the admin user.', 'Admin Moto', FALSE, 0); 
-- Example password: admin