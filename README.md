# API Antonio Dario — Projeto Final UC605

## Descrição
API REST desenvolvida em .NET 9 com SQL Server, Redis, Polly e Mountebank.
Autenticação via JWT Bearer Token.

## Tecnologias
- .NET 9 / ASP.NET Core Web API
- SQL Server 2022 (via Docker)
- Redis 7 (cache distribuído)
- Polly (retries + circuit breaker)
- Mountebank (mock de serviços externos)
- JWT (autenticação)
- Docker + docker-compose

## Como executar

### Pré-requisito
Docker Desktop instalado e em execução.

### Passos
```bash
git clone https://github.com/teu-user/api-antoniodario-projeto-final.git
cd api-antoniodario-projeto-final
docker-compose up --build
```

API disponível em: http://localhost:8080
Swagger: http://localhost:8080/swagger

## Endpoints

| Método | Endpoint | Auth | Descrição |
|--------|----------|------|-----------|
| POST | /api/auth/register | Não | Registar utilizador |
| POST | /api/auth/login | Não | Login e obter JWT |
| GET  | /api/produtos | Sim | Listar produtos (cache Redis) |
| POST | /api/produtos | Sim | Criar produto |
| PUT  | /api/produtos/{id} | Sim | Atualizar produto |
| DELETE | /api/produtos/{id} | Sim | Apagar produto |
| GET  | /api/imposter/inventory/{sku} | Sim | Inventário (mock) |
| POST | /api/imposter/payments | Sim | Pagamento (mock) |

## Base de Dados
Scripts em `database/schema.sql` e `database/seed.sql`.

## Variáveis de Ambiente
Ver `.env.example` para as variáveis necessárias.