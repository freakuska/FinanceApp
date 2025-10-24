# FinanceApp - Приложение для управления финансами

## Описание проекта

FinanceApp - это веб-приложение для управления личными финансами с функциями учета доходов, расходов, управления тегами и просмотра статистики.

## Архитектура

Проект состоит из трех основных компонентов:

1. **FinanceApp.Api** - REST API на ASP.NET Core с аутентификацией через JWT в httpOnly cookies
2. **FinanceApp.Web** - Веб-интерфейс на ASP.NET Core MVC + JavaScript
3. **FinanceApp.Infrastructure** - Слой данных с Entity Framework Core и PostgreSQL

## Реализованные функции

### API (FinanceApp.Api)

✅ Аутентификация через httpOnly cookies
✅ CORS настроен для работы с Web проектом
✅ JWT токены (Access и Refresh) хранятся в безопасных httpOnly cookies
✅ Эндпоинты для управления:
  - Аутентификацией (Login, Register, Logout, Refresh)
  - Операциями (CRUD, статистика)
  - Тегами (CRUD, поиск, популярные)
  - Отчетами

### Web Interface (FinanceApp.Web)

✅ **Страница входа** (`/Account/Login`)
  - Форма входа с валидацией
  - Обработка ошибок
  - Автоматическое перенаправление после входа

✅ **Страница регистрации** (`/Account/Register`)
  - Форма регистрации с валидацией полей
  - Проверка совпадения паролей
  - Согласие с условиями

✅ **Дашборд** (`/Dashboard`)
  - Виджеты статистики (доходы, расходы, баланс)
  - Форма быстрого добавления операций
  - Таблица последних операций
  - Удаление операций

✅ **Страница управления тегами** (`/Tags`)
  - Просмотр тегов по типам (Доходы, Расходы, Переводы)
  - Создание новых тегов с цветами и иконками
  - Удаление тегов

### JavaScript модули

✅ **api-client.js** - Базовый клиент для работы с API
✅ **auth.js** - Модуль аутентификации
✅ **operations.js** - Модуль работы с операциями
✅ **tags.js** - Модуль работы с тегами
✅ **dashboard.js** - Логика дашборда
✅ **tags-page.js** - Логика страницы тегов

## Запуск проекта

### Предварительные требования

- .NET 8.0 SDK
- PostgreSQL 12+
- Visual Studio 2022 или Rider (опционально)

### Шаг 1: Настройка базы данных

1. Убедитесь, что PostgreSQL запущен
2. Строка подключения находится в `FinanceApp.Api/appsettings.Development.json`
3. Примените миграции:

```bash
cd FinanceApp.Infrastructure
dotnet ef database update --startup-project ../FinanceApp.Api
```

### Шаг 2: Запуск API

```bash
cd FinanceApp.Api
dotnet run
```

API будет доступен по адресу: `https://localhost:7051`
Swagger UI: `https://localhost:7051/swagger`

### Шаг 3: Запуск Web интерфейса

В новом терминале:

```bash
cd FinanceApp.Web
dotnet run
```

Web приложение будет доступно по адресу: `https://localhost:7073`

### Шаг 4: Проверка портов

Убедитесь, что в `FinanceApp.Api/Program.cs` и `FinanceApp.Web/appsettings.json` указаны правильные порты:

- **API**: `https://localhost:7051` (http: 7000)
- **Web**: `https://localhost:7073` (http: 5260)

## Структура проекта

```
FinanceApp/
├── FinanceApp.Api/              # REST API
│   ├── Controllers/             # API контроллеры
│   │   ├── AuthController.cs
│   │   ├── OperationsController.cs
│   │   ├── TagsController.cs
│   │   └── ReportsController.cs
│   └── Program.cs               # Настройка CORS и Cookies
│
├── FinanceApp.Web/              # Веб-интерфейс
│   ├── Controllers/             # MVC контроллеры
│   │   ├── AccountController.cs
│   │   ├── DashboardController.cs
│   │   └── TagsController.cs
│   ├── Views/
│   │   ├── Account/
│   │   │   ├── Login.cshtml
│   │   │   └── Register.cshtml
│   │   ├── Dashboard/
│   │   │   └── Index.cshtml
│   │   ├── Tags/
│   │   │   └── Index.cshtml
│   │   └── Shared/
│   │       ├── _LoginLayout.cshtml
│   │       └── _MainLayout.cshtml
│   └── wwwroot/
│       ├── js/app/              # JavaScript модули
│       │   ├── api-client.js
│       │   ├── auth.js
│       │   ├── operations.js
│       │   ├── tags.js
│       │   ├── dashboard.js
│       │   └── tags-page.js
│       └── src/                 # Статические ресурсы (CSS, изображения)
│
├── FinanceApp.Infrastructure/   # Слой данных
│   ├── Data/
│   ├── Dtos/
│   └── Services/
│
└── FinanceApp.Dbo/              # Модели данных
    ├── Models/
    └── Enums/
```

## Технологии

### Backend
- ASP.NET Core 8.0
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Swagger/OpenAPI

### Frontend
- ASP.NET Core MVC (Razor Views)
- Bootstrap 5
- Vanilla JavaScript (ES6+)
- CORK Admin Template

## Безопасность

- ✅ JWT токены в httpOnly cookies (защита от XSS)
- ✅ SameSite=Strict cookies (защита от CSRF)
- ✅ Secure cookies (только HTTPS)
- ✅ CORS с явным указанием origin
- ✅ Автоматический refresh токенов

## API Endpoints

### Authentication
- `POST /api/auth/register` - Регистрация
- `POST /api/auth/login` - Вход
- `POST /api/auth/logout` - Выход
- `POST /api/auth/refresh` - Обновление токенов
- `GET /api/auth/me` - Получение данных текущего пользователя

### Operations
- `GET /api/operations` - Список операций с фильтрацией
- `GET /api/operations/{id}` - Получить операцию
- `POST /api/operations` - Создать операцию
- `PUT /api/operations/{id}` - Обновить операцию
- `DELETE /api/operations/{id}` - Удалить операцию
- `GET /api/operations/stats` - Статистика

### Tags
- `GET /api/tags` - Список тегов
- `GET /api/tags/tree` - Дерево тегов
- `GET /api/tags/{id}` - Получить тег
- `POST /api/tags` - Создать тег
- `PUT /api/tags/{id}` - Обновить тег
- `DELETE /api/tags/{id}` - Удалить тег
- `GET /api/tags/search?query={query}` - Поиск тегов

## Следующие шаги

Возможные улучшения:
- Добавить графики и визуализацию статистики (ApexCharts уже подключен)
- Реализовать фильтрацию операций по тегам
- Добавить экспорт данных в CSV/Excel
- Добавить загрузку аватара пользователя
- Реализовать уведомления (toast notifications)
- Добавить темную/светлую тему

## Разработчик

Проект создан для управления личными финансами.

