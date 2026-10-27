import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:taskflow/core/network/api_client.dart';
import 'package:taskflow/features/boards/presentation/boards_providers.dart';

class BoardDetailScreen extends ConsumerWidget {
  const BoardDetailScreen({required this.boardId, super.key});

  final int boardId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final board = ref.watch(boardProvider(boardId));

    return Scaffold(
      appBar: AppBar(
        leading: IconButton(
          tooltip: 'До списку дощок',
          icon: const Icon(Icons.arrow_back),
          onPressed: () => context.go('/'),
        ),
        title: Text(board.asData?.value.title ?? 'Дошка'),
        actions: [
          IconButton(
            tooltip: 'Оновити',
            icon: const Icon(Icons.refresh),
            onPressed: () => ref.invalidate(boardProvider(boardId)),
          ),
        ],
      ),
      body: board.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, _) => Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(userFacingError(error)),
              const SizedBox(height: 12),
              FilledButton(
                onPressed: () => ref.invalidate(boardProvider(boardId)),
                child: const Text('Повторити'),
              ),
            ],
          ),
        ),
        data: (details) {
          if (details.columns.isEmpty) {
            return const Center(child: Text('На дошці ще немає колонок.'));
          }
          return ListView.builder(
            padding: const EdgeInsets.all(16),
            itemCount: details.columns.length,
            itemBuilder: (context, index) {
              final column = details.columns[index];
              return Card(
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        column.title,
                        style: Theme.of(context).textTheme.titleMedium,
                      ),
                      if (column.cards.isEmpty)
                        const Padding(
                          padding: EdgeInsets.only(top: 12),
                          child: Text('Немає карток'),
                        ),
                      for (final card in column.cards)
                        ListTile(
                          contentPadding: EdgeInsets.zero,
                          title: Text(card.title),
                          subtitle: Text('Пріоритет: ${card.priority}'),
                        ),
                    ],
                  ),
                ),
              );
            },
          );
        },
      ),
    );
  }
}
