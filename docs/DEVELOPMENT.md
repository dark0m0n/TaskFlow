# Локальна розробка

## Потрібно встановити

- Docker Desktop із Docker Compose.
- Flutter SDK із Dart 3.12 або новішим і Android SDK для Android-запуску.
- Для запуску iOS потрібен macOS із Xcode.
- .NET 8 SDK потрібен лише для запуску API без Docker.

## Повний стек через Docker

У корені репозиторію:

```powershell
Copy-Item .env.example .env
docker compose up -d --build
docker compose ps
```

`.env` містить локальні налаштування та ігнорується Git. Типовий порт API — `5079`, Swagger — `http://localhost:5079/swagger`. Compose запускає PostgreSQL, Redis і API. API чекає готовності залежностей; для локального середовища увімкнено застосування EF Core міграцій під час його запуску.

```powershell
docker compose logs -f api
docker compose down
```

Том `postgres_data` зберігає дані між перезапусками контейнерів. `docker compose down` його не видаляє.

## Flutter на Android-емуляторі

```powershell
cd frontend
flutter pub get
flutter run
```

Типова адреса клієнта — `http://10.0.2.2:5079/api/`. Android-емулятор використовує `10.0.2.2` для доступу до `localhost` комп’ютера. Дозвіл на HTTP без TLS є лише в debug-збірці Android.

Для фізичного пристрою задайте URL, який відкривається з цього пристрою:

```powershell
flutter run --dart-define=API_BASE_URL=https://your-dev-host/api/
```

Значення `API_BASE_URL` має включати `/api/`. Для release-збірок використовуйте HTTPS.

## Запуск API без Docker

Можна підняти лише PostgreSQL і Redis:

```powershell
docker compose up -d postgres redis
cd backend
dotnet ef database update --project TaskFlow.Infrastructure --startup-project TaskFlow.API
dotnet run --project TaskFlow.API --launch-profile http
```

Flutter-клієнт продовжить працювати з портом `5079`. Дані для такого запуску беруться з `backend/TaskFlow.API/appsettings.json`.

## Git

`.gitignore` виключає локальні секрети, згенеровані Flutter/.NET файли, результати збірки й налаштування IDE. `frontend/pubspec.lock` залишається у Git, щоб усі розробники отримували однакові версії залежностей. `.gitattributes` вирівнює закінчення рядків між Windows та іншими системами.
