# Архітектурна та технічна документація

## 1. Організація шарів (Clean Architecture)

Рішення структуровано відповідно до принципів Clean Architecture із чітким розділенням відповідальностей та правилом напрямку залежностей всередину:

```
TaskFlow/
├── backend/
│   ├── TaskFlow.Domain/          # Сутності, Enum'и, бізнес-правила
│   ├── TaskFlow.Application/     # CQRS (Commands/Queries), DTO, валідатори, інтерфейси
│   ├── TaskFlow.Infrastructure/  # DbContext, міграції, зовнішні сервіси (Redis, SignalR, Auth)
│   └── TaskFlow.API/             # Controllers, Middlewares, SignalR Hubs, конфігурація DI
├── docs/                         # Документація проєкту
└── docker-compose.yml            # Інфраструктура локального оточення (Postgres, Redis)
```

### TaskFlow.Domain
Шар не має зовнішніх залежностей. Містить доменні моделі:
- `Board`, `BoardMember` (ролі: `Admin`, `Member`)
- `Column`, `Card`, `Comment`
- `ActivityLog` (запис аудиту дій)

### TaskFlow.Application
Реалізує сценарії використання за патерном CQRS (бібліотека MediatR):
- **Commands & Queries:** Розбиті по доменних модулях (`Boards`, `Columns`, `Cards`, `Comments`, `Activities`).
- **Pipeline Behaviors:**
  - `ValidationBehavior<TRequest, TResponse>`: Автоматична перевірка вхідних моделей через FluentValidation перед виконанням хендлера.
  - `LoggingBehavior<TRequest, TResponse>`: Логування часу виконання команд та виявлення повільних операцій через `Stopwatch`.
- **Інтерфейси абстракцій:** `IApplicationDbContext`, `ISignalRNotificationService`, `IActivityLogger`, `IBoardAuthorizationService`, `ICurrentUserService`.

### TaskFlow.Infrastructure
Реалізує технічні деталі та взаємодію з зовнішніми сховищами:
- `TaskFlowDbContext`: Конфігурація зв'язків моделей EF Core, налаштування індексів та каскадного видалення.
- `ActivityLogger`: Сервіс запису бізнес-подій у таблицю `ActivityLogs`.
- `SignalRNotificationService`: Інкапсуляція викликів до `IHubContext<BoardHub>`.

### TaskFlow.API
Точка входу в систему:
- Реєстрація залежностей у контейнері DI (`Program.cs`).
- Маршрутизація HTTP-запитів до відповідних команд/запитів MediatR.
- Обробка глобальних винятків через Custom Middleware.
- Конфігурація автентифікації (`JwtBearer`) та підключення SignalR хабу.

---

## 2. Реалізація CQRS та Pipeline Behaviors

Кожен запит проходить через конвеєр обробки MediatR:

1. **HTTP Запит** потрапляє в API Controller.
2. Контролер надсилає команду або запит у `IMediator.Send()`.
3. **LoggingBehavior:** Фіксує назву запиту, генерує ідентифікатор кореляції, запускає таймер.
4. **ValidationBehavior:** Знаходить зареєстровані валідатори FluentValidation. Якщо дані невалідні, викидає `ValidationException` із колекцією помилок (перехоплюється Middleware з поверненням коду HTTP 400).
5. **RequestHandler:** Виконує бізнес-логіку, модифікує БД, викликає сервіси аудиту або сповіщень.
6. **LoggingBehavior (завершення):** Логує тривалість виконання запиту.
7. Контролер повертає результат клієнту (HTTP 200/201/204).

---

## 3. Real-Time синхронізація (SignalR)

Для мінімізації повторних опитувань (polling) реалізовано двосторонній зв'язок через WebSockets.

- **Hub:** `BoardHub` (доступний за маршрутом `/hubs/board`).
- **Групування:** Клієнти підключаються до каналу конкретної дошки за допомогою виклику методу `JoinBoard(int boardId)`. Це додає з'єднання до групи `board_{boardId}`.
- **Події:**
  - `CardMoved`: Передає ідентифікатор картки, новий ID колонки та новий індекс позиції.
  - `CardCreated`: Сповіщає про створення нового завдання.
  - `CardUpdated` / `CardDeleted`: Сповіщає про оновлення або видалення картки.
  - `NewComment`: Транслює доданий коментар усім підключеним учасникам дошки.

---

## 4. Стратегія логування та аудиту

Система чітко розмежовує системне технічне логування та бізнес-аудит:

| Критерій | Технічне логування (Serilog) | Аудит дій (Activity Log) |
| :--- | :--- | :--- |
| **Призначення** | Моніторинг працездатності, аналіз винятків, відстеження швидкодії | Бізнес-історія операцій для користувачів |
| **Сховище** | Консоль / файлові логи (`logs/taskflow-.log`) | Таблиця `ActivityLogs` (PostgreSQL) |
| **Доступ** | Інженери / DevOps | Клієнтські застосунки через `GET /api/boards/{id}/activity` |
| **Зміст** | Stack trace, тривалість виконання, HTTP-статуси | «Користувач Олексій перемістив картку в 'Done'» |

---

## 5. Модель авторизації та безпеки

- **Ідентифікація:** Захищені маршрути вимагають заголовок `Authorization: Bearer <token>`.
- **Контекст користувача:** `ICurrentUserService` витягує `UserId` безпосередньо з `ClaimsPrincipal` поточного HTTP-контексту.
- **Доступ до дошок:** Перевірка прав здійснюється на рівні `BoardMembers`. При створенні дошки її творець автоматично додається до списку учасників із роллю `Admin`. Перевірка доступу до сутностей вимагає валідації присутності користувача в колекції учасників:
  ```csharp
  board.Members.Any(m => m.UserId == currentUserId)
  ```
