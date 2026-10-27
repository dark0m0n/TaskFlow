import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:taskflow/features/auth/presentation/login_screen.dart';
import 'package:taskflow/core/auth/session_store.dart';
import 'package:taskflow/features/boards/presentation/board_detail_screen.dart';
import 'package:taskflow/features/boards/presentation/board_list_screen.dart';

final routerProvider = Provider<GoRouter>((ref) {
  final router = GoRouter(
    routes: [
      GoRoute(
        path: '/',
        builder: (_, _) => const SessionGate(child: BoardListScreen()),
      ),
      GoRoute(
        path: '/boards/:id',
        builder: (_, state) {
          final boardId = int.tryParse(state.pathParameters['id'] ?? '');
          if (boardId == null) {
            return const Scaffold(
              body: Center(child: Text('Некоректна дошка')),
            );
          }
          return SessionGate(child: BoardDetailScreen(boardId: boardId));
        },
      ),
    ],
  );
  ref.onDispose(router.dispose);
  return router;
});

class TaskFlowApp extends ConsumerWidget {
  const TaskFlowApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return MaterialApp.router(
      title: 'TaskFlow',
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF4F46E5)),
      ),
      routerConfig: ref.watch(routerProvider),
    );
  }
}

class SessionGate extends ConsumerWidget {
  const SessionGate({required this.child, super.key});

  final Widget child;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final session = ref.watch(sessionStoreProvider);
    return session.when(
      data: (value) => switch (value) {
        SignedIn() => child,
        SignedOut(:final reason) => LoginScreen(
          sessionExpired: reason == SignOutReason.expired,
        ),
      },
      loading: () =>
          const Scaffold(body: Center(child: CircularProgressIndicator())),
      error: (_, _) => Scaffold(
        body: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text('Не вдалося відкрити збережену сесію.'),
              const SizedBox(height: 12),
              FilledButton(
                onPressed: () async {
                  try {
                    await ref.read(sessionStoreProvider.notifier).signOut();
                  } catch (_) {
                    if (context.mounted) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(
                          content: Text('Не вдалося очистити локальну сесію.'),
                        ),
                      );
                    }
                  }
                },
                child: const Text('Очистити сесію'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
