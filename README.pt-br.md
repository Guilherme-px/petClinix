[🇧🇷 Português](README.pt-br.md) | [🇺🇸 English](README.md)

# PetClinix

PetClinix é um MVP de SaaS multi-tenant para clínicas veterinárias, projetado e implementado como um projeto de portfólio público para demonstrar práticas de engenharia de software de nível sênior, pensamento de produto e tomada de decisão arquitetural.

Este repositório representa intencionalmente o **estágio de MVP público** do produto.  
Seu propósito é展示 os alicerces da plataforma, incluindo modelagem de domínio, arquitetura modular, limites de tenancy e as primeiras capacidades essenciais do negócio.

A evolução completa do produto, incluindo recursos avançados, hardening de produção e melhorias de nível comercial, deve continuar em um **repositório privado** após o marco do MVP.

---

## Propósito

Este projeto foi criado para demonstrar o design e a implementação de um produto SaaS do mundo real com um forte foco em:

- Clean Architecture
- Domain-Driven Design (DDD)
- Design modular
- Pensamento multi-tenant
- Manutenibilidade
- Escalabilidade
- Clara separação de preocupações (separation of concerns)
- Design de software focado no domínio

Em vez de construir uma aplicação CRUD genérica, o objetivo é modelar um domínio de negócio realista e construir uma base arquitetural sólida que possa suportar o crescimento futuro.

---

## Visão do Produto

O PetClinix visa fornecer às clínicas veterinárias uma plataforma centralizada para gerenciar suas operações diárias, equipe, pets, tutores, agendamentos, assinaturas e fluxos de trabalho clínicos.

A plataforma está sendo projetada como um produto SaaS onde cada clínica opera dentro de seu próprio escopo de dados isolado.

No estágio de MVP, o produto foca em validar a base operacional central da plataforma.

---

## Escopo do MVP Público

O MVP público tem como objetivo cobrir as bases fundamentais e os fluxos de trabalho essenciais da plataforma, incluindo:

- Onboarding e cadastro de clínicas
- Fluxo de assinatura e pagamento
- Autenticação
- Gerenciamento de funcionários
- Cadastro de pets
- Cadastro de tutores
- Gerenciamento de agenda
- Gerenciamento de serviços
- Rastreamento de vacinas

Este escopo é intencionalmente amplo o suficiente para representar uma fatia vertical realista do produto, ainda sendo limitado a um MVP adequado para demonstração pública.

---

## Estratégia de Repositório

Este repositório é a **vitrine do MVP público**.

Seu propósito é demonstrar:

- Fundações arquiteturais
- Design de domínio
- Estrutura do projeto
- Padrões de código
- Direção técnica
- Estilo de implementação

Após este MVP atingir um estado representativo e demonstrativo, o projeto continuará evoluindo em um **repositório privado**, onde o produto poderá ser mais refinado com:

- Módulos adicionais
- Hardening de nível de produção
- Regras de negócio mais profundas
- Melhorias de UX e preocupações operacionais
- Evolução de infraestrutura
- Melhorias comerciais e de segurança

Esta versão pública deve, portanto, ser vista como um **marco arquitetural de nível portfólio**, não a forma comercial final do produto.

---

## Arquitetura

O PetClinix está sendo construído como um **monolito modular** com limites de dependência claros e separação de módulos orientada ao negócio.

A solução segue os princípios de:

- Clean Architecture
- Domain-Driven Design
- Organização modular por contexto de negócio

### Estrutura atual do projeto

```text
├── PetClinix.Api/                              # Camada de Apresentação (Controllers, Middleware, DI)
├── PetClinix.BuildingBlocks.Application/       # Contratos base (interfaces CQRS, Result pattern)
├── PetClinix.BuildingBlocks.Domain/            # Contratos de domínio base (Entity, AggregateRoot, DomainEvents)
├── PetClinix.BuildingBlocks.Infrastructure/    # Contratos de infraestrutura base
├── PetClinix.Modules.Identity.Application/     # Casos de uso de Identidade (Commands, Handlers, Validators)
├── PetClinix.Modules.Identity.Domain/          # Lógica de negócio de Identidade (Entities, Value Objects)
└── PetClinix.Modules.Identity.Infrastructure/  # Persistência de Identidade (EF Core, Repositories)
tests/
├── PetClinix.UnitTests/                        # Testes rápidos e isolados usando NSubstitute e FluentAssertions
└── PetClinix.IntegrationTests/                 # Testes E2E de API usando WebApplicationFactory e Testcontainers
```

---

## Stack Tecnológica

- **Framework:** .NET 10 (Preview)
- **Arquitetura:** Monolito Modular, Clean Architecture, DDD
- **Banco de Dados:** PostgreSQL
- **ORM:** Entity Framework Core 9
- **Validação:** FluentValidation
- **Testes:** xUnit, NSubstitute, FluentAssertions, Testcontainers
- **Documentação da API:** Swagger / OpenAPI

---

## Pré-requisitos

Antes de começar, garanta que você tenha o seguinte instalado em sua máquina:

1. **.NET 10 SDK (Preview)**
   - Necessário para compilar e rodar a aplicação.
   - Download: [https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0)

2. **PostgreSQL**
   - Necessário para rodar o banco de dados da aplicação localmente.
   - Você pode instalá-lo nativamente ou rodá-lo via Docker:
     ```bash
     docker run --name petclinix-pg -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16-alpine
     ```

3. **Docker**
   - **Estritamente necessário para rodar os Testes de Integração.** Os testes de integração usam o `Testcontainers` para subir um banco de dados PostgreSQL real e efêmero dentro de um container Docker automaticamente.
   - Download: [https://www.docker.com/products/docker-desktop/](https://www.docker.com/products/docker-desktop/)

4. **Entity Framework Core Tools**
   - Necessário para criar e aplicar migrações de banco de dados.
   - Instale globalmente rodando:
     ```bash
     dotnet tool install --global dotnet-ef
     ```

---

## Iniciando

Siga estes passos para configurar e rodar o projeto localmente.

### 1. Clone o repositório
```bash
git clone https://github.com/seu-usuario/PetClinix.git
cd PetClinix
```

### 2. Configure a Conexão com o Banco de Dados
A aplicação usa uma abordagem de configuração em camadas. O template base está em `appsettings.json`, mas suas credenciais locais devem ser colocadas em `appsettings.Development.json` (que é ignorado pelo Git).

Crie o arquivo `PetClinix.Api/appsettings.Development.json` e adicione sua string de conexão:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=petclinix_db;Username=postgres;Password=sua_senha_aqui"
  }
}
```

### 3. Aplique as Migrações do Banco de Dados
Para criar o schema do banco de dados, rode o seguinte comando a partir do diretório raiz:

```bash
dotnet ef database update --project PetClinix.Modules.Identity.Infrastructure --startup-project PetClinix.Api
```

### 4. Rode a Aplicação
```bash
dotnet run --project PetClinix.Api
```
Uma vez em execução, abra seu navegador e navegue até o Swagger UI para testar os endpoints da API:
- **Swagger UI:** `http://localhost:<porta>/swagger` (verifique a saída do seu terminal para a porta exata, geralmente `5180` ou `5000`).

---

## Testes

O projeto contém uma suíte abrangente de testes dividida em Testes Unitários e Testes de Integração.

### Rodar Todos os Testes
Para rodar toda a suíte de testes. **Nota: O Docker deve estar em execução** para que os testes de integração sejam executados com sucesso:
```bash
dotnet test
```

### Rodar Apenas Testes Unitários
Testes unitários são rápidos e não requerem dependências externas como bancos de dados ou Docker.
```bash
dotnet test tests/PetClinix.UnitTests
```

### Rodar Apenas Testes de Integração
Testes de integração validam a API desde a requisição HTTP até o banco de dados PostgreSQL. **Eles exigem que o Docker esteja em execução**.
```bash
dotnet test tests/PetClinix.IntegrationTests
```

---

## Roteiro

- [x] Onboarding e Cadastro de Clínicas (Fundação multi-tenant)
- [x] Validação de Domínio e Value Objects
- [x] Integração com PostgreSQL usando EF Core
- [ ] Fluxo de checkout de assinatura via Stripe
- [ ] Tratamento de Webhook para sucesso de pagamento
- [ ] Módulo de Identidade: Definição de Senha e Login (JWT)
- [ ] Módulo de gerenciamento de funcionários
- [ ] Módulo de cadastro de Pets e Tutores
- [ ] Agendamento e fluxos de trabalho clínicos

---

## Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.