### Szenarien-Dokumentation

---

#### **Basisszenarien**

##### **Einfacher Change-Request, Umsetzung, Q-Gate, Merge-Request, Delivery und Deploy**
**Prozess:**
1. **Change-Request erstellen:**  
   Ein Entwickler erstellt ein Issue im GitHub-Repository, um den gewünschten Änderungsbedarf zu dokumentieren.

   Er labled das Issue als Bug, Feature oder Change Request.
2. **Feature-Branch erstellen:**  
   Ein neuer Branch wird aus `main` erstellt:
   ```bash
   git checkout -b feature/<name>
   ```
   Dann Wird darauf commited mit 
   ```bash
   git commit -m "commit message"
   ```
   und schlussendlich werden die Änderungen gepushed mit
   ```bash
   git push -u feature/<name>
   ```
   und ein PR wird nach Schritt 5 erstellt.
3. **Implementierung und Tests:**  
   Änderungen werden durchgeführt, und Unit-Tests werden hinzugefügt/aktualisiert.

   Dazu die Test Solution `WebTests` verwenden
4. **Code-Review und Q-Gate:**  
   Nach Abschluss wird ein Pull-Request (PR) in `main` erstellt. GitHub Actions führen automatische Tests und Quality-Gates aus 
   (z. B. CodeQL für Sicherheitsprüfungen und Unit Tests aus `WebTests`).
5. **Pull-Request:**  
   Der PR wird nach erfolgreichem Review und automatischem Testing gemergt.

   Hier gilt das 4 Augen Prinzip im Team.
6. **Delivery und Deployment:**  
   Änderungen in `main` werden durch CI/CD automatisiert in das produktive System (`docker-stack.yml`) deployed:
   ```bash
   docker stack deploy -c docker-stack.yml <stack-name>
   ```

##### **Einhaltung der Kriterien von 12-Factor-Applikationen**
Unser Projekt erfüllt die **12-Factor-App-Kriterien**:
- **Codebase:** Es gibt nur eine Codebasis, verwaltet über Git.
- **Dependencies:** Alle Abhängigkeiten sind in der `WebAPI.csproj` und den Dockerfiles explizit definiert.
- **Config:** Konfigurationswerte sind in `appsettings.json` und Umgebungsvariablen ausgelagert.
- **Backing Services:** PostgreSQL und Prometheus sind als austauschbare Ressourcen konfiguriert.
- **Build, Release, Run:** Trennung von Build (CI/CD), Release (Versionen) und Run (Containerisierung).
- **Processes:** Die App ist stateless und skaliert über Docker-Services.
- **Port Binding:** HTTP-Dienste werden über Port-Bindung bereitgestellt.
- **Concurrency:** Skalierbarkeit wird durch mehrere Instanzen in `docker-stack.yml` erreicht.
- **Disposability:** Container sind schnell start- und stoppbar.
- **Dev/Prod Parity:** Docker stellt gleiche Umgebungen für Entwicklung und Produktion sicher.
- **Logs:** Logs werden via Docker und Prometheus gestreamt. Hier auch nur über `stdout`
- **Admin Processes:** Datenbankmigrationen erfolgen mit EF Core.

#### **CD durch Docker-Swarm**
@Lorin vlt hasch du das Kapitel mache
so konkret:
- wie hemmers ufgsetzt
- sicher chli über das ufsetze vom runner:
  - https://docs.github.com/en/actions/hosting-your-own-runners/managing-self-hosted-runners/about-self-hosted-runners
- usw.
- Branch protection rule, also sis repo ufsetze

---

#### **Erweiterte Szenarien**

##### **Handhabung eines Notfall-Bugfixes (auf der Produktion)**
1. **Hotfix-Branch erstellen:**  
   Fehler direkt auf `main` identifizieren und fixieren:
   ```bash
   git checkout -b hotfix/<name>
   ```
2. **Fix und Testen:**  
   Änderung implementieren und Tests ausführen.
3. **Merge und Deployment:**  
   Nach erfolgreichem Review wird der Bugfix in `main` gemergt und automatisch durch CI/CD deployed.

Wir folgen hier ausschliesslich dem [Github Workflow](https://docs.github.com/en/get-started/using-github/github-flow)

##### **Rückfall auf ältere Version (nach missglücktem Release)**
1. **Revert durchführen:**  
   Ein Rückfall wird mit Git durchgeführt:
   ```bash
   git revert <commit-hash>
   ```
2. **Pull-Request erstellen:**  
   Revert in einen neuen PR bringen, Tests durchführen und nach Genehmigung in `main` mergen.

##### **Nachvollziehbare Behandlung eines Incidents und Korrektur**
TODO @Lorin: Da vlt. es kapitel wie mir über prometheus wie mer smonitoring möched

##### **Implementation eines Features mit Hilfe von Feature-Toggles**
**Prozessbeschreibung:**
1. **Feature-Flag definieren:**  
   Feature-Toggles sind in `appsettings.json` konfiguriert. Beispiel:
   ```json
   "FeatureManagement": {
       "NewFeature": false
   }
   ```
2. **Implementierung mit Toggle:**  
   Die Funktionalität wird bedingt implementiert:
   ```csharp
   if (await featureManager.IsEnabledAsync("NewFeature"))
   {
       // Neue Funktionalität
   }
   ```
3. **Deployment und Aktivierung:**  
   Das Feature wird zuerst deaktiviert ausgeliefert und kann durch das Setzen des Flags aktiviert werden.

---

#### **Individuelle Szenarien**

##### **Unterschiedliche Technologien und Sprachen im Einsatz**
Unser Projekt ist modular aufgebaut, sodass unterschiedliche Technologien eingebunden werden könnten. Aktuell verwenden wir:
- .NET 9 für die Backend-Entwicklung.
- PostgreSQL als Datenbanklösung.
- Docker für Containerisierung.
  Dies gewährleistet eine flexible und skalierbare Plattform.

##### **Austausch der Persistenz-Technologie**
Wir haben uns bewusst gegen den Einsatz eines allgemeinen `IRepository`-Interfaces entschieden. 
Der Grund ist, dass EF Core bereits eine abstrahierte Schicht bietet, die die meisten Anforderungen abdeckt. 
Zusätzliche Schichten könnten die Komplexität erhöhen, ohne signifikante Vorteile zu bieten.
Zudem war das herumspielen mit verschiedenen Datenbank-Technologien nicht al zu interessant für uns.

##### **Migration der Persistenz (Schema)**
Unsere Datenbankmigrationen werden durch EF Core automatisch verwaltet. Beispiel:
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```
Dies stellt sicher, dass alle Schemaänderungen versioniert und reproduzierbar sind.

Beim Deploy-Prozess werden jedes Mal Migrationen ausgeführt auf DDL ebene.

---