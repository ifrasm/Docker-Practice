CREATE DATABASE ecommerce;

CREATE TABLE products(
    id SERIAL PRIMARY KEY,
    name VARCHAR(100),
    price DECIMAL
);

INSERT INTO products(name,price)
VALUES
('Laptop',4500),
('Mouse',100),
('Keyboard',220);
