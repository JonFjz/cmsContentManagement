# CMS API për Menaxhimin e Përmbajtjes

Një API RESTful i fuqishëm i projektuar për sistemet e menaxhimit të përmbajtjes, që ofron aftësi për të menaxhuar përmbajtjen, kategoritë, etiketat (tags) dhe aksesin e sigurt përmes API keys.

## 🚀 Veçoritë

- **Menaxhimi i Përmbajtjes**: Krijimi, përditësimi, kërkimi dhe menaxhimi i ciklit të jetës së përmbajtjes.
- **Kategorizimi**: Organizimi i përmbajtjes me kategori dhe etiketa të personalizueshme.
- **Aksesi Publik**: Endpoint-e të dedikuara për konsumin publik të përmbajtjes të siguruara me API keys.
- **Siguria**: Autentifikim hibrid duke përdorur _JWT Bearer tokens_ për administrim dhe _API Keys_ për akses publik.

## 🔐 Autentifikimi

| Lloji          | Header                          | Përdorimi                                                                          |
| -------------- | ------------------------------- | ---------------------------------------------------------------------------------- |
| **JWT Bearer** | `Authorization: Bearer <token>` | Endpoint-et administrative (Përmbajtja, Kategoritë, Etiketat, Gjenerimi i API Key) |
| **API Key**    | `X-Api-Key: <your-api-key>`     | Endpoint-et për konsumin e përmbajtjes publike                                     |

## 📡 Pasqyra e Endpoint-eve

### Menaxhimi i API Key

_Menaxhoni çelësat e aksesit për klientët publikë._

- `POST /ApiKey/generate` - Gjenero një API key të ri.
- `GET /ApiKey` - Listo API keys aktivë.
- `DELETE /ApiKey/{keyId}` - Revoko një API key.

### Kategoritë

_Organizoni strukturën e përmbajtjes suaj._

- `GET /Category` - Merr të gjitha kategoritë.
- `POST /Category` - Krijo një kategori të re.
- `PUT /Category` - Përditëso një kategori ekzistuese.
- `GET /Category/{id}` - Merr detajet e kategorisë.
- `DELETE /Category/{id}` - Fshi një kategori.

### Menaxhimi i Përmbajtjes

_Administrimi kryesor për krijuesit e përmbajtjes._

- `GET /ContentManagment` - Kërkim i avancuar (filtra: query, tag, status, data).
- `GET /ContentManagment/{contentId}` - Merr detajet e plota të përmbajtjes.
- `PUT /ContentManagment/{contentId}` - Krijo ose përditëso përmbajtje.
- `DELETE /ContentManagment/{contentId}` - Fshi përmbajtje.
- `POST /ContentManagment/{contentId}/unpublish` - Tërhiq përmbajtjen e publikuar.
- `GET /ContentManagment/generate-new-id` - Mjet për të para-gjeneruar ID-të e përmbajtjes.

### Përmbajtja Publike

_Endpoint-et e drejtuara nga konsumatori._

- `GET /api/public/content` - Merr përmbajtjen e publikuar (mbështet faqrosjen & filtrimin).
- `GET /api/public/content/{slug}` - Merr një artikull të vetëm sipas slug-ut.

### Etiketat (Tags)

_Metadata fleksibël për përmbajtjen._

- `GET /Tag` - Listo të gjitha etiketat.
- `POST /Tag` - Krijo një etiketë të re.
- `PUT /Tag` - Përditëso një etiketë.
- `GET /Tag/{id}` - Merr informacionin e etiketës.
- `DELETE /Tag/{id}` - Fshi një etiketë.

## 🛠️ Shembuj Përdorimi

### Marrja e Përmbajtjes Publike

```bash
curl -X GET "https://api.cms.com/api/public/content?page=1&pageSize=10" \
     -H "X-Api-Key: YOUR_API_KEY"
```

### Krijimi i një Artikulli (Admin)

```bash
curl -X PUT "https://api.cms.com/ContentManagment/{contentId}" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -H "Content-Type: application/json" \
     -d '{
           "title": "Welcome to the CMS",
           "assetUrl": "https://example.com/image.png",
           "richContent": "<p>Hello World</p>",
           "categoryName": "News",
           "tags": ["announcement", "v1"]
         }'
```

## 🏗️ Zhvillimi

### Parakushtet

- .NET 8.0 SDK (ose version i pajtueshëm)
- Docker (për varësitë e bazës së të dhënave/infrastrukturës)

### Ekzekutimi Lokal

1. Starto shërbimet e infrastrukturës:
   ```bash
   docker compose up -d
   ```
2. Ekzekuto API-në:
   ```bash
   dotnet run --project cmsContentManagement.API
   ```

### 💾 Struktura e Bazës së Të Dhënave

Baza e të dhënave menaxhohet përmes **Entity Framework Core** dhe përbëhet nga entitetet kryesore të mëposhtme. Strukturat tabelare janë dizajnuar për performancë dhe integritet të të dhënave.

#### 1. Tabela `Content`

_Ruan entitetet kryesore të sistemit (artikujt, lajmet, postimet)._

| Kolona          | Tipi i të Dhënave | Përshkrimi                                                  |
| :-------------- | :---------------- | :---------------------------------------------------------- |
| **ContentId**   | `Guid` (PK)       | Çelësi primar unik për identifikimin e përmbajtjes.         |
| **Title**       | `String`          | Titulli kryesor i përmbajtjes.                              |
| **Slug**        | `String`          | Identifikues për URL (SEO-friendly).                        |
| **RichContent** | `String`          | Teksti i plotë ose HTML i përmbajtjes.                      |
| **AssetUrl**    | `String` (URL)    | URL për imazhet ose mediat e lidhura.                       |
| **Status**      | `String`          | Statusi i jetës së përmbajtjes (p.sh., "New", "Published"). |
| **CreatedOn**   | `DateTime`        | Data dhe koha e krijimit.                                   |
| **UpdatedOn**   | `DateTime`        | Data dhe koha e përditësimit të fundit.                     |
| **UserId**      | `Guid`            | Identifikuesi i përdoruesit që krijoi përmbajtjen.          |
| **CategoryId**  | `Guid` (FK)       | Referencë për kategorinë (Lidhje One-to-Many).              |

#### 2. Tabela `Category`

_Strukturimi dhe grupimi i përmbajtjes._

| Kolona          | Tipi i të Dhënave  | Përshkrimi                         |
| :-------------- | :----------------- | :--------------------------------- |
| **CategoryId**  | `Guid` (PK)        | Çelësi primar unik.                |
| **Name**        | `String` (Max 100) | Emri i kategorisë (i detyrueshëm). |
| **Description** | `String`           | Përshkrim opsional për kategorinë. |

#### 3. Tabela `Tag`

_Etiketat për klasifikim horizontal dhe filtrim._

| Kolona    | Tipi i të Dhënave | Përshkrimi                       |
| :-------- | :---------------- | :------------------------------- |
| **TagId** | `Guid` (PK)       | Çelësi primar unik.              |
| **Name**  | `String` (Max 50) | Emri i etiketës (i detyrueshëm). |

_Shënim: Lidhja Many-to-Many midis `Content` dhe `Tag` realizohet përmes një tabele të ndërmjetme (join table) të menaxhuar automatikisht._

#### 4. Tabela `ApiKey`

_Menaxhimi i sigurisë dhe aksesit për klientët e jashtëm._

| Kolona          | Tipi i të Dhënave | Përshkrimi                                        |
| :-------------- | :---------------- | :------------------------------------------------ |
| **Id**          | `Guid` (PK)       | Identifikuesi unik i çelësit.                     |
| **UserId**      | `Guid`            | ID e përdoruesit që zotëron çelësin.              |
| **Key**         | `String`          | Vlera aktuale e çelësit (string i koduar).        |
| **Description** | `String`          | Përshkrim për qëllimin e çelësit.                 |
| **IsActive**    | `Boolean`         | Përcakton nëse çelësi është aktiv apo i revokuar. |
| **CreatedAt**   | `DateTime`        | Data e gjenerimit të çelësit.                     |

#### 5. Relacionet Kryesore (ER)

- **Content ➡ Category**: Një përmbajtje i përket një kategorie (One-to-Many).
- **Content ➡ Tags**: Një përmbajtje mund të ketë shumë etiketa dhe një etiketë lidhet me shumë përmbajtje (Many-to-Many).

## ✅ Përputhshmëria me Kërkesat Teknike

Ky projekt është zhvilluar në përputhje me standardet moderne të shërbimeve web dhe plotëson kërkesat teknike kryesore si më poshtë:

### 1. Arkitektura e Sistemit

- **Dizajni Modular**: Është adoptuar **Clean Architecture** (Domain, Application, Infrastructure, API layers) duke siguruar ndarje të përgjegjësive dhe mirëmbajtje të lehtë.
- **RESTful API**: Metoda standarde HTTP, URI të bazuara në burime (resource-based), dhe negocim të përmbajtjes JSON.
- **Stateless**: Ndërveprimi është plotësisht stateless, duke u mbështetur në JWT dhe API tokens në vend të sesioneve në anën e serverit.

### 2. Siguria

- **Autentifikimi**:
  - Implementimi i **JWT (JSON Web Token)** për akses të sigurt administrativ (Skema Bearer).
  - Mekanizmi i **API Keys** (`X-Api-Key`) për autentifikimin e klientëve të jashtëm publikë.
- **Mbrojtja**: Pipeline i Middleware (`JwtValidationMiddleware`) siguron vlefshmërinë e kërkesave para përpunimit.

### 3. Performanca dhe Shkallëzueshmëria

- **Aksesi i Optimizuar i të Dhënave**: Mbështetje për **Pagination** (`page`, `pageSize`) për menaxhimin e ngarkesës.
- **Mbështetje për Elasticsearch**: Integrim (`withElastic`) për kërkim me performancë të lartë.
- **Caching**: Përdorimi i **Redis** për të ulur kohën e përgjigjes.
- **Dizajni Asinkron**: Përdorim i plotë i modeleve `async/await` të .NET për operacione jo-bllokuese.

### 4. Dokumentimi i API

- **OpenAPI 3.0**: Specifikim gjithëpërfshirës përmes **Swagger**, që detajon të gjitha endpoint-et, skemat dhe kërkesat e sigurisë.
- **Ndërfaqe Interaktive**: Swagger UI e aktivizuar për testim të drejtpërdrejtë dhe verifikim vizual të sipërfaqes së API.

### 5. Standardet dhe Teknologjitë

- **Tech Stack**: Ndërtuar mbi **.NET 8** (C#) dhe **Entity Framework Core**.
- **Cilësia e Kodit**: I përmbahet **parimeve SOLID** dhe praktikave të Clean Code.
- **Mjedisi**: Mbështetje për **Docker** e përfshirë për zhvillim të kontejnerizuar dhe konsistencë në vendosje (deploy).
