Shell integration notes (PowerShell)

What was changed
- `gh` completion was appended to your PowerShell profile (via `gh completion -s powershell`).
- `Import-Module posh-git` and `Import-Module PSReadLine` were appended to your PowerShell profile.
- `posh-git` and `PSReadLine` were installed for the current user.

How to undo
1. Open your PowerShell profile in an editor:

```powershell
notepad $PROFILE
```

2. Remove any lines that were added by this tool, for example lines containing:
- the `gh completion` block
- `Import-Module posh-git`
- `Import-Module PSReadLine`

3. (Optional) Uninstall the modules installed for the current user:

```powershell
Uninstall-Module posh-git -AllVersions -Scope CurrentUser -Force
Uninstall-Module PSReadLine -AllVersions -Scope CurrentUser -Force
```

4. (Optional) If you want to remove the NuGet provider installed for PowerShellGet:

```powershell
# only if you explicitly want to remove it
Get-PackageProvider -Name NuGet | Unregister-PackageSource -ErrorAction SilentlyContinue
```

5. Restart PowerShell to apply the profile changes.

Notes
- Editing `$PROFILE` is safe; keep a copy before changing it.
- If you want me to revert these changes automatically, say "revert shell integration" and I will remove the added lines and uninstall the modules.

File created by the assistant.
