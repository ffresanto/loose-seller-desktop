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
& 'C:\Program Files\dotnet\dotnet.exe' publish 'LooseSeller.Desktop.csproj' -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The output will be available in:

```text
bin\Release\net10.0-windows\win-x64\publish
```

## Roadmap

- Real image compression.
- Image previews and ordering.
- Description editor before saving.
- AI-assisted titles, keywords, and hashtags.
- Product preparation history.
- Description templates.
- Cloud storage synchronization for future iPhone access.
