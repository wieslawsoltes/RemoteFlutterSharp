import 'dart:async';
import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:rfw/formats.dart';
import 'package:rfw/rfw.dart';

const String _serverBaseUrl = String.fromEnvironment(
  'REMOTE_FLUTTER_SERVER',
  defaultValue: 'http://localhost:5000',
);

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const RemoteFlutterHostApp());
}

class RemoteFlutterHostApp extends StatelessWidget {
  const RemoteFlutterHostApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Remote Flutter Host',
      theme: ThemeData(
          colorSchemeSeed: Colors.indigo, brightness: Brightness.light),
      home: const RemoteCatalogScreen(),
      debugShowCheckedModeBanner: false,
    );
  }
}

class RemoteCatalogScreen extends StatefulWidget {
  const RemoteCatalogScreen({super.key});

  @override
  State<RemoteCatalogScreen> createState() => _RemoteCatalogScreenState();
}

class _RemoteCatalogScreenState extends State<RemoteCatalogScreen> {
  static const LibraryName _coreWidgets =
      LibraryName(<String>['core', 'widgets']);
  static const LibraryName _materialWidgets = LibraryName(<String>['material']);
  static const LibraryName _remoteLibrary = LibraryName(<String>['catalog']);
  static const FullyQualifiedWidgetName _catalogRoot =
      FullyQualifiedWidgetName(_remoteLibrary, 'root');
  static const FullyQualifiedWidgetName _productDetail =
      FullyQualifiedWidgetName(_remoteLibrary, 'ProductDetailScreen');
  static const FullyQualifiedWidgetName _cartRoot =
      FullyQualifiedWidgetName(_remoteLibrary, 'CartScreen');
  static const FullyQualifiedWidgetName _managerRoot =
      FullyQualifiedWidgetName(_remoteLibrary, 'ProductManagerScreen');

  final Runtime _runtime = Runtime();
  final DynamicContent _content = DynamicContent();

  FullyQualifiedWidgetName? _rootWidget;
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _runtime.update(_coreWidgets, createCoreWidgets());
    _runtime.update(_materialWidgets, createMaterialWidgets());
    unawaited(_refreshRemoteUi());
  }

  Future<void> _refreshRemoteUi() async {
    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final libraryUri = Uri.parse('$_serverBaseUrl/api/rfw/library');
      final dataUri = Uri.parse('$_serverBaseUrl/api/rfw/data');

      final responses = await Future.wait<http.Response>([
        http.get(libraryUri),
        http.get(dataUri),
      ]).timeout(const Duration(seconds: 5));

      final libraryResponse = responses[0];
      final dataResponse = responses[1];

      if (libraryResponse.statusCode != 200) {
        throw Exception(
            'Failed to load remote library: ${libraryResponse.statusCode}');
      }

      if (dataResponse.statusCode != 200) {
        throw Exception(
            'Failed to load remote data: ${dataResponse.statusCode}');
      }

      final remoteLibrary = parseLibraryFile(libraryResponse.body);
      _runtime.update(_remoteLibrary, remoteLibrary);

      final decoded = jsonDecode(dataResponse.body);
      if (decoded is! Map<String, dynamic>) {
        throw Exception('Unexpected data payload format.');
      }

      for (final entry in decoded.entries) {
        final value = entry.value;
        if (value is Map<String, dynamic>) {
          _content.update(entry.key, _convertMap(value));
        } else {
          throw Exception('Dynamic content root values must be objects.');
        }
      }

      setState(() {
        _rootWidget = _catalogRoot;
        _loading = false;
      });
    } catch (error, stackTrace) {
      debugPrint('Failed to refresh remote UI: $error\n$stackTrace');
      setState(() {
        _error = error.toString();
        _loading = false;
      });
    }
  }

  DynamicMap _convertMap(Map<String, dynamic> source) {
    final Map<String, Object> result = <String, Object>{};
    for (final entry in source.entries) {
      result[entry.key] = _convertValue(entry.value);
    }
    return result;
  }

  Object _convertValue(Object? value) {
    if (value == null) {
      throw Exception(
          'Remote Flutter Widgets data does not permit null values.');
    }

    if (value is int || value is bool || value is String) {
      return value;
    }

    if (value is double) {
      return value;
    }

    if (value is num) {
      return value.toDouble();
    }

    if (value is Map<String, dynamic>) {
      return _convertMap(value);
    }

    if (value is List) {
      return value.map(_convertValue).toList();
    }

    throw Exception('Unsupported dynamic content type: ${value.runtimeType}');
  }

  Future<Map<String, dynamic>?> _postEvent(
      String name, DynamicMap arguments) async {
    final uri = Uri.parse('$_serverBaseUrl/api/rfw/event');
    final payload = jsonEncode(<String, Object>{
      'name': name,
      'arguments': arguments,
    });

    try {
      final response = await http
          .post(uri,
              headers: const {'Content-Type': 'application/json'},
              body: payload)
          .timeout(const Duration(seconds: 3));

      if (response.statusCode >= 400) {
        debugPrint(
            'Remote event failed: ${response.statusCode} ${response.body}');
        return null;
      }
      if (response.body.isEmpty) {
        return null;
      }

      try {
        final decoded = jsonDecode(response.body);
        if (decoded is Map<String, dynamic>) {
          return decoded;
        }
      } catch (error, stackTrace) {
        debugPrint(
            'Failed to decode event response for $name: $error\n$stackTrace');
      }
    } catch (error, stackTrace) {
      debugPrint('Failed to send remote event $name: $error\n$stackTrace');
    }

    return null;
  }

  Future<Map<String, dynamic>> _fetchProductDetailJson(int id) async {
    final uri = Uri.parse('$_serverBaseUrl/api/rfw/product/$id');
    final response = await http.get(uri).timeout(const Duration(seconds: 5));

    if (response.statusCode != 200) {
      throw Exception('Product detail request failed (${response.statusCode})');
    }

    final decoded = jsonDecode(response.body);
    if (decoded is! Map<String, dynamic>) {
      throw Exception('Unexpected product detail payload.');
    }

    return decoded;
  }

  Future<void> _loadProductDetail(int id,
      {Map<String, dynamic>? initialDetail}) async {
    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final Map<String, dynamic> detailJson =
          initialDetail ?? await _fetchProductDetailJson(id);
      final detail = _convertMap(detailJson);
      if (!mounted) {
        return;
      }

      _content.update('detail', detail);
      setState(() {
        _rootWidget = _productDetail;
        _loading = false;
      });
    } catch (error, stackTrace) {
      debugPrint('Failed to load product detail: $error\n$stackTrace');
      if (!mounted) {
        return;
      }

      setState(() {
        _loading = false;
        _rootWidget = _catalogRoot;
      });

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Unable to load product detail')),
      );
    }
  }

  Future<void> _dispatchRemoteEvent(String name, DynamicMap arguments) async {
    final Map<String, dynamic>? response = await _postEvent(name, arguments);
    await _handleRemoteEvent(name, arguments, response);
  }

    Future<void> _handleRemoteEvent(
        String name, DynamicMap arguments, Map<String, dynamic>? response) async {
      switch (name) {
        case 'catalog.select':
        final int? productId = _extractProductId(arguments);
        if (productId == null) {
          debugPrint(
              'catalog.select missing integer id payload: ${arguments['id']}');
          return;
        }

        if (response != null && response.containsKey('error')) {
          if (mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(content: Text(response['error'].toString())),
            );
          }
          return;
        }

        await _loadProductDetail(productId, initialDetail: response);
        break;
      case 'catalog.back':
        setState(() {
          _rootWidget = _catalogRoot;
          _loading = false;
          _error = null;
        });
        break;
      case 'catalog.manage':
        setState(() {
          _rootWidget = _managerRoot;
          _loading = false;
          _error = null;
        });
        break;
      case 'catalog.cart':
        setState(() {
          _rootWidget = _cartRoot;
          _loading = false;
          _error = null;
        });
        break;
      case 'catalog.interest':
        _showFeedback(response?['message']?.toString() ?? 'Interest recorded on the server.');
        break;
      case 'catalog.create':
      case 'catalog.update':
      case 'catalog.delete':
        _applyResponseData(response);
        _showFeedback(response?['message']?.toString() ?? 'Catalog content updated.');
        break;
      case 'catalog.buy':
        final dynamic nameValue = arguments['name'];
        final String fallbackName =
            nameValue is String && nameValue.trim().isNotEmpty
                ? nameValue
                : 'Product';
        final String productName =
            (response != null && response['product'] is String)
                ? (response['product'] as String)
                : fallbackName;
        _showFeedback('Added $productName to cart');
        break;
      default:
        break;
    }
  }

  void _applyResponseData(Map<String, dynamic>? response) {
    if (response == null) {
      return;
    }
    final dynamic data = response['data'];
    if (data is Map<String, dynamic>) {
      for (final entry in data.entries) {
        final value = entry.value;
        if (value is Map<String, dynamic>) {
          _content.update(entry.key, _convertMap(value));
        }
      }
    }
  }

  void _showFeedback(String message) {
    if (!mounted) {
      return;
    }
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message)),
    );
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Scaffold(
        body: Center(child: CircularProgressIndicator()),
      );
    }

    if (_error != null) {
      return Scaffold(
        body: Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.error_outline,
                  size: 48, color: Colors.redAccent),
              const SizedBox(height: 16),
              Text(
                'Failed to load remote UI',
                style: Theme.of(context).textTheme.titleLarge,
              ),
              const SizedBox(height: 8),
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 24),
                child: Text(
                  _error!,
                  textAlign: TextAlign.center,
                ),
              ),
              const SizedBox(height: 24),
              FilledButton(
                onPressed: _refreshRemoteUi,
                child: const Text('Retry'),
              ),
            ],
          ),
        ),
      );
    }

    final root = _rootWidget;
    if (root == null) {
      return Scaffold(
        body: Center(
          child: FilledButton(
            onPressed: _refreshRemoteUi,
            child: const Text('Initialize Remote UI'),
          ),
        ),
      );
    }

    return Scaffold(
      body: AnimatedSwitcher(
        duration: const Duration(milliseconds: 500),
        switchInCurve: Curves.easeOut,
        switchOutCurve: Curves.easeIn,
        child: KeyedSubtree(
          key: ValueKey(root),
          child: RemoteWidget(
            runtime: _runtime,
            data: _content,
            widget: root,
            onEvent: (String name, DynamicMap arguments) {
              unawaited(_dispatchRemoteEvent(name, arguments));
            },
          ),
        ),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: _refreshRemoteUi,
        child: const Icon(Icons.refresh),
      ),
    );
  }

  int? _extractProductId(DynamicMap arguments) {
    final dynamic raw = arguments['id'];
    if (raw is int) {
      return raw;
    }
    if (raw is num) {
      return raw.toInt();
    }
    if (raw is String) {
      return int.tryParse(raw);
    }
    return null;
  }
}
