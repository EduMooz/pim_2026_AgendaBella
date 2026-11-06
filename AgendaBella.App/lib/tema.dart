import 'package:flutter/material.dart';

// Mesmas cores do site (tons de rosa com contraste acessível, WCAG AA).
const corRosa = Color(0xFFB5476A);
const corVinho = Color(0xFF7A1F3D);
const corFundo = Color(0xFFFFF8FB);
const corRosaClaro = Color(0xFFFFE4EC);

ThemeData temaAgendaBella() {
  final esquema = ColorScheme.fromSeed(seedColor: corRosa, primary: corRosa, surface: corFundo);

  return ThemeData(
    useMaterial3: true,
    colorScheme: esquema,
    scaffoldBackgroundColor: corFundo,
    // Botões com no mínimo 48 x 48 pontos: tamanho de toque recomendado para acessibilidade.
    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(
        backgroundColor: corRosa,
        foregroundColor: Colors.white,
        minimumSize: const Size(48, 48),
        shape: const StadiumBorder(),
      ),
    ),
    outlinedButtonTheme: OutlinedButtonThemeData(
      style: OutlinedButton.styleFrom(
        foregroundColor: corRosa,
        minimumSize: const Size(48, 48),
        shape: const StadiumBorder(),
        side: const BorderSide(color: corRosa, width: 1.5),
      ),
    ),
  );
}

/// Aparência padrão dos campos de formulário.
InputDecoration campo(String rotulo, {IconData? icone, String? dica}) => InputDecoration(
      labelText: rotulo,
      helperText: dica,
      prefixIcon: icone == null ? null : Icon(icone),
      filled: true,
      fillColor: Colors.white,
      border: OutlineInputBorder(borderRadius: BorderRadius.circular(14)),
    );
