import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:taskflow/core/network/api_client.dart';
import 'package:taskflow/features/auth/presentation/session_controller.dart';
import 'package:taskflow/features/boards/presentation/boards_providers.dart';

class BoardListScreen extends ConsumerWidget {
  const BoardListScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final boards = ref.watch(boardsProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Мої дошки'),
        actions: [
          IconButton(
            tooltip: 'Оновити',
            onPressed: () => ref.invalidate(boardsProvider),
            icon: const Icon(Icons.refresh),
          ),
          IconButton(
            tooltip: 'Вийти',
            onPressed: () =>
                ref.read(sessionControllerProvider.notifier).logout(),
            icon: const Icon(Icons.logout),
          ),
        ],
      ),
      body: boards.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, _) => Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(userFacingError(error)),
              const SizedBox(height: 12),
              FilledButton(
                onPressed: () => ref.invalidate(boardsProvider),
                child: const Text('Повторити'),
              ),
            ],
          ),
        ),
        data: (items) {
          if (items.isEmpty) {
            return const Center(child: Text('У вас ще немає дощок.'));
          }
          return RefreshIndicator(
            onRefresh: () async => ref.refresh(boardsProvider.future),
            child: ListView.builder(
              itemCount: items.length,
              itemBuilder: (context, index) {
                final board = items[index];
                return ListTile(
                  title: Text(board.title),
                  subtitle: board.description.isEmpty
                      ? null
                      : Text(board.description, maxLines: 2),
                  trailing: const Icon(Icons.chevron_right),
                  onTap: () => context.go('/boards/${board.id}'),
                );
              },
            ),
          );
        },
      ),
    );
  }
}
