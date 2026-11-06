// Funções pequenas de formatação usadas em várias telas.

String _doisDigitos(int n) => n.toString().padLeft(2, '0');

/// "2026-10-15" -> "15/10/2026"
String dataBr(String iso) {
  final partes = iso.split('-');
  if (partes.length < 3 || partes[2].length < 2) return iso;
  return '${partes[2].substring(0, 2)}/${partes[1]}/${partes[0]}';
}

/// DateTime -> "2026-10-15" (formato que a Api espera)
String isoData(DateTime d) =>
    '${d.year.toString().padLeft(4, '0')}-${_doisDigitos(d.month)}-${_doisDigitos(d.day)}';

/// DateTime -> "15/10/2026 14:30"
String dataHoraBr(DateTime d) =>
    '${_doisDigitos(d.day)}/${_doisDigitos(d.month)}/${d.year} ${_doisDigitos(d.hour)}:${_doisDigitos(d.minute)}';

/// 45.5 -> "R$ 45,50"
String moeda(double valor) => 'R\$ ${valor.toStringAsFixed(2).replaceAll('.', ',')}';

/// Texto do status com acento (no banco fica sem acento: "Concluido").
String textoStatus(String status) => status == 'Concluido' ? 'Concluído' : status;

/// Garante que o nome do arquivo termine com .jpg, .png ou .webp,
/// olhando os primeiros bytes da imagem (a Api confere a extensão).
String nomeComExtensao(String nome, List<int> bytes) {
  final minusculo = nome.toLowerCase();
  if (minusculo.endsWith('.jpg') || minusculo.endsWith('.jpeg') ||
      minusculo.endsWith('.png') || minusculo.endsWith('.webp')) {
    return nome;
  }
  if (bytes.length >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50) return '$nome.png';
  if (bytes.length >= 12 && bytes[8] == 0x57 && bytes[9] == 0x45) return '$nome.webp';
  return '$nome.jpg';
}
