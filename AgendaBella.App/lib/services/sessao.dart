import 'dart:convert';

import 'package:shared_preferences/shared_preferences.dart';

import '../models/modelos.dart';

/// Guarda quem está logado e o token JWT, no próprio aparelho (shared_preferences).
/// Assim a cliente não precisa digitar a senha toda vez que abre o app.
class Sessao {
  static const _chaveToken = 'token';
  static const _chaveUsuario = 'usuario';
  static const _chaveExpira = 'expiraEm';

  static String? token;
  static Usuario? usuario;

  static bool get logado => token != null && usuario != null;

  /// Chamado uma vez, quando o app abre (no main.dart).
  static Future<void> carregar() async {
    final prefs = await SharedPreferences.getInstance();
    final expira = DateTime.tryParse(prefs.getString(_chaveExpira) ?? '');
    final json = prefs.getString(_chaveUsuario);

    // Token vencido ou dados incompletos: começa deslogado.
    if (expira == null || expira.isBefore(DateTime.now().toUtc()) || json == null) {
      await sair();
      return;
    }

    token = prefs.getString(_chaveToken);
    usuario = Usuario.fromJson(jsonDecode(json) as Map<String, dynamic>);
  }

  static Future<void> salvar(String novoToken, Usuario novoUsuario, DateTime expiraEm) async {
    token = novoToken;
    usuario = novoUsuario;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_chaveToken, novoToken);
    await prefs.setString(_chaveUsuario, jsonEncode(novoUsuario.toJson()));
    await prefs.setString(_chaveExpira, expiraEm.toUtc().toIso8601String());
  }

  static Future<void> sair() async {
    token = null;
    usuario = null;
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_chaveToken);
    await prefs.remove(_chaveUsuario);
    await prefs.remove(_chaveExpira);
  }
}
