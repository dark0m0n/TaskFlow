import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:taskflow/core/network/api_client.dart';

final authRepositoryProvider = Provider<AuthRepository>((ref) {
  return AuthRepository(ref.watch(dioProvider));
});

class AuthRepository {
  AuthRepository(this._dio);

  final Dio _dio;

  Future<void> register(String email, String password) async {
    await _dio.post<void>(
      'account/register',
      data: {'email': email, 'password': password},
    );
  }

  Future<String> login(String email, String password) async {
    final response = await _dio.post<Map<String, dynamic>>(
      'account/login',
      data: {'email': email, 'password': password},
    );
    final token = response.data?['token'];
    if (token is! String || token.isEmpty) {
      throw const FormatException('Login response did not contain a token');
    }
    return token;
  }
}
