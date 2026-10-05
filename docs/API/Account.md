# Управление аккаунтом (Account API)

### 1. Получение текущего пользователя (GET /api/account/current)

```mermaid
sequenceDiagram
    autonumber
    actor UI as Компонент Blazor (Профиль/Шапка)
    participant Client as AccountClient
    participant Base as ApiHttpClient (Base)
    participant API as Бэкенд API (/api/account/current)

    UI->>Client: GetCurrentUserAsync()
    Client->>Base: GetAsync-запрос
    Base->>API: HTTP GET /api/account/current
    
    alt Сетевой сбой / Ошибка сервера / Не авторизован (catch)
        API--XBase: Сетевая ошибка или HTTP Error (401/500)
        Base--XClient: Проброс Exception
        Note over Client: Перехват в catch
        Client-->>UI: Возврат null
    end

    %% Основной успешный путь (Happy Path)
    API-->>Base: HTTP 200 OK + JSON данных пользователя
    Base-->>Client: Десериализованный UserInfoResponse
    Client-->>UI: JSON { "id": "string", "username": "string", "email": "string", "roles": [...] }
```

### 2. Авторизация пользователя (POST /api/account/login)

```mermaid
sequenceDiagram
    autonumber
    actor UI as Форма авторизации (UI)
    participant Client as AccountClient
    participant Base as ApiHttpClient (Base)
    participant API as Бэкенд API (/api/account/login)

    UI->>Client: LoginAsync(email, password)
    Note over Client: Формирование DTO запроса (Email, Password)
    Client->>Base: PostAsync-запрос
    Base->>API: HTTP POST /api/account/login + JSON
    
    alt Ошибка HTTP запроса (catch HttpRequestException)
        API--XBase: Ошибка протокола / Неверный логин или пароль
        Base--XClient: Выброс HttpRequestException
        Client-->>UI: Возврат JSON { "isSuccess": false, "errors": ["Текст ошибки"] }
    end

    alt Глобальный сбой связи (catch Exception)
        Base--XClient: Любое другое исключение
        Client-->>UI: Возврат JSON { "isSuccess": false, "errors": ["Ошибка связи с сервером..."] }
    end

    %% Основной успешный путь (Happy Path)
    API-->>Base: HTTP 200 OK (Установка сессии/кук)
    Base-->>Client: Успешное завершение таски
    Client-->>UI: Возврат JSON { "isSuccess": true, "errors": null }
```

### 3. Регистрация пользователя (POST /api/account/registration)

```mermaid
sequenceDiagram
    autonumber
    actor UI as Форма регистрации (UI)
    participant Client as AccountClient
    participant Base as ApiHttpClient (Base)
    participant API as Бэкенд API (/api/account/register)

    UI->>Client: RegisterAsync(username, email, password)
    Note over Client: Формирование DTO запроса (Username, Email, Password)
    Client->>Base: PostAsync-запрос
    Base->>API: HTTP POST /api/account/register + JSON
    
    alt Ошибка HTTP запроса (catch HttpRequestException)
        API--XBase: Ошибка валидации / Email уже занят
        Base--XClient: Выброс HttpRequestException
        Client-->>UI: Возврат JSON { "isSuccess": false, "errors": ["Текст ошибки"] }
    end

    alt Глобальный сбой связи (catch Exception)
        Base--XClient: Любое другое исключение
        Client-->>UI: Возврат JSON { "isSuccess": false, "errors": ["Ошибка связи с сервером..."] }
    end

    %% Основной успешный путь (Happy Path)
    API-->>Base: HTTP 200 OK (или 201 Created)
    Base-->>Client: Успешное завершение таски
    Client-->>UI: Возврат JSON { "isSuccess": true, "errors": [] }
```

### 4. Выход из аккаунта (POST /api/account/logout)

```mermaid
sequenceDiagram
    autonumber
    actor UI as Кнопка выхода (UI)
    participant Client as AccountClient
    participant Base as ApiHttpClient (Base)
    participant API as Бэкенд API (/api/account/logout)

    UI->>Client: LogoutAsync()
    Client->>Base: PostAsync-запрос
    Base->>API: HTTP POST /api/account/logout
    
    %% Локальной обработки ошибок нет, метод возвращает пустую Task
    API-->>Base: HTTP 200 OK (Удаление сессии/кук)
    Base-->>Client: Успешное завершение работы
    Client-->>UI: Возврат управления (Task завершена)
```