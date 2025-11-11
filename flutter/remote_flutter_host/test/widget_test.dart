// This is a basic Flutter widget test.
//
// To perform an interaction with a widget in your test, use the WidgetTester
// utility in the flutter_test package. For example, you can send tap and scroll
// gestures. You can also use WidgetTester to find child widgets in the widget
// tree, read text, and verify that the values of widget properties are correct.

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';

import 'package:remote_flutter_host/main.dart';

void main() {
  const libraryText = '''
import core.widgets;

widget root = Text(
  text: [
    "Remote host ready",
  ],
  textDirection: "ltr",
);
''';

  const dataJson = '{"catalog": {}}';

  testWidgets('Remote catalog screen renders fetched library',
      (WidgetTester tester) async {
    final mockClient = MockClient((request) async {
      if (request.url.path.endsWith('/library')) {
        return http.Response(libraryText, 200);
      }
      if (request.url.path.endsWith('/data')) {
        return http.Response(dataJson, 200,
            headers: {'content-type': 'application/json'});
      }

      return http.Response('Not found', 404);
    });

    await tester.pumpWidget(MaterialApp(
      home: RemoteCatalogScreen(
        serverBaseUrl: 'http://localhost',
        clientFactory: () => mockClient,
      ),
    ));

    await tester.pump(); // start initial frame
    await tester.pump(const Duration(milliseconds: 50));
    await tester.pumpAndSettle();

    expect(find.text('Remote host ready'), findsOneWidget);

    await tester.pumpWidget(const SizedBox());
    await tester.pump();
  });
}
