import 'dart:async';
import 'dart:convert';
import 'dart:typed_data';

import 'package:http/http.dart' as http;

import '../config.dart';
import '../models/modelos.dart';
import '../util/formatos.dart';
import 'sessao.dart';

/// Erro devolvido pela Api (ou falta de conexão), com uma mensagem pronta para a tela.
class ApiException implements Exception {
  final String mensagem;
  final int status;

  ApiException(this.mensagem, this.status);

  @override
  String toString() => mensagem;
}

/// ÚNICA classe do app que conversa com a AgendaBella.Api.
/// As telas nunca montam URL nem JSON: chamam os métodos daqui.
/// (É o mesmo papel que o Repositorio tem no C#, só que do lado do aplicativo.)
class ApiService {
  /// Definido no main.dart: o que fazer quando o token vence (voltar para o login).
  static void Function()? aoExpirarSessao;

  // ------------------------------------------------------------------ apoio

  Uri _uri(String caminho, [Map<String, String>? query]) =>
      Uri.parse('${Config.apiUrl}$caminho').replace(queryParameters: query);

  /// Cabeçalhos de toda chamada. O token vai em "Authorization: Bearer ...".
  Map<String, String> _cabecalhos({bool json = true}) => {
        if (json) 'Content-Type': 'application/json',
        'Accept': 'application/json',
        if (Sessao.token != null) 'Authorization': 'Bearer ${Sessao.token}',
      };

  /// Executa a chamada transformando falta de rede em uma mensagem clara.
  Future<http.Response> _enviar(Future<http.Response> Function() chamada) async {
    try {
      return await chamada().timeout(const Duration(seconds: 20));
    } on TimeoutException {
      throw ApiException('O servidor demorou para responder. Verifique se a Api está rodando.', 0);
    } catch (_) {
      throw ApiException('Sem conexão com o servidor (${Config.apiUrl}). Verifique se a Api está rodando.', 0);
    }
  }

  /// Lê a resposta: 2xx devolve o JSON; qualquer outro código vira ApiException.
  Future<dynamic> _tratar(http.Response resposta) async {
    dynamic corpo;
    if (resposta.bodyBytes.isNotEmpty) {
      try {
        corpo = jsonDecode(utf8.decode(resposta.bodyBytes));
      } catch (_) {
        corpo = null; // resposta que não é JSON
      }
    }

    if (resposta.statusCode >= 200 && resposta.statusCode < 300) return corpo;

    // 401 com alguém logado = token vencido ou inválido: encerra a sessão.
    if (resposta.statusCode == 401 && Sessao.token != null) {
      await Sessao.sair();
      aoExpirarSessao?.call();
      throw ApiException('Sua sessão expirou. Entre novamente.', 401);
    }

    throw ApiException(_mensagemDeErro(corpo, resposta.statusCode), resposta.statusCode);
  }

  /// A Api devolve erros em dois formatos:
  ///   { "erro": "mensagem" }                       -> regra de negócio (filtro da Api)
  ///   { "errors": { "Campo": ["mensagem"] } }      -> validação dos DTOs ([ApiController])
  static String _mensagemDeErro(dynamic corpo, int status) {
    if (corpo is Map<String, dynamic>) {
      final erro = corpo['erro'];
      if (erro is String) return erro;

      final erros = corpo['errors'];
      if (erros is Map<String, dynamic>) {
        final mensagens = erros.values
            .whereType<List<dynamic>>()
            .expand((lista) => lista)
            .map((m) => m.toString())
            .toList();
        if (mensagens.isNotEmpty) return mensagens.join('\n');
      }
    }
    if (status == 403) return 'Esta conta não tem acesso a esta função do aplicativo.';
    if (status == 404) return 'Registro não encontrado.';
    return 'Não foi possível concluir (erro $status). Tente novamente.';
  }

  List<T> _lista<T>(dynamic json, T Function(Map<String, dynamic>) converter) =>
      (json as List<dynamic>).map((e) => converter(e as Map<String, dynamic>)).toList();

  // ------------------------------------------------------------------ login e cadastro

  Future<Usuario> login(String email, String senha) async {
    final r = await _enviar(() => http.post(
          _uri('/api/auth/login'),
          headers: _cabecalhos(),
          body: jsonEncode({'email': email, 'senha': senha}),
        ));
    return _guardarSessao(await _tratar(r) as Map<String, dynamic>);
  }

  Future<Usuario> cadastrar({
    required String nome,
    required String email,
    required String senha,
    required String telefone,
  }) async {
    final r = await _enviar(() => http.post(
          _uri('/api/auth/cadastro'),
          headers: _cabecalhos(),
          body: jsonEncode({'nome': nome, 'email': email, 'senha': senha, 'telefone': telefone}),
        ));
    return _guardarSessao(await _tratar(r) as Map<String, dynamic>);
  }

  Future<Usuario> _guardarSessao(Map<String, dynamic> dados) async {
    final usuario = Usuario.fromJson(dados['usuario'] as Map<String, dynamic>);

    // O app é da cliente; a profissional usa o painel do site.
    if (usuario.perfil != 'Cliente') {
      throw ApiException('O aplicativo é para clientes. A profissional usa o painel no site.', 403);
    }

    await Sessao.salvar(dados['token'] as String, usuario, DateTime.parse(dados['expiraEm'] as String));
    return usuario;
  }

  // ------------------------------------------------------------------ serviços

  Future<List<Servico>> servicos() async {
    final r = await _enviar(() => http.get(_uri('/api/servicos'), headers: _cabecalhos()));
    return _lista(await _tratar(r), Servico.fromJson);
  }

  // ------------------------------------------------------------------ agendamentos

  /// Horários livres: a Api executa a stored procedure sp_Horarios_Disponiveis.
  Future<List<HorarioDisponivel>> horarios(DateTime data, int idServico, {int? ignorar}) async {
    final query = {
      'data': isoData(data),
      'idServico': '$idServico',
      if (ignorar != null) 'ignorar': '$ignorar',
    };
    final r = await _enviar(() => http.get(_uri('/api/agendamentos/horarios', query), headers: _cabecalhos()));
    return _lista(await _tratar(r), HorarioDisponivel.fromJson);
  }

  Future<List<Agendamento>> meusAgendamentos() async {
    final r = await _enviar(() => http.get(_uri('/api/agendamentos'), headers: _cabecalhos()));
    return _lista(await _tratar(r), Agendamento.fromJson);
  }

  Future<Agendamento> criarAgendamento({
    required int idServico,
    required DateTime data,
    required String horario,
    String? observacao,
  }) async {
    final r = await _enviar(() => http.post(
          _uri('/api/agendamentos'),
          headers: _cabecalhos(),
          body: jsonEncode({
            'idServico': idServico,
            'data': isoData(data),
            'horario': horario,
            'observacao': observacao,
          }),
        ));
    return Agendamento.fromJson(await _tratar(r) as Map<String, dynamic>);
  }

  /// Envia a foto de inspiração (multipart/form-data, campo "imagem").
  Future<Agendamento> enviarImagem(int idAgendamento, Uint8List bytes, String nomeArquivo) async {
    final r = await _enviar(() async {
      final requisicao = http.MultipartRequest('POST', _uri('/api/agendamentos/$idAgendamento/imagem'))
        ..headers.addAll(_cabecalhos(json: false))
        ..files.add(http.MultipartFile.fromBytes('imagem', bytes, filename: nomeArquivo));
      return http.Response.fromStream(await requisicao.send());
    });
    return Agendamento.fromJson(await _tratar(r) as Map<String, dynamic>);
  }

  Future<void> reagendar(int id, DateTime data, String horario) async {
    final r = await _enviar(() => http.patch(
          _uri('/api/agendamentos/$id/reagendar'),
          headers: _cabecalhos(),
          body: jsonEncode({'data': isoData(data), 'horario': horario}),
        ));
    await _tratar(r);
  }

  Future<void> cancelar(int id) async {
    final r = await _enviar(() => http.patch(_uri('/api/agendamentos/$id/cancelar'), headers: _cabecalhos()));
    await _tratar(r);
  }

  Future<List<EventoHistorico>> historico(int id) async {
    final r = await _enviar(() => http.get(_uri('/api/agendamentos/$id/historico'), headers: _cabecalhos()));
    return _lista(await _tratar(r), EventoHistorico.fromJson);
  }

  // ------------------------------------------------------------------ lista de espera

  Future<List<ItemListaEspera>> listaEspera() async {
    final r = await _enviar(() => http.get(_uri('/api/lista-espera'), headers: _cabecalhos()));
    return _lista(await _tratar(r), ItemListaEspera.fromJson);
  }

  Future<ItemListaEspera> entrarListaEspera(int idServico, DateTime data, String? horario) async {
    final r = await _enviar(() => http.post(
          _uri('/api/lista-espera'),
          headers: _cabecalhos(),
          body: jsonEncode({'idServico': idServico, 'dataDesejada': isoData(data), 'horarioDesejado': horario}),
        ));
    return ItemListaEspera.fromJson(await _tratar(r) as Map<String, dynamic>);
  }

  Future<void> sairListaEspera(int id) async {
    final r = await _enviar(() => http.delete(_uri('/api/lista-espera/$id'), headers: _cabecalhos()));
    await _tratar(r);
  }

  // ------------------------------------------------------------------ perfil

  Future<Preferencia> preferencias() async {
    final r = await _enviar(() => http.get(_uri('/api/perfil/preferencias'), headers: _cabecalhos()));
    return Preferencia.fromJson(await _tratar(r) as Map<String, dynamic>);
  }

  Future<void> salvarPreferencias(Preferencia preferencia) async {
    final r = await _enviar(() => http.put(
          _uri('/api/perfil/preferencias'),
          headers: _cabecalhos(),
          body: jsonEncode(preferencia.toJson()),
        ));
    await _tratar(r);
  }

  Future<SaldoPontos> pontos() async {
    final r = await _enviar(() => http.get(_uri('/api/perfil/pontos'), headers: _cabecalhos()));
    return SaldoPontos.fromJson(await _tratar(r) as Map<String, dynamic>);
  }
}

/// Instância única usada por todas as telas.
final api = ApiService();
