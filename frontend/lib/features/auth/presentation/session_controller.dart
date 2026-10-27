import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:taskflow/core/auth/token_storage.dart';
import 'package:taskflow/features/auth/data/auth_repository.dart';

final sessionControllerProvider =
    AsyncNotifierProvider<SessionController, bool>(SessionController.new);

class SessionController extends AsyncNotifier<bool> {
  @override
  Future<bool> build() async {
    final token = await ref.read(tokenStorageProvider).readToken();
    return token != null && token.isNotEmpty;
  }

  Future<void> login(String email, String password) async {
    await ref.read(authRepositoryProvider).login(email, password);
    state = const AsyncData(true);
  }

  Future<void> registerAndLogin(String email, String password) async {
    final repository = ref.read(authRepositoryProvider);
    await repository.register(email, password);
    await repository.login(email, password);
    state = const AsyncData(true);
  }

  Future<void> logout() async {
    await ref.read(authRepositoryProvider).logout();
    state = const AsyncData(false);
  }
}
