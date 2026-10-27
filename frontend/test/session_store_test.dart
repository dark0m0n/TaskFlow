import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:taskflow/app/app.dart';
import 'package:taskflow/core/auth/session_store.dart';
import 'package:taskflow/core/auth/token_storage.dart';
import 'package:taskflow/core/network/api_client.dart';
import 'package:taskflow/features/auth/data/auth_repository.dart';
import 'package:taskflow/features/auth/presentation/auth_actions.dart';

class FakeTokenStorage implements TokenStorage {
  FakeTokenStorage({this.token, this.failRead = false, this.failWrite = false});

  String? token;
  bool failRead;
  bool failWrite;
  int readCount = 0;

  @override
  Future<String?> readToken() async {
    readCount++;
    if (failRead) throw StateError('read failed');
    return token;
  }

  @override
  Future<void> writeToken(String value) async {
    if (failWrite) throw StateError('write failed');
    token = value;
  }

  @override
  Future<void> deleteToken() async {
    token = null;
  }
}

class FakeAuthRepository extends AuthRepository {
  FakeAuthRepository({this.failLogin = false}) : super(Dio());

  final bool failLogin;

  @override
  Future<String> login(String email, String password) async {
    if (failLogin) throw StateError('login failed');
    return 'issued-token';
  }
}

ProviderContainer containerWith(FakeTokenStorage storage) {
  return ProviderContainer.test(
    overrides: [tokenStorageProvider.overrideWith((ref) => storage)],
  );
}

void main() {
  testWidgets('unreadable session can be cleared from the screen', (
    tester,
  ) async {
    final storage = FakeTokenStorage(token: 'stale-token', failRead: true);
    await tester.pumpWidget(
      ProviderScope(
        overrides: [tokenStorageProvider.overrideWith((ref) => storage)],
        child: const MaterialApp(home: SessionGate(child: Text('Boards'))),
      ),
    );
    await tester.pumpAndSettle();

    expect(find.text('Не вдалося відкрити збережену сесію.'), findsOneWidget);
    await tester.tap(find.text('Очистити сесію'));
    await tester.pumpAndSettle();

    expect(storage.token, isNull);
    expect(find.text('TaskFlow'), findsOneWidget);
  });

  test('restores a saved token at startup', () async {
    final storage = FakeTokenStorage(token: 'saved-token');
    final container = containerWith(storage);
    addTearDown(container.dispose);

    final session = await container.read(sessionStoreProvider.future);

    expect(session, isA<SignedIn>());
    expect((session as SignedIn).token, 'saved-token');
    expect(storage.readCount, 1);
  });

  test('does not report sign in when secure storage write fails', () async {
    final storage = FakeTokenStorage(failWrite: true);
    final container = containerWith(storage);
    addTearDown(container.dispose);
    await container.read(sessionStoreProvider.future);

    await expectLater(
      container.read(sessionStoreProvider.notifier).signIn('new-token'),
      throwsStateError,
    );

    expect(storage.token, isNull);
    expect(
      container.read(sessionStoreProvider).asData?.value,
      isA<SignedOut>(),
    );
  });

  test('clears persisted and in-memory session on sign out', () async {
    final storage = FakeTokenStorage(token: 'saved-token');
    final container = containerWith(storage);
    addTearDown(container.dispose);
    await container.read(sessionStoreProvider.future);

    await container.read(sessionStoreProvider.notifier).signOut();

    expect(storage.token, isNull);
    final session = container.read(sessionStoreProvider).asData?.value;
    expect(session, isA<SignedOut>());
    expect((session as SignedOut).reason, SignOutReason.userLogout);
  });

  test('can clear storage after startup read fails', () async {
    final storage = FakeTokenStorage(token: 'stale-token', failRead: true);
    final container = containerWith(storage);
    addTearDown(container.dispose);
    await expectLater(
      container.read(sessionStoreProvider.future),
      throwsStateError,
    );

    await container.read(sessionStoreProvider.notifier).signOut();

    expect(storage.token, isNull);
    expect(
      container.read(sessionStoreProvider).asData?.value,
      isA<SignedOut>(),
    );
  });

  test(
    'Dio uses the current session without rereading secure storage',
    () async {
      final storage = FakeTokenStorage();
      final container = containerWith(storage);
      addTearDown(container.dispose);
      await container.read(sessionStoreProvider.future);

      final headers = <Object?>[];
      final dio = container.read(dioProvider);
      dio.interceptors.add(
        InterceptorsWrapper(
          onRequest: (options, handler) {
            headers.add(options.headers['Authorization']);
            handler.resolve(Response(requestOptions: options, statusCode: 200));
          },
        ),
      );

      await container.read(sessionStoreProvider.notifier).signIn('new-token');
      await dio.get<void>('boards');
      await container.read(sessionStoreProvider.notifier).signOut();
      await dio.get<void>('boards');

      expect(headers, ['Bearer new-token', null]);
      expect(storage.readCount, 1);
    },
  );

  test('auth action only opens a session after login succeeds', () async {
    final storage = FakeTokenStorage();
    final container = containerWith(storage);
    addTearDown(container.dispose);
    await container.read(sessionStoreProvider.future);
    final sessionStore = container.read(sessionStoreProvider.notifier);

    await expectLater(
      AuthActions(
        FakeAuthRepository(failLogin: true),
        sessionStore,
      ).login('user@example.com', 'password'),
      throwsStateError,
    );
    expect(storage.token, isNull);
    expect(
      container.read(sessionStoreProvider).asData?.value,
      isA<SignedOut>(),
    );

    await AuthActions(
      FakeAuthRepository(),
      sessionStore,
    ).login('user@example.com', 'password');
    expect(storage.token, 'issued-token');
    expect(container.read(sessionStoreProvider).asData?.value, isA<SignedIn>());
  });
}
