# Специфікація API та контрактів (API Reference)

Цей документ містить опис REST API ендпоінтів та протоколу взаємодії через SignalR Hub сервісу **TaskFlow**.

---

## 1. Загальні конвенції

- **Базовий URL:** `http://localhost:5200/api` (або HTTPS аналог).
- **Формат даних:** `application/json` для запитів та відповідей.
- **Автентифікація:** Більшість ендпоінтів захищені атрибутом `[Authorize]`. Необхідно передавати заголовок:
  ```http
  Authorization: Bearer <jwt_token>
  ```
- **Коди помилок:**
  - `400 Bad Request` — помилки валідації вхідних даних (FluentValidation).
  - `401 Unauthorized` — відсутній або недійсний JWT токен.
  - `403 Forbidden` — недостатньо прав (наприклад, не є учасником або адміном дошки).
  - `404 Not Found` — запитуваний ресурс не знайдено.
  - `500 Internal Server Error` — необроблена помилка сервера.

### Перерахування (Enums)

#### `BoardRole`
| Значення (Int) | Назва | Опис |
| :--- | :--- | :--- |
| `0` | `Admin` | Власник або адміністратор дошки (повні права на зміну ролей і видалення) |
| `1` | `Member` | Учасник дошки (створення/редагування колонок та карток) |
| `2` | `Viewer` | Тільки перегляд вмісту |

#### `PriorityLevel`
| Значення (Int) | Назва | Опис |
| :--- | :--- | :--- |
| `0` | `Low` | Низький пріоритет |
| `1` | `Medium` | Середній пріоритет |
| `2` | `High` | Високий пріоритет |
| `3` | `Urgent` | Терміновий пріоритет |

---

## 2. Автентифікація (`/api/account`)

### Реєстрація користувача
Створює новий обліковий запис у системі.

- **URL:** `POST /api/account/register`
- **Автентифікація:** Не потрібна
- **Request Body:**
  ```json
  {
    "email": "user@example.com",
    "password": "Password123!"
  }
  ```
- **Response (200 OK):**
  ```json
  {
    "message": "User registered successfully"
  }
  ```

### Вхід у систему
Перевіряє облікові дані та видає JWT токен (дійсний 2 години).

- **URL:** `POST /api/account/login`
- **Автентифікація:** Не потрібна
- **Request Body:**
  ```json
  {
    "email": "user@example.com",
    "password": "Password123!"
  }
  ```
- **Response (200 OK):**
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
  ```

---

## 3. Користувачі (`/api/users`)

### Пошук користувачів за Email
Використовується для знаходження користувачів при додаванні до дошки.

- **URL:** `GET /api/users/search?email={email}`
- **Автентифікація:** Обов'язкова
- **Response (200 OK):**
  ```json
  [
    {
      "id": "b3e94fd6-91e8-4663-8a3c-1b7713d80bf9",
      "email": "alex@example.com",
      "userName": "alex@example.com"
    }
  ]
  ```

---

## 4. Дошки (`/api/boards`)

### Отримання списку дощок користувача
Повертає всі дошки, де поточний користувач є учасником або адміністратором.

- **URL:** `GET /api/boards`
- **Автентифікація:** Обов'язкова
- **Response (200 OK):**
  ```json
  [
    {
      "id": 1,
      "title": "Roadmap 2026",
      "description": "Основна робоча дошка",
      "ownerId": "b3e94fd6-91e8-4663-8a3c-1b7713d80bf9",
      "createdAt": "2026-09-28T10:00:00Z",
      "columns": [],
      "members": []
    }
  ]
  ```

### Створення дошки
Створює нову дошку, роблячи поточного користувача власником та адміністратором.

- **URL:** `POST /api/boards`
- **Автентифікація:** Обов'язкова
- **Request Body:**
  ```json
  {
    "title": "Backend Sprint",
    "description": "Задачі для розробки бекенду"
  }
  ```
- **Response (201 Created):**
  ```json
  {
    "id": 2
  }
  ```

### Отримання деталей дошки
Повертає повне дерево дошки: колонки, упорядковані картки та список учасників.

- **URL:** `GET /api/boards/{boardId}`
- **Автентифікація:** Обов'язкова
- **Response (200 OK):**
  ```json
  {
    "id": 1,
    "title": "Backend Sprint",
    "description": "Задачі для розробки бекенду",
    "ownerId": "b3e94fd6-91e8-4663-8a3c-1b7713d80bf9",
    "createdAt": "2026-09-28T10:00:00Z",
    "columns": [
      {
        "id": 10,
        "title": "To Do",
        "order": 0,
        "cards": [
          {
            "id": 101,
            "title": "Налаштувати Redis",
            "description": "Інтегрувати розподілений кеш",
            "priority": 2,
            "dueDate": "2026-10-01T18:00:00Z",
            "order": 0,
            "assigneeId": "b3e94fd6-91e8-4663-8a3c-1b7713d80bf9"
          }
        ]
      }
    ],
    "members": [
      {
        "userId": "b3e94fd6-91e8-4663-8a3c-1b7713d80bf9",
        "role": 0
      }
    ]
  }
  ```

### Додавання учасника до дошки
- **URL:** `POST /api/boards/{boardId}/members`
- **Автентифікація:** Обов'язкова (тільки Admin)
- **Request Body:**
  ```json
  {
    "email": "colleague@example.com",
    "role": 1
  }
  ```
- **Response (200 OK):**
  ```json
  {
    "message": "Member added successfully"
  }
  ```

### Оновлення ролі учасника
- **URL:** `PUT /api/boards/{boardId}/members/{userId}/role`
- **Автентифікація:** Обов'язкова (тільки Admin)
- **Request Body:**
  ```json
  {
    "role": 0
  }
  ```
- **Response (200 OK):**
  ```json
  {
    "message": "Member role updated successfully"
  }
  ```

### Видалення учасника з дошки
- **URL:** `DELETE /api/boards/{boardId}/members/{userId}`
- **Автентифікація:** Обов'язкова (тільки Admin)
- **Response (200 OK):**
  ```json
  {
    "message": "Member deleted successfully"
  }
  ```

### Створення колонки на дошці
- **URL:** `POST /api/boards/{boardId}/columns`
- **Автентифікація:** Обов'язкова
- **Request Body:**
  ```json
  {
    "title": "In Progress"
  }
  ```
- **Response (200 OK):**
  ```json
  {
    "id": 11,
    "title": "In Progress",
    "order": 1,
    "cards": []
  }
  ```

### Отримання журналу активності (Activity Log)
- **URL:** `GET /api/boards/{boardId}/activity?limit={limit}` (за замовчуванням `limit = 20`)
- **Автентифікація:** Обов'язкова
- **Response (200 OK):**
  ```json
  [
    {
      "id": 1,
      "boardId": 1,
      "userId": "b3e94fd6-91e8-4663-8a3c-1b7713d80bf9",
      "action": "CardMoved",
      "details": "Переміщено картку 'Налаштувати Redis' в колонку 'In Progress'",
      "createdAt": "2026-09-28T10:15:30Z"
    }
  ]
  ```

---

## 5. Колонки (`/api/columns`)

### Оновлення колонки
Зміна назви або порядкового індексу.

- **URL:** `PUT /api/columns/{columnId}`
- **Автентифікація:** Обов'язкова
- **Request Body:**
  ```json
  {
    "title": "Review",
    "order": 2
  }
  ```
- **Response (200 OK):** Повертає оновлений об'єкт `ColumnDto`.

### Видалення колонки
- **URL:** `DELETE /api/columns/{columnId}`
- **Автентифікація:** Обов'язкова
- **Response (200 OK):**
  ```json
  {
    "message": "Column removed successfully"
  }
  ```

### Створення завдання у колонці
- **URL:** `POST /api/columns/{columnId}/cards`
- **Автентифікація:** Обов'язкова
- **Request Body:**
  ```json
  {
    "title": "Написати юніт-тести",
    "description": "Покрити тестами MoveCardCommandHandler",
    "priority": 1,
    "dueDate": "2026-10-05T12:00:00Z",
    "assigneeId": "b3e94fd6-91e8-4663-8a3c-1b7713d80bf9"
  }
  ```
- **Response (200 OK):** Повертає створений `CardDto`.

---

## 6. Завдання / Картки (`/api/cards`)

### Оновлення завдання
- **URL:** `PUT /api/cards/{cardId}`
- **Автентифікація:** Обов'язкова
- **Request Body:**
  ```json
  {
    "title": "Написати інтеграційні тести",
    "description": "Оновлений опис задачі",
    "priority": 2,
    "dueDate": "2026-10-07T18:00:00Z",
    "assigneeId": null
  }
  ```
- **Response (200 OK):** Повертає оновлений `CardDto`.

### Переміщення картки (Drag-and-Drop)
Змінює колонку та/або порядковий номер картки, перераховує індекси інших карток та транслює подію через SignalR.

- **URL:** `PUT /api/cards/{cardId}/move`
- **Автентифікація:** Обов'язкова
- **Request Body:**
  ```json
  {
    "targetColumnId": 11,
    "newOrder": 0
  }
  ```
- **Response (200 OK):** Повертає оновлений `CardDto`.

### Видалення картки
- **URL:** `DELETE /api/cards/{cardId}`
- **Автентифікація:** Обов'язкова
- **Response (200 OK):**
  ```json
  {
    "message": "Card removed successfully"
  }
  ```

### Отримання коментарів до картки
- **URL:** `GET /api/cards/{cardId}/comments`
- **Автентифікація:** Обов'язкова
- **Response (200 OK):**
  ```json
  [
    {
      "id": 5,
      "content": "Пуллреквест відкрито, чекаю рев'ю",
      "createdAt": "2026-09-28T11:00:00Z",
      "userId": "b3e94fd6-91e8-4663-8a3c-1b7713d80bf9",
      "cardId": 101
    }
  ]
  ```

### Додавання коментаря
Зберігає коментар, фіксує запис в Activity Log та транслює повідомлення в кімнату дошки через SignalR.

- **URL:** `POST /api/cards/{cardId}/comments`
- **Автентифікація:** Обов'язкова
- **Request Body:**
  ```json
  {
    "content": "Задачу виконано, перевірте будь ласка"
  }
  ```
- **Response (200 OK):** Повертає створений `CommentDto`.

---

## 7. Real-Time протокол (SignalR Hub)

- **Маршрут хабу:** `/hubs/board`
- **Транспорт:** WebSockets (fallback: Long Polling).

### Клієнтські методи (Client -> Server)
Клієнт повинен викликати ці методи для керування підпискою на кімнату конкретної дошки:

1. **`JoinBoard(int boardId)`**
   Додає поточне підключення (`ConnectionId`) до групи `board_{boardId}`. Необхідно викликати при вході користувача на сторінку дошки.
   ```javascript
   await hubConnection.invoke("JoinBoard", 1);
   ```

2. **`LeaveBoard(int boardId)`**
   Видаляє поточне підключення з групи дошки. Викликається при виході з дошки або закритті вкладки.
   ```javascript
   await hubConnection.invoke("LeaveBoard", 1);
   ```

### Серверні події (Server -> Client)
Події транслюються всім підключеним учасникам кімнати `board_{boardId}`:

1. **`CardMoved`**
   Транслюється при переміщенні або зміні порядку картки.
   - **Payload:**
     ```json
     {
       "cardId": 101,
       "newColumnId": 11,
       "newOrder": 0
     }
     ```

2. **`CommentAdded`**
   Транслюється при додаванні нового коментаря до будь-якої картки дошки.
   - **Payload:**
     ```json
     {
       "cardId": 101,
       "authorName": "alex@example.com",
       "content": "Задачу виконано, перевірте будь ласка"
     }
     ```
