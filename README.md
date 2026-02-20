# Books Clone API - Backend

![skoob_logo](https://github.com/user-attachments/assets/75da25c8-d35a-486a-b702-3dc0bfa9fc4e)

<div align="center">

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2CA5E0?style=for-the-badge&logo=docker&logoColor=white)
![NUnit](https://img.shields.io/badge/NUnit_4-239120?style=for-the-badge&logo=nunit&logoColor=white)

</div>

API RESTful construída em ASP.NET Core projetada para emular as funcionalidades do Skoob. O sistema atua como um gerenciador de livros, permitindo o registro de livros, acompanhamento de progresso de leitura e interação através de avaliações e resenhas.

A aplicação foi estruturada utilizando o padrão de camadas N-Tier (Controllers, Services, Repositories), garantindo uma clara separação de responsabilidades e facilitando a manutenção e testabilidade do código.

## Tecnologias e Ferramentas

- **Framework:** ASP.NET Core Web API.
- **Banco de Dados:** PostgreSQL 18.
- **ORM:** Entity Framework Core 10.0.
- **Segurança:** Hashes de senha utilizando `BCrypt`.
- **Infraestrutura:** Docker multi-stage.
- **Testes:** NUnit 4 e Moq para testes unitários isolados.
- **Documentação:** Swagger (OpenAPI).

## Funcionalidades Principais

- **Gestão de Usuários:** Criação de contas, atualização de perfis e visualização de perfil detalhada.
- **Catálogo de Livros:** Listagem e filtros com parâmetros (por título, autor e gênero).
- **Estante Virtual (UserBooks):** Controle de livros do usuário com regras de negócio:
- Transição de status automáticas (`Quero Ler`, `Lendo`, `Lido`).
  - Atualização dinâmica de páginas lidas e cálculo de percentual de conclusão.
  - Validação de resenhas (reviews) apenas para livros finalizados.
  - Sistema de avaliação (rating) de 1 a 5 estrelas.
  - Geração de relatório pessoal de acordo com o ano.

## Como Executar com Docker

### 1. Pré-requisitos

- [Docker](https://docs.docker.com/get-docker/) instalado.
- [Docker Compose](https://docs.docker.com/compose/install/) instalado.

### 2. Configuração de Ambiente

Crie um arquivo `.env` na raiz do repositório, baseando-se no arquivo `.env.example` fornecido:

```env
DB_USER=seu_usuario
DB_PASSWORD=sua_senha_segura
DB_NAME=skoob_db
PGADMIN_EMAIL=admin@admin.com
PGADMIN_PASS=admin
```

### 3. Subindo a Aplicação

No terminal, execute o comando abaixo na raiz do projeto:

```bash
docker compose up -d --build
```

### 4. Após o build, acesse:

- API Swagger: http://localhost:8080/swagger

- pgAdmin (Database Manager): http://localhost:5050

## Executando os Testes

```bash
dotnet test
```

## Problemas Comuns

- **Erro: `port is already allocated` ou `bind: address already in use`**
  Ocorre quando a porta TCP exigida pelo container já está em uso na máquina host. Geralmente, o conflito acontece na porta `5432` se você já tiver uma instância nativa do PostgreSQL rodando.

### **Soluções possíveis:**

**Opção A: Interromper o serviço nativo do host**

- **Linux:** `sudo systemctl stop postgresql`
- **Windows (Prompt como Admin):** `net stop postgresql-x64-18` _(o sufixo numérico depende da sua versão instalada)_
- **macOS (via Homebrew):** `brew services stop postgresql`

**Opção B: Alterar o bind de porta do Docker (Recomendado)**
Se precisar manter o seu PostgreSQL local rodando em paralelo, altere o mapeamento no `docker-compose.yml` para uma porta livre no host (ex: `"5433:5432"`).

---
