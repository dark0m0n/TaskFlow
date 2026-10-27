import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:taskflow/core/auth/session_store.dart';
import 'package:taskflow/core/config/app_config.dart';

final dioProvider = Provider<Dio>((ref) {
  final dio = Dio(
    BaseOptions(
      baseUrl: AppConfig.apiBaseUrl,
      connectTimeout: const Duration(seconds: 10),
      receiveTimeout: const Duration(seconds: 15),
      contentType: Headers.jsonContentType,
    ),
  );

  dio.interceptors.add(
    InterceptorsWrapper(
      onRequest: (options, handler) async {
        try {
          final session = await ref.read(sessionStoreProvider.future);
          if (session is SignedIn) {
            options.headers['Authorization'] = 'Bearer ${session.token}';
          }
          handler.next(options);
        } catch (error) {
          handler.reject(DioException(requestOptions: options, error: error));
        }
      },
    ),
  );

  ref.onDispose(() => dio.close(force: true));
  return dio;
});

String userFacingError(Object error) {
  if (error is DioException) {
    switch (error.response?.statusCode) {
      case 400:
        return 'Перевірте введені дані.';
      case 401:
        return 'Невірні дані входу або сесія завершилася.';
      case 403:
        return 'У вас немає доступу до цієї дії.';
      case 404:
        return 'Дані не знайдено.';
    }
    if (error.response == null) {
      return 'Немає з’єднання з сервером. Перевірте адресу API та мережу.';
    }
  }
  return 'Сталася помилка. Спробуйте ще раз.';
}
