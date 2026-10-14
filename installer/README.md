# Gerar o instalador

1. Execute `scripts\publish.ps1`.
2. Abra `installer\LooseSeller.iss` no Inno Setup.
3. Clique em **Compile**.
4. O instalador será criado em `installer-output`.

O instalador publica a aplicação como `win-x64`, self-contained, e cria atalhos na área de trabalho e no menu Iniciar.
