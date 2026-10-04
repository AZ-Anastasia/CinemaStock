# Управление жанрами (Genres API)

### 1. Получение списка жанров (GET /api/genres/list)

```mermaid
sequenceDiagram
    autonumber
    actor Client as Клиент (UI)
    participant Controller as GenresController
    participant Service as GenreService
    participant DB as СУБД PostgreSQL (Docker)

    Note over Client,Controller: Маршрут: /api/genres/list (Доступ анонимный)<br/>Метод: GetGenresList()
    Client->>Controller: HTTP GET /api/genres/list
    
    Note over Controller: Блок обработки try-catch
    Controller->>Service: GetGenresListAsync()
    
    alt Возникло исключение (Сбой Docker / сети / БД)
        DB--XService: Выброс Exception
        Service--XController: Проброс Exception в контроллер
        Note over Controller: Логирование критической ошибки
        Controller-->>Client: HTTP 500 Internal Server Error
    end

    %% Основной успешный путь (Happy Path) продолжается, если alt не сработал
    Service->>DB: Запрос к таблице Genres (через EF Core)
    DB-->>Service: Возврат записей из БД
    Service-->>Controller: Возврат коллекции DTO жанров
    Controller-->>Client: HTTP 200 OK + JSON [ { "id": "guid", "name": "string" }, ... ]
```

### 2. Добавление нового жанра (POST /api/genres)

```mermaid
sequenceDiagram
    autonumber
    actor Client as Администратор (UI)
    participant Auth as Auth Middleware (ASP.NET)
    participant Controller as GenresController
    participant Service as GenreService
    participant DB as СУБД PostgreSQL (Docker)

    Note over Client,Auth: Маршрут: POST /api/genres<br/>Доступ только для роли Admin
    Client->>Auth: HTTP POST /api/genres + JSON { "name": "string" }
    
    alt Проверка прав: Сессия невалидна или пользователь не Admin
        Auth-->>Client: HTTP 401 Unauthorized / 403 Forbidden
    end

    Auth->>Controller: Передача управления в контроллер
    
    alt Валидация: Имя жанра не заполнено
        Controller-->>Client: HTTP 400 BadRequest ("Название жанра не может быть пустым.")
    end

    Note over Controller: Блок обработки try-catch
    Controller->>Service: Запрос на создание жанра
    
    alt Критический сбой: Исключение на уровне инфраструктуры
        Service--XController: Проброс Exception в контроллер
        Note over Controller: Логирование критической ошибки
        Controller-->>Client: HTTP 500 Internal Server Error
    end

    Service->>DB: Проверка уникальности + Сохранение в БД
    DB-->>Service: Успешная запись сущности
    Service-->>Controller: Возврат созданного жанра
    
    alt Бизнес-логика: Жанр с таким именем уже существует
        Controller-->>Client: HTTP 400 BadRequest ("Такой жанр уже существует в БД.")
    end

    %% Основной успешный путь (Happy Path)
    Controller-->>Client: HTTP 200 OK + JSON { "id": "guid", "name": "string" }
```

### 3. Редактирование жанра (PUT /api/genres/{id})

```mermaid
sequenceDiagram
    autonumber
    actor Client as Администратор (UI)
    participant Auth as Auth Middleware (ASP.NET)
    participant Controller as GenresController
    participant Service as GenreService
    participant DB as СУБД PostgreSQL (Docker)

    Note over Client,Auth: Маршрут: PUT /api/genres/{id}<br/>Доступ только для роли Admin
    Client->>Auth: HTTP PUT /api/genres/{id} + JSON { "name": "string" }
    
    alt Проверка прав: Сессия невалидна или пользователь не Admin
        Auth-->>Client: HTTP 401 Unauthorized / 403 Forbidden
    end

    Auth->>Controller: Передача управления в контроллер
    
    alt Валидация: Новое имя жанра не заполнено
        Controller-->>Client: HTTP 400 BadRequest ("Название жанра не может быть пустым.")
    end

    Note over Controller: Блок обработки try-catch
    Controller->>Service: Запрос на обновление жанра (id, данные)
    
    alt Критический сбой: Исключение на уровне инфраструктуры
        Service--XController: Проброс Exception в контроллер
        Note over Controller: Логирование критической ошибки
        Controller-->>Client: HTTP 500 Internal Server Error
    end

    Service->>DB: Проверка существования ID + Проверка уникальности имени + Обновление в БД
    DB-->>Service: Успешное изменение записи
    Service-->>Controller: Возврат обновленного жанра
    
    alt Бизнес-логика: Жанр не найден или имя уже занято
        Controller-->>Client: HTTP 400 BadRequest ("Такой жанр не найден или уже существует в БД.")
    end

    %% Основной успешный путь (Happy Path)
    Controller-->>Client: HTTP 200 OK + JSON { "id": "guid", "name": "string" }
```

### 4. Удаление жанра (DELETE/api/genres/{id})

```mermaid
sequenceDiagram
    autonumber
    actor Client as Администратор (UI)
    participant Auth as Auth Middleware (ASP.NET)
    participant Controller as GenresController
    participant Service as GenreService
    participant DB as СУБД PostgreSQL (Docker)

    Note over Client,Auth: Маршрут: DELETE /api/genres/{id}<br/>Доступ только для роли Admin
    Client->>Auth: HTTP DELETE /api/genres/{id}
    
    alt Проверка прав: Сессия невалидна или пользователь не Admin
        Auth-->>Client: HTTP 401 Unauthorized / 403 Forbidden
    end

    Auth->>Controller: Передача управления в контроллер
    
    Note over Controller: Блок обработки try-catch
    Controller->>Service: Запрос на удаление жанра по id
    
    alt Критический сбой: Исключение на уровне инфраструктуры
        Service--XController: Проброс Exception в контроллер
        Note over Controller: Логирование критической ошибки
        Controller-->>Client: HTTP 500 Internal Server Error
    end

    Service->>DB: Проверка существования записи + Удаление из таблицы Genres
    DB-->>Service: Успешное удаление из БД
    Service-->>Controller: Возврат флага успешности (true / false)
    
    alt Бизнес-логика: Жанр с указанным ID не найден в системе
        Controller-->>Client: HTTP 404 NotFound ("Жанр не найден.")
    end
```