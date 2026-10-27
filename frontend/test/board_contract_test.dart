import 'package:flutter_test/flutter_test.dart';
import 'package:taskflow/features/boards/data/board_models.dart';

void main() {
  test('board API response keeps server order for columns and cards', () {
    final board = BoardDetail.fromJson({
      'id': 7,
      'title': 'Sprint',
      'description': '',
      'columns': [
        {
          'id': 2,
          'title': 'Done',
          'order': 1,
          'cards': <Map<String, Object>>[],
        },
        {
          'id': 1,
          'title': 'To do',
          'order': 0,
          'cards': [
            {'id': 10, 'title': 'Second', 'order': 2, 'priority': 1},
            {'id': 9, 'title': 'First', 'order': 1, 'priority': 2},
          ],
        },
      ],
    });

    expect(board.columns.map((column) => column.id), [1, 2]);
    expect(board.columns.first.cards.map((card) => card.id), [9, 10]);
  });
}
