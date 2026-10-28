import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:taskflow/core/auth/session_store.dart';
import 'package:taskflow/features/boards/data/board_models.dart';
import 'package:taskflow/features/boards/data/boards_repository.dart';

typedef BoardRequestKey = ({int boardId, int generation});

final boardsProvider = FutureProvider.autoDispose
    .family<List<BoardSummary>, int>((ref, generation) async {
      final store = ref.read(sessionStoreProvider.notifier);
      final repository = ref.watch(boardsRepositoryProvider);
      final session = await ref.read(sessionStoreProvider.future);
      if (session is! SignedIn || session.generation != generation) {
        throw StateError('Session changed before boards loaded');
      }

      try {
        final boards = await repository.getBoards();
        if (!store.isCurrent(session)) {
          throw StateError('Session changed while boards loaded');
        }
        return boards;
      } on DioException catch (error) {
        if (error.response?.statusCode == 401) {
          await store.signOutIfCurrent(
            token: session.token,
            generation: session.generation,
          );
        }
        rethrow;
      }
    }, retry: (_, _) => null);

final boardProvider = FutureProvider.autoDispose
    .family<BoardDetail, BoardRequestKey>((ref, key) async {
      final store = ref.read(sessionStoreProvider.notifier);
      final repository = ref.watch(boardsRepositoryProvider);
      final session = await ref.read(sessionStoreProvider.future);
      if (session is! SignedIn || session.generation != key.generation) {
        throw StateError('Session changed before board loaded');
      }

      try {
        final board = await repository.getBoard(key.boardId);
        if (!store.isCurrent(session)) {
          throw StateError('Session changed while board loaded');
        }
        return board;
      } on DioException catch (error) {
        if (error.response?.statusCode == 401) {
          await store.signOutIfCurrent(
            token: session.token,
            generation: session.generation,
          );
        }
        rethrow;
      }
    }, retry: (_, _) => null);
