import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:taskflow/core/network/api_client.dart';
import 'package:taskflow/features/boards/data/board_models.dart';

final boardsRepositoryProvider = Provider<BoardsRepository>(
  (ref) => BoardsRepository(ref.watch(dioProvider)),
);

class BoardsRepository {
  BoardsRepository(this._dio);

  final Dio _dio;

  Future<List<BoardSummary>> getBoards() async {
    final response = await _dio.get<List<dynamic>>('boards');
    return (response.data ?? [])
        .map((item) => BoardSummary.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<BoardDetail> getBoard(int id) async {
    final response = await _dio.get<Map<String, dynamic>>('boards/$id');
    final data = response.data;
    if (data == null) throw const FormatException('Empty board response');
    return BoardDetail.fromJson(data);
  }
}
