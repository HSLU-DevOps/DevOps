### Aktualisierte Entwickler-Onboarding-Dokumentation

---

#### **Einführung**
Willkommen im Projekt von Gruppe 06! (Wir haben keinen coolen Namen). <br/>
Diese Dokumentation dient als Leitfaden, um Ihre Entwicklungsumgebung einzurichten und sich mit unserem Projekt vertraut zu machen. Unser Projekt basiert auf einer modernen Technologie-Stack, darunter .NET 9, Docker, und die Nutzung von Git für Versionskontrolle.

---

#### **Projektstruktur**
Unsere Projektstruktur ist folgendermassen aufgebaut:
- **Root-Verzeichnis**
    - `.dockerignore`, `.gitignore`, `DevOps Project.sln`: Projektkonfiguration- und Versionskontrolldateien.
    - `docker-compose.yml`: Docker-Konfiguration für die lokale Entwicklung der Todo-App.
    - `docker-stack.yml`: Docker-Konfiguration für das Deployment auf das produktive System.
    - `.github/workflows`: GitHub Actions für CI/CD (`codeql.yml`, `labservices.yml`, und `QualityGate.yml`).
    - `Documentation`: Enthält die Projektdokumentation.
    - `WebAPI`: Hauptprojektordner für die Web-API.
        - **Controllers**: Controller wie `TodoController.cs` für die API-Endpunkte.
        - **Persistence**: Datenbankmodell und -kontext (`Todo.cs`, `TodoDBContext.cs`).
        - **Features**: Feature-Management.
        - **Properties**: Projektkonfigurationen (`launchSettings.json`).
        - **appsettings.json**: Projektweite Konfiguration wie Logging und Feature Flags.
    - `WebTests`: Unit- und Integrationstests.
---

#### **Setup der Entwicklungsumgebung**

1. **Systemanforderungen**
    - Installieren Sie [.NET 9 SDK](https://dotnet.microsoft.com/download).
    - Installieren Sie [Docker Desktop](https://www.docker.com/products/docker-desktop).
    - Installieren Sie eine IDE, z. B. [JetBrains Rider](https://www.jetbrains.com/rider/).

2. **Setup** <br/>
   Klonen Sie das Repository:
   ```bash
   git clone https://github.com/HSLU-DevOps/DevOps.git
   cd DevOps
   ```

3. **Abhängigkeiten installieren** <br/>
   Wechseln Sie in das Verzeichnis `WebAPI` und führen Sie den folgenden Befehl aus:
   ```bash
   dotnet restore
   ```

4. **Docker für die lokale Entwicklung** <br/>
   Starten Sie die Todo-App lokal:
   ```bash
   docker-compose up -d
   ```
   Die Konfiguration hierfür finden Sie in `docker-compose.yml`.
   Oder direkt in Rider starten

5. **Datenbank** <br/>
    - Das Projekt verwendet PostgreSQL, konfiguriert in der `Program.cs` Datei:
      ```csharp
      builder.Services.AddDbContext<TodoDbContext>(opt => {
          opt.UseNpgsql($"User ID={Environment.GetEnvironmentVariable("POSTGRES_USER")};" +
                        $"Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")};" +
                        $"Host=postgres;" +
                        $"Port=5432;" +
                        $"Database={Environment.GetEnvironmentVariable("POSTGRES_DB")};");
      });
      ```

6. **Projekt starten** (CLI)
    - API starten:
      ```bash
      dotnet run --project WebAPI/WebAPI.csproj
      ```
    - Tests ausführen:
      ```bash
      dotnet test WebTests/WebTests.csproj
      ```

---

#### **Deployment**
Für das Deployment auf das produktive System wird das `docker-stack.yml` verwendet. Dies ermöglicht die Bereitstellung der Anwendung in einer skalierbaren Umgebung:
```bash
docker stack deploy -c docker-stack.yml <stack-name>
```

Das Deployment passiert nur über einen Pull Request in Github auf den **main** Branch
<br/>
Siehe `labservices.yml`
<br/>
Ansonsten muss man selbst Environment Variablen setzen
---

#### **Branching-Strategie**
Wir verwenden eine vereinfachte Strategie mit **GitHub Actions** als CI/CD-Lösung:
- **main**: Der einzige langlebige Zweig. Änderungen werden durch Pull-Requests integriert.
- **feature/<name>**: Kurzlebige Feature-Zweige für neue Implementierungen.
- **hotfix/<name>**: Kurzlebige Zweige für Notfall-Bugfixes, die direkt auf `main` gemergt werden.

<br/>
Grund dafür ist das wir im Projekt zu zweit gearbeitet haben und nur ein produktives System zur Verfügung hatten.

---

#### **Verwendete Technologien**
- **Backend**: .NET 9
- **Datenbank**: PostgreSQL, verwaltet durch EF Core.
- **CI/CD**: GitHub Actions (`codeql.yml`, `QualityGate.yml`, `labservices.yml`).
- **Containerisierung**: Docker (`docker-compose.yml`, `docker-stack.yml`).
- **Feature Management**: Implementiert mit Microsoft.FeatureManagement.

---

#### **Wichtige Konfigurationen**
1. **Feature Flags**
   Die Feature Flags sind in `appsettings.json` definiert:
   ```json
   "FeatureManagement": {
       "Create": true,
       "Read": true,
       "Mutate": true
   }
   ```
   Sie ermöglichen die Steuerung der API-Funktionen wie `Create`, `Read` und `Mutate`.

2. **Health Checks**
   implementiert in `HealthCheckDb.cs` zur Überwachung der Datenbankverbindung.

---

#### **Health check**

Der Healthcheck passiert über den `/health` HTTP-Endpoint. Da drin wird die Verbindung zur Datenbank
und allen notwendigen Services gettested.

---

#### **Häufige Szenarien**
1. **Neues Feature implementieren**
    - Erstellen Sie einen neuen Feature-Branch:
      ```bash
      git checkout -b feature/<name>
      ```
    - Implementieren Sie Änderungen und erstellen Sie einen Pull-Request.

2. **Fehlerbehebung**
    - Notfall-Bugfixes erfolgen im `hotfix/<name>`-Zweig und werden direkt in `main` integriert.

3. **Tests**
   Unit-Tests wie in `TodoControllerTests.cs` beschrieben, verwenden die In-Memory-Datenbank für das Mocking der 
   Use-Cases.

---

#### **Erfahrungen und Lessons Learned**
- **Feature Flags** erleichtern inkrementelle Updates und die Einführung neuer Funktionen.
- **Docker** gewährleistet Konsistenz zwischen Entwicklung- und Produktionsumgebungen.
- **CI/CD** durch GitHub Actions beschleunigt den Bereitstellungsprozess erheblich.

--- 

Eine genauere Dokumentation finden Sie im `Documentation`-Ordner.