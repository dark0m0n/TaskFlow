import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:taskflow/core/auth/token_storage.dart';

sealed class SessionState {
  const SessionState();
}

final class SignedIn extends SessionState {
  const SignedIn(this.token);

  final String token;
}

enum SignOutReason { userLogout, expired }

final class SignedOut extends SessionState {
  const SignedOut({this.reason});

  final SignOutReason? reason;
}

final sessionStoreProvider = AsyncNotifierProvider<SessionStore, SessionState>(
  SessionStore.new,
);

class SessionStore extends AsyncNotifier<SessionState> {
  @override
  Future<SessionState> build() async {
    final token = await ref.read(tokenStorageProvider).readToken();
    return token == null || token.isEmpty ? const SignedOut() : SignedIn(token);
  }

  Future<void> signIn(String token) async {
    if (token.isEmpty) {
      throw ArgumentError.value(token, 'token', 'Token cannot be empty');
    }

    await future;
    await ref.read(tokenStorageProvider).writeToken(token);
    state = AsyncData(SignedIn(token));
  }

  Future<void> signOut({
    SignOutReason reason = SignOutReason.userLogout,
  }) async {
    // Clearing the persisted token must also work after an initialization error.
    try {
      await future;
    } catch (_) {
      // Deleting the persisted token below is the authoritative operation.
    }
    await ref.read(tokenStorageProvider).deleteToken();
    state = AsyncData(SignedOut(reason: reason));
  }
}
