# Very useful reasons

Uses the
endpoint https://bofh-api.bombeck.io/v1/excuses/all.

Credits go to https://bombeck.io/projects/bastard-operator-from-hell-generator 

## Usage

Library built upon .NET 8

```csharp
using Very_Useful_Reasons;

string reason = await UsefulReasons.GetReason();
```

## GitHub Actions Übersicht

```mermaid
flowchart TD

    %% === TRIGGER-EVENTS ===
    A1(["Pull Request geöffnet/aktualisiert"]) --> B1
    A2(["Push auf main"]) --> B4
    A3(["Pull Request geschlossen"]) --> B8

    %% === PR WORKFLOW ===
    subgraph "PR Workflow (pr.yml)"
        B1["Build Job<br>dotnet build"] --> B2["Test Job<br>dotnet test"]
        B2 --> B3["Pre-Release Job<br>nbgv Version + Pack mit Suffix"]
        B3 --> B3a["Push Pre-Release<br>NuGet Package → GitHub Packages"]
        B3a --> B3b["Pre-Release Package bereit<br>für PR-Tests"]
    end

    %% === RELEASE WORKFLOW ===
    subgraph "Release Workflow (release.yml)"
        B4["Build Job<br>dotnet build"] --> B5["Test Job<br>dotnet test"]
        B5 --> B6["Release Job<br>nbgv Version + dotnet pack"]
        B6 --> B6a["Push NuGet Package<br>→ GitHub Packages"]
        B6a --> B7["GitHub Release erstellen<br>mit .nupkg Artifact"]
    end

    %% === PR-CLOSE WORKFLOW ===
    subgraph "PR Close Workflow"
        B8["PR-Nummer ermitteln"] --> B9["Custom Action aufrufen<br>delete_github_packages_with_name"]
        B9 --> B10["Pre-Release-Versionen löschen<br>per Glob-Filter (\*pr-123*)"]
    end

    %% === CUSTOM ACTION DETAIL ===
    subgraph "Custom Action"
        C1["Fetch package versions<br>streamed + paginated"]
        C2["Globs anwenden<br>Include/Exclude"]
        C3["Versionen löschen<br>GitHub API"]
        C4["Summary loggen<br>deleted / skipped / failed"]
    end

    %% === VERKNÜPFUNGEN ===
    B9 --> C1
    C1 --> C2 --> C3 --> C4

    B3b -.->|"PR wird gemerged"| A2
    B7 -.->|"Release abgeschlossen"| E1["✅ NuGet Package verfügbar"]
    B10 --> E2["♻️ Pre-Release Packages bereinigt"]

    %% === STYLES ===
    classDef success fill:#b3f0b3,stroke:#2e7d32,stroke-width:1px,color:#1a1a1a;
    class E1,E2 success;

```
