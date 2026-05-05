USE ProjetoFinalDB;
GO

INSERT INTO Utilizadores (Nome, Email, Password, Role)
VALUES ('Admin', 'admin@teste.pt', 'Admin123!', 'Admin');

INSERT INTO Utilizadores (Nome, Email, Password, Role)
VALUES ('Antonio User', 'antonio@teste.pt', 'User123!', 'User');

INSERT INTO Produtos (Nome, Descricao, Preco, Stock, SKU)
VALUES
  ('Produto A', 'Descrição do produto A', 29.99, 100, 'SKU-001'),
  ('Produto B', 'Descrição do produto B', 49.99, 50,  'SKU-002'),
  ('Produto C', 'Descrição do produto C', 9.99,  200, 'SKU-003');
GO