import 'package:flutter/material.dart';

import '../models/modelos.dart';
import '../services/api_service.dart';
import '../services/sessao.dart';
import '../tema.dart';
import '../util/formatos.dart';
import '../widgets/componentes.dart';
import 'login_screen.dart';

/// Aba "Perfil": pontos de fidelidade, preferências de unhas e botão de sair.
class PerfilPage extends StatefulWidget {
  final VoidCallback aoAlterarDados;

  const PerfilPage({super.key, required this.aoAlterarDados});

  @override
  State<PerfilPage> createState() => _PerfilPageState();
}

class _PerfilPageState extends State<PerfilPage> {
  final _formato = TextEditingController();
  final _cor = TextEditingController();
  final _estilo = TextEditingController();
  final _observacao = TextEditingController();

  SaldoPontos? _pontos;
  bool _carregando = true;
  bool _salvando = false;
  String? _erro;

  @override
  void initState() {
    super.initState();
    _carregar();
  }

  @override
  void dispose() {
    _formato.dispose();
    _cor.dispose();
    _estilo.dispose();
    _observacao.dispose();
    super.dispose();
  }

  Future<void> _carregar() async {
    setState(() {
      _carregando = true;
      _erro = null;
    });
    try {
      final resultados = await Future.wait([api.preferencias(), api.pontos()]);
      if (!mounted) return;
      final pref = resultados[0] as Preferencia;
      setState(() {
        _formato.text = pref.formatoUnha ?? '';
        _cor.text = pref.corFavorita ?? '';
        _estilo.text = pref.estilo ?? '';
        _observacao.text = pref.observacao ?? '';
        _pontos = resultados[1] as SaldoPontos;
      });
    } on ApiException catch (e) {
      if (mounted) setState(() => _erro = e.mensagem);
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _salvar() async {
    setState(() => _salvando = true);
    try {
      String? valor(TextEditingController c) => c.text.trim().isEmpty ? null : c.text.trim();
      await api.salvarPreferencias(Preferencia(
        formatoUnha: valor(_formato),
        corFavorita: valor(_cor),
        estilo: valor(_estilo),
        observacao: valor(_observacao),
      ));
      if (!mounted) return;
      avisar(context, 'Preferências salvas. A Erika verá isso na agenda dela.');
      widget.aoAlterarDados();
    } on ApiException catch (e) {
      if (mounted) avisar(context, e.mensagem, erro: true);
    } finally {
      if (mounted) setState(() => _salvando = false);
    }
  }

  Future<void> _sair() async {
    await Sessao.sair();
    if (!mounted) return;
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const LoginScreen()),
      (rota) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    if (_carregando) return const Center(child: CircularProgressIndicator());
    final erro = _erro;
    if (erro != null) return MensagemErro(mensagem: erro, aoTentarDeNovo: _carregar);

    final pontos = _pontos;
    final usuario = Sessao.usuario;

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        LarguraMaxima(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              ListTile(
                contentPadding: EdgeInsets.zero,
                leading: const CircleAvatar(backgroundColor: corRosaClaro, child: Icon(Icons.person, color: corVinho)),
                title: Text(usuario?.nome ?? ''),
                subtitle: Text(usuario?.email ?? ''),
              ),

              const TituloSecao('Programa de fidelidade'),
              Card(
                color: Colors.white,
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('${pontos?.saldo ?? 0}',
                          style: Theme.of(context).textTheme.displaySmall?.copyWith(color: corVinho, fontWeight: FontWeight.w700)),
                      const Text('ponto(s) disponíveis • 1 ponto para cada R\$ 1,00 em atendimentos concluídos'),
                      const SizedBox(height: 8),
                      for (final m in (pontos?.extrato ?? <MovimentoPontos>[]).take(5))
                        ListTile(
                          dense: true,
                          contentPadding: EdgeInsets.zero,
                          title: Text(m.descricao ?? ''),
                          subtitle: Text(dataHoraBr(m.data)),
                          trailing: Text(
                            '${m.quantidade > 0 ? '+' : ''}${m.quantidade}',
                            style: const TextStyle(fontWeight: FontWeight.w700),
                          ),
                        ),
                    ],
                  ),
                ),
              ),

              const TituloSecao('Minhas preferências'),
              TextField(controller: _formato, decoration: campo('Formato da unha', dica: 'Ex.: amendoada, quadrada')),
              const SizedBox(height: 10),
              TextField(controller: _cor, decoration: campo('Cor favorita')),
              const SizedBox(height: 10),
              TextField(controller: _estilo, decoration: campo('Estilo', dica: 'Ex.: minimalista, decorado')),
              const SizedBox(height: 10),
              TextField(
                controller: _observacao,
                maxLines: 3,
                maxLength: 500,
                decoration: campo('Observações (alergias, cuidados...)'),
              ),
              const SizedBox(height: 8),
              FilledButton(onPressed: _salvando ? null : _salvar, child: const Text('Salvar preferências')),
              const SizedBox(height: 24),
              OutlinedButton.icon(onPressed: _sair, icon: const Icon(Icons.logout), label: const Text('Sair da conta')),
              const SizedBox(height: 24),
            ],
          ),
        ),
      ],
    );
  }
}
