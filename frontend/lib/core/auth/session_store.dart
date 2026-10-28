import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:taskflow/core/auth/token_storage.dart';

sealed class SessionState {
  const SessionState();
}

final class SignedIn extends SessionState {
  const SignedIn(this.token, this.generation);

  final String token;
  final int generation;
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
  int _generation = 0;
  Future<void> _mutationQueue = Future<void>.value();

  @override
  Future<SessionState> build() async {
    final token = await ref.read(tokenStorageProvider).readToken();
    return token == null || token.isEmpty
        ? const SignedOut()
        : SignedIn(token, ++_generation);
  }

  Future<void> signIn(String token) async {
    if (token.isEmpty) {
      throw ArgumentError.value(token, 'token', 'Token cannot be empty');
    }

    return _serialize(() async {
      await future;
      await ref.read(tokenStorageProvider).writeToken(token);
      state = AsyncData(SignedIn(token, ++_generation));
    });
  }

  Future<void> signOut({
    SignOutReason reason = SignOutReason.userLogout,
  }) async {
    return _serialize(() async {
      // Clearing the persisted token must also work after a startup read error.
      try {
        await future;
      } catch (_) {
        // The delete operation below remains available for recovery.
      }
      await _clear(reason);
    });
  }

  Future<bool> signOutIfCurrent({
    required String token,
    required int generation,
    SignOutReason reason = SignOutReason.expired,
  }) async {
    return _serialize(() async {
      try {
        await future;
      } catch (_) {
        return false;
      }

      final current = state.asData?.value;
      if (current is! SignedIn ||
          current.token != token ||
          current.generation != generation) {
        return false;
      }

      await _clear(reason);
      return true;
    });
  }

  bool isCurrent(SignedIn expected) {
    final current = state.asData?.value;
    return current is SignedIn &&
        current.token == expected.token &&
        current.generation == expected.generation;
  }

  Future<void> _clear(SignOutReason reason) async {
    await ref.read(tokenStorageProvider).deleteToken();
    _generation++;
    state = AsyncData(SignedOut(reason: reason));
  }

  Future<T> _serialize<T>(Future<T> Function() operation) {
    final result = _mutationQueue.then((_) => operation());
    _mutationQueue = result.then<void>(
      (_) {},
      onError: (Object _, StackTrace _) {},
    );
    return result;
  }
}
