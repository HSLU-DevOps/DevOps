# HSLU DEVOPS G06 - Entwickler-Onboarding

---

## Einführung

Willkommen im Projekt von Gruppe 06! (Wir haben keinen coolen Namen).

Diese Dokumentation dient als Leitfaden, um Ihre Entwicklungsumgebung einzurichten und sich mit unserem Projekt vertraut
zu machen. Bei unserem Projekt handelt es sich um eine ToDo-Applikation, die auf einem modernen Technologie-Stack
basiert. Das Projekt verwendet .NET 9, Docker (inkl. Docker Swarm), und GitHub für die Versionskontrolle sowie GitHub
Actions für CI/CD.

Eine detaillierte Dokumentation des Projektes finden Sie im [`Documentation`](Documentation)-Ordner.

---

## Projektübersicht

Unser Projekt besteht aus einem Monorepo und hat folgende Projektstruktur:

- `.github/workflows`: GitHub Actions für CI/CD.
- `Documentation`: Enthält die Projektdokumentation.
- `Infrastructure`: Konfigurationsdateien und Scripts für das Deployment der Infrastruktur auf Docker Swarm.
- `WebAPI`: Hauptprojektordner für die Web-API der ToDo-Applikation.
    - **Controllers**: Controller wie `TodoController.cs` für die API-Endpunkte.
    - **Persistence**: Datenbankmodell und -kontext (`Todo.cs`, `TodoDBContext.cs`).
    - **Features**: Feature-Management.
    - **Properties**: Projektkonfigurationen (`launchSettings.json`).
    - **appsettings.json**: Projektweite Konfiguration wie Logging und Feature Flags.
- `WebTests`: Unit- und Integrationstests.
- `.dockerignore`, `.gitignore`, `DevOps Project.sln`: Projektkonfiguration- und Versionskontrolldateien.
- `docker-compose.yml`: Docker Compose Konfiguration für die lokale Entwicklung der Todo-App.
- `docker-stack.yml`: Docker Swarm Konfiguration für das Deployment auf das produktive System.

### Verwendete Technologien

Das Projekt verwendet die folgenden Technologien:

- **Backend**: .NET 9
- **Datenbank**: PostgreSQL, verwaltet durch [EF Core](https://learn.microsoft.com/en-us/ef/core/)
- **Feature Management**: Implementiert
  mit [.NET Feature Management](https://learn.microsoft.com/en-us/azure/azure-app-configuration/feature-management-dotnet-reference)
- **CI/CD**: GitHub Actions ([`.github/workflows`](.github/workflows))
  und [Self-Hosted Runners](https://docs.github.com/en/actions/hosting-your-own-runners/managing-self-hosted-runners/about-self-hosted-runners).
- **Containerisierung**: [Docker](https://www.docker.com/) und [Docker Swarm](https://docs.docker.com/engine/swarm/)
- **Metriken**: [Prometheus](https://prometheus.io/) und [Grafana](https://grafana.com/grafana/)
- **Logging**: [Grafana Loki](https://grafana.com/oss/loki/)
  und [Promtail](https://grafana.com/docs/loki/latest/send-data/promtail/)

### Deployment

Für das Deployment auf das produktive System wird [Docker Swarm](https://docs.docker.com/engine/swarm/) verwendet. Dies
ermöglicht die Bereitstellung der Anwendung in einer skalierbaren Container-Umgebung. In der Datei [
`docker-stack.yml`](docker-stack.yml) sind die Services definiert, welche auf dem produktiven System deployt werden.

Das Deployment wird automatisiert über [GitHub Actions](https://github.com/features/actions) durchgeführt. Wenn
Änderungen auf den `main` Branch gemergt werden, werden diese automatisch auf dem produktiven System deployt. Die
entsprechende GitHub Action ist in der Datei [`labservices.yml`](.github/workflows/labservices.yml) definiert.

---

## Einrichten der Entwicklungsumgebung

1. **Systemanforderungen**
    - Installiere  [.NET 9 SDK](https://dotnet.microsoft.com/download).
    - Installiere [Docker Desktop](https://www.docker.com/products/docker-desktop) auf Windows, macOS oder Linux.
      Auf Linux kann alternativ auch die [Docker Engine](https://docs.docker.com/engine/install/) installiert werden.
    - Installiere eine IDE, z.B. [JetBrains Rider](https://www.jetbrains.com/rider/).

2. **Setup**

   Klone das Repository:
   ```shell
   git clone https://github.com/HSLU-DevOps/DevOps.git
   cd DevOps
   ```

3. **Environment Datei erstellen**

   Für die Datenbank wird ein Benutzer und Passwort benötigt. Erstelle hierzu eine `.env`-Datei im Root-Verzeichnis des
   Projektes:
   ```ini
   # .env
   POSTGRES_PASSWORD=<password>
   POSTGRES_USER=<user>
   POSTGRES_DB=todo
   ```

4. **Abhängigkeiten installieren**

   Wechsle in das Verzeichnis `WebAPI` und führe den folgenden Befehl aus:
   ```shell
   dotnet restore
   ```

5. **Docker Compose für die lokale Entwicklung**

   Starte die Todo-App lokal mit Docker Compose:
   ```shell
   docker compose up -d
   ```
   Die Konfiguration hierfür befindet sich in der Datei [`docker-compose.yml`](docker-compose.yml). Alternativ kann die
   Applikation auch direkt in der IDE gestartet werden. Dazu muss jedoch die Konfiguration, um auf die Datenbank zu
   verbinden, angepasst werden.

6. **Datenbank**

   Das Projekt verwendet PostgreSQL als Datenbank. Die Konfiguration hierzu befindet sich in der [
   `Program.cs`](WebAPI/Program.cs)-Datei:
   ```csharp
   builder.Services.AddDbContext<TodoDbContext>(opt => {
       opt.UseNpgsql($"User ID={Environment.GetEnvironmentVariable("POSTGRES_USER")};" +
                     $"Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")};" +
                     $"Host=postgres;" +
                     $"Port=5432;" +
                     $"Database={Environment.GetEnvironmentVariable("POSTGRES_DB")};");
   });
   ```

7. **Projekt starten** (CLI)
    - API starten:
      ```shell
      dotnet run --project WebAPI/WebAPI.csproj
      ```
    - Tests ausführen:
      ```shell
      dotnet test WebTests/WebTests.csproj
      ```

---

## Workflow

Das Projekt verwendet den [GitHub flow](https://docs.github.com/en/get-started/using-github/github-flow), eine einfache,
branchbasierte Strategie zur Versionsverwaltung.

Dabei werden in diesem Projekt hauptsächlich folgende Branches verwendet:

- **`main`**: Der einzige langlebige Branch. Änderungen werden durch Pull-Requests integriert.
- **`feature/<feature>`**: Kurzlebige Feature-Branches für neue Implementierungen.
- **`hotfix/<fix>`**: Kurzlebige Branches für Notfall-Bugfixes, die direkt auf `main` gemergt werden.

### Wichtige Konfigurationen

**Feature Flags**:

Die Feature Flags werden in der Datei [`appsettings.json`](WebAPI/appsettings.json) definiert:

```json
{
  "FeatureManagement": {
    "Create": true,
    "Read": true,
    "Mutate": true
  }
}
```

Sie ermöglichen beispielsweise die Steuerung der API-Funktionen wie `Create`, `Read` und `Mutate`.

**Logging**:

Das Log Level wird ebenfalls über die Datei [`appsettings.json`](WebAPI/appsettings.json) gesteuert:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  }
}
```

### Häufige Szenarien

**Neues Feature implementieren**

1. Erstelle einen neuen Feature-Branch:
   ```shell
   git checkout -b feature/<feature>
   ```
2. Implementiere und pushe die Änderungen auf den Feature-Branch.
3. Erstelle einen Pull-Request auf GitHub.
4. Implementiere allfällige Änderungen aus dem Pull-Request Review oder den Rückmeldungen der Quality Gate Action.
5. Merge den Feature-Branch sobald der Pull-Request freigegeben wurde.
6. Lösche den Feature-Branch.

**Fehlerbehebung**

Der Prozess ist derselbe wie beim Hinzufügen von einem neuen Feature, verwende jedoch einen Hotfix-Branch:

```shell
git checkout -b hostfix/<fix>
```

**Tests**

Neue Features oder Änderungen am Code müssen immer automatisch getestet werden. Dabei können sowohl Unit-Tests als auch
Integration Tests eingesetzt werden. Beispielsweise wird für die Unit-Tests des TodoControllers (
[`TodoControllerTests.cs`](WebTests/TodoControllerTests.cs)) eine In-Memory-Datenbank verwendet.
