# SecureFileCloud

## Descrição

SecureFileCloud é uma Web API didática em ASP.NET Core para upload, download, listagem e consulta de metadados de arquivos. A API salva os arquivos físicos em storage local e mantém os metadados persistidos em SQLite, com apoio de cache em memória.

## Objetivo do projeto

O objetivo do projeto é praticar a construção de uma Web API em .NET, incluindo upload e download de arquivos, organização por responsabilidades, persistência de metadados, uso de middleware, filtro de ação, logging e manipulação de arquivos no servidor.

## Funcionalidades

- Upload de arquivo
- Upload múltiplo
- Download de arquivo
- Download múltiplo em ZIP
- Listagem de metadados
- Consulta de metadados por ID
- Validação simples por token no header `X-Access-Token`
- Persistência de metadados em SQLite
- Cache/dicionário em memória para metadados
- Middleware de auditoria e bloqueio de requisições maiores que 5 MB

## Tecnologias Utilizadas

- .NET 9
- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQLite
- Swagger/OpenAPI
- Arquivo `.http`
- LINQ
- `ConcurrentDictionary`
- `System.IO.Compression`

## Estrutura do Projeto

- `Application`: contratos, DTOs e regras de aplicação relacionadas ao processamento de upload, download e metadados.
- `Domain`: entidade principal de arquivo.
- `Infrastructure`: persistência com Entity Framework Core/SQLite, repositórios e armazenamento local dos arquivos físicos.
- `Presentation`: controller, filtro de validação de token e middleware de auditoria.
- `Extensions`: métodos de extensão para registrar CORS, Swagger, persistência e serviços da aplicação.
- `AppData`: diretório local usado pela aplicação para banco SQLite e arquivos enviados.

## Decisões Técnicas

### Persistência com SQLite

O enunciado do PDF sugere armazenamento em memória para os metadados, mas o projeto adiciona SQLite como persistência real. Com isso, os metadados continuam disponíveis entre execuções da aplicação, tornando o comportamento mais próximo de uma API real sem comprometer o caráter didático.

### Dicionário em Memória Como Cache

Para manter aderência ao requisito didático do PDF, o projeto também utiliza um dicionário em memória para metadados. A implementação usa `ConcurrentDictionary`, uma variação thread-safe de `Dictionary`, adequada ao contexto de uma Web API que pode receber múltiplas requisições simultâneas.

### Decorator no Repositório

O `CachedArquivoRepository` decora a interface `IArquivoRepository`. Ele tenta consultar primeiro o cache em memória e delega ao repositório SQLite quando necessário. Essa decisão preserva o contrato usado pela aplicação e evita misturar regra de cache dentro do service principal.

### Storage Local

Os arquivos físicos são armazenados em `AppData/FileStorage`. O banco SQLite guarda apenas os metadados, como ID, nome original, nome armazenado, content type, tamanho e data de upload.

### Token de Acesso

A API usa o header `X-Access-Token` para uma validação simples de acesso. Essa abordagem é suficiente para fins didáticos, mas não substitui autenticação robusta de produção, como OAuth2 ou JWT.

### Tratamento Básico de Erro

O projeto registra `AddProblemDetails()` e usa `UseExceptionHandler()`, oferecendo um tratamento básico para erros inesperados. Essa é uma melhoria simples para padronizar falhas gerais, não um requisito central do enunciado.

### Swagger/OpenAPI

O Swagger está configurado em ambiente de desenvolvimento e permite informar o header `X-Access-Token` pelo botão de autorização. Isso facilita testar endpoints protegidos sem montar manualmente cada requisição.

### Arquivo `.http`

O arquivo `SecureFileCloud.API.http` contém exemplos de requisições para testes manuais diretamente pelo editor ou IDE, incluindo upload, download, listagem e consulta por ID.

## Configuração

As principais chaves esperadas em `appsettings.json` são:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=AppData/Database/securefilecloud.db"
  },
  "Storage": {
    "BasePath": "AppData/FileStorage"
  },
  "Security": {
    "AccessToken": "seu-token-aqui"
  },
  "Cors": {
    "AllowedOrigin": "https://exemplo.com"
  }
}
```

Não exponha tokens reais, connection strings sensíveis ou caminhos absolutos da máquina em repositórios públicos.

## Como Executar

1. Clone o repositório.
2. Restaure os pacotes:

```bash
dotnet restore
```

3. Confira as chaves de configuração em `src/SecureFileCloud.API/appsettings.json`.
4. Aplique a migration existente para criar ou atualizar o banco SQLite:

```bash
dotnet ef database update --project src/SecureFileCloud.API
```

5. Execute a API:

```bash
dotnet run --project src/SecureFileCloud.API
```

6. Em ambiente de desenvolvimento, acesse o Swagger no endereço exibido pela aplicação, normalmente em `/swagger`.

## Endpoints

Todas as rotas abaixo são protegidas e exigem o header:

```http
X-Access-Token: seu-token-aqui
```

| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/arquivo` | Lista os metadados dos arquivos cadastrados. |
| `GET` | `/api/arquivo/{id}` | Obtém os metadados de um arquivo pelo ID. |
| `POST` | `/api/arquivo/enviar` | Envia um arquivo usando `multipart/form-data`, no campo `arquivo`. |
| `POST` | `/api/arquivo/enviar-multiplos` | Envia múltiplos arquivos usando `multipart/form-data`, no campo `arquivos`. |
| `GET` | `/api/arquivo/baixar/{id}` | Baixa um arquivo pelo ID. |
| `GET` | `/api/arquivo/baixar-multiplos?ids={id1}&ids={id2}` | Baixa múltiplos arquivos em um ZIP. |

## Testando Pelo Swagger

Execute a aplicação em ambiente de desenvolvimento, abra o Swagger e use o botão `Authorize` para informar o valor do header `X-Access-Token`. Depois disso, as chamadas feitas pela interface do Swagger enviarão o token junto das requisições.

## Testando Pelo Arquivo `.http`

O arquivo `src/SecureFileCloud.API/SecureFileCloud.API.http` contém exemplos prontos de requisições reais da API. Ajuste as variáveis `host`, `accessToken`, `arquivoId` e `arquivoId2` antes de executar as chamadas.

## Aderência ao Enunciado

| Requisito | Implementação no projeto | Observação |
| --- | --- | --- |
| Controller `ArquivoController` | Implementado em `Presentation/Controllers/ArquivoController.cs`. | Requisito atendido diretamente. |
| Rotas customizadas por atributos | Rotas como `/api/arquivo/baixar/{id:guid}` e `/api/arquivo/enviar`. | Requisito atendido diretamente, com prefixo `api`. |
| Upload de arquivo | `POST /api/arquivo/enviar`, recebendo `IFormFile`. | Requisito atendido diretamente. |
| Download de arquivo | `GET /api/arquivo/baixar/{id}`. | O projeto baixa por ID em vez de nome do arquivo, usando os metadados persistidos. |
| Nome armazenado diferente do original | O storage salva com nome baseado em `Guid` e mantém a extensão. | Requisito atendido diretamente. |
| Middleware de auditoria | `AuditoriaMiddleware` registra método, caminho, status e tempo. | Requisito atendido diretamente. |
| Bloqueio acima de 5 MB | O middleware retorna `413 Payload Too Large` quando `Content-Length` excede 5 MB. | Requisito atendido diretamente. |
| Filtro de validação de token | `ValidadorDeChaveAttribute` verifica `X-Access-Token`. | Requisito atendido diretamente. |
| CORS para origem específica | Configuração via `Cors:AllowedOrigin`. | Requisito atendido diretamente. |
| Logging no controller | O controller registra início de upload e falhas de download. | Requisito atendido diretamente. |
| Async/Await em I/O | Upload, leitura e persistência usam operações assíncronas. | Requisito bônus atendido. |
| Metadados em memória com busca via LINQ | `CachedArquivoRepository` usa `ConcurrentDictionary` e ordenação com LINQ. | Embora o enunciado sugira armazenamento em memória, o projeto mantém cache em memória para aderência ao requisito e adiciona SQLite como melhoria de persistência. |
| Persistência em SQLite | Metadados persistidos com EF Core e SQLite. | Melhoria opcional além do solicitado no PDF. |
| Upload múltiplo | `POST /api/arquivo/enviar-multiplos`. | Melhoria opcional além do solicitado no PDF. |
| Download múltiplo em ZIP | `GET /api/arquivo/baixar-multiplos`. | Melhoria opcional além do solicitado no PDF. |
| Swagger com token | Swagger configurado para envio de `X-Access-Token`. | Melhoria opcional para facilitar testes. |
| Arquivo `.http` | Exemplos manuais em `SecureFileCloud.API.http`. | Melhoria opcional para facilitar testes pelo editor/IDE. |

## Melhorias Futuras

- Validações mais fortes para tipo e tamanho de arquivo
- Autenticação real com JWT
- Testes automatizados
- Uso de storage externo
- Padronização completa de erros com ProblemDetails
- Paginação na listagem de arquivos
