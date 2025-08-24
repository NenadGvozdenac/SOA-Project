# Implementacija: Korisnici mogu da čitaju blogove samo onih korisnika koje su zapratili

## Pregled implementacije

Implementirana je funkcionalnost koja omogućava korisnicima da čitaju blogove samo onih korisnika koje su zapratili. Implementacija je podeljena između dva servisa:

### 1. followings_service (Glavna logika)

#### Novi endpoint:
- **GET** `/api/followers/blogs` - Vraća blogove od svih korisnika koje je trenutni korisnik zapraćivao

#### Implementirane komponente:

1. **GetBlogsFromFollowedUsers Feature**
   - `GetBlogsFromFollowedUsersQuery.cs` - Query za MediatR
   - `GetBlogsFromFollowedUsersDTOs.cs` - DTO definicije
   - `GetBlogsFromFollowedUsersHandler.cs` - Handler koji:
     - Pronalazi sve korisnike koje trenutni korisnik prati (Neo4j)
     - Poziva blogs_service da dobije blogove od tih korisnika
     - Poziva stakeholders_service da dobije dodatne informacije o autorima
     - Kombinuje podatke i vraća kompletnu listu blogova

2. **BlogsServiceClient**
   - `IBlogsServiceClient.cs` - Interface za komunikaciju sa blogs_service
   - `BlogsServiceClient.cs` - HTTP client implementacija
   - `ApiResponse.cs` - Response wrapper

3. **Konfiguracija**
   - Dodato u `ApplicationStartup.cs` za DI registraciju
   - Dodato u `appsettings.json` i `appsettings.Development.json` za service URL

### 2. blogs_service (Pomoćni endpoint)

#### Novi endpoint:
- **POST** `/api/internal/blogs/by-authors` - Prima listu author ID-jeva i vraća sve blogove od tih autora
- **NAPOMENA**: Kreiran je novi `InternalBlogsController` bez `[Authorize]` atributa za interne pozive između servisa

#### Implementirane komponente:

1. **GetBlogsByAuthors Feature**
   - `GetBlogsByAuthorsQuery.cs` - Query za MediatR
   - `GetBlogsByAuthorsDTOs.cs` - DTO definicije
   - `GetBlogsByAuthorsHandler.cs` - Handler koji filtrira blogove po author ID-jima

2. **InternalBlogsController**
   - Novi controller bez autorizacije za interne API pozive
   - Endpoint `/api/internal/blogs/by-authors`

## Tok izvršavanja

1. Korisnik poziva `GET /api/followers/blogs` na followings_service
2. followings_service poziva Neo4j da dobije listu praćenih korisnika
3. followings_service poziva blogs_service da dobije blogove od tih korisnika
4. followings_service poziva stakeholders_service da dobije detalje o autorima
5. followings_service kombinuje podatke i vraća kompletnu listu

## Konfiguracija za Docker

Dodano je u appsettings.json:
```json
{
  "Services": {
    "Stakeholders": {
      "Url": "http://stakeholders_service:8080"
    },
    "Blogs": {
      "Url": "http://blogs_service:8080"
    }
  }
}
```

## Testiranje

Oba servisa se uspešno build-uju bez grešaka. Za testiranje treba pokrenuti docker-compose i pozvati:

```
GET /api/followers/blogs
Authorization: Bearer <token>
```

Očekuje se response sa listom blogova od praćenih korisnika.
