# API Antonio Dario — Projeto Final UC605

## Descrição:
Esta é uma solução completa de Gestão de Produtos e Pagamentos, composta por uma API REST robusta desenvolvida em .NET 8 e uma interface Frontend construída com HTML5 e JavaScript. O sistema destaca-se pela utilização de um padrão de cache híbrido (L1 + L2) para performance e políticas de resiliência HTTP para integração com serviços externos.  

## Tecnologias
### Backend: 
.NET 8 / ASP.NET Core Web API.  

### Frontend: 
HTML5, CSS3 (Dark Theme) e JavaScript (ES6+).  

### Base de Dados: 
SQL Server 2022 (via Docker).  

### Cache: Redis 7 (L2) e IMemoryCache (L1).  

### Resiliência: 
Polly (Wait and Retry + Circuit Breaker).  

### Mocks: 
Mountebank (Simulação de Inventário e Pagamentos).  

### Segurança: Autenticação JWT com Roles (Admin e User).  

### Orquestração: Docker + Docker Compose.  

# Como executar
Pré-requisitos: Docker Desktop instalado e em execução.
Passos:
1. Clone o repositório:Bashgit clone "https://github.com/Antoni-Costa/api-antoniodario-projeto-final.git"
cd api-antoniodario-projeto-final
2. Inicie os contentores (API, SQL, Redis e Mountebank) com o comando docker-compose up --build na mesma pasta do docker-compose.yml

# API/Frontend: http://localhost:8080.Swagger UI: http://localhost:8080/swagger.  

# Endpoints Principais
Método  Endpoint                        Auth    Role    Descrição
POST    /api/auth/login                 Não     -       Login e obtenção do JWT.  
GET     /api/produtos                   Sim     Any     Listar produtos.  
POST    /api/produtos                   Sim     Admin   Criar novo produto.  
GET     /api/imposter/inventory/{sku}   Sim     Any     Consultar stock via Mountebank.  
POST    /api/imposter/payments          Sim     Any     Processar pagamento via Mock Externo.  
GET     /api/utilizadores               Sim     Admin   Listar todos os utilizadores.  
DELETE  /api/utilizadores/{id}          Sim     Admin   Apagar conta de utilizador.  Resiliência e CacheCache 

# Base de Dados
A API aplica as migrações automaticamente ao arrancar via Docker. Caso prefira execução manual, os esquemas encontram-se na pasta database/.  Variáveis de AmbienteAs configurações de ligação (SQL Server, Redis e JWT) estão pré-configuradas no docker-compose.yml.