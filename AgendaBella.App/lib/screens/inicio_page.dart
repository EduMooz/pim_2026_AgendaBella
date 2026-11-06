import 'package:flutter/material.dart';

import '../models/modelos.dart';
import '../services/api_service.dart';
import '../services/sessao.dart';
import '../tema.dart';
import '../util/formatos.dart';
import '../widgets/componentes.dart';

/// Aba "Início": saudação, aviso de vaga liberada e catálogo de serviços.
class InicioPage extends StatefulWidget {
  final void Function(int idServico) aoAgendar;

  const InicioPage({super.key, required this.aoAgendar});

  @override
  State<InicioPage> createState() => _InicioPageState();
}

class _InicioPageState extends State<InicioPage> {
  List<Servico> _servicos = [];
  List<ItemListaEspera> _vagas = [];
  bool _carregando = true;
  String? _erro;

  @override
  void initState() {
    super.initState();
    _carregar();
  }

  Future<void> _carregar() async {
    setState(() {
      _carregando = true;
      _erro = null;
    });
    try {
      // As duas chamadas saem ao mesmo tempo (Future.wait) e a tela espera as duas.
      final resultados = await Future.wait([api.servicos(), api.listaEspera()]);
      if (!mounted) return;
      setState(() {
        _servicos = resultados[0] as List<Servico>;
        _vagas = (resultados[1] as List<ItemListaEspera>).where((i) => i.vagaLiberada).toList();
      });
    } on ApiException catch (e) {
      if (mounted) setState(() => _erro = e.mensagem);
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_carregando) return const Center(child: CircularProgressIndicator());
    final erro = _erro;
    if (erro != null) return MensagemErro(mensagem: erro, aoTentarDeNovo: _carregar);

    return RefreshIndicator(
      onRefresh: _carregar,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          LarguraMaxima(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Text(
                  'Olá, ${Sessao.usuario?.nome ?? ''}!',
                  style: Theme.of(context).textTheme.headlineSmall?.copyWith(color: corVinho, fontWeight: FontWeight.w700),
                ),
                const Text('Escolha um serviço e agende em poucos toques.'),

                // Aviso de vaga liberada: a trigger do banco mudou o status para "Notificado".
                for (final vaga in _vagas) _cartaoVaga(vaga),

                const TituloSecao('Serviços'),
                for (final s in _servicos) _cartaoServico(s),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _cartaoVaga(ItemListaEspera vaga) {
    return Card(
      color: corRosaClaro,
      margin: const EdgeInsets.only(top: 16),
      child: ListTile(
        leading: const Icon(Icons.notifications_active, color: corVinho),
        title: const Text('Abriu uma vaga!'),
        subtitle: Text('${vaga.servicoNome} em ${dataBr(vaga.dataDesejada)}. Agende antes que alguém pegue.'),
        trailing: FilledButton(
          onPressed: () => widget.aoAgendar(vaga.idServico),
          child: const Text('Agendar'),
        ),
      ),
    );
  }

  Widget _cartaoServico(Servico s) {
    return Card(
      color: Colors.white,
      margin: const EdgeInsets.only(bottom: 12),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(s.nome, style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
            if (s.descricao != null) ...[
              const SizedBox(height: 4),
              Text(s.descricao!),
            ],
            const SizedBox(height: 8),
            Row(
              children: [
                Expanded(child: Text('${moeda(s.preco)} • ${s.duracao} min', style: const TextStyle(color: corVinho))),
                OutlinedButton(
                  onPressed: () => widget.aoAgendar(s.id),
                  child: const Text('Agendar'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
