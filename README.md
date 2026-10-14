# Loose Seller Desktop

Windows desktop application for preparing 3D-printing products before manually listing them on Shopee and TikTok Shop.

## Purpose

Loose Seller Desktop centralizes the product preparation workflow:

- Enter the product title, material, color, and dimensions.
- Select the 3D-printing model file.
- Select up to five product images.
- Create the product folder automatically.
- Copy the 3D model file.
- Organize and rename the images.
- Generate an initial `descricao.txt` file.

The final listing process on Shopee and TikTok Shop remains manual in the current version.

## Technology

- C#
- .NET 10
- WPF
- Windows 10/11
- Layered architecture

## Architecture

The solution follows the same five-layer organization used by `jarvis-auth-api`, adapted for a WPF desktop application:

```text
src/
├── LooseSeller.Desktop         Presentation: WPF views, code-behind and composition root
├── LooseSeller.Application     Use cases and application contracts
├── LooseSeller.Core            Shared primitives and cross-cutting contracts
├── LooseSeller.Domain          Entities and business rules
└── LooseSeller.Infrastructure  File system and external integrations
```

Dependencies point inward: `Desktop` composes the application, `Infrastructure` implements application contracts, `Application` orchestrates use cases, and `Domain` remains independent from WPF and file-system details.

```text
LooseSeller.Desktop       WPF presentation layer
        |
        +-- Application    Use cases and contracts
        |
        +-- Business       Domain entities and business rules
        |
        +-- Infrastructure File system and external integrations
```

### Business

Contains domain entities and business rules independent from the user interface, such as `Product`.

### Application

Contains use cases and interfaces that represent application operations. This layer must not depend on WPF, file-system details, or external APIs.

### Infrastructure

Implements external details. The current `FileProductPreparationService` creates the product folder, copies the files, and generates the initial description.

### Desktop

The WPF presentation layer and composition root. The main window collects user input and executes the product preparation use case.

## Output folder

Prepared products are saved to:

```text
C:\Users\Admin\Documents\LooseArt 3D
```

Example:

```text
LooseArt 3D/
└── Minimalist Phone Stand/
    ├── model.stl
    ├── image-01.jpg
    ├── image-02.jpg
    └── descricao.txt
```

## Running the application

Open `loose-seller-desktop.slnx` in Visual Studio and press `F5`.

From PowerShell, using the default .NET SDK path:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' build 'loose-seller-desktop.slnx'
& 'C:\Program Files\dotnet\dotnet.exe' run --project 'LooseSeller.Desktop.csproj'
```

## Publishing

To create a self-contained Windows 64-bit build:

```powershell
.\scripts\publish.ps1
```

The output will be available in:

```text
publish

## Installer

The installer is built with Inno Setup. After publishing the application, open `installer\LooseSeller.iss` and compile it with `Ctrl+F9`.

The generated installer is written to:

```text
installer-output\LooseSellerSetup-1.0.0.exe
```

The installer creates desktop and Start Menu shortcuts, uses `icon_installer.ico` for the setup program, and uses `icon_main.ico` for the installed application.

Application settings are stored in `%LocalAppData%\Loose Seller\appsettings.json`, so the output directory can be changed without administrator permissions.
```

## Roadmap

- Image compression with high JPEG quality.
- Image previews and ordering.
- Description editor before saving.
- AI-assisted titles, keywords, and hashtags.
- Product preparation history.
- Description template based on `docs\description_example.txt`.
- Cloud storage synchronization for future iPhone access.
