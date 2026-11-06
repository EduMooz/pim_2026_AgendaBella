import 'package:flutter/material.dart';

import '../services/api_service.dart';
import '../tema.dart';
import 'home_screen.dart';

/// Tela inicial para quem não está logada: abas "Entrar" e "Criar conta".
class LoginScreen extends StatelessWidget {
  const LoginScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return DefaultTabController(
      length: 2,
      child: Scaffold(
        body: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 480),
              child: Column(
                children: [
                  const SizedBox(height: 24),
                  Image.asset('assets/logo.png', height: 72, semanticLabel: 'Logotipo Agenda Bella'),
                  const SizedBox(height: 8),
                  Text(
                    'Erika Santana',
                    style: Theme.of(context).textTheme.headlineSmall?.copyWith(color: corVinho, fontWeight: FontWeight.w700),
                  ),
                  const Text('Manicure e nail design'),
                  const SizedBox(height: 16),
                  const TabBar(tabs: [Tab(text: 'Entrar'), Tab(text: 'Criar conta')]),
                  const Expanded(child: TabBarView(children: [_FormLogin(), _FormCadastro()])),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

/// Depois do login (ou cadastro), troca para a tela principal e apaga o histórico de telas,
/// para o botão "voltar" do celular não retornar ao login.
void _irParaInicio(BuildContext context) {
  Navigator.of(context).pushAndRemoveUntil(
    MaterialPageRoute(builder: (_) => const HomeScreen()),
    (rota) => false,
  );
}

// ------------------------------------------------------------------ Entrar

class _FormLogin extends StatefulWidget {
  const _FormLogin();

  @override
  State<_FormLogin> createState() => _FormLoginState();
}

class _FormLoginState extends State<_FormLogin> {
  final _form = GlobalKey<FormState>();
  final _email = TextEditingController();
  final _senha = TextEditingController();
  bool _carregando = false;
  String? _erro;

  @override
  void dispose() {
    _email.dispose();
    _senha.dispose();
    super.dispose();
  }

  Future<void> _entrar() async {
    if (!_form.currentState!.validate()) return;

    setState(() {
      _carregando = true;
      _erro = null;
    });
    try {
      await api.login(_email.text.trim(), _senha.text);
      if (!mounted) return;
      _irParaInicio(context);
    } on ApiException catch (e) {
      if (mounted) setState(() => _erro = e.mensagem);
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(24),
      child: Form(
        key: _form,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            if (_erro != null)
              Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: Text(_erro!, style: const TextStyle(color: Color(0xFFB02A37))),
              ),
            TextFormField(
              controller: _email,
              decoration: campo('E-mail', icone: Icons.email_outlined),
              keyboardType: TextInputType.emailAddress,
              autofillHints: const [AutofillHints.email],
              validator: (v) => (v == null || !v.contains('@')) ? 'Informe um e-mail válido.' : null,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _senha,
              decoration: campo('Senha', icone: Icons.lock_outline),
              obscureText: true,
              autofillHints: const [AutofillHints.password],
              validator: (v) => (v == null || v.isEmpty) ? 'Informe a senha.' : null,
              onFieldSubmitted: (_) => _entrar(),
            ),
            const SizedBox(height: 20),
            FilledButton(
              onPressed: _carregando ? null : _entrar,
              child: _carregando
                  ? const SizedBox(width: 22, height: 22, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                  : const Text('Entrar'),
            ),
          ],
        ),
      ),
    );
  }
}

// ------------------------------------------------------------------ Criar conta

class _FormCadastro extends StatefulWidget {
  const _FormCadastro();

  @override
  State<_FormCadastro> createState() => _FormCadastroState();
}

class _FormCadastroState extends State<_FormCadastro> {
  final _form = GlobalKey<FormState>();
  final _nome = TextEditingController();
  final _email = TextEditingController();
  final _senha = TextEditingController();
  final _telefone = TextEditingController();
  bool _carregando = false;
  String? _erro;

  @override
  void dispose() {
    _nome.dispose();
    _email.dispose();
    _senha.dispose();
    _telefone.dispose();
    super.dispose();
  }

  Future<void> _cadastrar() async {
    if (!_form.currentState!.validate()) return;

    setState(() {
      _carregando = true;
      _erro = null;
    });
    try {
      await api.cadastrar(
        nome: _nome.text.trim(),
        email: _email.text.trim(),
        senha: _senha.text,
        telefone: _telefone.text.trim(),
      );
      if (!mounted) return;
      _irParaInicio(context);
    } on ApiException catch (e) {
      if (mounted) setState(() => _erro = e.mensagem);
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(24),
      child: Form(
        key: _form,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            if (_erro != null)
              Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: Text(_erro!, style: const TextStyle(color: Color(0xFFB02A37))),
              ),
            TextFormField(
              controller: _nome,
              decoration: campo('Nome', icone: Icons.person_outline),
              textCapitalization: TextCapitalization.words,
              autofillHints: const [AutofillHints.name],
              validator: (v) => (v == null || v.trim().isEmpty) ? 'Informe o nome.' : null,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _email,
              decoration: campo('E-mail', icone: Icons.email_outlined),
              keyboardType: TextInputType.emailAddress,
              autofillHints: const [AutofillHints.email],
              validator: (v) => (v == null || !v.contains('@')) ? 'Informe um e-mail válido.' : null,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _senha,
              decoration: campo('Senha', icone: Icons.lock_outline, dica: 'Mínimo de 6 caracteres'),
              obscureText: true,
              autofillHints: const [AutofillHints.newPassword],
              validator: (v) => (v == null || v.length < 6) ? 'A senha deve ter pelo menos 6 caracteres.' : null,
            ),
            const SizedBox(height: 12),
            TextFormField(
              controller: _telefone,
              decoration: campo('Telefone', icone: Icons.phone_outlined, dica: 'Ex.: (16) 99999-9999'),
              keyboardType: TextInputType.phone,
              autofillHints: const [AutofillHints.telephoneNumber],
              validator: (v) => (v == null || v.trim().isEmpty) ? 'Informe o telefone.' : null,
            ),
            const SizedBox(height: 20),
            FilledButton(
              onPressed: _carregando ? null : _cadastrar,
              child: _carregando
                  ? const SizedBox(width: 22, height: 22, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                  : const Text('Criar conta'),
            ),
          ],
        ),
      ),
    );
  }
}
