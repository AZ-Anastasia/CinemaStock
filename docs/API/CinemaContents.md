# Управление киноконтентом (CinemaContent API)

### 1. Получение списка киноконтента (GET /api/cinema-content/get-cinemas)

```
sequenceDiagram
    autonumber
    actor Client as Клиент (UI)
    participant Controller as CinemaContentController
    participant Service as CinemaContentService
    participant DB as СУБД PostgreSQL (Docker)

    Note over Client,Controller: Маршрут: GET /api/cinema-content/get-cinemas<br/>Параметры: ?filter=str&pageNumber=1&pageSize=15
    Client->>Controller: HTTP GET /api/cinema-content/get-cinemas?filter=...&pageNumber=...&pageSize=...
    
    Note over Controller: Блок обработки try-catch
    Controller->>Service: GetCinemaContentsListAsync(filter, pageNumber, pageSize)
    
    alt Критический сбой базы данных (Перехват в catch)
        DB--XService: Выброс Exception (Сбой БД)
        Service--XController: Проброс Exception в контроллер
        Note over Controller: Перехват в catch + _logger.LogError()
        Controller-->>Client: HTTP 500 Internal Server Error ("Внутренняя ошибка сервера при чтении записей.")
    end

    %% Основной успешный путь (Happy Path)
    Service->>DB: Запрос к таблице MediaContents с пагинацией (Skip/Take)
    DB-->>Service: Возврат выборки сущностей
    Service-->>Controller: Возврат списка CinemaContentResponse
    Controller-->>Client: HTTP 200 OK + JSON [ { "id": "guid", "localTitle": "string", "originalTitle": "string", ... } ]
```

### 2. Добавление киноконтента (POST /api/cinema-content/create-cinema)

```
sequenceDiagram
    autonumber
    actor Client as Администратор (UI)
    participant Controller as CinemaContentController
    participant FS as Файловая система (Директория картинок)
    participant Service as CinemaContentService
    participant DB as СУБД PostgreSQL (Docker)

    Note over Client,Controller: Маршрут: POST /api/cinema-content/create-cinema<br/>Данные: Form-Data (включая Picture в Base64)
    Client->>Controller: HTTP POST /api/cinema-content/create-cinema + Form Data
    
    alt Валидация: Тело запроса пустое (Request == null)
        Controller-->>Client: HTTP 400 BadRequest ("Тело запроса не может быть пустым.")
    end

    Note over Controller: Блок обработки try-catch

    opt Обработка изображения (Если Picture содержит "data:image/")
        Note over Controller: Декодирование Mime-типа и конвертация Base64 в байты
        
        alt Защита DoS: Размер файла > 5 МБ
            Controller-->>Client: HTTP 400 BadRequest ("Файл слишком большой.")
        end
        
        Note over Controller: Генерация безопасного GUID имени файла
        Controller->>FS: Проверка/Создание директории + Запись файла (FileStream)
        FS-->>Controller: Успешное сохранение на диск
        Note over Controller: Замена Picture на относительный путь хранения
    end

    Controller->>Service: CreateCinemaContentAsync(request)
    
    alt Бизнес-ошибка: Невалидные аргументы в сервисе
        Service--XController: Выброс ArgumentException
        Note over Controller: Перехват catch (ArgumentException)
        Controller-->>Client: HTTP 400 BadRequest + JSON { "message": "Текст ошибки" }
    end

    alt Критический сбой: Любое другое исключение (БД / Инфраструктура)
        Service--XController: Выброс общего Exception
        Note over Controller: Перехват catch (Exception) + StatusCode(500, ...)
        Controller-->>Client: HTTP 500 Internal Server Error ("Внутренняя ошибка сервера при записи...")
    end

    %% Основной успешный путь (Happy Path)
    Service->>DB: Сохранение киноконтента в БД (через EF Core)
    DB-->>Service: Успешная запись сущности
    Service-->>Controller: Возврат createdMedia
    Controller-->>Client: HTTP 201 Created + JSON переданного объекта request
```

### 3. Редактирование киноконтента (PUT /api/cinema-content/{id})

```
sequenceDiagram
    autonumber
    actor Client as Администратор (UI)
    participant Controller as CinemaContentController
    participant FS as Файловая система (Директория картинок)
    participant Service as CinemaContentService
    participant DB as СУБД PostgreSQL (Docker)

    Note over Client,Controller: Маршрут: PUT /api/cinema-content/{id}<br/>Доступ: Identity.Application (Admin)<br/>Данные: Form-Data + ID в пути
    Client->>Controller: HTTP PUT /api/cinema-content/{id} + Form Data
    
    alt Валидация: Тело запроса пустое (Request == null)
        Controller-->>Client: HTTP 400 BadRequest ("Тело запроса не может быть пустым.")
    end

    Note over Controller: Блок обработки try-catch

    opt Обработка нового изображения (Если Picture содержит "data:image/")
        Note over Controller: Декодирование Mime-типа и конвертация Base64 в байты
        
        alt Защита DoS: Размер файла > 5 МБ
            Controller-->>Client: HTTP 400 BadRequest ("Файл слишком большой.")
        end
        
        Note over Controller: Генерация безопасного GUID имени файла
        Controller->>FS: Проверка/Создание директории + Запись нового файла (FileStream)
        FS-->>Controller: Успешное сохранение на диск
        Note over Controller: Замена Picture на относительный путь хранения нового файла
    end

    Controller->>Service: UpdateCinemaContentAsync(id, request)
    
    alt Бизнес-ошибка: Киноконтент с указанным ID не найден в БД
        Service-->>Controller: Возврат null
        Controller-->>Client: HTTP 404 NotFound ("Киноконтент не найден для обновления.")
    end

    alt Бизнес-ошибка: Невалидные аргументы (например, студия)
        Service--XController: Выброс ArgumentException
        Note over Controller: Перехват catch (ArgumentException)
        Controller-->>Client: HTTP 400 BadRequest + JSON { "message": "Текст ошибки" }
    end

    alt Критический сбой: Любое другое исключение (БД / Инфраструктура)
        Service--XController: Выброс общего Exception
        Note over Controller: Перехват catch (Exception) + StatusCode(500)
        Controller-->>Client: HTTP 500 Internal Server Error ("Внутренняя ошибка сервера...")
    end

    %% Основной успешный путь (Happy Path)
    Service->>DB: Поиск записи + Обновление полей и связей (EF Core)
    DB-->>Service: Успешное сохранение изменений (SaveChangesAsync)
    Service-->>Controller: Возврат обновленного объекта CinemaContentResponse
    Controller-->>Client: HTTP 200 OK + JSON объекта CinemaContentResponse
```

### 4. Удаление киноконтента (DELTE /api/cinema-content/{id})

```
sequenceDiagram
    autonumber
    actor Client as Администратор (UI)
    participant Controller as CinemaContentController
    participant Service as CinemaContentService
    participant DB as СУБД PostgreSQL (Docker)

    Note over Client,Controller: Маршрут: DELETE /api/cinema-content/{id}<br/>Доступ: Identity.Application (Admin)<br/>Данные: ID в пути запроса
    Client->>Controller: HTTP DELETE /api/cinema-content/{id}
    
    Note over Controller: Блок обработки try-catch
    Controller->>Service: DeleteCinemaContentAsync(id)
    
    alt Бизнес-логика: Запись с указанным ID отсутствует в системе
        Service-->>Controller: Возврат флага false
        Note over Controller: Вызов _logger.LogWarning(...)
        Controller-->>Client: HTTP 404 NotFound ("Киноконтент не найден.")
    end

    alt Критический сбой: Исключение на уровне инфраструктуры или базы
        Service--XController: Выброс общего Exception
        Note over Controller: Перехват в catch + _logger.LogError()
        Controller-->>Client: HTTP 500 Internal Server Error ("Внутренняя ошибка сервера...")
    end

    %% Основной успешный путь (Happy Path)
    Service->>DB: Поиск сущности + Удаление записи (через EF Core)
    DB-->>Service: Успешное удаление из БД и сохранение
    Service-->>Controller: Возврат флага успешности true
    Note over Controller: Вызов _logger.LogInformation(...)
    Controller-->>Client: HTTP 200 OK
```