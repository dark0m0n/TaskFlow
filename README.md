# TaskFlow API

TaskFlow — це бекенд-сервіс для системи керування завданнями за методологією Kanban, розроблений на платформі .NET 8. Сервіс надає RESTful API для маніпуляції сутностями та SignalR Hub для синхронізації змін між клієнтами в режимі реального часу.

Докладніше про внутрішній устрій та дизайн системи читайте в [Архітектурній документації](docs/ARCHITECTURE.md), а специфікацію всіх ендпоінтів та протоколу WebSockets — у [Специфікації API](docs/API.md).

---

## Технологічний стек

- **Runtime:** .NET 8 (C# 12)
- **Архітектура:** Clean Architecture, CQRS (Command Query Responsibility Segregation)
- **Медіатор & Пайплайни:** MediatR (з реалізацією `ValidationBehavior` та `LoggingBehavior`)
- **База даних:** PostgreSQL 16
- **ORM:** Entity Framework Core 8 (Code-First, міграції)
- **Кешування:** Redis 7
- **Real-time зв'язок:** ASP.NET Core SignalR (WebSockets)
- **Автентифікація та авторизація:** ASP.NET Core Identity, JWT Bearer Tokens
- **Валідація:** FluentValidation
- **Логування:** Serilog (структуроване логування у консоль та файли)
- **Фонові задачі:** `BackgroundService` (`ExpiredCardsCheckWorker`)

---

## Реалізований функціонал

- **Автентифікація та безпека:** Реєстрація, вхід, випуск та валідація JWT токенів, хешування паролів через ASP.NET Identity.
- **Керування дошками (Boards):** Створення, редагування, видалення дощок, розмежування прав доступу на основі ролей учасників (`Admin`, `Member`).
- **Структура робочого простору:** Колонки (Columns) та завдання (Cards) з підтримкою сортування (`OrderIndex`), дедлайнів, пріоритетів та призначення виконавців (`AssigneeId`).
- **Drag-and-Drop операції:** Ендпоінт переміщення карток (`MoveCardCommand`) зі зміною колонки та позиції, автоматичним перерахунком індексів та трансляцією події через WebSockets.
- **Коментарі:** Додавання та перегляд обговорень під картками.
- **Activity Log (Аудит дій):** Фіксація ключових подій (створення карток, зміна статусу, коментарі) в базі даних із можливістю отримання історії змін по дошці.
- **Real-time сповіщення:** SignalR-хаб транслює події оновлення/переміщення карток усім підключеним клієнтам у межах кімнати дошки (`board_{boardId}`).
- **Фонова обробка:** Фоновий воркер періодично перевіряє дедлайни завдань та маркує прострочені картки.

---

## Швидкий старт

### Вимоги
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) та Docker Compose

### 1. Запуск інфраструктури (PostgreSQL та Redis)
У кореневій директорії проєкту запустіть контейнери:

```bash
docker compose up -d
```

### 2. Конфігурація оточення
Параметри підключення визначені в `backend/TaskFlow.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=taskflowdb;Username=myuser;Password=mypassword",
    "Redis": "localhost:6379"
  },
  "JWT": {
    "Issuer": "http://localhost:5200",
    "Audience": "http://localhost:5200",
    "SigningKey": "TaskFlowSuperSecretKeyThatIsAtLeast32BytesLong123!"
  }
}
```

### 3. Застосування міграцій бази даних
Перейдіть до каталогу `backend` та застосуйте міграції:

```bash
cd backend
dotnet ef database update --project TaskFlow.Infrastructure --startup-project TaskFlow.API
```

### 4. Запуск бекенд-сервісу
```bash
dotnet run --project TaskFlow.API
```

Після запуску Swagger UI доступний за адресою: `http://localhost:5200/swagger` (або за адресою, вказаною в терміналі).
SignalR Hub доступний за ендпоінтом `/hubs/board`.
