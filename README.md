# Challenge Mottu - Sprint 4 - ADVANCED BUSINESS DEVELOPMENT WITH .NET

## 👥 Integrantes:
- Eduardo Guilherme Dias - RM557886 - 2TDSPV
- Gabriel Alves Thomaz - RM558637 - 2TDSPV
- Luiz Sadao Kamada – RM557652 – 2TDSPV


API RESTful
Entidades principais: **Pátios**, **Zonas** e **Motos**.

---

## Contexto Mottu

O desafio da Mottu é mapear e gerenciar **motos** alocadas em **zonas** dentro de **pátios** logísticos.  
Escolhemos as entidades **Pátio → Zona → Moto** porque representam a hierarquia real de localização e permitem cobrir cenários típicos de **inventário**, **alocação** e **movimentação** com endpoints REST claros.

---

## 🏛️ Arquitetura

- **.NET 8 Minimal API**
- **EF Core InMemory** (execução rápida e testes simples)
- Pastas: `Models`, `DTOs`, `Data`, `Utils`
- **Swagger** com XML comments (já habilitado no `.csproj`)
- **API Versioning** (`/api/v1/...`) com documentação agrupada no Swagger
- **Segurança por API Key** (`X-API-Key`) aplicada aos endpoints da API
- **Health Checks** em `/health`
- **Endpoint analítico com ML.NET** para prever manutenção preventiva
- **HATEOAS**: links `self`, `next`, `prev` e links de navegação entre recursos

---

## ▶️ Como executar

Pré-requisito: **.NET 8 SDK** instalado

```bash
dotnet restore MottuYardApi
dotnet restore MottuYardApi.Tests
dotnet run --project MottuYardApi
```
Swagger: abra a URL exibida no console (ex.: `http://localhost:5000`).

> O endpoint raiz `/` redireciona para `/swagger`.

### Autenticação por API Key

Todos os endpoints versionados (`/api/v1/...`) exigem o cabeçalho `X-API-Key`.
Durante o desenvolvimento e testes a chave padrão é `local-dev-key`.

Exemplo com `curl`:

```bash
curl -H "X-API-Key: local-dev-key" http://localhost:5000/api/v1/patios
```

### Rodar testes
```bash
dotnet test MottuYardApi.Tests
```

---

## 🧭 Endpoints principais + exemplos de uso

### **PÁTIO**

#### Criar Pátio

```
POST /api/v1/patios
```

```json
{
  "nome": "CD Curitiba",
  "cidade": "Curitiba",
  "estado": "PR"
}
```

#### Listar Pátios (paginado)

```
GET /api/v1/patios?page=1&pageSize=10
```

#### Obter Pátio por ID

```
GET /api/v1/patios/1
```

#### Atualizar Pátio

```
PUT /api/v1/patios/1
```

```json
{
  "nome": "CD São Paulo",
  "cidade": "São Paulo",
  "estado": "SP"
}
```

#### Remover Pátio

```
DELETE /api/v1/patios/1
```

---

### **ZONA**

#### Criar Zona

```
POST /api/v1/zonas
```

```json
{
  "nome": "Zona C1",
  "patioId": 1
}
```

#### Listar Zonas (paginado)

```
GET /api/v1/zonas?page=1&pageSize=10
```

#### Obter Zona por ID

```
GET /api/v1/zonas/1
```

#### Listar Zonas de um Pátio

```
GET /api/v1/zonas/patio/1
```

#### Atualizar Zona

```
PUT /api/v1/zonas/1
```

```json
{
  "nome": "Zona C2",
  "patioId": 1
}
```

#### Remover Zona

```
DELETE /api/v1/zonas/1
```

---

### **MOTO**

#### Criar Moto

```
POST /api/v1/motos
```

```json
{
  "placa": "AAA1B23",
  "modelo": "CG 160",
  "status": "Ativa",
  "zonaId": 1
}
```

#### Listar Motos (paginado)

```
GET /api/v1/motos?page=1&pageSize=10
```

#### Obter Moto por ID

```
GET /api/v1/motos/1
```

#### Listar Motos de uma Zona

```
GET /api/v1/motos/zona/1
```

#### Atualizar Moto

```
PUT /api/v1/motos/1
```

```json
{
  "placa": "BBB2C34",
  "modelo": "CG 160 Fan",
  "status": "Manutenção",
  "zonaId": 1
}
```

#### Remover Moto

```
DELETE /api/v1/motos/1
```

#### Mover Moto entre Zonas (ação de negócio)

```
POST /api/v1/motos/1/mover
```

```json
{
  "novaZonaId": 2
}
```

---

### **Saúde da aplicação**

```text
GET /health
```

Retorna `200 OK` quando o serviço está íntegro.

---

### **Analytics / ML.NET**

```text
POST /api/v1/analytics/maintenance-prediction
```

```json
{
  "daysSinceMaintenance": 30,
  "completedDeliveries": 120,
  "breakdownHistory": 2
}
```

Resposta exemplo:

```json
{
  "requiresMaintenance": true,
  "probability": 0.82
}
```

---


## 🔧 Decisões de arquitetura

* **Minimal API** para endpoints enxutos e simples
* **DTOs** para manter contratos limpos
* **InMemory** para testes rápidos; pode ser trocado por SQL Server/PostgreSQL em produção
* **Paginação + HATEOAS** para navegação padrão REST
* **Status codes corretos**:

  * `201 Created` para POST
  * `204 No Content` para PUT/DELETE
  * `404 Not Found` para inexistentes
  * `400 Bad Request` para erros de validação

---


## 🧪 Sobre os testes

- Projeto `MottuYardApi.Tests` com **xUnit + EF InMemory**
- **Testes unitários** para a lógica de predição com ML.NET
- **Testes de integração** via `WebApplicationFactory` validando versionamento, health check e segurança por API Key
