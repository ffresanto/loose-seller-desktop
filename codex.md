# Guia de desenvolvimento — Loose Seller Desktop

Este arquivo orienta futuras alterações feitas por agentes, colaboradores ou pelo Codex.

## Regras arquiteturais

Mantenha as responsabilidades separadas:

- `Business` não pode depender de `Application`, `Infrastructure` ou WPF.
- `Application` pode depender apenas de `Business`.
- `Infrastructure` implementa interfaces definidas em `Application`.
- O projeto WPF pode depender das três camadas para fazer a composição.
- Regras de negócio não devem ser colocadas no code-behind da tela.

## Fluxo recomendado

Para adicionar uma funcionalidade:

1. Modele ou ajuste a entidade em `Business`.
2. Crie uma interface e um caso de uso em `Application`.
3. Implemente detalhes de arquivos, APIs ou serviços em `Infrastructure`.
4. Conecte a operação à interface WPF.
5. Compile toda a solução antes de finalizar.

## Convenções

- Use C# com nullable habilitado.
- Use nomes em inglês para classes, propriedades, interfaces e métodos.
- Use mensagens visíveis ao usuário em português.
- Use `async`/`await` para operações de arquivo ou rede.
- Valide dados antes de executar operações externas.
- Evite colocar caminhos fixos diretamente em múltiplos arquivos.
- Não adicione dependências de WPF às bibliotecas `Business` e `Application`.

## Interface

A identidade visual deve seguir a extensão do navegador:

- Fundo base: `#100D1B`.
- Brilho radial roxo: `#3B1464`.
- Painéis: `#19142A`.
- Bordas: `#30254A`.
- Roxo: `#7C3AED`.
- Magenta: `#E11D8D`.
- Texto principal: `#F8F5FF`.
- Texto secundário: `#A69BBD`.

Use a logo existente em `logo.png` e mantenha `logo.ico` configurado como ícone do executável.

## Build e validação

Antes de considerar uma alteração pronta, execute:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' build 'loose-seller-desktop.slnx'
```

O build deve terminar com zero erros. Se o executável estiver aberto, encerre `LooseSeller.Desktop` antes de compilar novamente.

## Cuidados

- Não misture arquivos das camadas no projeto WPF. O `.csproj` remove explicitamente essas pastas da compilação do projeto de apresentação.
- Não substitua a arquitetura por código diretamente no `MainWindow.xaml.cs`.
- Preserve o suporte futuro para uma interface MAUI/iPhone reutilizando `Business` e `Application`.
- Ao adicionar arquivos de imagem ou ícones, atualize o `.csproj` como `Resource` quando necessário.
- Preserve acentos e salve arquivos de texto em UTF-8.

## Current structure and release workflow

The solution follows five layers under `src`:

- `LooseSeller.Core`: shared configuration models and cross-cutting types.
- `LooseSeller.Domain`: entities, value objects, business validation, and domain contracts.
- `LooseSeller.Application`: application services and use-case orchestration.
- `LooseSeller.Infrastructure`: file storage, image compression, JSON configuration loading, and external details.
- `LooseSeller.Desktop`: WPF views, user interaction, composition root, and visual resources.

The application supports multiple product sizes, up to nine images, description generation from `docs\description_example.txt`, and a persisted output directory.

To create a release, run `scripts\publish.ps1`. This generates a self-contained `win-x64` publication in `publish`. Then compile `installer\LooseSeller.iss` with Inno Setup. The setup executable is generated in `installer-output`.

The setup icon is `docs\icons\icon_installer.ico`. The installed application and its shortcuts use `docs\icons\icon_main.ico`. Keep these roles separate when changing icons.
