import 'package:flutter/foundation.dart';

/// Endereço da AgendaBella.Api.
///
/// Pode ser trocado sem mexer no código, na hora de rodar:
///   flutter run --dart-define=API_URL=http://192.168.0.10:5218
class Config {
  static const String _definidoNoComando = String.fromEnvironment('API_URL');

  static String get apiUrl {
    if (_definidoNoComando.isNotEmpty) return _definidoNoComando;

    // O emulador Android enxerga o computador pelo endereço especial 10.0.2.2
    // ("localhost" dentro do emulador é o próprio emulador).
    if (!kIsWeb && defaultTargetPlatform == TargetPlatform.android) {
      return 'http://10.0.2.2:5218';
    }

    // Navegador (flutter run -d chrome) e Windows: a Api está na mesma máquina.
    return 'http://localhost:5218';
  }
}
