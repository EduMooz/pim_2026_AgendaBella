// Teste automático (rode com: flutter test).
// Confere as funções de formatação usadas nas telas.
import 'package:agendabella_app/util/formatos.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  test('formata data no padrão brasileiro', () {
    expect(dataBr('2026-10-15'), '15/10/2026');
    expect(isoData(DateTime(2026, 1, 5)), '2026-01-05');
  });

  test('formata valores em reais', () {
    expect(moeda(45.5), 'R\$ 45,50');
    expect(moeda(120), 'R\$ 120,00');
  });

  test('acrescenta a extensão da imagem quando falta', () {
    expect(nomeComExtensao('foto.png', [0x89, 0x50, 0x4E, 0x47]), 'foto.png');
    expect(nomeComExtensao('foto', [0x89, 0x50, 0x4E, 0x47]), 'foto.png');
    expect(nomeComExtensao('foto', [0xFF, 0xD8, 0xFF]), 'foto.jpg');
  });
}
