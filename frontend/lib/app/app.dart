import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:taskflow/features/auth/presentation/login_screen.dart';
import 'package:taskflow/features/auth/presentation/session_controller.dart';
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
    final session = ref.watch(sessionControllerProvider);
    return session.when(
      data: (signedIn) => signedIn ? child : const LoginScreen(),
      loading: () =>
          const Scaffold(body: Center(child: CircularProgressIndicator())),
      error: (error, _) => Scaffold(
        body: Center(child: Text('Не вдалося відкрити сесію: $error')),
      ),
    );
  }
}
