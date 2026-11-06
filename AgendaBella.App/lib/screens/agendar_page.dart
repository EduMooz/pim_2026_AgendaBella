import 'dart:typed_data';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';

import '../models/modelos.dart';
import '../services/api_service.dart';
import '../tema.dart';
import '../util/formatos.dart';
import '../widgets/componentes.dart';
import '../widgets/seletor_data_horario.dart';

/// Aba "Agendar": serviço -> data -> horário livre -> observação e foto -> confirmar.
class AgendarPage extends StatefulWidget {
  final int? idServicoInicial;
  final VoidCallback aoConcluir;

  const AgendarPage({super.key, this.idServicoInicial, required this.aoConcluir});

  @override
  State<AgendarPage> createState() => _AgendarPageState();
}

class _AgendarPageState extends State<AgendarPage> {
  static const _limiteImagem = 5 * 1024 * 1024; // 5 MB, o mesmo limite da Api

  List<Servico> _servicos = [];
  Servico? _servico;
  DateTime? _data;
  String? _horario;
  final _observacao = TextEditingController();
  Uint8List? _imagem;
  String? _nomeImagem;

  bool _carregando = true;
  bool _enviando = false;
  String? _erro;

  @override
  void initState() {
    super.initState();
    _carregarServicos();
  }

  @override
  void dispose() {
    _observacao.dispose();
    super.dispose();
  }

  Future<void> _carregarServicos() async {
    setState(() {
      _carregando = true;
      _erro = null;
    });
    try {
      final lista = await api.servicos();
      if (!mounted) return;
      setState(() {
        _servicos = lista;
        _servico = lista.where((s) => s.id == widget.idServicoInicial).firstOrNull;
      });
    } on ApiException catch (e) {
      if (mounted) setState(() => _erro = e.mensagem);
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  void _escolherServico(Servico s) {
    setState(() {
      _servico = s;
      _data = null; // trocar o serviço muda a duração, então a data/horário são escolhidos de novo
      _horario = null;
    });
  }

  Future<void> _escolherFoto(ImageSource origem) async {
    final arquivo = await ImagePicker().pickImage(source: origem, maxWidth: 1600, imageQuality: 85);
    if (arquivo == null) return;

    final bytes = await arquivo.readAsBytes();
    if (!mounted) return;
    if (bytes.length > _limiteImagem) {
      avisar(context, 'A imagem pode ter no máximo 5 MB.', erro: true);
      return;
    }
    setState(() {
      _imagem = bytes;
      _nomeImagem = nomeComExtensao(arquivo.name, bytes);
    });
  }

  Future<void> _entrarNaListaDeEspera(DateTime data) async {
    final servico = _servico;
    if (servico == null) return;
    try {
      await api.entrarListaEspera(servico.id, data, null);
      if (!mounted) return;
      avisar(context, 'Você entrou na lista de espera. Avisaremos quando abrir uma vaga.');
    } on ApiException catch (e) {
      if (mounted) avisar(context, e.mensagem, erro: true);
    }
  }

  Future<void> _confirmar() async {
    final servico = _servico;
    final data = _data;
    final horario = _horario;
    if (servico == null || data == null || horario == null) return;

    setState(() => _enviando = true);
    try {
      final criado = await api.criarAgendamento(
        idServico: servico.id,
        data: data,
        horario: horario,
        observacao: _observacao.text.trim().isEmpty ? null : _observacao.text.trim(),
      );

      // A foto vai DEPOIS de o agendamento existir (é anexada a ele).
      final imagem = _imagem;
      final nome = _nomeImagem;
      if (imagem != null && nome != null) {
        try {
          await api.enviarImagem(criado.id, imagem, nome);
        } on ApiException catch (e) {
          if (mounted) avisar(context, 'Agendamento criado, mas a foto não foi enviada: ${e.mensagem}', erro: true);
        }
      }

      if (!mounted) return;
      avisar(context, 'Agendamento realizado! Aguarde a confirmação da Erika.');
      widget.aoConcluir();
    } on ApiException catch (e) {
      if (mounted) avisar(context, e.mensagem, erro: true);
    } finally {
      if (mounted) setState(() => _enviando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_carregando) return const Center(child: CircularProgressIndicator());
    final erro = _erro;
    if (erro != null) return MensagemErro(mensagem: erro, aoTentarDeNovo: _carregarServicos);

    final servico = _servico;
    final podeConfirmar = servico != null && _data != null && _horario != null && !_enviando;

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        LarguraMaxima(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const TituloSecao('1. Escolha o serviço'),
              for (final s in _servicos)
                Card(
                  color: s.id == servico?.id ? corRosaClaro : Colors.white,
                  child: ListTile(
                    title: Text(s.nome),
                    subtitle: Text('${moeda(s.preco)} • ${s.duracao} min'),
                    trailing: s.id == servico?.id
                        ? const Icon(Icons.check_circle, color: corRosa, semanticLabel: 'Selecionado')
                        : null,
                    selected: s.id == servico?.id,
                    onTap: () => _escolherServico(s),
                  ),
                ),

              if (servico != null) ...[
                const TituloSecao('2. Escolha a data e o horário'),
                SeletorDataHorario(
                  // Recria o seletor quando o serviço muda (a duração muda os horários livres).
                  key: ValueKey(servico.id),
                  idServico: servico.id,
                  aoMudar: (data, horario) => setState(() {
                    _data = data;
                    _horario = horario;
                  }),
                  quandoSemHorario: (data) => Padding(
                    padding: const EdgeInsets.only(top: 8),
                    child: OutlinedButton.icon(
                      onPressed: () => _entrarNaListaDeEspera(data),
                      icon: const Icon(Icons.hourglass_top),
                      label: const Text('Entrar na lista de espera deste dia'),
                    ),
                  ),
                ),

                const TituloSecao('3. Detalhes (opcional)'),
                TextField(
                  controller: _observacao,
                  maxLength: 500,
                  maxLines: 3,
                  decoration: campo('Observação', dica: 'Ex.: formato amendoado, cor nude'),
                ),
                const SizedBox(height: 8),
                Wrap(
                  spacing: 8,
                  runSpacing: 8,
                  children: [
                    OutlinedButton.icon(
                      onPressed: () => _escolherFoto(ImageSource.gallery),
                      icon: const Icon(Icons.photo_library_outlined),
                      label: const Text('Foto da galeria'),
                    ),
                    // A câmera não existe no navegador; o botão só aparece no celular.
                    if (!kIsWeb)
                      OutlinedButton.icon(
                        onPressed: () => _escolherFoto(ImageSource.camera),
                        icon: const Icon(Icons.photo_camera_outlined),
                        label: const Text('Tirar foto'),
                      ),
                  ],
                ),
                if (_imagem != null) ...[
                  const SizedBox(height: 12),
                  Row(
                    children: [
                      ClipRRect(
                        borderRadius: BorderRadius.circular(12),
                        child: Image.memory(_imagem!, width: 88, height: 88, fit: BoxFit.cover,
                            semanticLabel: 'Foto de inspiração escolhida'),
                      ),
                      const SizedBox(width: 12),
                      TextButton.icon(
                        onPressed: () => setState(() {
                          _imagem = null;
                          _nomeImagem = null;
                        }),
                        icon: const Icon(Icons.close),
                        label: const Text('Remover foto'),
                      ),
                    ],
                  ),
                ],

                const SizedBox(height: 20),
                Text(
                  _data == null || _horario == null
                      ? 'Escolha a data e o horário para confirmar.'
                      : 'Resumo: ${servico.nome} • ${dataBr(isoData(_data!))} às $_horario',
                  style: const TextStyle(fontWeight: FontWeight.w600),
                ),
                const SizedBox(height: 12),
                FilledButton(
                  onPressed: podeConfirmar ? _confirmar : null,
                  child: _enviando
                      ? const SizedBox(width: 22, height: 22, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                      : const Text('Confirmar agendamento'),
                ),
                const SizedBox(height: 24),
              ],
            ],
          ),
        ),
      ],
    );
  }
}
