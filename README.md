# TaskFlow

TaskFlow — Kanban-проєкт із .NET 8 API та Flutter-клієнтом для Android/iOS. API вже містить авторизацію, дошки, колонки, картки, коментарі й SignalR. Flutter-клієнт зараз підтримує реєстрацію, вхід і перегляд дощок.

## Структура

```text
backend/            .NET API, Application, Domain, Infrastructure
frontend/           Flutter-застосунок
docs/               Документація API та розробки
docker-compose.yml  Локальний API, PostgreSQL і Redis
```

## Швидкий старт

Потрібні Docker Desktop, Flutter SDK із Dart 3.12+ та Android SDK.

```powershell
Copy-Item .env.example .env
docker compose up -d --build
cd frontend
flutter pub get
flutter run
```

Swagger: `http://localhost:5079/swagger`. Flutter за замовчуванням використовує `http://10.0.2.2:5079/api/` для Android-емулятора. Інші адреси задаються через `--dart-define=API_BASE_URL=...`.

## Документація

- [Локальний запуск, Docker та Git](docs/DEVELOPMENT.md)
- [Flutter: архітектура й поточний стан](docs/FRONTEND.md)
- [Контракт API](docs/API.md)
- [Архітектура бекенду](docs/ARCHITECTURE.md)
