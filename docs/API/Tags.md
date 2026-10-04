# Управление тегами (Tags API)

### 1. Получение списка тегов (GET /api/tags/list)

```mermaid
sequenceDiagram
    autonumber
    actor Client as Клиент (UI)
    participant Controller as TagsController
    participant Service as TagService
    participant DB as СУБД PostgreSQL (Docker)

    Note over Client,Controller: Маршрут: /api/tags/list (Доступ анонимный)<br/>Метод: GetTagsList()
    Client->>Controller: HTTP GET /api/tags/list
    
    Note over Controller: Блок обработки try-catch
    Controller->>Service: GetTagsListAsync()
    
    alt Возникло исключение (Сбой Docker / сети / БД)
        DB--XService: Выброс Exception
        Service--XController: Проброс Exception в контроллер
        Note over Controller: Перехват в catch + _logger.LogError()
        Controller-->>Client: HTTP 500 Internal Server Error
    end

    %% Основной успешный путь (Happy Path)
    Service->>DB: Запрос к таблице Tags (через EF Core)
    DB-->>Service: Возврат записей из БД
    Service-->>Controller: Возврат коллекции DTO тегов
    Controller-->>Client: HTTP 200 OK + JSON [ { "id": "guid", "name": "string" }, ... ]
```

### 2. Добавление нового тега (POST /api/tags)

```mermaid
sequenceDiagram
    autonumber
    actor Client as Администратор (UI)
    participant Auth as Auth Middleware (ASP.NET)
    participant Controller as TagsController
    participant Service as TagService
    participant DB as СУБД PostgreSQL (Docker)

    Note over Client,Auth: Маршрут: POST /api/tags<br/>Доступ только для роли Admin
    Client->>Auth: HTTP POST /api/tags + JSON { "name": "string" }
    
    alt Проверка прав: Сессия невалидна или пользователь не Admin
        Auth-->>Client: HTTP 401 Unauthorized / 403 Forbidden
    end

    Auth->>Controller: Передача управления в контроллер
    
    alt Валидация: Имя тега не заполнено
        Controller-->>Client: HTTP 400 BadRequest ("Название тега не может быть пустым.")
    end

    Note over Controller: Блок обработки try-catch
    Controller->>Service: Запрос на создание тега
    
    alt Критический сбой: Исключение на уровне инфраструктуры
        Service--XController: Проброс Exception в контроллер
        Note over Controller: Логирование критической ошибки
        Controller-->>Client: HTTP 500 Internal Server Error
    end

    Service->>DB: Проверка уникальности + Сохранение в БД
    DB-->>Service: Успешная запись сущности
    Service-->>Controller: Возврат созданного тега
    
    alt Бизнес-логика: Тег с таким именем уже существует
        Controller-->>Client: HTTP 400 BadRequest ("Такой тег уже существует в БД.")
    end

    %% Основной успешный путь (Happy Path)
    Controller-->>Client: HTTP 200 OK + JSON { "id": "guid", "name": "string" }
```

### 3. Редактирование тега (PUT /api/tags/{id})

```mermaid
sequenceDiagram
    autonumber
    actor Client as Администратор (UI)
    participant Auth as Auth Middleware (ASP.NET)
    participant Controller as TagsController
    participant Service as TagService
    participant DB as СУБД PostgreSQL (Docker)

    Note over Client,Auth: Маршрут: PUT /api/tags/{id}<br/>Доступ только для роли Admin
    Client->>Auth: HTTP PUT /api/tags/{id} + JSON { "name": "string" }
    
    alt Проверка прав: Сессия невалидна или пользователь не Admin
        Auth-->>Client: HTTP 401 Unauthorized / 403 Forbidden
    end

    Auth->>Controller: Передача управления в контроллер
    
    alt Валидация: Новое имя тега не заполнено
        Controller-->>Client: HTTP 400 BadRequest ("Название тега не может быть пустым.")
    end

    Note over Controller: Блок обработки try-catch
    Controller->>Service: Запрос на обновление тега (id, данные)
    
    alt Критический сбой: Исключение на уровне инфраструктуры
        Service--XController: Проброс Exception в контроллер
        Note over Controller: Логирование критической ошибки
        Controller-->>Client: HTTP 500 Internal Server Error
    end

    Service->>DB: Проверка существования ID + Проверка уникальности имени + Обновление в БД
    DB-->>Service: Успешное изменение записи
    Service-->>Controller: Возврат обновленного тега
    
    alt Бизнес-логика: Тег не найден или имя уже занято
        Controller-->>Client: HTTP 400 BadRequest ("Такой тег не найден или уже существует в БД.")
    end

    %% Основной успешный путь (Happy Path)
    Controller-->>Client: HTTP 200 OK + JSON { "id": "guid", "name": "string" }
```

### 4. Удаление тега (DELETE/api/tags/{id})

```mermaid
sequenceDiagram
    autonumber
    actor Client as Администратор (UI)
    participant Auth as Auth Middleware (ASP.NET)
    participant Controller as TagsController
    participant Service as TagService
    participant DB as СУБД PostgreSQL (Docker)

    Note over Client,Auth: Маршрут: DELETE /api/tags/{id}<br/>Доступ только для роли Admin
    Client->>Auth: HTTP DELETE /api/tags/{id}
    
    alt Проверка прав: Сессия невалидна или пользователь не Admin
        Auth-->>Client: HTTP 401 Unauthorized / 403 Forbidden
    end

    Auth->>Controller: Передача управления в контроллер
    
    Note over Controller: Блок обработки try-catch
    Controller->>Service: Запрос на удаление тега по id
    
    alt Критический сбой: Исключение на уровне инфраструктуры
        Service--XController: Проброс Exception в контроллер
        Note over Controller: Логирование критической ошибки
        Controller-->>Client: HTTP 500 Internal Server Error
    end

    Service->>DB: Проверка существования записи + Удаление из таблицы Tags
    DB-->>Service: Успешное удаление из БД
    Service-->>Controller: Возврат флага успешности (true / false)
    
    alt Бизнес-логика: Тег с указанным ID не найден в системе
        Controller-->>Client: HTTP 404 NotFound ("Тег не найден.")
    end

    %% Основной успешный путь (Happy Path)
    Controller-->>Client: HTTP 200 OK
```