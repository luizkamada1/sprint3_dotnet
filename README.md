# Challenge Mottu - Sprint 3 - ADVANCED BUSINESS DEVELOPMENT WITH .NET

## 👥 Integrantes:
- Eduardo Guilherme Dias - RM557886 - 2TDSPV
- Gabriel Alves Thomaz - RM558637 - 2TDSPV
- Luiz Sadao Kamada – RM557652 – 2TDSPV


API RESTful
Entidades principais: **Pátios**, **Zonas** e **Motos**.

---

## Contexto Mottu

O problema de negócio do é mapear e gerenciar **motos** alocadas em **zonas** dentro de **pátios** logísticos.  
Escolhemos as entidades **Pátio → Zona → Moto** porque representam a hierarquia real de localização e permitem cobrir cenários típicos de **inventário**, **alocação** e **movimentação** com endpoints REST claros.

---

## 🏛️ Arquitetura

- **.NET 8 Minimal API**
- **EF Core InMemory** (execução rápida e testes simples)
- Pastas: `Models`, `DTOs`, `Data`, `Utils`
- **Swagger** com XML comments (já habilitado no `.csproj`)
- **HATEOAS**: links `self`, `next`, `prev` e links de navegação entre recursos

---

## ▶️ Como executar

Pré-requisito: **.NET 8 SDK** instalado

```bash
dotnet restore
dotnet run --project MottuYardApi
```
Swagger: abra a URL exibida no console (ex.: `http://localhost:5201/swagger`).

> O endpoint raiz `/` redireciona para `/swagger`.

### Rodar testes
```bash
dotnet test
```

---

## 🧭 Endpoints principais

### Pátios
- `GET /api/patios?page=1&pageSize=10`
- `GET /api/patios/{id}`
- `POST /api/patios`
- `PUT /api/patios/{id}`
- `DELETE /api/patios/{id}`

### Zonas
- `GET /api/zonas?page=1&pageSize=10`
- `GET /api/zonas/{id}`
- `GET /api/zonas/patio/{patioId}`
- `POST /api/zonas`
- `PUT /api/zonas/{id}`
- `DELETE /api/zonas/{id}`

### Motos
- `GET /api/motos?page=1&pageSize=10`
- `GET /api/motos/{id}`
- `GET /api/motos/zona/{zonaId}`
- `POST /api/motos`
- `PUT /api/motos/{id}`
- `DELETE /api/motos/{id}`
- **Ação de negócio:** `POST /api/motos/{id}/mover` (mover moto entre zonas)

---

## 📦 Exemplos de payloads

**POST /api/patios**
```json
{
  "nome": "CD Curitiba",
  "cidade": "Curitiba",
  "estado": "PR"
}
```

**POST /api/zonas**
```json
{
  "nome": "C1",
  "patioId": 1
}
```

**POST /api/motos**
```json
{
  "placa": "AAA1B23",
  "modelo": "CG 160",
  "status": "Ativa",
  "zonaId": 2
}
```

**POST /api/motos/{id}/mover**
```json
{
  "novaZonaId": 3
}
```

---

## 🔧 Decisões de arquitetura

- **Minimal API** para baixo overhead e foco em endpoints claros
- **DTOs** para separar contrato público do modelo de dados
- **InMemory** para rodar localmente sem depender de SGBD; em produção, usar SQL Server/PostgreSQL
- **Paginação** consistente com `page` e `pageSize` + **HATEOAS** para navegação
- **Status codes**: 201 (POST), 204 (PUT/DELETE), 404 (não encontrado), 400 (validação)

---

## 🧪 Sobre os testes

- Projeto `MottuYardApi.Tests` com **xUnit + EF InMemory**
- Testes cobrem _seed_, criação de entidades e movimentação de moto
