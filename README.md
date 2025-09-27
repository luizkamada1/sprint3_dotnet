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
dotnet restore MottuYardApi
dotnet restore MottuYardApi.Tests
dotnet run --project MottuYardApi
```
Swagger: abra a URL exibida no console (ex.: `http://localhost:5201/swagger`).

> O endpoint raiz `/` redireciona para `/swagger`.

### Rodar testes
```bash
dotnet test MottuYardApi.Tests
```

---

## 🧭 Endpoints principais + exemplos de uso

### **PÁTIO**

#### Criar Pátio

```
POST /api/patios
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
GET /api/patios?page=1&pageSize=10
```

#### Obter Pátio por ID

```
GET /api/patios/1
```

#### Atualizar Pátio

```
PUT /api/patios/1
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
DELETE /api/patios/1
```

---

### **ZONA**

#### Criar Zona

```
POST /api/zonas
```

```json
{
  "nome": "Zona C1",
  "patioId": 1
}
```

#### Listar Zonas (paginado)

```
GET /api/zonas?page=1&pageSize=10
```

#### Obter Zona por ID

```
GET /api/zonas/1
```

#### Listar Zonas de um Pátio

```
GET /api/zonas/patio/1
```

#### Atualizar Zona

```
PUT /api/zonas/1
```

```json
{
  "nome": "Zona C2",
  "patioId": 1
}
```

#### Remover Zona

```
DELETE /api/zonas/1
```

---

### **MOTO**

#### Criar Moto

```
POST /api/motos
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
GET /api/motos?page=1&pageSize=10
```

#### Obter Moto por ID

```
GET /api/motos/1
```

#### Listar Motos de uma Zona

```
GET /api/motos/zona/1
```

#### Atualizar Moto

```
PUT /api/motos/1
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
DELETE /api/motos/1
```

#### Mover Moto entre Zonas (ação de negócio)

```
POST /api/motos/1/mover
```

```json
{
  "novaZonaId": 2
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
- Testes cobrem _seed_, criação de entidades e movimentação de moto
