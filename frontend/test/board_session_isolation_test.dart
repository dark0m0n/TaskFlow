import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:taskflow/app/app.dart';
import 'package:taskflow/core/auth/session_store.dart';
import 'package:taskflow/core/auth/token_storage.dart';
import 'package:taskflow/features/boards/data/board_models.dart';
import 'package:taskflow/features/boards/data/boards_repository.dart';
import 'package:taskflow/features/boards/presentation/board_detail_screen.dart';
import 'package:taskflow/features/boards/presentation/board_list_screen.dart';
import 'package:taskflow/features/boards/presentation/boards_providers.dart';

class MemoryTokenStorage implements TokenStorage {
  MemoryTokenStorage(this.token);

  String? token;
  bool failDelete = false;

  @override
  Future<String?> readToken() async => token;

  @override
  Future<void> writeToken(String value) async => token = value;

  @override
  Future<void> deleteToken() async {
    if (failDelete) throw StateError('delete failed');
    token = null;
  }
}

class ControlledBoardsRepository extends BoardsRepository {
  ControlledBoardsRepository() : super(Dio());

  final listRequests = <Completer<List<BoardSummary>>>[];
  final detailRequests = <({int id, Completer<BoardDetail> result})>[];

  @override
  Future<List<BoardSummary>> getBoards() {
    final result = Completer<List<BoardSummary>>();
    listRequests.add(result);
    return result.future;
  }

  @override
  Future<BoardDetail> getBoard(int id) {
    final result = Completer<BoardDetail>();
    detailRequests.add((id: id, result: result));
    return result.future;
  }
}

ProviderContainer testContainer(
  MemoryTokenStorage storage,
  ControlledBoardsRepository repository,
) {
  return ProviderContainer.test(
    overrides: [
      tokenStorageProvider.overrideWith((ref) => storage),
      boardsRepositoryProvider.overrideWith((ref) => repository),
    ],
  );
}

Future<void> flushRequests() => Future<void>.delayed(Duration.zero);

BoardSummary summary(String title) =>
    BoardSummary(id: 1, title: title, description: '');

BoardDetail detail(String title) =>
    BoardDetail(id: 42, title: title, description: '', columns: const []);

DioException unauthorized() {
  final request = RequestOptions(path: 'boards');
  return DioException(
    requestOptions: request,
    response: Response(requestOptions: request, statusCode: 401),
  );
}

void main() {
  test(
    'account B gets a separate list while account A cache is still alive',
    () async {
      final repository = ControlledBoardsRepository();
      final container = testContainer(
        MemoryTokenStorage('token-a'),
        repository,
      );
      addTearDown(container.dispose);
      final store = container.read(sessionStoreProvider.notifier);
      final a = await container.read(sessionStoreProvider.future) as SignedIn;
      final oldList = boardsProvider(a.generation);
      final oldListener = container.listen(oldList, (_, _) {});
      addTearDown(oldListener.close);
      await flushRequests();
      repository.listRequests[0].complete([summary('A board')]);
      expect((await container.read(oldList.future)).single.title, 'A board');

      await store.signOut();
      await store.signIn('token-b');
      final b = await container.read(sessionStoreProvider.future) as SignedIn;
      final newList = boardsProvider(b.generation);
      final newListener = container.listen(newList, (_, _) {});
      addTearDown(newListener.close);
      await flushRequests();

      expect(b.generation, isNot(a.generation));
      expect(repository.listRequests, hasLength(2));
      expect(container.read(newList).isLoading, isTrue);
      repository.listRequests[1].complete([summary('B board')]);
      expect((await container.read(newList.future)).single.title, 'B board');
    },
  );

  test('returning to the same account still fetches a fresh list', () async {
    final repository = ControlledBoardsRepository();
    final container = testContainer(
      MemoryTokenStorage('same-token'),
      repository,
    );
    addTearDown(container.dispose);
    final store = container.read(sessionStoreProvider.notifier);
    final first = await container.read(sessionStoreProvider.future) as SignedIn;
    final firstList = boardsProvider(first.generation);
    final firstListener = container.listen(firstList, (_, _) {});
    addTearDown(firstListener.close);
    await flushRequests();
    repository.listRequests[0].complete([summary('Old board')]);
    await container.read(firstList.future);

    await store.signOut();
    await store.signIn('same-token');
    final second =
        await container.read(sessionStoreProvider.future) as SignedIn;
    final secondList = boardsProvider(second.generation);
    final secondListener = container.listen(secondList, (_, _) {});
    addTearDown(secondListener.close);
    await flushRequests();

    expect(second.generation, isNot(first.generation));
    expect(repository.listRequests, hasLength(2));
    expect(container.read(secondList).isLoading, isTrue);
    repository.listRequests[1].complete([summary('Updated board')]);
    expect(
      (await container.read(secondList.future)).single.title,
      'Updated board',
    );
  });

  test('the same numeric board ID is isolated between accounts', () async {
    final repository = ControlledBoardsRepository();
    final container = testContainer(MemoryTokenStorage('token-a'), repository);
    addTearDown(container.dispose);
    final store = container.read(sessionStoreProvider.notifier);
    final a = await container.read(sessionStoreProvider.future) as SignedIn;
    final oldBoard = boardProvider((boardId: 42, generation: a.generation));
    final oldListener = container.listen(oldBoard, (_, _) {});
    addTearDown(oldListener.close);
    await flushRequests();
    repository.detailRequests[0].result.complete(detail('A private board'));
    expect((await container.read(oldBoard.future)).title, 'A private board');

    await store.signOut();
    await store.signIn('token-b');
    final b = await container.read(sessionStoreProvider.future) as SignedIn;
    final newBoard = boardProvider((boardId: 42, generation: b.generation));
    final newListener = container.listen(newBoard, (_, _) {});
    addTearDown(newListener.close);
    await flushRequests();

    expect(repository.detailRequests.map((request) => request.id), [42, 42]);
    expect(container.read(newBoard).isLoading, isTrue);
    repository.detailRequests[1].result.complete(detail('B private board'));
    expect((await container.read(newBoard.future)).title, 'B private board');
  });

  test('late success from account A cannot populate account B state', () async {
    final repository = ControlledBoardsRepository();
    final container = testContainer(MemoryTokenStorage('token-a'), repository);
    addTearDown(container.dispose);
    final store = container.read(sessionStoreProvider.notifier);
    final a = await container.read(sessionStoreProvider.future) as SignedIn;
    final oldList = boardsProvider(a.generation);
    final oldListener = container.listen(oldList, (_, _) {});
    addTearDown(oldListener.close);
    final oldFuture = container.read(oldList.future);
    await flushRequests();

    await store.signOut();
    await store.signIn('token-b');
    final b = await container.read(sessionStoreProvider.future) as SignedIn;
    final newList = boardsProvider(b.generation);
    final newListener = container.listen(newList, (_, _) {});
    addTearDown(newListener.close);
    await flushRequests();

    repository.listRequests[0].complete([summary('A late board')]);
    await expectLater(oldFuture, throwsStateError);
    expect(container.read(newList).isLoading, isTrue);
    repository.listRequests[1].complete([summary('B current board')]);
    expect(
      (await container.read(newList.future)).single.title,
      'B current board',
    );
  });

  test(
    'late board detail from account A cannot populate account B detail',
    () async {
      final repository = ControlledBoardsRepository();
      final container = testContainer(
        MemoryTokenStorage('token-a'),
        repository,
      );
      addTearDown(container.dispose);
      final store = container.read(sessionStoreProvider.notifier);
      final a = await container.read(sessionStoreProvider.future) as SignedIn;
      final oldBoard = boardProvider((boardId: 42, generation: a.generation));
      final oldListener = container.listen(oldBoard, (_, _) {});
      addTearDown(oldListener.close);
      final oldFuture = container.read(oldBoard.future);
      await flushRequests();

      await store.signOut();
      await store.signIn('token-b');
      final b = await container.read(sessionStoreProvider.future) as SignedIn;
      final newBoard = boardProvider((boardId: 42, generation: b.generation));
      final newListener = container.listen(newBoard, (_, _) {});
      addTearDown(newListener.close);
      await flushRequests();

      repository.detailRequests[0].result.complete(
        detail('A late private board'),
      );
      await expectLater(oldFuture, throwsStateError);
      expect(container.read(newBoard).isLoading, isTrue);
      repository.detailRequests[1].result.complete(
        detail('B current private board'),
      );
      expect(
        (await container.read(newBoard.future)).title,
        'B current private board',
      );
    },
  );

  test('signing out while a list loads rejects its late result', () async {
    final repository = ControlledBoardsRepository();
    final storage = MemoryTokenStorage('token-a');
    final container = testContainer(storage, repository);
    addTearDown(container.dispose);
    final store = container.read(sessionStoreProvider.notifier);
    final a = await container.read(sessionStoreProvider.future) as SignedIn;
    final list = boardsProvider(a.generation);
    final listener = container.listen(list, (_, _) {});
    addTearDown(listener.close);
    final pending = container.read(list.future);
    await flushRequests();

    await store.signOut();
    repository.listRequests[0].complete([
      summary('Private board after logout'),
    ]);
    await expectLater(pending, throwsStateError);
    expect(storage.token, isNull);
    expect(
      container.read(sessionStoreProvider).asData?.value,
      isA<SignedOut>(),
    );
  });

  test('late 401 from account A cannot sign out account B', () async {
    final repository = ControlledBoardsRepository();
    final container = testContainer(MemoryTokenStorage('token-a'), repository);
    addTearDown(container.dispose);
    final store = container.read(sessionStoreProvider.notifier);
    final a = await container.read(sessionStoreProvider.future) as SignedIn;
    final oldList = boardsProvider(a.generation);
    final oldListener = container.listen(oldList, (_, _) {});
    addTearDown(oldListener.close);
    final oldFuture = container.read(oldList.future);
    await flushRequests();

    await store.signOut();
    await store.signIn('token-b');
    final b = await container.read(sessionStoreProvider.future) as SignedIn;
    repository.listRequests[0].completeError(unauthorized());
    await expectLater(oldFuture, throwsA(isA<DioException>()));

    expect(store.isCurrent(b), isTrue);
    expect(
      (container.read(sessionStoreProvider).asData?.value as SignedIn).token,
      'token-b',
    );
  });

  test('current account 401 still signs out that account', () async {
    final repository = ControlledBoardsRepository();
    final storage = MemoryTokenStorage('token-a');
    final container = testContainer(storage, repository);
    addTearDown(container.dispose);
    final a = await container.read(sessionStoreProvider.future) as SignedIn;
    final list = boardsProvider(a.generation);
    final listener = container.listen(list, (_, _) {});
    addTearDown(listener.close);
    final pending = container.read(list.future);
    await flushRequests();

    repository.listRequests[0].completeError(unauthorized());
    await expectLater(pending, throwsA(isA<DioException>()));

    expect(storage.token, isNull);
    final session = container.read(sessionStoreProvider).asData?.value;
    expect(session, isA<SignedOut>());
    expect((session as SignedOut).reason, SignOutReason.expired);
  });

  testWidgets('switching accounts never paints the old board list', (
    tester,
  ) async {
    final repository = ControlledBoardsRepository();
    final storage = MemoryTokenStorage('token-a');
    final container = testContainer(storage, repository);
    addTearDown(container.dispose);
    await tester.pumpWidget(
      UncontrolledProviderScope(
        container: container,
        child: const MaterialApp(home: SessionGate(child: BoardListScreen())),
      ),
    );
    await tester.pump();
    await tester.pump();
    expect(repository.listRequests, hasLength(1));
    repository.listRequests[0].complete([summary('A private board')]);
    await tester.pumpAndSettle();
    expect(find.text('A private board'), findsOneWidget);

    final store = container.read(sessionStoreProvider.notifier);
    await store.signOut();
    await store.signIn('token-b');
    await tester.pump();
    await tester.pump();

    expect(repository.listRequests, hasLength(2));
    expect(find.text('A private board'), findsNothing);
    expect(find.byType(CircularProgressIndicator), findsOneWidget);
    repository.listRequests[1].complete([summary('B private board')]);
    await tester.pumpAndSettle();
    expect(find.text('B private board'), findsOneWidget);
    expect(find.text('A private board'), findsNothing);
  });

  testWidgets('switching accounts never paints the old board detail', (
    tester,
  ) async {
    final repository = ControlledBoardsRepository();
    final container = testContainer(MemoryTokenStorage('token-a'), repository);
    addTearDown(container.dispose);
    await tester.pumpWidget(
      UncontrolledProviderScope(
        container: container,
        child: const MaterialApp(
          home: SessionGate(child: BoardDetailScreen(boardId: 42)),
        ),
      ),
    );
    await tester.pump();
    await tester.pump();
    repository.detailRequests[0].result.complete(detail('A private detail'));
    await tester.pumpAndSettle();
    expect(find.text('A private detail'), findsOneWidget);

    final store = container.read(sessionStoreProvider.notifier);
    await store.signOut();
    await store.signIn('token-b');
    await tester.pump();
    await tester.pump();

    expect(repository.detailRequests, hasLength(2));
    expect(find.text('A private detail'), findsNothing);
    expect(find.byType(CircularProgressIndicator), findsOneWidget);
    repository.detailRequests[1].result.complete(detail('B private detail'));
    await tester.pumpAndSettle();
    expect(find.text('B private detail'), findsOneWidget);
    expect(find.text('A private detail'), findsNothing);
  });

  testWidgets('failed secure storage deletion shows a retryable logout error', (
    tester,
  ) async {
    final repository = ControlledBoardsRepository();
    final storage = MemoryTokenStorage('token-a')..failDelete = true;
    final container = testContainer(storage, repository);
    addTearDown(container.dispose);
    await tester.pumpWidget(
      UncontrolledProviderScope(
        container: container,
        child: const MaterialApp(home: SessionGate(child: BoardListScreen())),
      ),
    );
    await tester.pump();
    await tester.pump();
    repository.listRequests[0].complete([summary('Still signed in')]);
    await tester.pumpAndSettle();

    await tester.tap(find.byTooltip('Вийти'));
    await tester.pumpAndSettle();
    expect(find.text('Не вдалося вийти. Спробуйте ще раз.'), findsOneWidget);
    expect(find.text('Still signed in'), findsOneWidget);
    expect(storage.token, 'token-a');

    storage.failDelete = false;
    await tester.tap(find.byTooltip('Вийти'));
    await tester.pumpAndSettle();
    expect(storage.token, isNull);
    expect(find.text('TaskFlow'), findsOneWidget);
  });
}
