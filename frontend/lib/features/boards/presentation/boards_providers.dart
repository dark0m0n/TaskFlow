import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:taskflow/core/auth/session_store.dart';
import 'package:taskflow/features/boards/data/board_models.dart';
import 'package:taskflow/features/boards/data/boards_repository.dart';

final boardsProvider = FutureProvider<List<BoardSummary>>((ref) async {
  try {
    return await ref.watch(boardsRepositoryProvider).getBoards();
  } on DioException catch (error) {
    if (error.response?.statusCode == 401) {
      await ref
          .read(sessionStoreProvider.notifier)
          .signOut(reason: SignOutReason.expired);
    }
    rethrow;
  }
});

final boardProvider = FutureProvider.family<BoardDetail, int>((ref, id) async {
  try {
    return await ref.watch(boardsRepositoryProvider).getBoard(id);
  } on DioException catch (error) {
    if (error.response?.statusCode == 401) {
      await ref
          .read(sessionStoreProvider.notifier)
          .signOut(reason: SignOutReason.expired);
    }
    rethrow;
  }
});
