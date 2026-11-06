# AgendaBella.App — aplicativo Flutter da cliente

O app consome a **AgendaBella.Api** (não acessa o banco) e oferece à cliente:
login/cadastro, catálogo, agendamento com horários livres, foto de inspiração (galeria ou câmera),
reagendar/cancelar, histórico, **lista de espera com aviso de vaga liberada** (a funcionalidade
complementar), preferências e pontos de fidelidade.

## Estrutura

```
lib/
├── main.dart                     início do app: carrega a sessão e escolhe Login ou Home
├── config.dart                   endereço da Api (10.0.2.2 no emulador, localhost no navegador)
├── tema.dart                     cores e estilo (as mesmas do site)
├── models/modelos.dart           classes que espelham os DTOs da Api (fromJson)
├── services/api_service.dart     ÚNICA classe que faz chamadas HTTP para a Api
├── services/sessao.dart          guarda o token JWT no aparelho
├── util/formatos.dart            datas, moeda e nome de arquivo
├── widgets/componentes.dart      peças reutilizáveis (selo de status, mensagem de erro...)
├── widgets/seletor_data_horario.dart   data + horários livres (usado em Agendar e Reagendar)
└── screens/                      as telas
    ├── login_screen.dart          Entrar / Criar conta
    ├── home_screen.dart           5 abas (embaixo no celular, na lateral em tela larga)
    ├── inicio_page.dart           serviços + aviso de vaga liberada
    ├── agendar_page.dart          novo agendamento com foto
    ├── meus_agendamentos_page.dart lista, cancelar, histórico
    ├── reagendar_screen.dart      escolher novo dia/horário
    ├── lista_espera_page.dart     entrar/sair da fila
    └── perfil_page.dart           pontos, preferências, sair
```

## Como rodar

1. Deixe a **AgendaBella.Api rodando** (`http://localhost:5218`).
2. Nesta pasta, gere as pastas de plataforma (só na primeira vez):
   ```
   flutter create . --project-name agendabella_app --org br.com.agendabella --platforms android,ios,web,windows
   ```
   O comando **não sobrescreve** o que já existe (a pasta `lib`, o `pubspec.yaml` e o
   `android/app/src/main/AndroidManifest.xml`, que já vem ajustado).
3. Baixe os pacotes: `flutter pub get`
4. Rode:
   - no emulador Android: `flutter run` (o app usa `http://10.0.2.2:5218`, que é o "localhost" do computador visto de dentro do emulador);
   - no navegador: `flutter run -d chrome` (usa `http://localhost:5218`).
5. Testes automáticos: `flutter test`

### Celular de verdade (fora do emulador)

O celular não enxerga o `localhost` do computador. Rode a Api ouvindo na rede e informe o IP do computador:

```
dotnet run --project AgendaBella.Api --urls http://0.0.0.0:5218
flutter run --dart-define=API_URL=http://192.168.0.10:5218
```

(troque `192.168.0.10` pelo IP do seu computador; celular e computador na mesma rede Wi-Fi).

### iPhone (opcional)

Acrescente no `ios/Runner/Info.plist` as chaves `NSPhotoLibraryUsageDescription` e
`NSCameraUsageDescription` (texto explicando o uso da foto de inspiração) e, para chamar a Api
por `http://`, a chave `NSAppTransportSecurity` › `NSAllowsArbitraryLoads` = `true` (apenas em desenvolvimento).

## Login de demonstração

`cliente@agendabella.com.br` / `Cliente@123` (criado pelo script `Banco/05_dados_demonstracao.sql`).
A conta da profissional não entra no app: ela usa o painel do site.
