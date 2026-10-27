import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:taskflow/core/network/api_client.dart';
import 'package:taskflow/core/auth/session_store.dart';
import 'package:taskflow/features/boards/presentation/boards_providers.dart';

class BoardListScreen extends ConsumerStatefulWidget {
  const BoardListScreen({super.key});

  @override
  ConsumerState<BoardListScreen> createState() => _BoardListScreenState();
}

class _BoardListScreenState extends ConsumerState<BoardListScreen> {
  bool _signingOut = false;

  Future<void> _signOut() async {
    if (_signingOut) return;
    setState(() => _signingOut = true);
    try {
      await ref.read(sessionStoreProvider.notifier).signOut();
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Не вдалося вийти. Спробуйте ще раз.')),
        );
      }
    } finally {
      if (mounted) setState(() => _signingOut = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final session = ref.watch(sessionStoreProvider).asData?.value;
    if (session is! SignedIn) return const SizedBox.shrink();

    final key = boardsProvider(session.generation);
    final boards = ref.watch(key);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Мої дошки'),
        actions: [
          IconButton(
            tooltip: 'Оновити',
            onPressed: () => ref.invalidate(key),
            icon: const Icon(Icons.refresh),
          ),
          IconButton(
            tooltip: 'Вийти',
            onPressed: _signingOut ? null : _signOut,
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
                onPressed: () => ref.invalidate(key),
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
            onRefresh: () async => ref.refresh(key.future),
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
