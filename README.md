# CinemaStock 🎬

CinemaStock — это веб-приложение для учета киноконтента (в дальнейшем возможен учет не только киноконтента).

Проект построен на базе раздельной архитектуры (Decoupled Architecture): серверная часть представляет собой защищенный REST API, а клиентская — интерактивное SPA-приложение на Blazor WebAssembly.

---

## 🛠 Стек технологий

### Back-end (API)
* **.NET 9.0** (ASP.NET Core Web API)
* **Entity Framework Core** — для работы с базой данных
* **PostgreSQL** — запуск через Docker
* **JWT Authentication** — для безопасной авторизации пользователей

### Front-end (Client)
* **Blazor WebAssembly (WASM)** — интерактивный UI на C# без JavaScript
* **Bootstrap 5** — для верстки и адаптивного дизайна
* **HTTP Client** — для взаимодействия с API

---

## 📂 Структура репозитория

Репозиторий разделен на три основных независимых проекта:

* `/CinemaStock.API` — Серверная часть, управление данными, бизнес-логика и авторизация.
* `/CinemaStock.Client` — Клиентское WebAssembly-приложение, работающее прямо в браузере.
* `/CinemaStock.Shared` — Общие файлы, требуемые в .API и .Client.

---

## 🚀 Быстрый запуск локально

Для запуска проекта вам понадобятся установленный [.NET SDK](https://microsoft.com) и запущенная СУБД.

### 1. Развертывание БД (PostgreSQL)
В корне проекта .API находится файл `docker-compose.yml`, который поднимает контейнер с PostgreSQL.

```bash
docker compose up -d
```

### 2. Запуск серверной части (API)
В проекте есть файлы `appsettings.json` и `appsettings.Development.json`. Там есть:
```
"DefaultConnection": ""
```
По умолчанию, задана строка подключения через `dotnet user-secrets` (просмотреть список всех можно через `dotnet user-secrets list`). Можно задать прямо в файле, напротив `DefaultConnection`:
```cs
Host=localhost;Port=[Порт_БД_из_Docker(5432)];Database=[Название_БД];Username=[Имя_пользователя];Password=[Пароль]
```
*Примечание 1*: `[]` писать не нужно, это сделано, чтобы разграничить данные для лучшего понимания. \
*Примечание 2*: `user-secrets` также прописывается в `CinemaStock.API.csproj`. На случай возникновения ошибки или предупреждения следует удалить старый `UserSecretsId`, `Guid` которого не содержится в системе локально.

Перейдите в папку сервера, настройте строку подключения в `appsettings.json`, примените миграции и запустите проект:

```bash
cd CinemaStock.API
dotnet ef database update  # Применение миграций к БД в Docker
dotnet run
```
По умолчанию API будет доступен по адресу из терминала (или проверьте ваш `launchSettings.json`).

Данные для запуска и строки подключения хранятся в корне проекта `CinemaStock.API` в файле `.env`:
```
POSTGRES_USER=[Имя_пользователя]
POSTGRES_PASSWORD=[Пароль]
POSTGRES_DB=[Название_БД]
LANG=en_US.utf8
```

*Примечание*: запятые ставить не надо.

***Посмотреть и поработать с БД:***
```bash
docker exec -it postgres-cinemastock psql -U [Имя_пользователя] -d [Название_БД]
```
*Примечание*: `postgres-cinemastock` - название контейнера из `docker-compose.yml`:
```yml
container_name: postgres-cinemastock
```

### 3. Запуск клиентской части (Blazor Client)
В отдельном окне терминала перейдите в папку клиента и запустите веб-приложение:

```bash
cd CinemaStock.Client
dotnet run
```
Клиент откроется в браузере по адресу, отображенному в терминале.

---

## ⚙️ Настройка окружения (Environment)

* **CORS:** В API настроена политика CORS, разрешающая запросы от локального Blazor-клиента.

---