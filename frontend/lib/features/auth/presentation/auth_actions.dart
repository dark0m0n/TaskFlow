import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:taskflow/core/auth/session_store.dart';
import 'package:taskflow/features/auth/data/auth_repository.dart';

final authActionsProvider = Provider<AuthActions>((ref) {
  return AuthActions(
    ref.watch(authRepositoryProvider),
    ref.watch(sessionStoreProvider.notifier),
  );
});

class AuthActions {
  AuthActions(this._repository, this._sessionStore);

  final AuthRepository _repository;
  final SessionStore _sessionStore;

  Future<void> login(String email, String password) async {
    final token = await _repository.login(email, password);
    await _sessionStore.signIn(token);
  }

  Future<void> registerAndLogin(String email, String password) async {
    await _repository.register(email, password);
    await login(email, password);
  }
}
