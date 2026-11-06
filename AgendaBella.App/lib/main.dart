import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

import 'screens/home_screen.dart';
import 'screens/login_screen.dart';
import 'services/api_service.dart';
import 'services/sessao.dart';
import 'tema.dart';

/// Chave do navegador: permite trocar de tela a partir de fora de um widget
/// (usada quando o token vence no meio do uso).
final navegadorGlobal = GlobalKey<NavigatorState>();

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Recupera o login salvo no aparelho (se o token ainda for válido).
  await Sessao.carregar();

  ApiService.aoExpirarSessao = () {
    navegadorGlobal.currentState?.pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const LoginScreen()),
      (rota) => false,
    );
  };

  runApp(const AgendaBellaApp());
}

class AgendaBellaApp extends StatelessWidget {
  const AgendaBellaApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Agenda Bella',
      debugShowCheckedModeBanner: false,
      navigatorKey: navegadorGlobal,
      theme: temaAgendaBella(),
      // Português do Brasil no calendário, no relógio e nos textos padrão.
      locale: const Locale('pt', 'BR'),
      supportedLocales: const [Locale('pt', 'BR')],
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      home: Sessao.logado ? const HomeScreen() : const LoginScreen(),
    );
  }
}
