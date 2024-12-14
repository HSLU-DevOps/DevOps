# HSLU DEVOPS G06 - Dokumentation

---

## Inhaltsverzeichnis

* [Einführung](#einführung)
* [Team](#team)
* [Projektinformationen](#projektinformationen)
    * [Projektstruktur](#projektstruktur)
    * [Branching-Strategie](#branching-strategie)
    * [Zugriffspunkte](#zugriffspunkte)
    * [Verwendete Technologien](#verwendete-technologien)
* [Infrastruktur](#infrastruktur)
    * [Installation](#installation)
    * [Docker Swarm Setup](#docker-swarm-setup)
    * [Docker Swarm Node mit persistentem Speicher](#docker-swarm-node-mit-persistentem-speicher)
    * [Docker Metriken](#docker-metriken)
    * [Metriken](#metriken)
    * [Logging](#logging)
* [CI/CD-Pipelines mit GitHub Actions](#cicd-pipelines-mit-github-actions)
    * [Self-Hosted-Runner](#self-hosted-runner)
    * [GitHub Actions](#github-actions)
* [Szenarien](#szenarien)
    * [Basisszenarien](#basisszenarien)
        * [Einfacher Change-Request, Umsetzung, Q-Gate, Merge-Request, Delivery und Deploy](#einfacher-change-request-umsetzung-q-gate-merge-request-delivery-und-deploy)
        * [Aufsetzen der Entwicklungsumgebung für neue Mitarbeiter](#aufsetzen-der-entwicklungsumgebung-für-neue-mitarbeiter)
        * [Einhaltung der Kriterien von 12-Factor-Applikationen](#einhaltung-der-kriterien-von-12-factor-applikationen)
    * [Erweiterte Szenarien](#erweiterte-szenarien)
        * [Handhabung eines Notfall-Bugfixes (auf der Produktion)](#handhabung-eines-notfall-bugfixes-auf-der-produktion)
        * [Rückfall auf ältere Version (nach missglücktem Release)](#rückfall-auf-ältere-version-nach-missglücktem-release)
        * [Nachvollziehbare Behandlung eines Incidents und Korrektur](#nachvollziehbare-behandlung-eines-incidents-und-korrektur)
        * [Implementation eines Features mithilfe von Feature-Toggles](#implementation-eines-features-mithilfe-von-feature-toggles)
    * [Individuelle Szenarien](#individuelle-szenarien)
        * [Unterschiedliche Technologien und Sprachen im Einsatz](#unterschiedliche-technologien-und-sprachen-im-einsatz)
        * [Austausch der Persistenz-Technologie](#austausch-der-persistenz-technologie)
        * [Migration der Persistenz (Schema)](#migration-der-persistenz-schema)
        * [Health Check](#health-check)

## Einführung

Dieses Dokument beschreibt die Implementierung einer CI/CD-Pipeline und eines Monitoring-Systems für eine .NET
Todo-Applikation mit REST-API im Rahmen des DEVOPS-Moduls.

Das Projekt demonstriert die Automatisierung von Build-, Test- und Deployment-Prozessen mithilfe von GitHub Actions. Die
Anwendung wird als Container mittels Docker Swarm deployt. Zur Überwachung der Anwendung und Infrastruktur werden
Prometheus, Grafana, Loki und Promtail eingesetzt.

## Team

Das Team besteht aus folgenden Personen:

- Bucher, Lorin <lorin.bucher@stud.hslu.ch>
- Milovanovic, Ilija <ilija.milovanovic@stud.hslu.ch>
- ~~Büchi, Silvan <silvan.buechi@stud.hslu.ch>~~

## Projektinformationen

Obwohl einige Punkte bereits in der [README.md](../README.md) erwähnt werden, werden sie hier im Sinne der
Vollständigkeit nochmals aufgegriffen und vertieft.

### Projektstruktur

Aufgrund des überschaubaren Umfangs dieses Projekts bietet sich ein Monorepo an, welches die Verwaltung aller
Komponenten in einem zentralen Repository vereinfacht. Die Projektstruktur gestaltet sich wie folgt:

- `.github/workflows`: GitHub Actions für die CI/CD-Pipelines. Diese werden im
  Kapitel [CI/CD-Pipelines mit GitHub Actions](#cicd-pipelines-mit-github-actions) genauer beschrieben.
- `Documentation`: Enthält die Projektdokumentation.
- `Infrastructure`: Konfigurationsdateien und Scripts die für das Deployment der Monitoring- und Logging-Infrastruktur
  auf Docker Swarm verwendet werden.
- `WebAPI`: Hauptprojektordner für die Web-API der ToDo-Applikation.
    - **Controllers:** Controller wie `TodoController.cs` für die API-Endpunkte.
    - **Persistence:** Datenbankmodell und -kontext (`Todo.cs`, `TodoDBContext.cs`).
    - **Features:** Feature-Management um neue Features über Feature-Toggles ausrollen zu können.
    - **Properties:** Projektkonfigurationen (`launchSettings.json`).
    - **appsettings.json:** Projektweite Konfiguration wie Logging und Feature Flags.
- `WebTests`: Unit- und Integrationstests.
- `.dockerignore`, `.gitignore`, `DevOps Project.sln`: Projektkonfiguration- und Versionskontrolldateien.
- `docker-compose.yml`: Docker Compose Konfiguration für die lokale Entwicklung der Todo-App.
- `docker-stack.yml`: Docker Swarm Konfiguration für das Deployment auf das produktive System.

### Branching-Strategie

Das Projekt verwendet den [GitHub flow](https://docs.github.com/en/get-started/using-github/github-flow), eine einfache,
branchbasierte Strategie zur Versionsverwaltung. Durch die Nutzung von Feature Branches und Pull Requests ermöglicht sie
eine transparente und kollaborative Entwicklung. Änderungen werden frühzeitig integriert und getestet, was zu einer
schnelleren Entwicklung und einer höheren Codequalität führt. GitHub Flow ist flexibel anpassbar und eignet sich sowohl
für kleine als auch für grosse Projekte, die Wert auf kontinuierliche Integration und Deployment legen.

In diesem Projekt werden hauptsächlich folgende Branches verwendet:

- **`main`**: Der einzige langlebige Branch, der den aktuellen Produktionsstand repräsentiert. Änderungen werden in
  diesen Branch über Pull-Requests integriert, um Code-Reviews und eine hohe Codequalität sicherzustellen.
- **`feature/<feature>`**: Für die Entwicklung neuer Features werden kurzlebige Feature-Branches erstellt. Dies
  ermöglicht die isolierte Entwicklung und verhindert Konflikte mit dem main Branch.
- **`hotfix/<fix>`**: In dringenden Fällen, wie zum Beispiel bei kritischen Bugs in der Produktion, werden
  Hotfix-Branches verwendet. Diese ermöglichen schnelle Fehlerbehebungen, die direkt in den main Branch integriert
  werden.

Um zu verhindern, dass direkt auf den `main`-Branch gepusht werden kann, wurde eine Branch-Protection-Rule definiert:

- Direktes pushen auf den `main`-Branch ist nicht möglich.
- Ein Pull-Request ist notwendig, um Änderungen auf den `main`-Branch zu mergen.
- Mindestens eine andere Person muss den Pull-Request freigeben, um diesen mergen zu können.

### Zugriffspunkte

**Server:**

Bei diesem Projekt werden folgende VMs verwendet:

- devops-004
- devops-014
- devops-025

Die Server formen zusammen ein Docker Swarm Cluster. Dementsprechend kann auf die Dienste über eine beliebige VM
zugegriffen werden.

**Dienste:**

Im Rahmen dieses Projekts wurde auf die Einrichtung eines Reverse Proxys und die Verwendung von SSL-Zertifikaten
verzichtet. Daher erfolgt der Zugriff auf die Dienste über verschiedene Ports und unverschlüsselte Verbindungen. Die
Zugriffspunkte sind in der nachfolgenden Tabelle beschrieben:

| Dienst     | Port | Beschreibung                                                                                    |
|------------|------|-------------------------------------------------------------------------------------------------|
| REST-API   | 8080 | REST-API der ToDo-Applikation                                                                   |
| Grafana    | 3000 | Zugriff auf die Prometheus Metriken und Logs von Loki. Es wurden keine Dashboards konfiguriert. |
| Prometheus | 9090 | Dashboard von Prometheus um Status von Prometheus zu prüfen.                                    |

**REST-API:**

Die REST-API der ToDo-Applikation hat folgende Endpunkte:

| Endpoint           | Method | Resource | Body |
|--------------------|:------:|:--------:|:----:|
| /todo/getall       |  GET   |    -     |  -   |
| /todo/getbyid/{id} |  GET   | id: GUID |  -   |
| /todo/create       |  POST  |    -     | JSON |
| /todo/delete/{id}  | DELETE | id: GUID |  -   |

### Verwendete Technologien

Dieses Projekt verwendet eine Reihe von modernen Technologien, um eine robuste und skalierbare Todo-App mit REST-API zu
erstellen. Im Folgenden werden die wichtigsten Komponenten und ihre Funktion im Projekt beschrieben:

**Backend:**

- [.NET 9](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview): Das Backend der Todo-Applikation
  basiert auf dem .NET 9 Framework von Microsoft. .NET 9 bietet eine hohe Performance, Skalierbarkeit und Sicherheit und
  eignet sich ideal für die Entwicklung von Webanwendungen und APIs.

**Datenbank:**

- [PostgreSQL](https://www.postgresql.org/): Als Datenbank kommt PostgreSQL zum Einsatz, ein zuverlässiges und
  leistungsstarkes Open-Source-Datenbanksystem. PostgreSQL speichert alle relevanten Daten der Anwendung, wie Aufgaben
  und deren Status.
- [Entity Framework Core (EF Core)](https://learn.microsoft.com/en-us/ef/core/): Die Interaktion mit der
  PostgreSQL-Datenbank wird durch Entity Framework Core vereinfacht. EF Core ist ein Object-Relational Mapper (ORM), der
  es ermöglicht, mit der Datenbank über Objekte im .NET-Code zu interagieren, anstatt SQL-Abfragen zu schreiben.
  Zusätzlich können mit EF Core auch Datenbank-Migrationen durchgeführt werden.

**Feature Management:**

- [.NET Feature Management](https://learn.microsoft.com/en-us/azure/azure-app-configuration/feature-management-dotnet-reference):
  Um neue Funktionen kontrolliert in der Anwendung auszurollen, wird .NET Feature Management verwendet.

**CI/CD:**

- [GitHub Actions](https://github.com/features/actions): Der Build-, Test- und Deployment-Prozess der Anwendung wird
  durch GitHub Actions automatisiert. GitHub Actions ist ein CI/CD-System, das direkt in GitHub integriert ist.
- [Self-Hosted Runners](https://docs.github.com/en/actions/hosting-your-own-runners/managing-self-hosted-runners/about-self-hosted-runners):
  Für die Ausführung von manchen GitHub Actions werden Self-Hosted Runners verwendet. Diese
  bieten mehr Kontrolle über die Build-Umgebung und ermöglichen die Nutzung von spezifischer Hardware oder Software.

**Container:**

- [Docker](https://www.docker.com/): Die Anwendung wird als Docker Image paketiert und ausgeliefert. Docker ermöglicht
  es, die Anwendung und alle ihre Abhängigkeiten in einem isolierten Container zu verpacken, was die Portabilität und
  das Deployment vereinfacht.

- [Docker Swarm](https://docs.docker.com/engine/swarm/): Zum Deployment und zur Skalierung der Anwendung wird Docker
  Swarm verwendet. Docker Swarm ist ein Orchestrierungstool für Docker-Container, das die Verwaltung von Containern auf
  einem Cluster von Servern ermöglicht.

**Metriken:**

- [Prometheus](https://prometheus.io/): Zur Überwachung der Anwendung werden Metriken mit Prometheus gesammelt und
  gespeichert. Prometheus ist ein Open-Source-Monitoring-System, das eine leistungsstarke Abfragesprache und
  verschiedene
  Visualisierungsmöglichkeiten bietet.
- [Grafana](https://grafana.com/grafana/): Die Visualisierung der Metriken erfolgt mit Grafana. Grafana ist eine
  Open-Source-Plattform, mit der sich Dashboards erstellen lassen, um die Performance der Anwendung zu überwachen und
  Trends zu erkennen.

**Logging:**

- [Grafana Loki](https://grafana.com/oss/loki/): Für die Log-Aggregation wird Grafana Loki verwendet. Loki ist ein
  horizontales skalierbares und kostenfreies Log-Aggregationssystem.
- [Promtail](https://grafana.com/docs/loki/latest/send-data/promtail/): Promtail sammelt Logs von den Anwendungen über
  Docker und sendet sie an Grafana Loki.

---

## Infrastruktur

Da nur drei VMs zur Verfügung standen, hat das Team entschieden damit ein Docker Swarm Cluster aufzubauen. Das
Cluster wird für die produktive Umgebung verwendet. In einem realen Projekt sollten mehrere Cluster aufgebaut werden,
um mehrere Umgebungen wie DEV oder TEST bereitzustellen um die Applikationen vor dem Deployment auf PROD zu testen.

### Installation

Auf allen VMs muss die [Docker Engine](https://docs.docker.com/engine/) installiert werden.
Die [Anleitung](https://docs.docker.com/engine/install/debian/) hierzu kann in der Docker Dokumentation gefunden werden.

### Docker Swarm Setup

Nach dem Aufsetzen der VMs werden diese zu einem Docker Swarm Cluster zusammengeschlossen. Um eine hohe Verfügbarkeit
des Clusters zu gewährleisten und Ausfälle von Docker Swarm Manager Nodes abzufangen, wird empfohlen, eine ungerade
Anzahl von Manager Nodes zu verwenden. Damit ist die Chance bei einer Netzwerkpartition höher, dass das Quorum erhalten
werden kann. Zudem werden mindestens drei Manager Nodes benötigt, um bei einem Ausfall eines Nodes keinen Unterbruch zu
generieren. Mehr Informationen können aus
der [Docker Dokumentation](https://docs.docker.com/engine/swarm/admin_guide/#add-manager-nodes-for-fault-tolerance)
entnommen werden.

Folgende Schritte müssen durchgeführt werden, um ein Docker Swarm Cluster aufzusetzen:

1. Docker Swarm initialisieren:
   ```shell
   docker swarm init --advertise-addr <IP-Adresse von VM>
   ```
2. Docker Swarm Manager Token auslesen:
   ```shell
   docker swarm join-token manager
   ```
3. Manager Nodes mit dem Manager Token hinzufügen:
   ```shell
   docker swarm join --token <Token> <IP-Adresse von Init VM>:2377
   ```
4. Docker Swarm Worker Token auslesen (optional):
   ```shell
   docker swarm join-token worker
   ```
5. Worker Nodes mit dem Worker Token hinzufügen (optional):
   ```shell
   docker swarm join --token <Token> <IP-Adresse von Init VM>:2377
   ```

### Docker Swarm Node mit persistentem Speicher

Da kein Netzwerkspeicher zur Verfügung steht, welcher von allen VMs genutzt werden kann, hat das Team sich dafür
entschieden, ein Node für Container zu verwenden, welche persistenten Speicher benötigen. Dazu wurde ein Label auf dem
Node `devops-004` hinzugefügt:

```shell
docker node update --label-add volumes=true devops-004
```

Anschliessend kann in den Docker Stack Dateien bei den entsprechenden Services ein Placement Constraint gesetzt werden:

```yaml
services:
  postgres:
    # ...
    deploy:
      mode: replicated
      placement:
        constraints:
          - node.labels.volumes==true
```

### Docker Metriken

Um Metriken von den Docker Engines auf den VMs zu sammeln, können diese in der Docker Daemon Konfigurationsdatei
`/etc/docker/daemon.json` aktiviert werden:

```json
{
  "metrics-addr": "0.0.0.0:9323"
}
```

Anschliessend muss der Docker Service neu gestartet werden:

```shell
sudo systemctl restart docker.service docker.socket
```

### Metriken

Die Metriken werden mit Prometheus gesammelt. Prometheus ist so konfiguriert, dass automatisch Metriken von allen Nodes
des Docker Swarm Clusters gesammelt werden. Dabei wird auf das von Prometheus bereitgestellte Service-Discovery-System
zurückgegriffen. Es werden sowohl Metriken von der Docker Engine als auch von den laufenden Docker Swarm Services
gesammelt, welche ein entsprechendes Label gesetzt haben:

```yaml
services:
  webapi:
    # ...
    deploy:
      labels:
        prometheus-job: webapi
```

Die Metriken können anschliessend über Grafana abgerufen werden, allerdings wurden noch keine Dashboards erstellt.

### Logging

Um die Logs zentral zu sammeln und analysieren zu können, wird Grafana Loki und Promtail eingesetzt. Die Logs können
wiederum über Grafana abgerufen werden. Da die Docker Logs nur auf dem jeweiligen Host gesammelt werden können, wird auf
jedem Node ein Promtail Agent gestartet. Dies lässt sich einfach realisieren, indem der Service im Modus `global`
deployt wird:

```yaml
services:
  promtail:
    # ...
    deploy:
      mode: global
```

Promtail schickt die Logdaten von jedem Node anschliessend an den zentralen Loki Container.

---

## CI/CD-Pipelines mit GitHub Actions

Für die CI/CD-Pipelines werden GitHub Actions verwendet. Diese befinden sich im Order [
`.github/workflows`](../.github/workflows).

### Self-Hosted-Runner

Um die Applikationen auf den VMs zu deployen, muss
ein [Self-Hosted-Runner](https://docs.github.com/en/actions/hosting-your-own-runners/managing-self-hosted-runners/about-self-hosted-runners)
auf einer der VMs aufgesetzt werden. Dieser wurde auf der VM `devops-014` entsprechend eingerichtet.
Die [Anleitung](https://docs.github.com/en/actions/hosting-your-own-runners/managing-self-hosted-runners/adding-self-hosted-runners)
hierfür kann in der GitHub Dokumentation gefunden werden. Dabei gibt es
einige [Sicherheitsaspekte](https://docs.github.com/en/actions/hosting-your-own-runners/managing-self-hosted-runners/adding-self-hosted-runners)
zu bedenken, insbesondere bei öffentlichen Repositories.

### GitHub Actions

In diesem Projekt werden vier GitHub Actions verwendet, welche nachfolgend kurz beschrieben werden.

**Vulnerability Scanning ([`codeql.yml`](../.github/workflows/codeql.yml)):**

Diese GitHub Action kümmert sich um die automatisierte Code-Analyse des Projekts. Sie nutzt CodeQL, um den Code auf
Sicherheitslücken und Fehler zu untersuchen. Die Action wird bei jedem Push zum main Branch, bei jedem Pull Request zum
main Branch und nach einem Zeitplan (wöchentlich) ausgeführt.

**Infrastructure Deployment ([`infrastructure.yml`](../.github/workflows/infrastructure.yml)):**

Diese GitHub Action ist für die Bereitstellung der Monitoring- und Logging-Infrastruktur des Projekts
zuständig. Sie wird ausgelöst, wenn Änderungen an den Infrastrukturdateien vorgenommen und in den main Branch gepusht
werden. Zusätzlich kann der Workflow manuell gestartet werden.

Die Action beinhaltet einen Job namens `deploy`, der auf einem selbstgehosteten Runner ausgeführt wird. Dieser Job führt
folgende Schritte aus:

- Check-out des Codes: Der Code des Repositorys wird auf den Runner ausgecheckt.
- Bereitstellung der Docker Konfigurationen: Ein Shell-Skript namens `deploy-config.sh` wird ausgeführt. Dieses Skript
  liest die Konfigurationsdateien im `config` Ordner des `Infrastructure` Verzeichnisses ein und erstellt daraus [Docker
  Configs](https://docs.docker.com/engine/swarm/configs/). Dabei wird eine Versionsverwaltung implementiert, sodass bei
  jeder Änderung eine neue Version der Konfiguration erstellt und die letzten drei Versionen behalten werden.
- Deployment auf Docker Swarm: Die Infrastruktur-Komponenten werden mithilfe von `docker stack deploy` auf dem Docker
  Swarm Cluster deployt. Die `docker-stack.yml` Datei im `Infrastructure` Ordner enthält die Definition des Stacks.

**Application Deployment ([`labservices.yml`](../.github/workflows/labservices.yml)):**

Diese GitHub Action ist der Kern des CI/CD-Prozesses. Sie wird bei jedem Push zum main Branch ausgelöst, ausser die
Änderungen betreffen nur Dokumentation, Infrastruktur, Workflows oder die Readme-Datei. Zusätzlich kann der Workflow
manuell gestartet werden.

Die Action besteht aus zwei Jobs:

`build`: Dieser Job wird auf einem Ubuntu-Runner von GitHub ausgeführt. Er loggt sich zunächst in die GitHub Container
Registry (GHCR) ein. Anschliessend baut er das Docker Image der Anwendung und pusht es mit einem eindeutigen Tag,
basierend auf dem Hash des Commits, in die GHCR.

`deploy`: Dieser Job wird auf einem selbstgehosteten Runner ausgeführt und benötigt den erfolgreichen Abschluss des
build Jobs. Er loggt sich ebenfalls in die GHCR ein und deployt die Anwendung mithilfe von `docker stack deploy` auf dem
Docker Swarm Cluster. Der `docker-stack.yml` Datei entnimmt er die Konfiguration des Deployments. Umgebungsvariablen,
wie Datenbank-Credentials, werden aus GitHub Secrets bezogen. Abschliessend führt der Job Integrationstests durch, um
die erfolgreiche Bereitstellung und Funktionalität der Anwendung zu überprüfen.

**Quality Gate ([`qualitygate.yml`](../.github/workflows/qualitygate.yml)):**

Diese GitHub Action dient der Qualitätssicherung des Codes. Sie wird automatisch bei jedem Pull Request zum main Branch
ausgeführt, sofern die Änderungen nicht nur die Dokumentation, Infrastruktur oder die Workflows selbst betreffen.

Die Action besteht aus zwei Jobs:

- `lint`: Dieser Job überprüft die Codeformatierung mithilfe von dotnet format. So wird sichergestellt, dass der Code
  den
  definierten Stilrichtlinien entspricht.
- `test`: Dieser Job führt die Unit-Tests des Projekts aus und ermittelt die Testabdeckung. Dadurch wird die
  Funktionalität des Codes geprüft und sichergestellt, dass Änderungen keine bestehenden Funktionen beeinträchtigen.

---

## Szenarien

In den folgenden Abschnitten werden die umgesetzten Szenarien kurz beschrieben.

### Basisszenarien

#### Einfacher Change-Request, Umsetzung, Q-Gate, Merge-Request, Delivery und Deploy

**Prozess:**

1. **Change-Request erstellen:**  
   Ein Entwickler erstellt ein Issue im GitHub-Repository, um den gewünschten Änderungsbedarf zu dokumentieren.

   Das Issue wird mit dem Label Bug, Feature oder Change Request annotiert.
2. **Feature-Branch erstellen:**  
   Ein neuer Branch wird aus `main` erstellt:
   ```shell
   git checkout -b feature/<name>
   ```

   Die Änderungen werden implementiert und auf dem branch eingecheckt:
   ```shell
   git add <changed files>
   git commit -m "commit message"
   ```

   Die Änderungen werden anschliessend gepusht:
   ```shell
   git push -u origin feature/<name>
   ```

   Ein Pull-Request wird anschliessend beim Schritt 5 erstellt.

3. **Implementierung und Tests:**

   Änderungen werden implementiert und Unit-Tests werden hinzugefügt oder aktualisiert.

   Dazu kann die Test Solution im `WebTests`-Verzeichnis verwendet werden.
4. **Code-Review und Q-Gate:**

   Nach Abschluss wird ein Pull-Request (PR) auf den `main`-Branch erstellt. GitHub Actions führen automatische Tests
   und Quality-Gates aus (z. B. CodeQL für Sicherheitsprüfungen und Unit-Tests aus `WebTests`).
5. **Pull-Request:**

   Der PR wird nach erfolgreichem Review und automatischen Testing gemergt.

   Hier gilt das 4 Augen Prinzip im Team.
6. **Delivery und Deployment:**  
   Änderungen in `main` werden durch die GitHub Actions automatisiert in das produktive System deployt.

#### Aufsetzen der Entwicklungsumgebung für neue Mitarbeiter

Die Dokumentation für das Aufsetzen der Entwicklungsumgebung sowie das Onboarding eines neuen Mitarbeiters befindet sich
im [README.md](../README.md).

#### Einhaltung der Kriterien von 12-Factor-Applikationen

Die ToDo-Applikation erfüllt die **12-Factor-App-Kriterien**:

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
- **Logs:** Logs werden via Docker und Prometheus gestreamt. Hier auch nur über `stdout`.
- **Admin Processes:** Datenbankmigrationen erfolgen mit EF Core.

### Erweiterte Szenarien

#### Handhabung eines Notfall-Bugfixes (auf der Produktion)

1. **Hotfix-Branch erstellen:**  
   Fehler auf dem `main`-Branch identifizieren und beheben, anschliessend kann ein Hotfix-Branch erstellt werden:
   ```shell
   git checkout -b hotfix/<name>
   ```
2. **Fix und Testen:**  
   Änderung implementieren und Tests ausführen.
3. **Merge und Deployment:**  
   Nach erfolgreichem Review wird der Bugfix in `main` gemergt und automatisch durch die GitHub Action deployt.

Der Prozess folgt hier ausschliesslich
dem [GitHub Workflow](https://docs.github.com/en/get-started/using-github/github-flow).

#### Rückfall auf ältere Version (nach missglücktem Release)

1. **Revert durchführen:**  
   Ein Revert wird mit Git durchgeführt:
   ```shell
   git revert <commit-hash>
   ```
2. **Pull-Request erstellen:**  
   Für den Revert einen neuen Pull-Request erstellen, Tests durchführen und nach Genehmigung in `main` mergen.

#### Nachvollziehbare Behandlung eines Incidents und Korrektur

Die Alerts und Grafana Dashboard sind aktuell nicht konfiguriert, allerdings ist der Prozess nachfolgend beschrieben.

**Prozess:**

1. **Monitoring und Alarmierung:**

   Durch das Monitoring von den Metriken welche von Prometheus gesammelt werden, können automatisch Alerts über das
   Grafana Dashboard ausgelöst werden.
2. **Schnelle Reaktion:**

   Das Team wird umgehend benachrichtigt und kann den Incident analysieren.
3. **Identifizierung und Behebung der Ursache:**

   Mithilfe der Metriken und den Logs, welche von Promtail und Loki gesammelt werden, kann das Team die Ursache
   aufspüren und beheben.
4. **Korrekturen vornehmen, wenn nötig:**

   Allfällige Korrekturen am Code werden implementiert und getestet. Anschliessend werden diese gemergt und automatisch
   deployt.
5. **Überwachung und Dokumentation:**

   Nach dem Deployment wird die Plattform weiter überwacht, um sicherzustellen, dass der Fehler behoben ist und keine
   weiteren Probleme auftreten. Der gesamte Incident wird detailliert dokumentiert, um zukünftig ähnliche Incidents
   schneller lösen zu können.

#### Implementation eines Features mithilfe von Feature-Toggles

**Prozess:**

1. **Feature-Flag definieren:**  
   Feature-Toggles sind in der `appsettings.json`-Datei konfiguriert. Beispiel:
   ```json
   {
     "FeatureManagement": {
         "NewFeature": false
     }
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

Eine Veränderung zur Laufzeit ist aktuell nicht vorgesehen, wäre jedoch auch möglich, da eine Änderung an der
`appsettings.json`-Datei dynamisch zur Laufzeit erfolgen kann.

### Individuelle Szenarien

#### Unterschiedliche Technologien und Sprachen im Einsatz

Das Projekt ist durch die Verwendung von Container-Technologien grundsätzlich modular, sodass unterschiedliche
Technologien eingebunden werden könnten. Aktuell werden folgende Sprachen und Technologien eingesetzt:

- .NET 9 für die Backend-Entwicklung.
- PostgreSQL als Datenbanklösung.
- Docker für Containerisierung.

Dies gewährleistet eine flexible und skalierbare Plattform.

#### Austausch der Persistenz-Technologie

Das Team hat sich bewusst gegen den Einsatz eines allgemeinen `IRepository`-Interfaces entschieden.
Der Grund ist, dass EF Core bereits eine abstrahierte Schicht bietet, die die meisten Anforderungen abdeckt.
Zusätzliche Schichten könnten die Komplexität erhöhen, ohne signifikante Vorteile zu bieten.
Zudem war das herumspielen mit verschiedenen Datenbank-Technologien nicht im Interesse des Teams.

#### Migration der Persistenz (Schema)

Die Datenbankmigrationen werden durch EF Core automatisch verwaltet. Beispiel:

```shell
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

Dies stellt sicher, dass alle Schemaänderungen versioniert und reproduzierbar sind.

Beim Deploy-Prozess werden jedes Mal Migrationen ausgeführt auf DDL-Ebene.

#### Health Check

Der Healthcheck passiert über den `/health` HTTP-Endpoint. Da drin wird die Verbindung zur Datenbank
und allen notwendigen Services getestet.
