class BoardSummary {
  const BoardSummary({
    required this.id,
    required this.title,
    required this.description,
  });

  final int id;
  final String title;
  final String description;

  factory BoardSummary.fromJson(Map<String, dynamic> json) {
    return BoardSummary(
      id: json['id'] as int,
      title: json['title'] as String,
      description: json['description'] as String? ?? '',
    );
  }
}

class BoardDetail {
  const BoardDetail({
    required this.id,
    required this.title,
    required this.description,
    required this.columns,
  });

  final int id;
  final String title;
  final String description;
  final List<BoardColumn> columns;

  factory BoardDetail.fromJson(Map<String, dynamic> json) {
    final columns =
        (json['columns'] as List<dynamic>? ?? [])
            .map((item) => BoardColumn.fromJson(item as Map<String, dynamic>))
            .toList()
          ..sort((a, b) => a.order.compareTo(b.order));

    return BoardDetail(
      id: json['id'] as int,
      title: json['title'] as String,
      description: json['description'] as String? ?? '',
      columns: columns,
    );
  }
}

class BoardColumn {
  const BoardColumn({
    required this.id,
    required this.title,
    required this.order,
    required this.cards,
  });

  final int id;
  final String title;
  final int order;
  final List<BoardCard> cards;

  factory BoardColumn.fromJson(Map<String, dynamic> json) {
    final cards =
        (json['cards'] as List<dynamic>? ?? [])
            .map((item) => BoardCard.fromJson(item as Map<String, dynamic>))
            .toList()
          ..sort((a, b) => a.order.compareTo(b.order));

    return BoardColumn(
      id: json['id'] as int,
      title: json['title'] as String,
      order: json['order'] as int,
      cards: cards,
    );
  }
}

class BoardCard {
  const BoardCard({
    required this.id,
    required this.title,
    required this.order,
    required this.priority,
  });

  final int id;
  final String title;
  final int order;
  final int priority;

  factory BoardCard.fromJson(Map<String, dynamic> json) {
    return BoardCard(
      id: json['id'] as int,
      title: json['title'] as String,
      order: json['order'] as int,
      priority: json['priority'] as int,
    );
  }
}
